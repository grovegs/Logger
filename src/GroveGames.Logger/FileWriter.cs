using System.Collections.Concurrent;

namespace Berserk.Scripts.Infra.Logging
{
    public class FileWriter : IDisposable
    {
        private const int WriteInterval = 2000;

        private readonly ConcurrentQueue<char> _messageQueue;

        private Thread _writeThread;
        private StreamWriter _writer;

        private bool _isRunning;

        public FileWriter(StreamWriter streamWriter)
        {
            _messageQueue = new ConcurrentQueue<char>());

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
                while (_messageQueue.TryDequeue(out var result))
                {
                    try
                    {
                        await _writer.WriteAsync(result);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine($"Error occured while buffering logs with exception: {e}");
                        Dispose();
                    }

                }

                await _writer.FlushAsync();

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

}
