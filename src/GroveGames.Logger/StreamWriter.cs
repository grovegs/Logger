using System.Buffers;
using System.Text;
using System.Threading.Channels;

namespace GroveGames.Logger;

public sealed class StreamWriter : IStreamWriter
{
    private readonly Stream _stream;
    private readonly Channel<byte[]> _channel;
    private readonly Task _writeTask;
    private readonly byte[] _newLine;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly int _bufferSize;
    private volatile bool _disposed;

    public StreamWriter(Stream stream, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        _stream = stream;
        _bufferSize = bufferSize;
        _newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        _channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        _cancellationTokenSource = new CancellationTokenSource();
        _writeTask = ProcessEntriesAsync(_cancellationTokenSource.Token);
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        int maxBytes = Encoding.UTF8.GetMaxByteCount(entry.Length) + _newLine.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxBytes);

        try
        {
            int bytesWritten = Encoding.UTF8.GetBytes(entry, buffer);
            _newLine.CopyTo(buffer.AsSpan(bytesWritten));

            if (!_channel.Writer.TryWrite(buffer))
            {
                ArrayPool<byte>.Shared.Return(buffer);
                throw new InvalidOperationException("Failed to write to channel");
            }
        }
        catch
        {
            ArrayPool<byte>.Shared.Return(buffer);
            throw;
        }
    }

    private async Task ProcessEntriesAsync(CancellationToken cancellationToken)
    {
        byte[] batchBuffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
        int batchSize = 0;

        try
        {
            while (!cancellationToken.IsCancellationRequested && await _channel.Reader.WaitToReadAsync(cancellationToken))
            {
                while (_channel.Reader.TryRead(out var entry))
                {
                    if (batchSize + entry.Length > _bufferSize)
                    {
                        if (batchSize > 0)
                        {
                            await _stream.WriteAsync(batchBuffer.AsMemory(0, batchSize), cancellationToken);
                            batchSize = 0;
                        }

                        if (entry.Length > _bufferSize)
                        {
                            await _stream.WriteAsync(entry.AsMemory(), cancellationToken);
                            continue;
                        }
                    }

                    entry.CopyTo(batchBuffer.AsSpan(batchSize));
                    batchSize += entry.Length;
                }
            }

            if (batchSize > 0)
            {
                await _stream.WriteAsync(batchBuffer.AsMemory(0, batchSize), cancellationToken);
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(batchBuffer);
        }
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

        try
        {
            _writeTask.Wait();
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            _cancellationTokenSource.Dispose();
            _stream.Dispose();
        }
    }
}