namespace GroveGames.Logger;

public class FileWriter : IDisposable
{
    private const int WriteInterval = 2000;

    private readonly Queue<char> _messageQueue;

    private readonly Thread _writeThread;
    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _semaphore;

    private bool _isRunning;

    public FileWriter(StreamWriter streamWriter)
    {
        _semaphore = new SemaphoreSlim(1, 1);
        _messageQueue = new Queue<char>(1000);

        _writer = streamWriter;

        _writeThread = new Thread(Write) { Name = "LogWriteThread" };

        _isRunning = true;
        _writeThread.Start();
    }

    public void AddToQueue(ReadOnlySpan<char> message)
    {
        foreach (var ch in message)
        {
            _messageQueue.Enqueue(ch);
        }
        _messageQueue.Enqueue('\n');
    }

    private async void Write()
    {
        while (_isRunning)
        {
            await _semaphore.WaitAsync();
            while (_messageQueue.TryDequeue(out var result))
            {
                try
                {
                    await _writer.WriteAsync(result);
                }
                catch
                {
                    Dispose();
                }
            }

            await _writer.FlushAsync();
            _semaphore.Release();

            await Task.Delay(WriteInterval);
        }
    }

    public void Dispose()
    {
        _isRunning = false;
        _writeThread?.Join();
        _writer?.Close();
        _writer?.Dispose();
    }
}
