using System.Buffers;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Channels;

namespace GroveGames.Logger;

public sealed class StreamWriter : IStreamWriter
{
    private static readonly byte[] NewLine = [.. Encoding.UTF8.GetBytes(Environment.NewLine)];

    private readonly Stream _stream;
    private readonly int _bufferSize;
    private readonly Channel<ReadOnlyMemory<byte>> _channel;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _writeTask;
    private volatile bool _disposed;

    public StreamWriter(Stream stream, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        _stream = stream;
        _bufferSize = bufferSize;
        _channel = Channel.CreateBounded<ReadOnlyMemory<byte>>(
        new BoundedChannelOptions(1024)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.Wait
        });
        _cancellationTokenSource = new CancellationTokenSource();
        _writeTask = ProcessEntriesAsync(_cancellationTokenSource.Token);
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var maxBytes = Encoding.UTF8.GetMaxByteCount(entry.Length) + NewLine.Length;
        var buffer = ArrayPool<byte>.Shared.Rent(maxBytes);
        var bytesWritten = Encoding.UTF8.GetBytes(entry, buffer);
        NewLine.CopyTo(buffer.AsSpan(bytesWritten));
        var totalLength = bytesWritten + NewLine.Length;
        var memorySegment = buffer.AsMemory(0, totalLength);

        if (!_channel.Writer.TryWrite(memorySegment))
        {
            ArrayPool<byte>.Shared.Return(buffer);
            throw new InvalidOperationException("Failed to write to channel");
        }
    }

    private async Task ProcessEntriesAsync(CancellationToken cancellationToken)
    {
        var batchBuffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
        var batchSize = 0;

        while (!cancellationToken.IsCancellationRequested && await _channel.Reader.WaitToReadAsync(cancellationToken))
        {
            while (_channel.Reader.TryRead(out var memory))
            {
                var length = memory.Length;

                if (batchSize + length > _bufferSize)
                {
                    if (batchSize > 0)
                    {
                        await _stream.WriteAsync(batchBuffer.AsMemory(0, batchSize), cancellationToken);
                        batchSize = 0;
                    }

                    if (length > _bufferSize)
                    {
                        await _stream.WriteAsync(memory, cancellationToken);
                    }
                    else
                    {
                        memory.Span.CopyTo(batchBuffer.AsSpan(batchSize));
                        batchSize += length;
                    }
                }
                else
                {
                    memory.Span.CopyTo(batchBuffer.AsSpan(batchSize));
                    batchSize += length;
                }

                if (MemoryMarshal.TryGetArray(memory, out var segment) &&
                    segment.Array != null &&
                    segment.Array.Length > segment.Count)
                {
                    ArrayPool<byte>.Shared.Return(segment.Array);
                }
            }

            if (batchSize > 0)
            {
                await _stream.WriteAsync(batchBuffer.AsMemory(0, batchSize), cancellationToken);
                batchSize = 0;
            }
        }

        while (_channel.Reader.TryRead(out var memory))
        {
            if (MemoryMarshal.TryGetArray(memory, out var segment) &&
                segment.Array != null &&
                segment.Array.Length > segment.Count)
            {
                ArrayPool<byte>.Shared.Return(segment.Array);
            }
        }

        ArrayPool<byte>.Shared.Return(batchBuffer);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _channel.Writer.Complete();
        _cancellationTokenSource.Cancel();

        if (!_writeTask.IsCompleted)
        {
            _ = _writeTask.ContinueWith(_ => { },
                TaskContinuationOptions.OnlyOnFaulted);
            _writeTask.GetAwaiter().GetResult();
        }

        _cancellationTokenSource.Dispose();
        _stream.Dispose();
    }
}