namespace GroveGames.Logger;

public interface ILoggerBuilder
{
    ILoggerBuilder AddLogProcessor(ILogProcessor processor);
    ILoggerBuilder SetMinimumLevel(LogLevel level);
}
