namespace GroveGames.Logger;

public sealed class Logger : ILogger
{
    public static readonly Logger Shared = new();

    private readonly List<ILogProcessor> _logProcessors;

    public Logger()
    {
        _logProcessors = [];
    }

    public void Log(LogLevel level, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        foreach (var logProcessor in _logProcessors)
        {
            logProcessor.Process(level, tag, message.Written);
        }
    }

    public void AddLogProcessor(ILogProcessor logProcessor)
    {
        _logProcessors.Add(logProcessor);
    }

    public void RemoveLogProcessor(ILogProcessor logProcessor)
    {
        _logProcessors.Remove(logProcessor);
    }

    public void Dispose()
    {
        foreach (var logProcessor in _logProcessors)
        {
            if (logProcessor is not IDisposable disposable)
            {
                continue;
            }

            disposable.Dispose();
        }

        _logProcessors.Clear();
    }
}
