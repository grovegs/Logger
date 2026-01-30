namespace GroveGames.Logger;

public sealed class LoggerBuilder : ILoggerBuilder
{
    private readonly List<ILogProcessor> _logProcessors;
    private readonly List<System.Func<ILogProcessor[], ILogSource>> _logSourceFactories;
    private LogLevel _minimumLevel;

    public LoggerBuilder()
    {
        _logProcessors = [];
        _logSourceFactories = [];
    }

    public ILoggerBuilder AddLogProcessor(ILogProcessor processor)
    {
        _logProcessors.Add(processor);
        return this;
    }

    public ILoggerBuilder SetMinimumLevel(LogLevel level)
    {
        _minimumLevel = level;
        return this;
    }

    public ILoggerBuilder AddLogSource(System.Func<ILogProcessor[], ILogSource> handlerFactory)
    {
        ArgumentNullException.ThrowIfNull(handlerFactory);
        _logSourceFactories.Add(handlerFactory);
        return this;
    }

    public Logger Build()
    {
        ILogProcessor[] processors = [.. _logProcessors];

        ILogSource[] handlers = new ILogSource[_logSourceFactories.Count];
        for (int i = 0; i < _logSourceFactories.Count; i++)
        {
            handlers[i] = _logSourceFactories[i](processors);
        }

        return new Logger(processors, handlers, _minimumLevel);
    }
}
