using System;
using System.Collections.Generic;
using UnityEditor.DeviceSimulation;
using UnityEngine;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using UnityEditor.AdaptivePerformance.Simulator.Editor;
using UnityEngine.AdaptivePerformance;
#if UNITY_2023_1_OR_NEWER
using UnityEngine.AdaptivePerformance.Google.Android;
using UnityEngine.Android;
#endif

namespace UnityEditor.AdaptivePerformance.Google.Android.Editor
{
    /// <summary>
    /// Device Simulator plug-in implementation for the Adaptive Performance Google provider.
    /// </summary>
    public class AdaptivePerformanceGoogleUIExtension :
        DeviceSimulatorPlugin,
        ISerializationCallbackReceiver
    {
        /// <summary>
        /// Title for the plug-in UI.
        /// </summary>
        override public string title => "Adaptive Performance Android";

        VisualElement m_ExtensionFoldout;
        Foldout m_SettingsFoldout;
        Foldout m_AndroidSystemFoldout;
        EnumField m_GameMode;
        SimulatorAdaptivePerformanceSubsystem m_Subsystem;

        [SerializeField, HideInInspector]
        AdaptivePerformanceStates m_SerializationStates;

        /// <summary>
        /// The VisualElement that this method returns is embedded in the Device Simulator window. If the method returns null, plug-in UI is not embedded.
        /// </summary>
        /// <returns>Returns the VisualElement to embed in the Device Simulator window.</returns>
        public override VisualElement OnCreateUI()
        {
            m_ExtensionFoldout = new VisualElement();

            VisualTreeAsset tree =
#if UNITY_2023_1_OR_NEWER
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.unity.adaptiveperformance.google.android/Editor/DeviceSimulator/AdaptivePerformanceExtension.uxml");
#else
                null;
#endif

            if (tree == null)
            {
                Label warningLabel = new Label("Adaptive Performance Android options are available in Unity 2023.1+");
                warningLabel.style.whiteSpace = WhiteSpace.Normal;
                m_ExtensionFoldout.Add(warningLabel);
                return m_ExtensionFoldout;
            }

            m_ExtensionFoldout.Add(tree.CloneTree());

            m_AndroidSystemFoldout = m_ExtensionFoldout.Q<Foldout>("google-android-system");
            m_AndroidSystemFoldout.value = m_SerializationStates.androidSystemFoldout;
            m_GameMode = m_ExtensionFoldout.Q<EnumField>("google-game-mode");

            m_GameMode.RegisterCallback<ChangeEvent<Enum>>(evt =>
            {
                evt.StopPropagation();

                SimulatorAdaptivePerformanceSubsystem subsystem = Subsystem();
                if (subsystem == null)
                    return;

#if UNITY_2023_1_OR_NEWER
                subsystem.PerformanceMode = PerformanceModeUtilities.ConvertGameModeToPerformanceMode((AndroidGameMode)evt.newValue);
#else
                subsystem.PerformanceMode = PerformanceMode.Unknown;
#endif
            });

            EditorApplication.playModeStateChanged += LogPlayModeState;

            return m_ExtensionFoldout;
        }

        [System.Serializable]
        internal struct AdaptivePerformanceStates
        {
            public bool androidSystemFoldout;
        };

        /// <summary>
        /// Transforms data into serializable data types immediately before an object is serialized.
        /// </summary>
        public void OnBeforeSerialize()
        {
            if (m_SettingsFoldout == null)
                return;

            m_SerializationStates.androidSystemFoldout = m_AndroidSystemFoldout.value;
        }

        /// <summary>
        /// Transforms data back into runtime data types after an object is deserialized.
        /// </summary>
        public void OnAfterDeserialize() { }

        void LogPlayModeState(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.EnteredPlayMode)
            {
                SyncSubsystemSettingsToEditor();
                SubscribeToAPEvents();
            }
            else
            {
                UnsubscribeToAPEvents();
            }
        }

        void SyncSubsystemSettingsToEditor()
        {
#if UNITY_2023_1_OR_NEWER
            var ap = Holder.Instance;
            if (ap == null)
                return;

            var gameMode = PerformanceModeUtilities.ConvertPerformanceModeToGameMode(ap.PerformanceModeStatus.PerformanceMode);
            m_GameMode.value = gameMode;
#endif
        }

        SimulatorAdaptivePerformanceSubsystem Subsystem()
        {
            if (!Application.isPlaying)
                return null;

            var loader = AdaptivePerformanceGeneralSettings.Instance?.Manager.activeLoader;
            return loader == null
                ? null
                : loader.GetLoadedSubsystem<SimulatorAdaptivePerformanceSubsystem>();
        }

        void SubscribeToAPEvents()
        {
            var ap = Holder.Instance;
            if (ap == null)
                return;

            ap.PerformanceModeStatus.PerformanceModeEvent += OnPerformanceModeEvent;
        }

        void UnsubscribeToAPEvents()
        {
            var ap = Holder.Instance;
            if (ap == null)
                return;

            ap.PerformanceModeStatus.PerformanceModeEvent -= OnPerformanceModeEvent;
        }

        void OnPerformanceModeEvent(PerformanceMode performanceMode)
        {
#if UNITY_2023_1_OR_NEWER
            var gameMode = PerformanceModeUtilities.ConvertPerformanceModeToGameMode(performanceMode);
            if ((AndroidGameMode)m_GameMode.value == gameMode)
                return;

            m_GameMode.value = gameMode;
#endif
        }
    }
}
