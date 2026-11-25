using System.Buffers;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace GroveGames.Logger;

public sealed class StreamWriter : IStreamWriter
{
    private static readonly byte[] s_newLine = [.. Encoding.UTF8.GetBytes(Environment.NewLine)];
    private static readonly TimeSpan s_flushTimeout = TimeSpan.FromSeconds(1);

    private readonly Stream _stream;
    private readonly int _bufferSize;
    private readonly Channel<ArraySegment<byte>> _channel;
    private readonly object _progressLock;
    private readonly Task _writeTask;
    private long _accepted;
    private long _written;
    private volatile bool _failed;
    private volatile int _disposed;

    public StreamWriter(Stream stream, int bufferSize, int channelCapacity)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(channelCapacity);

        _stream = stream;
        _bufferSize = bufferSize;
        _channel = Channel.CreateBounded<ArraySegment<byte>>(new BoundedChannelOptions(channelCapacity)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
        _progressLock = new object();
        _accepted = 0;
        _written = 0;
        _failed = false;
        _disposed = 0;
        _writeTask = Task.Run(ProcessEntriesAsync);
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        if (_disposed != 0 || _failed)
        {
            return;
        }

        var byteCount = Encoding.UTF8.GetByteCount(entry);
        var totalLength = byteCount + s_newLine.Length;
        var buffer = ArrayPool<byte>.Shared.Rent(totalLength);
        var bytesWritten = Encoding.UTF8.GetBytes(entry, buffer);
        s_newLine.CopyTo(buffer.AsSpan(bytesWritten));

        lock (_progressLock)
        {
            if (_channel.Writer.TryWrite(new ArraySegment<byte>(buffer, 0, totalLength)))
            {
                _accepted++;
                return;
            }
        }

        ArrayPool<byte>.Shared.Return(buffer, false);
    }

    public void Flush()
    {
        if (_disposed != 0)
        {
            return;
        }

        var start = Stopwatch.GetTimestamp();

        lock (_progressLock)
        {
            var target = _accepted;

            while (_written < target && !_failed)
            {
                var remaining = s_flushTimeout - TimeSpan.FromSeconds((double)(Stopwatch.GetTimestamp() - start) / Stopwatch.Frequency);

                if (remaining <= TimeSpan.Zero)
                {
                    return;
                }

                Monitor.Wait(_progressLock, remaining);
            }
        }
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        _channel.Writer.TryComplete();

        try
        {
            _writeTask.Wait();
        }
        catch (Exception)
        {
        }

        try
        {
            _stream.Dispose();
        }
        catch (Exception)
        {
        }
    }

    private async Task ProcessEntriesAsync()
    {
        var batchBuffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
        var batchPosition = 0;
        var processed = 0L;
        var reader = _channel.Reader;

        try
        {
            while (await reader.WaitToReadAsync().ConfigureAwait(false))
            {
                while (reader.TryRead(out var segment))
                {
                    var entryBuffer = segment.Array!;
                    var entryLength = segment.Count;

                    try
                    {
                        if (batchPosition + entryLength > _bufferSize)
                        {
                            if (batchPosition > 0)
                            {
                                await _stream.WriteAsync(batchBuffer.AsMemory(0, batchPosition)).ConfigureAwait(false);
                                batchPosition = 0;
                            }

                            if (entryLength > _bufferSize)
                            {
                                await _stream.WriteAsync(entryBuffer.AsMemory(0, entryLength)).ConfigureAwait(false);
                            }
                            else
                            {
                                Buffer.BlockCopy(entryBuffer, 0, batchBuffer, 0, entryLength);
                                batchPosition = entryLength;
                            }
                        }
                        else
                        {
                            Buffer.BlockCopy(entryBuffer, 0, batchBuffer, batchPosition, entryLength);
                            batchPosition += entryLength;
                        }
                    }
                    finally
                    {
                        ArrayPool<byte>.Shared.Return(entryBuffer, false);
                    }

                    processed++;

                    if (batchPosition >= _bufferSize * 3 / 4)
                    {
                        await WriteBatchAsync(batchBuffer, batchPosition, processed).ConfigureAwait(false);
                        batchPosition = 0;
                    }
                }

                await WriteBatchAsync(batchBuffer, batchPosition, processed).ConfigureAwait(false);
                batchPosition = 0;
            }
        }
        catch (Exception)
        {
            Fail();
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(batchBuffer, false);
        }
    }

    private async Task WriteBatchAsync(byte[] batchBuffer, int batchPosition, long processed)
    {
        if (batchPosition == 0 && processed == _written)
        {
            return;
        }

        if (batchPosition > 0)
        {
            await _stream.WriteAsync(batchBuffer.AsMemory(0, batchPosition)).ConfigureAwait(false);
        }

        await _stream.FlushAsync().ConfigureAwait(false);

        lock (_progressLock)
        {
            _written = processed;
            Monitor.PulseAll(_progressLock);
        }
    }

    private void Fail()
    {
        lock (_progressLock)
        {
            _failed = true;
            Monitor.PulseAll(_progressLock);
        }

        while (_channel.Reader.TryRead(out var segment))
        {
            ArrayPool<byte>.Shared.Return(segment.Array!, false);
        }
    }
}
