using NUnit.Framework;

namespace GroveGames.Logger.Unity.Editor.Tests
{
    public sealed class LoggerSettingsAssetTests
    {
        [Test]
        public void GetOrCreate_SettingsAsset_IsLoadedFromResources()
        {
            var asset = LoggerSettingsAsset.GetOrCreate();

            Assert.AreSame(asset, LoggerSettings.GetOrCreate());
        }
    }
}
