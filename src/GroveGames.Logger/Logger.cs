namespace GroveGames.Logger;

public sealed class Logger : ILogger, IDisposable
{
    private readonly ILogProcessor[] _logProcessors;
    private readonly ILogSource[] _logSources;
    private readonly LogLevel _minimumLevel;
    private bool _disposed;

    public LogLevel MinimumLevel => _minimumLevel;

    public Logger(ILogProcessor[] logProcessors, ILogSource[] logSources, LogLevel minimumLevel)
    {
        ArgumentNullException.ThrowIfNull(logProcessors);
        ArgumentNullException.ThrowIfNull(logSources);

        if (logProcessors.Length == 0)
        {
            throw new ArgumentException("At least one log processor is required.", nameof(logProcessors));
        }

        foreach (ILogProcessor processor in logProcessors)
        {
            ArgumentNullException.ThrowIfNull(processor, nameof(logProcessors));
        }

        foreach (ILogSource source in logSources)
        {
            ArgumentNullException.ThrowIfNull(source, nameof(logSources));
        }

        _logProcessors = logProcessors;
        _logSources = logSources;
        _minimumLevel = minimumLevel;
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

        foreach (ILogSource source in _logSources)
        {
            source.Dispose();
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
