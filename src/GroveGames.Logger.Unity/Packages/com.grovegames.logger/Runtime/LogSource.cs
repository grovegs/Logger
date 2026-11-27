using System;
using System.Threading;
using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public sealed class LogSource : ILogSource
    {
        private readonly SourceLogProcessor _logProcessor;
        private readonly string _tag;
        private volatile int _disposed;

        public LogSource(ILogProcessor[] processors, string tag = "Unity") : this(processors, tag, LogLevel.Debug)
        {
        }

        public LogSource(ILogProcessor[] processors, string tag, LogLevel minimumLevel)
        {
            _logProcessor = new SourceLogProcessor(processors, minimumLevel);
            _tag = tag;
            Application.logMessageReceivedThreaded += OnLogMessageReceived;
        }

        private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
        {
            if (_disposed == 1 || ConsoleLogProcessor.IsWriting)
            {
                return;
            }

            var level = ConvertLogType(type);

            if (!_logProcessor.IsEnabled(level))
            {
                return;
            }

            if (level == LogLevel.Error && !string.IsNullOrEmpty(stackTrace))
            {
                _logProcessor.ProcessLog(level, _tag.AsSpan(), $"{condition}\n{stackTrace}".AsSpan());
                return;
            }

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
            if (Interlocked.Exchange(ref _disposed, 1) == 1)
            {
                return;
            }

            Application.logMessageReceivedThreaded -= OnLogMessageReceived;
        }
    }
}
