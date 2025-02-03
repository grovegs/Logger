namespace GroveGames.Logger;

public class FileWriter : IFileWriter
{
    private readonly StreamWriter _writer;
    private readonly int _writeInterval;
    private readonly SemaphoreSlim _semaphore;
    private readonly Queue<char> _characterQueue;
    private readonly Thread _writeThread;
    private bool _isRunning;

    public FileWriter(StreamWriter streamWriter, int writeInterval, int characterQueueSize)
    {
        _writer = streamWriter;
        _writeInterval = writeInterval;
        _semaphore = new SemaphoreSlim(1, 1);
        _characterQueue = new Queue<char>(characterQueueSize);
        _writeThread = new Thread(Write) { Name = "LogFileWriteThread" };
        _isRunning = true;
        _writeThread.Start();
    }

    public void AddEntry(ReadOnlySpan<char> entry)
    {
        foreach (var character in entry)
        {
            _characterQueue.Enqueue(character);
        }

        _characterQueue.Enqueue('\n');
    }

    private async void Write()
    {
        while (_isRunning)
        {
            await _semaphore.WaitAsync();

            while (_characterQueue.TryDequeue(out var result))
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

            await Task.Delay(_writeInterval);
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
