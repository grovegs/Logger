using System.Diagnostics;

namespace GroveGames.Logger;

public sealed class Logger : ILogger
{
    private readonly List<ILogProcessor> _logProcessors;

    public Logger()
    {
        _logProcessors = [];
    }

    private void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        foreach (var logProcessor in _logProcessors)
        {
            logProcessor.ProcessLog(level, tag, message);
        }
    }

    [Conditional("DEBUG")]
    private void ProcessDebugLog(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        ProcessLog(LogLevel.Debug, tag, message);
    }

    public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        if (level == LogLevel.Debug)
        {
            ProcessDebugLog(tag, message);
            return;
        }

        ProcessLog(level, tag, message);
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
