using System;
using System.Collections.Generic;

namespace GroveGames.Logger.Unity
{
    public sealed class LogHistory : ILogHistory
    {
        private readonly object _lock = new();
        private readonly LogEntry[] _entries;
        private int _start;
        private int _count;
        private int _version;

        public int Capacity => _entries.Length;

        public int Version
        {
            get
            {
                lock (_lock)
                {
                    return _version;
                }
            }
        }

        public LogHistory(int capacity)
        {
            _entries = new LogEntry[Math.Max(1, capacity)];
        }

        public void Add(LogEntry entry)
        {
            lock (_lock)
            {
                _entries[(_start + _count) % _entries.Length] = entry;

                if (_count < _entries.Length)
                {
                    _count++;
                }
                else
                {
                    _start = (_start + 1) % _entries.Length;
                }

                _version++;
            }
        }

        public void Clear()
        {
            lock (_lock)
            {
                Array.Clear(_entries, 0, _entries.Length);
                _start = 0;
                _count = 0;
                _version++;
            }
        }

        public void CopyTo(List<LogEntry> destination)
        {
            lock (_lock)
            {
                destination.Clear();

                for (var i = 0; i < _count; i++)
                {
                    destination.Add(_entries[(_start + i) % _entries.Length]);
                }
            }
        }
    }
}
