namespace GroveGames.Logger;

public sealed class Logger : ILogger
{
    private readonly List<ILogProcessor> _logProcessors;
    private bool _disposed;

    public Logger()
    {
        _logProcessors = [];
        _disposed = false;
    }

    public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        foreach (var logProcessor in _logProcessors)
        {
            logProcessor.ProcessLog(level, tag, message);
        }
    }

    public void AddLogProcessor(ILogProcessor logProcessor)
    {
        ArgumentNullException.ThrowIfNull(logProcessor);
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logProcessors.Add(logProcessor);
    }

    public void RemoveLogProcessor(ILogProcessor logProcessor)
    {
        ArgumentNullException.ThrowIfNull(logProcessor);
        ObjectDisposedException.ThrowIf(_disposed, this);
        _logProcessors.Remove(logProcessor);
    }

    public void Dispose()
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