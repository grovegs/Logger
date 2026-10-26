using System;

namespace GroveGames.Logger.Unity
{
    public sealed class LogHistoryProcessor : ILogProcessor
    {
        private readonly ILogHistory _history;

        public LogHistoryProcessor(ILogHistory history)
        {
            _history = history;
        }

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            _history.Add(new LogEntry(level, tag.ToString(), message.ToString(), DateTime.Now));
        }
    }
}
