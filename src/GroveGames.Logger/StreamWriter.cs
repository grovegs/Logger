using System.Buffers;
using System.Text;
using System.Threading.Channels;

namespace GroveGames.Logger;

public sealed class StreamWriter : IStreamWriter
{
    private static readonly byte[] NewLine = [.. Encoding.UTF8.GetBytes(Environment.NewLine)];

    private readonly Stream _stream;
    private readonly int _bufferSize;
    private readonly Channel<ArraySegment<byte>> _channel;
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly Task _writeTask;
    private readonly Lock _flushLock = new();
    private volatile bool _disposed;

    public StreamWriter(Stream stream, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        _stream = stream;
        _bufferSize = bufferSize;
        _channel = Channel.CreateUnbounded<ArraySegment<byte>>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
            AllowSynchronousContinuations = false
        });
        _writeTask = Task.Run(() => ProcessEntriesAsync(_cancellationTokenSource.Token));
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var byteCount = Encoding.UTF8.GetByteCount(entry);
        var totalLength = byteCount + NewLine.Length;

        var buffer = ArrayPool<byte>.Shared.Rent(totalLength);

        var bytesWritten = Encoding.UTF8.GetBytes(entry, buffer);
        NewLine.CopyTo(buffer.AsSpan(bytesWritten));

        if (!_channel.Writer.TryWrite(new ArraySegment<byte>(buffer, 0, totalLength)))
        {
            ArrayPool<byte>.Shared.Return(buffer, clearArray: false);
            throw new InvalidOperationException("Failed to write to channel");
        }
    }

    private async Task ProcessEntriesAsync(CancellationToken cancellationToken)
    {
        var batchBuffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
        var batchPosition = 0;

        await foreach (var segment in _channel.Reader.ReadAllAsync(cancellationToken))
        {
            var entryBuffer = segment.Array!;
            var entryLength = segment.Count;

            if (batchPosition + entryLength > _bufferSize)
            {
                if (batchPosition > 0)
                {
                    await _stream.WriteAsync(batchBuffer.AsMemory(0, batchPosition), cancellationToken);
                    batchPosition = 0;
                }

                if (entryLength > _bufferSize)
                {
                    await _stream.WriteAsync(entryBuffer.AsMemory(0, entryLength), cancellationToken);
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

            ArrayPool<byte>.Shared.Return(entryBuffer, false);

            if (batchPosition >= _bufferSize * 3 / 4)
            {
                await _stream.WriteAsync(batchBuffer.AsMemory(0, batchPosition), cancellationToken);
                batchPosition = 0;
            }
        }

        if (batchPosition > 0)
        {
            await _stream.WriteAsync(batchBuffer.AsMemory(0, batchPosition), cancellationToken);
        }

        ArrayPool<byte>.Shared.Return(batchBuffer, false);
    }

    public void Flush()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        using (_flushLock.EnterScope())
        {
            _stream.Flush();
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _channel.Writer.TryComplete();
        _cancellationTokenSource.Cancel();
        _writeTask.Wait(TimeSpan.FromSeconds(5));
        _cancellationTokenSource.Dispose();
        _stream.Dispose();
    }
}