using UnityEditor;

namespace GroveGames.Logger.Unity.Editor
{
    public sealed class UnityLoggerSettingsProvider : SettingsProvider
    {
        private static readonly string[] s_keywords = { "Logger", "Log", "Grove Games", "File", "Console" };

        private UnityLoggerSettings _settings;

        private UnityLoggerSettingsProvider(string path, SettingsScope scopes)
            : base(path, scopes, s_keywords)
        {
        }

        public override void OnActivate(string searchContext, UnityEngine.UIElements.VisualElement rootElement)
        {
            _settings = UnityLoggerSettings.Load();
        }

        public override void OnGUI(string searchContext)
        {
            EditorGUILayout.Space(10);

            EditorGUI.BeginChangeCheck();

            _settings.MinLogLevel = (LogLevel)EditorGUILayout.EnumPopup("Min Log Level", _settings.MinLogLevel);
            _settings.MaxFileCount = EditorGUILayout.IntField("Max File Count", _settings.MaxFileCount);
            _settings.FileFolderName = EditorGUILayout.TextField("File Folder Name", _settings.FileFolderName);
            _settings.FileBufferSize = EditorGUILayout.IntField("File Buffer Size", _settings.FileBufferSize);
            _settings.FileChannelCapacity = EditorGUILayout.IntField("File Channel Capacity", _settings.FileChannelCapacity);

            if (EditorGUI.EndChangeCheck())
            {
                _settings.Save();
            }
        }

        [SettingsProvider]
        public static SettingsProvider CreateSettingsProvider()
        {
            return new UnityLoggerSettingsProvider("Project/Grove Games/Logger", SettingsScope.Project);
        }
    }
}
