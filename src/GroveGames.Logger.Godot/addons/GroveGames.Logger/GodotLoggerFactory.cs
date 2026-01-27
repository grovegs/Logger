using System;
using Godot;

namespace GroveGames.Logger;

public sealed class GodotLoggerFactory
{
    public static Logger CreateLogger(Action<ILoggerBuilder> configure)
    {
        return CreateLogger(GodotLoggerSettingsResource.GetOrCreate(), configure);
    }

    public static Logger CreateLogger(GodotLoggerSettingsResource settings, Action<ILoggerBuilder> configure)
    {
        if (settings == null)
        {
            GD.PushError("GodotLoggerSettingsResource cannot be null");
            settings = new();
        }

        LoggerBuilder builder = new();
        builder.SetMinimumLevel(settings.MinLogLevel);
        configure(builder);
        return builder.Build();
    }
}
