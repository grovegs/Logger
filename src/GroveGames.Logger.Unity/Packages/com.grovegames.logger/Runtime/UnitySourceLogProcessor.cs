using System;

namespace GroveGames.Logger.Unity;

internal sealed class UnitySourceLogProcessor : ILogProcessor
{
    private readonly ILogProcessor[] _processors;

    public UnitySourceLogProcessor(ILogProcessor[] processors)
    {
        var count = 0;
        for (var i = 0; i < processors.Length; i++)
        {
            if (processors[i] is not UnityConsoleLogProcessor)
            {
                count++;
            }
        }

        _processors = new ILogProcessor[count];
        var index = 0;
        for (var i = 0; i < processors.Length; i++)
        {
            if (processors[i] is not UnityConsoleLogProcessor)
            {
                _processors[index++] = processors[i];
            }
        }
    }

    public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        for (var i = 0; i < _processors.Length; i++)
        {
            _processors[i].ProcessLog(level, tag, message);
        }
    }
}
