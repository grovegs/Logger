using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace GroveGames.Logger.Unity
{
    public sealed class LoggerSettings : ScriptableObject
    {
        private const string ConfigName = "com.grovegames.logger.settings";

        private static LoggerSettings s_loaded;

        [SerializeField] private LogLevel _minLogLevel = LogLevel.Information;
        [SerializeField] private int _maxFileCount = 10;
        [SerializeField] private string _fileFolderName = "logs";
        [SerializeField] private int _fileBufferSize = 8192;
        [SerializeField] private int _fileChannelCapacity = 1000;

        public LogLevel MinLogLevel => _minLogLevel;
        public int MaxFileCount => _maxFileCount;
        public string FileFolderName => _fileFolderName;
        public int FileBufferSize => _fileBufferSize;
        public int FileChannelCapacity => _fileChannelCapacity;

        private void OnEnable()
        {
            s_loaded = this;
        }

        public static LoggerSettings GetOrCreate()
        {
    #if UNITY_EDITOR
            if (EditorBuildSettings.TryGetConfigObject<LoggerSettings>(ConfigName, out var settings) && settings != null)
            {
                return settings;
            }
    #else
            if (s_loaded != null)
            {
                return s_loaded;
            }
    #endif
            var defaultSettings = CreateInstance<LoggerSettings>();
            defaultSettings.name = ConfigName;
            return defaultSettings;
        }

        public static string GetConfigName() => ConfigName;
    }
}
