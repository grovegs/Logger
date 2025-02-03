using Godot;

namespace GroveGames.Logger;

public sealed class GodotLoggerFactory
{
    public static ILogger CreateLogger(Action<ILoggerBuilder> configure)
    {
        var logLevel = ProjectSettings.GetSetting(GodotProjectSettingsKey.LogLevel).As<LogLevel>();
        var builder = new LoggerBuilder();
        builder.SetMinimumLevel(logLevel);
        configure(builder);
        return builder.Build();
    }
}
