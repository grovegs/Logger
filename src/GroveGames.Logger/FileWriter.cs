using System.Buffers;
using System.Text;
using System.Threading.Channels;

namespace GroveGames.Logger;

public sealed class FileWriter : IFileWriter
{
    private readonly Stream _stream;
    private readonly int _writeInterval;
    private readonly Channel<byte[]> _channel;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _writeTask;
    private readonly Encoder _encoder;
    private bool _disposed;

    public FileWriter(Stream stream, int writeInterval, int bufferSize = 4096)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(writeInterval);

        _stream = stream;
        _writeInterval = writeInterval;
        _channel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(128)
        {
            SingleReader = true,
            SingleWriter = false
        });

        _encoder = Encoding.UTF8.GetEncoder();
        _cancellationTokenSource = new();
        _writeTask = ProcessEntriesAsync(_cancellationTokenSource.Token);
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        int maxBytes = entry.Length * 4 + 1;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(maxBytes);

        try
        {
            int bytesWritten = _encoder.GetBytes(entry, buffer, flush: false);
            buffer[bytesWritten] = 10; // Newline

            if (!_channel.Writer.TryWrite(buffer[..(bytesWritten + 1)]))
            {
                ArrayPool<byte>.Shared.Return(buffer);
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
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                while (_channel.Reader.TryRead(out var entry))
                {
                    await _stream.WriteAsync(entry, cancellationToken);
                    ArrayPool<byte>.Shared.Return(entry);
                }

                await _stream.FlushAsync(cancellationToken);
                await Task.Delay(_writeInterval, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            while (_channel.Reader.TryRead(out var entry))
            {
                await _stream.WriteAsync(entry, cancellationToken);
                ArrayPool<byte>.Shared.Return(entry);
            }
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

        GC.SuppressFinalize(this);
    }
}