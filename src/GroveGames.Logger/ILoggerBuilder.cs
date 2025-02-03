namespace GroveGames.Logger;

public interface ILoggerBuilder
{
    ILoggerBuilder AddLogProcessor(ILogProcessor logProcessor);
    ILoggerBuilder SetMinimumLogLevel(LogLevel minimumLogLevel);
    ILogger Build();
}