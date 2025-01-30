namespace GroveGames.Logger;

public sealed class Logger : ILogger
{
    private readonly List<ILogProcessor> _processors;

    public Logger()
    {
        _processors = [];
    }

    public void Info(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        for (var i = 0; i < _processors.Count; i++)
        {
            _processors[i].ProcessInfo(tag, message.Written);
        }
    }

    public void Warning(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        for (var i = 0; i < _processors.Count; i++)
        {
            _processors[i].ProcessWarning(tag, message.Written);
        }
    }

    public void Error(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        for (var i = 0; i < _processors.Count; i++)
        {
            _processors[i].ProcessError(tag, message.Written);
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
        for (var i = 0; i < _processors.Count; i++)
        {
            if (_processors[i] is not IDisposable disposable)
            {
                continue;
            }

            disposable.Dispose();
        }
    }
}
