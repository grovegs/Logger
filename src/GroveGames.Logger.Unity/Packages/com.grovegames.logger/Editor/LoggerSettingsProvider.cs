using System.Collections.Generic;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace GroveGames.Logger.Unity.Editor
{
    internal static class LoggerSettingsProvider
    {
        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new SettingsProvider("Project/GroveGames/Logger", SettingsScope.Project)
            {
                label = "Logger",
                activateHandler = (searchContext, rootElement) =>
                {
                    var settings = LoggerSettingsAsset.GetOrCreate();
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

                    container.Add(new Label("Logger Settings")
                    {
                        style =
                        {
                            fontSize = 19,
                            unityFontStyleAndWeight = FontStyle.Bold,
                            marginBottom = 10
                        }
                    });

                    var assetField = new ObjectField("Settings Asset")
                    {
                        objectType = typeof(LoggerSettings),
                        value = settings,
                        style = { marginBottom = 10 }
                    };
                    assetField.SetEnabled(false);
                    container.Add(assetField);

                    container.Add(new PropertyField(serializedObject.FindProperty("_minLogLevel"), "Min Log Level"));
                    container.Add(new PropertyField(serializedObject.FindProperty("_maxFileCount"), "Max File Count"));
                    container.Add(new PropertyField(serializedObject.FindProperty("_fileFolderName"), "File Folder Name"));
                    container.Add(new PropertyField(serializedObject.FindProperty("_fileBufferSize"), "File Buffer Size"));
                    container.Add(new PropertyField(serializedObject.FindProperty("_fileChannelCapacity"), "File Channel Capacity"));

                    rootElement.Add(container);
                    rootElement.Bind(serializedObject);
                },
                keywords = new HashSet<string>(new[] { "Logger", "Log", "Level", "File", "Buffer", "Channel", "Grove Games" })
            };
        }
    }
}
