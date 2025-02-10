namespace GroveGames.Logger;

public sealed class LoggerBuilder : ILoggerBuilder
{
    private readonly List<ILogProcessor> _logProcessors;
    private LogLevel _minimumLevel;

    public LoggerBuilder()
    {
        _logProcessors = [];
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

    public Logger Build()
    {
        return new Logger([.. _logProcessors], _minimumLevel);
    }
}
