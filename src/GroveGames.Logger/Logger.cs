namespace GroveGames.Logger;

public sealed class Logger : ILogger, IDisposable
{
    private readonly ILogProcessor[] _logProcessors;
    private readonly LogLevel _minimumLogLevel;
    private bool _disposed;

    public LogLevel MinimumLogLevel => _minimumLogLevel;

    public Logger(ILogProcessor[] logProcessors, LogLevel minimumLogLevel)
    {
        _logProcessors = logProcessors;
        _minimumLogLevel = minimumLogLevel;
        _disposed = false;
    }

    public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);

        if (level < _minimumLogLevel)
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
