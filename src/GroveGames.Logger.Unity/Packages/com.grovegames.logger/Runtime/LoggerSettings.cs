using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public sealed class LoggerSettings : ScriptableObject
    {
        public const string ResourcePath = "GroveGames/LoggerSettings";

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

        public static LoggerSettings GetOrCreate()
        {
            var settings = Resources.Load<LoggerSettings>(ResourcePath);
            return settings != null ? settings : CreateInstance<LoggerSettings>();
        }
    }
}
