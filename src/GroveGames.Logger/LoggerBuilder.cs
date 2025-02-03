namespace GroveGames.Logger;

public sealed class LoggerBuilder : ILoggerBuilder
{
    private readonly List<ILogProcessor> _logProcessors;
    private LogLevel _minimumLogLevel;

    public LoggerBuilder()
    {
        _logProcessors = [];
        _minimumLogLevel = LogLevel.Information;
    }

    public ILoggerBuilder AddLogProcessor(ILogProcessor logProcessor)
    {
        _logProcessors.Add(logProcessor);
        return this;
    }

    public ILoggerBuilder SetMinimumLogLevel(LogLevel minimumLogLevel)
    {
        _minimumLogLevel = minimumLogLevel;
        return this;
    }

    public ILogger Build()
    {
        return new Logger([.. _logProcessors], _minimumLogLevel);
    }
}
