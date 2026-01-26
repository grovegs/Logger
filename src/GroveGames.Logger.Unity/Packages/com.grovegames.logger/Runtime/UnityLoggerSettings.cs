using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public sealed class UnityLoggerSettings : ScriptableObject
    {
        private const string ConfigName = "com.grovegames.logger.settings";

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

        public static UnityLoggerSettings GetOrCreate()
        {
            if (EditorBuildSettings.TryGetConfigObject(ConfigName, out UnityLoggerSettings settings))
            {
                return settings;
            }

            return CreateInstance<UnityLoggerSettings>();
        }

        internal static string GetConfigName() => ConfigName;
    }
}
