namespace GroveGames.Logger;

public sealed class Logger : ILogger, IDisposable
{
    private readonly ILogProcessor[] _logProcessors;
    private readonly LogLevel _minimumLevel;
    private bool _disposed;

    public LogLevel MinimumLevel => _minimumLevel;

    public Logger(ILogProcessor[] logProcessors, LogLevel minimumLevel)
    {
        _logProcessors = logProcessors;
        _minimumLevel = minimumLevel;
        _disposed = false;
    }

    public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (level < _minimumLevel)
        {
            return;
        }

        foreach (var logProcessor in _logProcessors)
        {
            logProcessor.ProcessLog(level, tag, message);
        }
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
