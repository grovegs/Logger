using System;
using Godot;

namespace GroveGames.Logger;

public sealed class GodotLoggerFactory
{
    public static Logger CreateLogger(Action<ILoggerBuilder> configure)
    {
        return CreateLogger(LoggerSettingsResource.GetOrCreate(), configure);
    }

    public static Logger CreateLogger(LoggerSettingsResource settings, Action<ILoggerBuilder> configure)
    {
        if (settings == null)
        {
            GD.PushError("LoggerSettingsResource cannot be null");
            settings = new();
        }

        var builder = new LoggerBuilder();
        builder.SetMinimumLevel(settings.MinLogLevel);
        configure(builder);
        return builder.Build();
    }
}
