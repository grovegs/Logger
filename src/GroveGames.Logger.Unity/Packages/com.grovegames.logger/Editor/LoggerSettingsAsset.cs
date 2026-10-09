#nullable disable
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GroveGames.Logger.Unity.Editor
{
    internal static class LoggerSettingsAsset
    {
        public const string AssetPath = "Assets/Settings/Resources/" + LoggerSettings.ResourcePath + ".asset";

        private const string LegacyConfigName = "com.grovegames.logger.settings";

        [InitializeOnLoadMethod]
        private static void MigrateOnLoad()
        {
            EditorApplication.delayCall += Migrate;
        }

        public static LoggerSettings GetOrCreate()
        {
            Migrate();
            var settings = AssetDatabase.LoadAssetAtPath<LoggerSettings>(AssetPath);

            if (settings != null)
            {
                return settings;
            }

            CreateFolder();
            AssetDatabase.CreateAsset(ScriptableObject.CreateInstance<LoggerSettings>(), AssetPath);
            AssetDatabase.SaveAssets();
            return AssetDatabase.LoadAssetAtPath<LoggerSettings>(AssetPath);
        }

        public static void Migrate()
        {
            if (AssetDatabase.LoadAssetAtPath<LoggerSettings>(AssetPath) == null)
            {
                var existing = FindExisting();

                if (existing != null)
                {
                    CreateFolder();
                    var error = AssetDatabase.MoveAsset(AssetDatabase.GetAssetPath(existing), AssetPath);

                    if (!string.IsNullOrEmpty(error))
                    {
                        Debug.LogError($"Could not move {nameof(LoggerSettings)} to {AssetPath}: {error}");
                    }
                }
            }

            if (EditorBuildSettings.TryGetConfigObject<LoggerSettings>(LegacyConfigName, out _))
            {
                EditorBuildSettings.RemoveConfigObject(LegacyConfigName);
            }

            RemoveFromPreloadedAssets();
        }

        private static LoggerSettings FindExisting()
        {
            if (EditorBuildSettings.TryGetConfigObject<LoggerSettings>(LegacyConfigName, out var legacy) && legacy != null)
            {
                return legacy;
            }

            foreach (var guid in AssetDatabase.FindAssets($"t:{nameof(LoggerSettings)}"))
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);

                if (path.StartsWith("Assets/", System.StringComparison.Ordinal))
                {
                    return AssetDatabase.LoadAssetAtPath<LoggerSettings>(path);
                }
            }

            return null;
        }

        private static void CreateFolder()
        {
            var directory = Path.GetDirectoryName(AssetPath).Replace('\\', '/');

            if (AssetDatabase.IsValidFolder(directory))
            {
                return;
            }

            var current = "Assets";

            foreach (var part in directory.Substring("Assets/".Length).Split('/'))
            {
                var next = current + "/" + part;

                if (!AssetDatabase.IsValidFolder(next))
                {
                    AssetDatabase.CreateFolder(current, part);
                }

                current = next;
            }
        }

        private static void RemoveFromPreloadedAssets()
        {
            var preloaded = new List<Object>(PlayerSettings.GetPreloadedAssets());

            if (preloaded.RemoveAll(asset => asset is LoggerSettings) > 0)
            {
                PlayerSettings.SetPreloadedAssets(preloaded.ToArray());
            }
        }
    }
}
