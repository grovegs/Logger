using System.Text;
using System.Threading.Channels;

namespace GroveGames.Logger;

public sealed class FileWriter : IFileWriter
{
    private readonly Stream _stream;
    private readonly int _writeInterval;
    private readonly byte[] _buffer;
    private readonly Channel<byte[]> _channel;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _writeTask;
    private readonly ThreadLocal<Encoder> _encoder;
    private bool _disposed;

    public FileWriter(Stream stream, int writeInterval, int bufferSize)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(writeInterval);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        _stream = stream;
        _writeInterval = writeInterval;
        _buffer = new byte[bufferSize];
        _channel = Channel.CreateBounded<byte[]>(new BoundedChannelOptions(128)
        {
            SingleReader = true,
            SingleWriter = false,
            FullMode = BoundedChannelFullMode.DropWrite
        });

        _encoder = new(Encoding.UTF8.GetEncoder);
        _cancellationTokenSource = new();
        _writeTask = ProcessEntriesAsync(_cancellationTokenSource.Token);
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        int maxBytes = entry.Length * 4 + 1;

        if (maxBytes > _buffer.Length)
        {
            throw new InvalidOperationException("Entry too large for buffer.");
        }

        var encoder = _encoder.Value!;
        int bytesWritten = encoder.GetBytes(entry, _buffer, flush: false);
        _buffer[bytesWritten] = 10;

        if (!_channel.Writer.TryWrite(_buffer[..(bytesWritten + 1)]))
        {
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
            _encoder.Dispose();
            _stream.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}