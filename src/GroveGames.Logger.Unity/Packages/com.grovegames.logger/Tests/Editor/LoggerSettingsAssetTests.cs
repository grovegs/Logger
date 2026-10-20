using NUnit.Framework;
using UnityEditor;

namespace GroveGames.Logger.Unity.Editor.Tests
{
    public sealed class LoggerSettingsAssetTests
    {
        [Test]
        public void GetOrCreate_SettingsAsset_IsReturnedAtRuntime()
        {
            var asset = LoggerSettingsAsset.GetOrCreate();

            var settings = LoggerSettings.GetOrCreate();

            Assert.AreSame(asset, settings);
        }

        [Test]
        public void GetOrCreate_SettingsAsset_IsPreloaded()
        {
            var asset = LoggerSettingsAsset.GetOrCreate();

            var preloaded = PlayerSettings.GetPreloadedAssets();

            Assert.Contains(asset, preloaded);
        }
    }
}
