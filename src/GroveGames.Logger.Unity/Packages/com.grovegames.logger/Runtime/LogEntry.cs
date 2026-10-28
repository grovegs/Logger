using System;

namespace GroveGames.Logger.Unity
{
    public readonly struct LogEntry
    {
        private readonly LogLevel _level;
        private readonly string _tag;
        private readonly string _message;
        private readonly DateTime _time;

        public LogLevel Level => _level;
        public string Tag => _tag;
        public string Message => _message;
        public DateTime Time => _time;

        public LogEntry(LogLevel level, string tag, string message, DateTime time)
        {
            _level = level;
            _tag = tag;
            _message = message;
            _time = time;
        }
    }
}
