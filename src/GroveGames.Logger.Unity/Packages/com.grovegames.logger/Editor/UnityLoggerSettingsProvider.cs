using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

namespace GroveGames.Logger.Unity.Editor
{
    internal static class UnityLoggerSettingsProvider
    {
        private const string AssetPath = "Assets/Settings/LoggerSettings.asset";

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/GroveGames/Logger", SettingsScope.Project)
            {
                label = "Logger",
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = GetCurrentSettings();
                    var serializedObject = new SerializedObject(settings);

                    var container = new VisualElement
                    {
                        style =
                        {
                            paddingLeft = 10,
                            paddingRight = 10,
                            paddingTop = 10,
                            paddingBottom = 10
                        }
                    };

                    var title = new Label("Logger Settings")
                    {
                        style =
                        {
                            fontSize = 19,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginBottom = 10
                        }
                    };
                    container.Add(title);

                    var assetField = new ObjectField("Settings Asset")
                    {
                        objectType = typeof(UnityLoggerSettings),
                        value = settings,
                        style = { marginBottom = 10 }
                    };
                    assetField.RegisterValueChangedCallback(evt =>
                    {
                        if (evt.newValue is UnityLoggerSettings newSettings)
                        {
                            EditorBuildSettings.AddConfigObject(UnityLoggerSettings.GetConfigName(), newSettings, true);
                            serializedObject.Dispose();
                            serializedObject = new SerializedObject(newSettings);
                            rootElement.Bind(serializedObject);
                        }
                    });
                    container.Add(assetField);

                    container.Add(new PropertyField(serializedObject.FindProperty("_minLogLevel"), "Min Log Level"));
                    container.Add(new PropertyField(serializedObject.FindProperty("_maxFileCount"), "Max File Count"));
                    container.Add(new PropertyField(serializedObject.FindProperty("_fileFolderName"), "File Folder Name"));
                    container.Add(new PropertyField(serializedObject.FindProperty("_fileBufferSize"), "File Buffer Size"));
                    container.Add(new PropertyField(serializedObject.FindProperty("_fileChannelCapacity"), "File Channel Capacity"));

                    rootElement.Add(container);
                    rootElement.Bind(serializedObject);
                },
                keywords = new System.Collections.Generic.HashSet<string>(new[] { "Logger", "Log", "Level", "File", "Buffer", "Channel", "Grove Games" })
            };
        }

        private static UnityLoggerSettings GetCurrentSettings()
        {
            if (EditorBuildSettings.TryGetConfigObject<UnityLoggerSettings>(UnityLoggerSettings.GetConfigName(), out var existingSettings))
            {
                if (existingSettings != null)
                {
                    return existingSettings;
                }
            }

            var settings = AssetDatabase.LoadAssetAtPath<UnityLoggerSettings>(AssetPath);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<UnityLoggerSettings>();

                string directory = System.IO.Path.GetDirectoryName(AssetPath);
                if (!AssetDatabase.IsValidFolder(directory))
                {
                    System.IO.Directory.CreateDirectory(directory);
                    AssetDatabase.Refresh();
                }

                AssetDatabase.CreateAsset(settings, AssetPath);
                AssetDatabase.SaveAssets();
            }

            EditorBuildSettings.AddConfigObject(UnityLoggerSettings.GetConfigName(), settings, true);
            return settings;
        }
    }
}
