namespace GroveGames.Logger;

public sealed class Logger : ILogger, IDisposable
{
    private readonly ILogProcessor[] _logProcessors;
    private readonly ILogHandler[] _logHandlers;
    private readonly LogLevel _minimumLevel;
    private bool _disposed;

    public LogLevel MinimumLevel => _minimumLevel;

    public Logger(ILogProcessor[] logProcessors, ILogHandler[] logHandlers, LogLevel minimumLevel)
    {
        ArgumentNullException.ThrowIfNull(logProcessors);
        ArgumentNullException.ThrowIfNull(logHandlers);

        if (logProcessors.Length == 0)
        {
            throw new ArgumentException("At least one log processor is required.", nameof(logProcessors));
        }

        foreach (ILogProcessor processor in logProcessors)
        {
            ArgumentNullException.ThrowIfNull(processor, nameof(logProcessors));
        }

        foreach (ILogHandler handler in logHandlers)
        {
            ArgumentNullException.ThrowIfNull(handler, nameof(logHandlers));
        }

        _logProcessors = logProcessors;
        _logHandlers = logHandlers;
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

        foreach (ILogProcessor logProcessor in _logProcessors)
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

        foreach (ILogHandler handler in _logHandlers)
        {
            handler.Dispose();
        }

        foreach (ILogProcessor logProcessor in _logProcessors)
        {
            if (logProcessor is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        _disposed = true;
    }
}
