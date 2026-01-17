using System;
using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public sealed class UnityLogHandler : IDisposable
    {
        private readonly ILogProcessor _logProcessor;
        private readonly string _tag;
        private bool _disposed;

        public UnityLogHandler(ILogProcessor logProcessor, string tag = "Unity")
        {
            _logProcessor = logProcessor;
            _tag = tag;
            Application.logMessageReceived += OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (_disposed)
            {
                return;
            }

            var level = ConvertLogType(type);
            _logProcessor.ProcessLog(level, _tag.AsSpan(), condition.AsSpan());
        }

        private static LogLevel ConvertLogType(LogType type)
        {
            return type switch
            {
                LogType.Error => LogLevel.Error,
                LogType.Assert => LogLevel.Error,
                LogType.Exception => LogLevel.Error,
                LogType.Warning => LogLevel.Warning,
                LogType.Log => LogLevel.Information,
                _ => LogLevel.Debug
            };
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Application.logMessageReceived -= OnLogMessageReceived;
        }
    }
}
