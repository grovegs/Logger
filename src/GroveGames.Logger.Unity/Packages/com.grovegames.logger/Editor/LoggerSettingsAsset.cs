using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GroveGames.Logger.Unity.Editor
{
    internal static class LoggerSettingsAsset
    {
        public const string AssetPath = "Assets/Settings/GroveGames/LoggerSettings.asset";

        private const string LegacyAssetPath = "Assets/Settings/Resources/GroveGames/LoggerSettings.asset";

        [InitializeOnLoadMethod]
        private static void MigrateLegacyAsset()
        {
            if (File.Exists(LegacyAssetPath) && !EditorBuildSettings.TryGetConfigObject(UnityLoggerSettings.ConfigName, out UnityLoggerSettings _))
            {
                EditorApplication.delayCall += () => GetOrCreate();
            }
        }

        public static UnityLoggerSettings GetOrCreate()
        {
            if (!EditorBuildSettings.TryGetConfigObject(UnityLoggerSettings.ConfigName, out UnityLoggerSettings settings) || settings == null)
            {
                settings = LoadOrCreate();
                EditorBuildSettings.AddConfigObject(UnityLoggerSettings.ConfigName, settings, true);
            }

            Preload(settings);
            return settings;
        }

        private static UnityLoggerSettings LoadOrCreate()
        {
            CreateFolder();

            if (AssetDatabase.LoadAssetAtPath<UnityLoggerSettings>(AssetPath) == null && AssetDatabase.LoadAssetAtPath<UnityLoggerSettings>(LegacyAssetPath) != null)
            {
                AssetDatabase.MoveAsset(LegacyAssetPath, AssetPath);
            }

            var settings = AssetDatabase.LoadAssetAtPath<UnityLoggerSettings>(AssetPath);

            if (settings != null)
            {
                return settings;
            }

            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<UnityLoggerSettings>(), AssetPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<UnityLoggerSettings>(AssetPath);
        }

        private static void Preload(UnityLoggerSettings settings)
        {
            var preloaded = PlayerSettings.GetPreloadedAssets();
            var assets = new List<Object>(preloaded.Length + 1);
            var isPreloaded = false;

            for (var i = 0; i < preloaded.Length; i++)
            {
                var asset = preloaded[i];

                if (asset is UnityLoggerSettings && asset != settings)
                {
                    continue;
                }

                isPreloaded |= asset == settings;
                assets.Add(asset);
            }

            if (!isPreloaded)
            {
                assets.Add(settings);
            }

            if (isPreloaded && assets.Count == preloaded.Length)
            {
                return;
            }

            PlayerSettings.SetPreloadedAssets(assets.ToArray());
        }

        private static void CreateFolder()
        {
            var current = "Assets";

            foreach (var part in AssetPath.Substring("Assets/".Length).Split('/'))
            {
                if (part.EndsWith(".asset", System.StringComparison.Ordinal))
                {
                    return;
                }

                var next = current + "/" + part;

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, part);
                }

                current = next;
            }
        }
    }
}
