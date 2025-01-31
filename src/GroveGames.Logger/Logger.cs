using System.Collections.Immutable;

namespace GroveGames.Logger;

public sealed class Logger : ILogger
{
    public static readonly Logger Shared = new();

    private readonly ImmutableArray<ILogProcessor> _processors;

    public Logger()
    {
        _processors = [];
    }

    public void Info(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        foreach (ILogProcessor processor in _processors)
        {
            processor.ProcessInfo(tag, message.Written);
        }
    }

    public void Warning(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        foreach (ILogProcessor processor in _processors)
        {
            processor.ProcessWarning(tag, message.Written);
        }
    }

    public void Error(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        foreach (ILogProcessor processor in _processors)
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
        foreach (ILogProcessor v in _processors)
        {
            if (v is not IDisposable disposable)
            {
                continue;
            }

            disposable.Dispose();
        }

        _processors.Clear();
    }
}
