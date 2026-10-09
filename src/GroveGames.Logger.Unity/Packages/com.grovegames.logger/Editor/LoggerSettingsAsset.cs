using UnityEditor;
using UnityEngine;

namespace GroveGames.Logger.Unity.Editor
{
    internal static class LoggerSettingsAsset
    {
        public const string AssetPath = "Assets/Settings/Resources/" + LoggerSettings.ResourcePath + ".asset";

        public static LoggerSettings GetOrCreate()
        {
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
