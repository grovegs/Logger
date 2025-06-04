namespace GroveGames.Logger;

public sealed class LoggerFactory
{
    public static Logger CreateLogger(Action<ILoggerBuilder> configure)
    {
        var builder = new LoggerBuilder();
        configure(builder);
        return builder.Build();
    }
}
