using System.Buffers;
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
        _writeInterval = writeInterval;
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

        int byteCount = Encoding.UTF8.GetByteCount(entry);
        int totalBytes = byteCount + _newLine.Length;
        byte[] buffer = ArrayPool<byte>.Shared.Rent(totalBytes);

        try
        {
            int bytesWritten = Encoding.UTF8.GetBytes(entry, buffer);
            _newLine.CopyTo(buffer.AsSpan(bytesWritten));

            if (!_channel.Writer.TryWrite(buffer))
            {
                throw new InvalidOperationException("Failed to write to channel");
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
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