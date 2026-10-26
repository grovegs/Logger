using System;

namespace GroveGames.Logger.Unity
{
    public readonly struct LogEntry
    {
        public LogLevel Level { get; }
        public string Tag { get; }
        public string Message { get; }
        public DateTime Time { get; }

        public LogEntry(LogLevel level, string tag, string message, DateTime time)
        {
            Level = level;
            Tag = tag;
            Message = message;
            Time = time;
        }
    }
}
