using System;
using Godot;

namespace GroveGames.Logger.Godot;

public sealed class GodotLoggerFactory
{
    public static Logger CreateLogger(Action<ILoggerBuilder> configure)
    {
        return CreateLogger(LoggerSettingsResource.GetOrCreate(), configure);
    }

    public static Logger CreateLogger(ILoggerSettings settings, Action<ILoggerBuilder> configure)
    {
        if (settings == null)
        {
            GD.PushError("Settings cannot be null");
            settings = new LoggerSettingsResource();
        }

        return LoggerFactory.CreateLogger(settings, configure);
    }
}
