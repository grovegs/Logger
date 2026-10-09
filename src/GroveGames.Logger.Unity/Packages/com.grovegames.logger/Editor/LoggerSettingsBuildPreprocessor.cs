using UnityEditor.Build;
using UnityEditor.Build.Reporting;

namespace GroveGames.Logger.Unity.Editor
{
    internal sealed class LoggerSettingsBuildPreprocessor : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            LoggerSettingsAsset.Migrate();
        }
    }
}
