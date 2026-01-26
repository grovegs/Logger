using System;

namespace GroveGames.Logger;

public sealed class GodotLoggerFactory
{
    public static Logger CreateLogger(Action<ILoggerBuilder> configure)
    {
        return CreateLogger(GodotLoggerSettingsResource.GetOrCreate(), configure);
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
