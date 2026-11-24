using System;

namespace GroveGames.Logger.Testing
{
    public sealed class FakeLogger : ILogger
    {
        private readonly LogLevel _minimumLevel;

        private int _errorCount;
        private string _lastError = string.Empty;

        public LogLevel MinimumLevel => _minimumLevel;
        public int ErrorCount => _errorCount;
        public string LastError => _lastError;

        public FakeLogger() : this(LogLevel.Debug)
        {
        }

        public FakeLogger(LogLevel minimumLevel)
        {
            _minimumLevel = minimumLevel;
        }

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
