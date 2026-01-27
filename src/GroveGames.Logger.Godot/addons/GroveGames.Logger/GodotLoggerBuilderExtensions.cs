using Godot;

namespace GroveGames.Logger;

public static class GodotLoggerBuilderExtensions
{
    public static void AddGodotFileLogProcessor(this ILoggerBuilder builder)
    {
        GodotLoggerSettingsResource settings = GodotLoggerSettingsResource.GetOrCreate();
        GodotLogFileFactory godotFileFactory = new(settings.FileFolderName, settings.MaxFileCount, settings.FileBufferSize);
        StreamWriter streamWriter = new(godotFileFactory.CreateFile(), settings.FileBufferSize, settings.FileChannelCapacity);
        FileLogFormatter fileLogFormatter = new();
        builder.AddLogProcessor(new FileLogProcessor(streamWriter, fileLogFormatter));
    }

    public static void AddGodotConsoleLogProcessor(this ILoggerBuilder builder)
    {
        GodotConsoleLogFormatter godotConsoleLogFormatter = new();
        builder.AddLogProcessor(new GodotConsoleLogProcessor(godotConsoleLogFormatter));
    }
}
