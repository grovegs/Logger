using System.Text;
using System.Threading.Channels;

namespace GroveGames.Logger;

public sealed class FileWriter : IFileWriter
{
    private readonly Stream _stream;
    private readonly int _writeInterval;
    private readonly Channel<byte[]> _channel;
    private readonly byte[] _newLine;
    private readonly CancellationTokenSource _cancellationTokenSource;
    private readonly Task _writeTask;
    private bool _disposed;

    public FileWriter(Stream stream, int writeInterval)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(writeInterval);

        _stream = stream;
        _channel = Channel.CreateUnbounded<byte[]>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });
        _newLine = Encoding.UTF8.GetBytes(Environment.NewLine);
        _cancellationTokenSource = new();
        _writeTask = ProcessEntriesAsync(_cancellationTokenSource.Token);
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        var bytes = new byte[Encoding.UTF8.GetByteCount(entry) + _newLine.Length];
        var bytesWritten = Encoding.UTF8.GetBytes(entry, bytes);
        _newLine.CopyTo(bytes, bytesWritten);

        if (!_channel.Writer.TryWrite(bytes))
        {
            throw new InvalidOperationException("Failed to write to channel");
        }
    }

    private async Task ProcessEntriesAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var entry = await _channel.Reader.ReadAsync(cancellationToken);
                await _stream.WriteAsync(entry, cancellationToken);
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
            _stream.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}