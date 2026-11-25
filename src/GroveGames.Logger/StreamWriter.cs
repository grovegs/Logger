using System.Diagnostics;
using System.Text;

namespace GroveGames.Logger;

public sealed class StreamWriter : IStreamWriter
{
    private static readonly byte[] s_newLine = [.. Encoding.UTF8.GetBytes(Environment.NewLine)];
    private static readonly TimeSpan s_flushTimeout = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan s_disposeTimeout = TimeSpan.FromSeconds(5);
    private const int MaxBufferSize = 0x7FFFFFC7;

    private readonly Stream _stream;
    private readonly object _lock;
    private readonly Thread _thread;
    private readonly int _capacity;
    private int _activeEntries;
    private byte[] _active;
    private int _activeLength;
    private byte[]? _spare;
    private byte[]? _pending;
    private int _pendingLength;
    private long _submitted;
    private long _completed;
    private bool _failed;
    private bool _stopping;
    private bool _writerIdle;
    private int _disposed;

    public StreamWriter(Stream stream, int bufferSize, int channelCapacity)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channelCapacity);

        _stream = stream;
        _lock = new object();
        _capacity = channelCapacity;
        _activeEntries = 0;
        _active = new byte[bufferSize];
        _activeLength = 0;
        _spare = new byte[bufferSize];
        _pending = null;
        _pendingLength = 0;
        _submitted = 0;
        _completed = 0;
        _failed = false;
        _stopping = false;
        _writerIdle = false;
        _disposed = 0;
        _thread = new Thread(Run) { IsBackground = true, Name = "GroveGames.Logger" };
        _thread.Start();
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            return;
        }

        lock (_lock)
        {
            if (_failed || _stopping || (_pending != null && _activeEntries >= _capacity))
            {
                return;
            }

            if (_activeLength + ((long)entry.Length + 1) * 3 + s_newLine.Length > _active.Length)
            {
                var entryLength = (long)Encoding.UTF8.GetByteCount(entry) + s_newLine.Length;

                if (_activeLength + entryLength > _active.Length)
                {
                    TrySubmit();

                    if (_activeLength + entryLength > _active.Length && !TryGrow(_activeLength + entryLength))
                    {
                        return;
                    }
                }
            }

            var written = Encoding.UTF8.GetBytes(entry, _active.AsSpan(_activeLength));
            s_newLine.CopyTo(_active.AsSpan(_activeLength + written));
            _activeLength += written + s_newLine.Length;
            _activeEntries++;

            if (_activeLength >= _active.Length - (_active.Length >> 2))
            {
                TrySubmit();
            }
            else if (_writerIdle)
            {
                _writerIdle = false;
                Monitor.PulseAll(_lock);
            }
        }
    }

    public void Flush()
    {
        if (Volatile.Read(ref _disposed) != 0)
        {
            return;
        }

        var start = Stopwatch.GetTimestamp();

        lock (_lock)
        {
            while (!_failed && !TrySubmit())
            {
                if (!Wait(start))
                {
                    return;
                }
            }

            var target = _submitted;

            while (_completed < target && !_failed)
            {
                if (!Wait(start))
                {
                    return;
                }
            }
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        lock (_lock)
        {
            _stopping = true;
            Monitor.PulseAll(_lock);
        }

        _thread.Join(s_disposeTimeout);

        try
        {
            _stream.Dispose();
        }
        catch (Exception)
        {
        }
    }

    private bool TrySubmit()
    {
        if (_activeLength == 0)
        {
            return true;
        }

        if (_pending != null)
        {
            return false;
        }

        _pending = _active;
        _pendingLength = _activeLength;
        _active = _spare!;
        _spare = null;
        _activeLength = 0;
        _activeEntries = 0;
        _submitted++;
        Monitor.PulseAll(_lock);
        return true;
    }

    private bool TryGrow(long required)
    {
        if (required > MaxBufferSize)
        {
            return false;
        }

        long size = _active.Length;

        while (size < required)
        {
            size *= 2;
        }

        var bigger = new byte[Math.Min(size, MaxBufferSize)];
        Buffer.BlockCopy(_active, 0, bigger, 0, _activeLength);
        _active = bigger;
        return true;
    }

    private bool Wait(long start)
    {
        var elapsed = TimeSpan.FromSeconds((double)(Stopwatch.GetTimestamp() - start) / Stopwatch.Frequency);
        var remaining = s_flushTimeout - elapsed;

        if (remaining <= TimeSpan.Zero)
        {
            return false;
        }

        Monitor.Wait(_lock, remaining);
        return true;
    }

    private void Run()
    {
        try
        {
            while (true)
            {
                byte[] buffer;
                int length;

                lock (_lock)
                {
                    while (_pending == null)
                    {
                        if (_activeLength > 0)
                        {
                            TrySubmit();
                            break;
                        }

                        if (_stopping)
                        {
                            return;
                        }

                        _writerIdle = true;
                        Monitor.Wait(_lock);
                        _writerIdle = false;
                    }

                    buffer = _pending!;
                    length = _pendingLength;
                }

                _stream.Write(buffer, 0, length);
                _stream.Flush();

                lock (_lock)
                {
                    _spare = buffer;
                    _pending = null;
                    _completed++;
                    Monitor.PulseAll(_lock);
                }
            }
        }
        catch (Exception)
        {
            lock (_lock)
            {
                _failed = true;
                Monitor.PulseAll(_lock);
            }
        }
    }
}
