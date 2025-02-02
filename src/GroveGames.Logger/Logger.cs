namespace GroveGames.Logger;

public sealed class Logger : ILogger
{
    public static readonly Logger Shared = new();

    private readonly List<ILogProcessor> _logProcessors = [];
    private readonly object _lock = new();
    private bool _disposed;

    public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            foreach (var logProcessor in _logProcessors)
            {
                logProcessor.ProcessLog(level, tag, message);
            }
        }
    }

    public void AddLogProcessor(ILogProcessor logProcessor)
    {
        ArgumentNullException.ThrowIfNull(logProcessor);
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            _logProcessors.Add(logProcessor);
        }
    }

    public void RemoveLogProcessor(ILogProcessor logProcessor)
    {
        ArgumentNullException.ThrowIfNull(logProcessor);
        ObjectDisposedException.ThrowIf(_disposed, this);

        lock (_lock)
        {
            _logProcessors.Remove(logProcessor);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        lock (_lock)
        {
            if (_disposed)
            {
                return;
            }

            foreach (var logProcessor in _logProcessors)
            {
                if (logProcessor is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _disposed = true;
        }
    }
}