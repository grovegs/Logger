using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public sealed class LoggerSettings : ScriptableObject
    {
        public const string ConfigName = "com.grovegames.logger.settings";

        private static LoggerSettings? s_instance;

        [SerializeField] private LogLevel _minLogLevel = LogLevel.Information;
        [SerializeField] private int _maxFileCount = 10;
        [SerializeField] private string _fileFolderName = "logs";
        [SerializeField] private int _fileBufferSize = 8192;
        [SerializeField] private int _fileChannelCapacity = 1000;
        [SerializeField] private int _historyCapacity = 1000;

        public LogLevel MinLogLevel => _minLogLevel;
        public int MaxFileCount => _maxFileCount;
        public string FileFolderName => _fileFolderName;
        public int FileBufferSize => _fileBufferSize;
        public int FileChannelCapacity => _fileChannelCapacity;
        public int HistoryCapacity => _historyCapacity;

        public static LoggerSettings GetOrCreate()
        {
#if UNITY_EDITOR
            if (UnityEditor.EditorBuildSettings.TryGetConfigObject(ConfigName, out LoggerSettings settings) && settings != null)
            {
                return settings;
            }
#else
            if (s_instance == null)
            {
                var preloaded = Resources.FindObjectsOfTypeAll<LoggerSettings>();
                s_instance = preloaded.Length > 0 ? preloaded[0] : null;
            }
#endif
            if (s_instance == null)
            {
                s_instance = CreateInstance<LoggerSettings>();
                s_instance.hideFlags = HideFlags.DontSave;
            }

            return s_instance;
        }
    }
}
