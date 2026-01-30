namespace GroveGames.Logger;

public sealed class LoggerBuilder : ILoggerBuilder
{
    private readonly List<ILogProcessor> _logProcessors;
    private readonly List<ILogHandler> _logHandlers;
    private LogLevel _minimumLevel;

    public LoggerBuilder()
    {
        _logProcessors = [];
        _logHandlers = [];
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

    public ILoggerBuilder AddLogHandler(ILogHandler handler)
    {
        ArgumentNullException.ThrowIfNull(handler);
        _logHandlers.Add(handler);
        return this;
    }

    public Logger Build()
    {
        ILogProcessor[] processors = [.. _logProcessors];
        ILogHandler[] handlers = [.. _logHandlers];

        if (handlers.Length > 0)
        {
            foreach (ILogHandler handler in handlers)
            {
                handler.Initialize(processors);
            }
        }

        return new Logger(processors, handlers, _minimumLevel);
    }
}
