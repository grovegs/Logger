namespace GroveGames.Logger;

public sealed class Log : ILog
{
    public static readonly Log Shared = new();

    private readonly List<ILogProcessor> _processors;

    public Log()
    {
        _processors = [];
    }

#if DEBUG
    public void Debug(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        foreach (var processor in _processors)
        {
            processor.ProcessDebug(tag, message.Written);
        }
    }
#else
    public void Debug(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
    }
#endif

    public void Information(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        foreach (var processor in _processors)
        {
            processor.ProcessInformation(tag, message.Written);
        }
    }

    public void Warning(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        foreach (var processor in _processors)
        {
            processor.ProcessWarning(tag, message.Written);
        }
    }

    public void Error(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        foreach (var processor in _processors)
        {
            processor.ProcessError(tag, message.Written);
        }
    }

    public void AddProcessor(ILogProcessor processor)
    {
        _processors.Add(processor);
    }

    public void RemoveProcessor(ILogProcessor processor)
    {
        _processors.Remove(processor);
    }

    public void Dispose()
    {
        foreach (var processor in _processors)
        {
            if (processor is not IDisposable disposable)
            {
                continue;
            }

            disposable.Dispose();
        }

        _processors.Clear();
    }
}
