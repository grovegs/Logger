namespace GroveGames.Logger;

public class FileWriter : IFileWriter
{
    private const int WriteInterval = 1000;
    private const int InitialMessageCapacity = 256;

    private readonly Queue<char> _messageQueue;
    private readonly Thread _writeThread;
    private readonly StreamWriter _writer;
    private readonly SemaphoreSlim _semaphore;

    private bool _isRunning;

    public FileWriter(StreamWriter streamWriter)
    {
        _semaphore = new SemaphoreSlim(1, 1);
        _messageQueue = new Queue<char>(InitialMessageCapacity);
        _writer = streamWriter;
        _writeThread = new Thread(Write) { Name = "LogWriteThread" };
        _isRunning = true;
        _writeThread.Start();
    }

    public void AddToQueue(ReadOnlySpan<char> message)
    {
        foreach (var character in message)
        {
            _messageQueue.Enqueue(character);
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
