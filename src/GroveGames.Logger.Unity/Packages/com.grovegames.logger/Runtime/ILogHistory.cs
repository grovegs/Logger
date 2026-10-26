using System.Collections.Generic;

namespace GroveGames.Logger.Unity
{
    public interface ILogHistory
    {
        int Capacity { get; }
        int Version { get; }

        void Add(LogEntry entry);
        void Clear();
        void CopyTo(List<LogEntry> destination);
    }
}
