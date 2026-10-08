using System;
using NUnit.Framework;
using UnityEditor;

namespace GroveGames.Logger.Unity.Editor.Tests
{
    public sealed class LoggerSettingsBuildPreprocessorTests
    {
        [Test]
        public void OnPreprocessBuild_ConfiguredSettings_AddsSettingsToPreloadedAssets()
        {
            if (!EditorBuildSettings.TryGetConfigObject<LoggerSettings>(LoggerSettings.GetConfigName(), out var settings) || settings == null)
            {
                Assert.Ignore("No configured settings asset.");
            }
    
            var preloadedAssets = PlayerSettings.GetPreloadedAssets();
            PlayerSettings.SetPreloadedAssets(Array.FindAll(preloadedAssets, asset => asset is not LoggerSettings));
    
            try
            {
                new LoggerSettingsBuildPreprocessor().OnPreprocessBuild(null);
    
                CollectionAssert.Contains(PlayerSettings.GetPreloadedAssets(), settings);
            }
            finally
            {
                PlayerSettings.SetPreloadedAssets(preloadedAssets);
            }
        }
    }
}
