namespace GroveGames.Logger;

public interface ILoggerBuilder
{
    public ILoggerBuilder AddLogProcessor(ILogProcessor processor);
    public ILoggerBuilder SetMinimumLevel(LogLevel level);
    public ILoggerBuilder AddLogSource(Func<ILogProcessor[], ILogSource> sourceFactory);
}
