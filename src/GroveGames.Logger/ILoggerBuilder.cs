namespace GroveGames.Logger;

public interface ILoggerBuilder
{
    public ILoggerBuilder AddLogProcessor(ILogProcessor processor);
    public ILoggerBuilder SetMinimumLevel(LogLevel level);
    public ILoggerBuilder AddLogSource(System.Func<ILogProcessor[], ILogSource> sourceFactory);
}
