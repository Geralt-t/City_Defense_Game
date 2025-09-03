#if UNITY_ANDROID

using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UnityEngine.Scripting;
using UnityEngine.AdaptivePerformance.Provider;
using AOT;
using UnityEngine.Rendering;

#if UNITY_2023_1_OR_NEWER
using UnityEngine.Android;
#endif

namespace UnityEngine.AdaptivePerformance.Google.Android
{
    internal static class ADPFLog
    {
        static GoogleAndroidProviderSettings settings = GoogleAndroidProviderSettings.GetSettings();

        [Conditional("DEVELOPMENT_BUILD")]
        public static void Debug(string format, params object[] args)
        {
            if (settings != null && settings.googleProviderLogging)
                UnityEngine.Debug.Log(System.String.Format("[AP ADPF] " + format, args));
        }
    }

    [Preserve]
    public class GoogleAndroidAdaptivePerformanceSubsystem : AdaptivePerformanceSubsystem
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static AdaptivePerformanceSubsystemDescriptor RegisterDescriptor()
        {
            if (!GoogleAndroidAdaptivePerformanceSubsystemProvider.NativeApi.IsAvailable())
            {
                ADPFLog.Debug($"The native API for this provider is not available. Aborting registering the Adaptive Performance provider descriptor.");
                return null;
            }

            var registeredDesc = AdaptivePerformanceSubsystemDescriptor.RegisterDescriptor(new AdaptivePerformanceSubsystemDescriptor.Cinfo
            {
                id = "GoogleAndroid",
                providerType = typeof(GoogleAndroidAdaptivePerformanceSubsystem.GoogleAndroidAdaptivePerformanceSubsystemProvider),
                subsystemTypeOverride = typeof(GoogleAndroidAdaptivePerformanceSubsystem)
            });
            return registeredDesc;
        }

        public class GoogleAndroidAdaptivePerformanceSubsystemProvider : APProvider, IApplicationLifecycle, IDevicePerformanceLevelControl
        {
            NativeApi m_Api = null;

            PerformanceDataRecord m_Data = new PerformanceDataRecord();
            object m_DataLock = new object();

            float m_Temperature = 0.0f;
            float m_TemperatureUpdateTimestamp;
            float m_TemperatureUpdateInterval = 10.0f; // per Google's request

            bool m_ThermalInitialized = false;
            bool m_HintInitialized = false;

            Version m_Version = null;
            PerformanceMode m_PerformanceMode = PerformanceMode.Unknown;

            public override IApplicationLifecycle ApplicationLifecycle { get { return this; } }
            public override IDevicePerformanceLevelControl PerformanceLevelControl { get { return this; } }

            public int MaxCpuPerformanceLevel { get; set; }
            public int MaxGpuPerformanceLevel { get; set; }

            static GoogleAndroidProviderSettings s_Settings = GoogleAndroidProviderSettings.GetSettings();

            public GoogleAndroidAdaptivePerformanceSubsystemProvider()
            {
                MaxCpuPerformanceLevel = 3;
                MaxGpuPerformanceLevel = 3;

                m_Api = new NativeApi(OnPerformanceWarning);
            }

            void OnPerformanceWarning(WarningLevel warningLevel)
            {
                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.WarningLevel;
                    m_Data.WarningLevel = warningLevel;
                }
            }

