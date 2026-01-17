using System;
using System.IO;
using UnityEngine;

namespace GroveGames.Logger.Unity
{
    [Serializable]
    public sealed class UnityLoggerSettings
    {
        private const string SettingsPath = "ProjectSettings/GroveGamesLoggerSettings.json";

        [SerializeField]
        private LogLevel _minLogLevel = LogLevel.Information;

        [SerializeField]
        private int _maxFileCount = 10;

        [SerializeField]
        private string _fileFolderName = "logs";

        [SerializeField]
        private int _fileBufferSize = 8192;

        [SerializeField]
        private int _fileChannelCapacity = 1000;

        public LogLevel MinLogLevel
        {
            get => _minLogLevel;
            set => _minLogLevel = value;
        }

        public int MaxFileCount
        {
            get => _maxFileCount;
            set => _maxFileCount = value;
        }

        public string FileFolderName
        {
            get => _fileFolderName;
            set => _fileFolderName = value;
        }

        public int FileBufferSize
        {
            get => _fileBufferSize;
            set => _fileBufferSize = value;
        }

        public int FileChannelCapacity
        {
            get => _fileChannelCapacity;
            set => _fileChannelCapacity = value;
        }

        public static string GetSettingsPath() => SettingsPath;

        public static UnityLoggerSettings Load()
        {
            if (!File.Exists(SettingsPath))
            {
                return new UnityLoggerSettings();
            }

            var json = File.ReadAllText(SettingsPath);
            return JsonUtility.FromJson<UnityLoggerSettings>(json) ?? new UnityLoggerSettings();
        }

        public void Save()
        {
            var json = JsonUtility.ToJson(this, true);
            File.WriteAllText(SettingsPath, json);
        }
    }
}
