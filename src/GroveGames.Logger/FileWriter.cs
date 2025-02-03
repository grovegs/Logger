using System.Buffers;

namespace GroveGames.Logger;

public sealed class FileWriter : IFileWriter, IDisposable
{
    private readonly StreamWriter _writer;
    private readonly int _writeInterval;
    private readonly Queue<char> _characterQueue;
    private readonly object _queueLock = new();
    private readonly CancellationTokenSource _cancellationTokenSource;
    private bool _disposed;
    private readonly Task _writeTask;

    public FileWriter(StreamWriter writer, int writeInterval, int characterQueueSize)
    {
        ArgumentNullException.ThrowIfNull(writer);
        _writer = writer;
        _writeInterval = writeInterval;
        _characterQueue = new Queue<char>(characterQueueSize);
        _queueLock = new();
        _cancellationTokenSource = new();
        _disposed = false;
        _writeTask = Task.Run(() => StartWriteLoop(_cancellationTokenSource.Token));
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        if (_cancellationTokenSource.IsCancellationRequested)
        {
            return;
        }

        lock (_queueLock)
        {
            foreach (var character in entry)
            {
                _characterQueue.Enqueue(character);
            }

            foreach (var character in Environment.NewLine)
            {
                _characterQueue.Enqueue(character);
            }
        }
    }

    private void StartWriteLoop(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                Task.Delay(_writeInterval, cancellationToken).Wait(cancellationToken);
                WriteEntries();
            }
        }
        catch (OperationCanceledException)
        {
        }
    }

    private void WriteEntries()
    {
        char[] buffer;
        int count;

        lock (_queueLock)
        {
            if (_characterQueue.Count == 0)
            {
                return;
            }

            count = _characterQueue.Count;
            buffer = ArrayPool<char>.Shared.Rent(count);

            for (int i = 0; i < count; i++)
            {
                buffer[i] = _characterQueue.Dequeue();
            }
        }

        try
        {
            _writer.Write(buffer, 0, count);
            _writer.Flush();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        _cancellationTokenSource.Cancel();

        try
        {
            _writeTask.Wait();
        }
        catch (AggregateException ex) when (ex.InnerException is OperationCanceledException)
        {
        }
        finally
        {
            _cancellationTokenSource.Dispose();
            _writer.Dispose();
        }

        GC.SuppressFinalize(this);
    }
}