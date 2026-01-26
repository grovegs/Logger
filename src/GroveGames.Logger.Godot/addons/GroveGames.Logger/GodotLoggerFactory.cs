using System;

namespace GroveGames.Logger;

public sealed class GodotLoggerFactory
{
    public static Logger CreateLogger(Action<ILoggerBuilder> configure)
    {
        GodotSettings.CreateIfNotExist();
        var minLevel = GodotSettings.MinLogLevel.Value;
        var builder = new LoggerBuilder();
        builder.SetMinimumLevel(minLevel);
        configure(builder);
        return builder.Build();
    }

    public static Logger CreateLogger(GodotLoggerSettingsResource settings, Action<ILoggerBuilder> configure)
    {
        ArgumentNullException.ThrowIfNull(settings);
        var builder = new LoggerBuilder();
        builder.SetMinimumLevel(settings.MinLogLevel);
        configure(builder);
        return builder.Build();
    }
}
