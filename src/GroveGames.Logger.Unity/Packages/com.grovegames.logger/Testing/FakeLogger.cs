using System;

namespace GroveGames.Logger.Testing
{
    public sealed class FakeLogger : ILogger
    {
        private int _errorCount;
        private string _lastError = string.Empty;

        public LogLevel MinimumLevel => LogLevel.Debug;
        public int ErrorCount => _errorCount;
        public string LastError => _lastError;

        public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            if (level < LogLevel.Error)
            {
                return;
            }

            _errorCount++;
            _lastError = message.ToString();
        }
    }
}
