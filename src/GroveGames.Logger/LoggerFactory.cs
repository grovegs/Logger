namespace GroveGames.Logger;

public sealed class LoggerFactory
{
    public static Logger CreateLogger(Action<ILoggerBuilder> configure)
    {
        var builder = new LoggerBuilder();
        configure(builder);
        return builder.Build();
    }

    public static Logger CreateLogger(ILoggerSettings settings, Action<ILoggerBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(configure);

        var builder = new LoggerBuilder();
        builder.SetMinimumLevel(settings.MinLogLevel);
        configure(builder);
        return builder.Build();
    }
}
