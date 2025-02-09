namespace GroveGames.Logger;

public sealed class GodotLoggerFactory
{
    public static Logger CreateLogger(Action<ILoggerBuilder> configure)
    {
        var minLevel = GodotSettings.MinLogLevel.Value;
        var builder = new LoggerBuilder();
        builder.SetMinimumLevel(minLevel);
        configure(builder);
        return builder.Build();
    }
}