            void ImmediateUpdateTemperature()
            {
                if (!Capabilities.HasFlag(Feature.TemperatureLevel))
                    return;

                UpdateTemperatureLevel();
                m_TemperatureUpdateTimestamp = Time.time;

                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.TemperatureLevel;
                    m_Data.TemperatureLevel = m_Temperature;
                }
            }

            void TimedUpdateTemperature()
            {
                if (!Capabilities.HasFlag(Feature.TemperatureLevel))
                    return;

                var timestamp = Time.time;
                var canUpdateTemperature = (timestamp - m_TemperatureUpdateTimestamp) > m_TemperatureUpdateInterval;

                if (!canUpdateTemperature)
                    return;

                var previousTemperature = m_Temperature;
                UpdateTemperatureLevel();
                var isTemperatureChanged = previousTemperature != m_Temperature;
                m_TemperatureUpdateTimestamp = timestamp;

                if (!isTemperatureChanged)
                    return;

                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.TemperatureLevel;
                    m_Data.TemperatureLevel = m_Temperature;
                }
            }

            void ImmediateUpdateThermalStatus()
            {
                if (!Capabilities.HasFlag(Feature.WarningLevel))
                    return;

                var warningLevel = m_Api.GetThermalStatusWarningLevel();

                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.WarningLevel;
                    m_Data.WarningLevel = warningLevel;
                }
            }

            void ImmediateUpdatePerformanceMode()
            {
                if (!Capabilities.HasFlag(Feature.PerformanceMode))
                    return;

#if UNITY_2023_1_OR_NEWER
                var gameMode = m_Api.GetGameMode();
                m_PerformanceMode = PerformanceModeUtilities.ConvertGameModeToPerformanceMode(gameMode);
#else
                m_PerformanceMode = PerformanceMode.Unknown;
#endif

                if (m_Data.PerformanceMode == m_PerformanceMode)
                    return;

                lock (m_DataLock)
                {
                    m_Data.ChangeFlags |= Feature.PerformanceMode;
                    m_Data.PerformanceMode = m_PerformanceMode;
                }
            }

            protected override bool TryInitialize()
            {
                if (Initialized)
                {
                    return true;
                }

                if (!base.TryInitialize())
                {
                    return false;
                }

                var apiLevel = NativeApi.GetApiLevel();
                if (apiLevel < 30)
                {
                    return false;
                }

                m_Version = new Version(apiLevel, 0, 0);
                m_ThermalInitialized = m_Api.SetupThermal();
                if (m_ThermalInitialized)
                {
                    MaxCpuPerformanceLevel = m_Api.GetMaxCpuPerformanceLevel();
                    MaxGpuPerformanceLevel = m_Api.GetMaxGpuPerformanceLevel();
                    Capabilities = Feature.CpuPerformanceLevel | Feature.GpuPerformanceLevel | Feature.WarningLevel;
                    if (apiLevel >= 31)
                    {
                        Capabilities |= Feature.TemperatureLevel;
                    }
                }

                // NDK Performance Hint API is available from API 33, but even though it's not officially available, it works on Pixel 6 / Android 12 (API 31)
                if (apiLevel >= 31)
                {
                    Capabilities |= Feature.PerformanceMode;
                    m_HintInitialized = m_Api.SetupHints();
                    if (m_HintInitialized)
                    {
                        Capabilities |= Feature.PerformanceLevelControl;
                    }
                }

                Initialized = m_ThermalInitialized || m_HintInitialized;

                if (Initialized)
                {
                    m_Data.PerformanceLevelControlAvailable = true;
                }

                return Initialized;
            }

            public override void Start()
            {
                if (!Initialized)
                {
                    return;
                }

                if (m_Running)
                {
                    return;
                }

                // check for availability of the APIS since some devices don't report thermals at all
                if (Capabilities.HasFlag(Feature.WarningLevel) && !NativeApi.IsThermalStatusValid())
                {
                    ADPFLog.Debug("This device does not report thermal status correctly. Disabling thermal status readings.");
                    Capabilities &= ~Feature.WarningLevel;
                }
                if (Capabilities.HasFlag(Feature.TemperatureLevel) && double.IsNaN(m_Api.GetThermalHeadroom()))
                {
                    ADPFLog.Debug("This device does not report thermal headroom correctly. Disabling thermal headroom readings.");
                    Capabilities &= ~Feature.TemperatureLevel;
                }

                ImmediateUpdateTemperature();
                ImmediateUpdateThermalStatus();
                ImmediateUpdatePerformanceMode();

                m_Running = true;
            }

            public override void Stop()
            {
                m_Running = false;
            }

            public override void Destroy()
            {
                if (m_Running)
                {
                    Stop();
                }

                if (!Initialized)
                {
                    return;
                }

                if (m_ThermalInitialized)
                {
                    NativeApi.ThermalTeardown();
                    m_ThermalInitialized = false;
                }
                if (m_HintInitialized)
                {
                    NativeApi.HintTeardown();
                    m_HintInitialized = false;
                }
                Initialized = false;
            }

            public override string Stats => $"SkinTemp={m_Temperature} PerformanceMode={m_PerformanceMode}";

            public override PerformanceDataRecord Update()
            {
                if(Capabilities.HasFlag(Feature.PerformanceLevelControl))
                    m_Api.UpdateHintSystem();

                TimedUpdateTemperature();

                lock (m_DataLock)
                {
                    PerformanceDataRecord result = m_Data;
                    m_Data.ChangeFlags = Feature.None;

                    return result;
                }
            }

            public override Version Version => m_Version;

            public override Feature Capabilities { get; set; }

            public override bool Initialized { get; set; }

            public bool SetPerformanceLevel(ref int cpuLevel, ref int gpuLevel)
            {
                return false;
            }

            public bool EnableCpuBoost()
            {
                return false;
            }

            public bool EnableGpuBoost()
            {
                return false;
            }

            public void ApplicationPause() { }

            public void ApplicationResume()
            {
                ImmediateUpdateTemperature();
                ImmediateUpdatePerformanceMode();
            }

            void UpdateTemperatureLevel()
            {
                if (!Capabilities.HasFlag(Feature.TemperatureLevel))
                    return;
                var thermalHeadroom = m_Api.GetThermalHeadroom();
                // getThermalHeadroom may return NaN in some cases (https://developer.android.com/ndk/reference/group/thermal#athermal_getthermalheadroom)
                if (!double.IsNaN(thermalHeadroom))
                {
                    m_Temperature = (float)Math.Round(thermalHeadroom, 2, MidpointRounding.AwayFromZero);
                }
            }

            internal class NativeApi
            {
                const int k_TemperatureWarningLevelErrorOrUnknown = -1;
                const int k_TemperatureWarningLevelNoWarning = 0;
                const int k_TemperatureWarningLevelThrottlingImminent = 1;
                const int k_TemperatureWarningLevelThrottling = 2;

                static Action<WarningLevel> s_PerformanceWarningEvent;

                public NativeApi(Action<WarningLevel> sustainedPerformanceWarning)
                {
                    s_PerformanceWarningEvent = sustainedPerformanceWarning;
                }

                /// <summary>
                /// A delegate representation of <see cref="OnHighTempWarning(int)"/>. This maintains a strong
                /// reference to the delegate, which is converted to an IntPtr by <see cref="m_OnHighTempWarningHandlerFuncPtr"/>.
                /// </summary>
                static Action<int> s_OnHighTempWarningHandler = OnHighTempWarning;

                /// <summary>
                /// A pointer to a method to be called immediately when thermal state changes.
                /// </summary>
                readonly IntPtr m_OnHighTempWarningHandlerFuncPtr = Marshal.GetFunctionPointerForDelegate(s_OnHighTempWarningHandler);

                [Preserve]
                [MonoPInvokeCallback(typeof(Action<int>))]
                static void OnHighTempWarning(int warningLevel)
                {
                    ADPFLog.Debug("Listener: onHighTempWarning(warningLevel={0})", warningLevel);

                    if (warningLevel == k_TemperatureWarningLevelNoWarning || warningLevel == k_TemperatureWarningLevelErrorOrUnknown)
                        s_PerformanceWarningEvent(WarningLevel.NoWarning);
                    else if (warningLevel == k_TemperatureWarningLevelThrottlingImminent)
                        s_PerformanceWarningEvent(WarningLevel.ThrottlingImminent);
                    else if (warningLevel == k_TemperatureWarningLevelThrottling)
                        s_PerformanceWarningEvent(WarningLevel.Throttling);
                }

                static int m_ApiLevel = 0;
                public static int GetApiLevel()
                {
#if !UNITY_EDITOR
                    if (m_ApiLevel == 0)
                    {
                        using (var version = new AndroidJavaClass("android.os.Build$VERSION"))
                        {
                            m_ApiLevel = version.GetStatic<int>("SDK_INT");
                            ADPFLog.Debug($"Device API Level: {m_ApiLevel}");
                        }
                    }
#endif
                    return m_ApiLevel;
                }

                public static bool IsAvailable()
                {
                    return GetApiLevel() >= 30;
                }

                public static bool IsThermalStatusValid() => GetLatestThermalStatus() != k_TemperatureWarningLevelErrorOrUnknown;

                [DllImport("AdaptivePerformanceThermalHeadroom", EntryPoint = "Unity_AdaptivePerformance_ThermalHeadroom_Setup")]
                public static extern void ThermalSetup(IntPtr _onHighTempWarning);
                [DllImport("AdaptivePerformanceThermalHeadroom", EntryPoint = "Unity_AdaptivePerformance_ThermalHeadroom_Teardown")]
                public static extern void ThermalTeardown();
                [DllImport("AdaptivePerformanceThermalHeadroom", EntryPoint = "Unity_AdaptivePerformance_ThermalHeadroom_GetLatestThermalStatus")]
                public static extern int GetLatestThermalStatus();
                [DllImport("AdaptivePerformanceThermalHeadroom", EntryPoint = "Unity_AdaptivePerformance_ThermalHeadroom_GetPluginCallback")]
                public static extern IntPtr GetThermalPluginCallback();
                [DllImport("AdaptivePerformanceThermalHeadroom", EntryPoint = "Unity_AdaptivePerformance_ThermalHeadroom_GetThermalHeadroomForSeconds")]
                public static extern double GetThermalHeadroomForSeconds(int forecastSeconds);

                [DllImport("AdaptivePerformanceHint", EntryPoint = "Unity_AdaptivePerformance_Hint_Multithreaded")]
                public static extern bool HintMultithreaded();
                [DllImport("AdaptivePerformanceHint", EntryPoint = "Unity_AdaptivePerformance_Hint_CreateSession")]
                public static extern int HintCreateSession(bool mainThread, bool gfxThread, long desiredDuration);
                [DllImport("AdaptivePerformanceHint", EntryPoint = "Unity_AdaptivePerformance_Hint_Teardown")]
                public static extern void HintTeardown();
                [DllImport("AdaptivePerformanceHint", EntryPoint = "Unity_AdaptivePerformance_Hint_ReportCompletionTimes")]
                public static extern void ReportCompletionTimes(int session, long totalDuration, long cpuDuration, long gpuDuration, long workStart);
                [DllImport("AdaptivePerformanceHint", EntryPoint = "Unity_AdaptivePerformance_Hint_UpdateTargetWorkDuration")]
                public static extern void UpdateTargetWorkDuration(int session, long targetDuration);
                [DllImport("AdaptivePerformanceHint", EntryPoint = "Unity_AdaptivePerformance_Hint_GetPluginCallback")]
                public static extern IntPtr GetHintPluginCallback();

                public bool SetupThermal()
                {
                    bool success = false;
                    try
                    {
                        ThermalSetup(m_OnHighTempWarningHandlerFuncPtr);
                        success = true;
                    }
                    catch (Exception ex)
                    {
                        ADPFLog.Debug($"[Exception] RegisterListener() Thermal failed with {ex}!");
                    }

                    if (!success)
                        ADPFLog.Debug($"failed to register thermal");

                    return success;
                }

                public double GetThermalHeadroom(int forecastInSeconds = 0)
                {
                    return GetThermalHeadroomForSeconds(forecastInSeconds);
                }

                public WarningLevel GetThermalStatusWarningLevel()
                {
                    var thermalStatus = GetLatestThermalStatus();
                    var warningLevel = WarningLevel.NoWarning;

                    if (thermalStatus == k_TemperatureWarningLevelThrottlingImminent)
                        warningLevel = WarningLevel.ThrottlingImminent;
                    else if (thermalStatus == k_TemperatureWarningLevelThrottling)
                        warningLevel = WarningLevel.Throttling;

                    return warningLevel;
                }

                long GetDesiredDuration()
                {
                    var desiredDuration = 16666666L; // nanoseconds
                    var fps = Application.targetFrameRate;
                    if (fps == -1)
                    {
                        // default FPS for mobile platforms
                        fps = 30;
                    }
                    desiredDuration = (long)(1.0 / (double)fps * 1000000000.0);
                    return desiredDuration;
                }

                long GetDesiredTotalDuration(long desiredDuration, long totalDuration)
                {
                    if (totalDuration < desiredDuration)
                        return desiredDuration;
                    if (totalDuration < desiredDuration * 2)
                        return desiredDuration * 2;
                    return desiredDuration * 3;
                }

                long DoubleMsToNanos(double time) => (long)(time * 1000000.0);

                int m_HintSessionCommon = -1;
                int m_HintSessionCPU = -1;
                long m_ReportedDurationCommon = 0;
                long m_ReportedDurationCPU = 0;
                bool m_HintMultithreaded = false;

                public bool SetupHints()
                {
                    bool success = false;
                    try
                    {
                        // FrameTimingsManager must be enabled to use with Performance Hints API in Unity 2022.1+
                        if (!FrameTimingManager.IsFeatureEnabled())
                        {
                            ADPFLog.Debug($"Frame Timing Stats are not enable in Player Settings, using hints will be disabled.");
                            return false;
                        }
                        var gfxThread = true;
                        var commandBuffer = new CommandBuffer();
                        commandBuffer.IssuePluginEventAndData(GetHintPluginCallback(), 0, (IntPtr)0);
                        Graphics.ExecuteCommandBuffer(commandBuffer);
                        var desiredDuration = GetDesiredDuration();
                        m_HintSessionCommon = HintCreateSession(true, gfxThread, desiredDuration);
                        if (m_HintSessionCommon >= 0)
                        {
                            m_ReportedDurationCommon = desiredDuration;
                            m_HintSessionCPU = HintCreateSession(true, gfxThread, desiredDuration);
                            m_ReportedDurationCommon = m_ReportedDurationCPU = desiredDuration;
                            m_HintMultithreaded = HintMultithreaded();
                            success = true;
                        }
                    }
                    catch (Exception ex)
                    {
                        ADPFLog.Debug($"[Exception] Hint Setup failed with {ex}!");
                    }

                    if (!success)
                        ADPFLog.Debug($"failed to setup hints");

                    return success;
                }

                UnityEngine.FrameTiming[] m_FrameTimings = new UnityEngine.FrameTiming[1];

                public void UpdateHintSystem()
                {
                    FrameTimingManager.CaptureFrameTimings();
                    var res = FrameTimingManager.GetLatestTimings(1, m_FrameTimings);
                    if (res == 0)
                    {
                        ADPFLog.Debug($"FrameTimingManager does not have results, skip reporting.");
                        return;
                    }
                    var cpuTime = DoubleMsToNanos(m_FrameTimings[0].cpuMainThreadFrameTime);
                    var gfxTime = DoubleMsToNanos(m_FrameTimings[0].cpuRenderThreadFrameTime);
                    var gpuTime = DoubleMsToNanos(m_FrameTimings[0].gpuFrameTime);
                    var startTime = (long)m_FrameTimings[0].frameStartTimestamp;
                    var totalCpuTime = m_HintMultithreaded ? cpuTime + gfxTime : cpuTime;
                    ReportCompletionTimes(m_HintSessionCommon, totalCpuTime + gpuTime, totalCpuTime, gpuTime, startTime);
                    ReportCompletionTimes(m_HintSessionCPU, Math.Max(cpuTime, gfxTime), Math.Max(cpuTime, gfxTime), 0L, startTime);

                    var desiredDuration = GetDesiredDuration();
                    if (desiredDuration != m_ReportedDurationCPU)
                    {
                        UpdateTargetWorkDuration(m_HintSessionCPU, desiredDuration);
                        m_ReportedDurationCPU = desiredDuration;
                    }
                    var desiredTotalDuration = GetDesiredTotalDuration(desiredDuration, totalCpuTime + gpuTime);
                    if (desiredTotalDuration != m_ReportedDurationCommon)
                    {
                        UpdateTargetWorkDuration(m_HintSessionCommon, desiredTotalDuration);
                        m_ReportedDurationCommon = desiredTotalDuration;
                    }
                }

#if UNITY_2023_1_OR_NEWER
                public AndroidGameMode GetGameMode()
                {
                    return AndroidGame.GameMode;
                }
#endif

                public bool EnableCpuBoost()
                {
                    return false;
                }

                public bool EnableGpuBoost()
                {
                    return false;
                }

                public int GetClusterInfo()
                {
                    int result = -999;
                    return result;
                }

                public int GetMaxCpuPerformanceLevel()
                {
                    int maxCpuPerformanceLevel = -1;
                    return maxCpuPerformanceLevel;
                }

                public int GetMaxGpuPerformanceLevel()
                {
                    int maxGpuPerformanceLevel = -1;
                    return maxGpuPerformanceLevel;
                }
            }
        }
    }
}
#endif
