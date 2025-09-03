using UnityEngine;
using UnityEngine.AdaptivePerformance.Google.Android;

namespace UnityEditor.AdaptivePerformance.Google.Android.Editor
{
    [InitializeOnLoad]
    static internal class GoogleAndroidProviderFrameStatsDialog
    {
        static internal bool DialogDisplayed { get; private set; } = false;

        static GoogleAndroidProviderFrameStatsDialog()
        {
            if (Application.isBatchMode)
            {
                return;
            }
            GoogleAndroidProviderSettings settings = null;
            var assets = AssetDatabase.FindAssets($"t:{typeof(GoogleAndroidProviderSettings).Name}");
            if (assets.Length > 0)
            {
                var path = AssetDatabase.GUIDToAssetPath(assets[0]);
                settings = AssetDatabase.LoadAssetAtPath(path, typeof(GoogleAndroidProviderSettings)) as GoogleAndroidProviderSettings;
            }
            if (!DialogDisplayed && settings != null)
            {
                DialogDisplayed = settings.frameStatsDialogDisplayed;
            }
            if (DialogDisplayed)
            {
                return;
            }
            DialogDisplayed = true;
            if (settings != null)
            {
                settings.frameStatsDialogDisplayed = true;
                EditorUtility.SetDirty(settings);
            }
            if (PlayerSettings.enableFrameTimingStats)
            {
                return;
            }
            if (EditorUtility.DisplayDialog("Adaptive Performance Android",
                "\"Frame Timing Stats\" option needs to be enabled in the Player Settings to provide precise frame time information necessary for the Google Performance Hint Manager to function as expected.",
                "Enable", "Don't enable"))
            {
                PlayerSettings.enableFrameTimingStats = true;
            }
        }
    }
}
