using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace GroveGames.Logger.Unity.Editor
{
    internal sealed class UnityLoggerSettingsBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            if (EditorBuildSettings.TryGetConfigObject<UnityLoggerSettings>(UnityLoggerSettings.GetConfigName(), out var settings) && settings != null)
            {
                UnityLoggerSettingsProvider.AddToPreloadedAssets(settings);
            }
        }
    }
}
