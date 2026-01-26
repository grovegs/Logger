using Godot;

namespace GroveGames.Logger;

public static class GodotLoggerBuilderExtensions
{
    public static void AddGodotFileLogProcessor(this ILoggerBuilder builder)
    {
        var settings = GodotLoggerSettingsResource.GetOrCreate();
        var godotFileFactory = new GodotLogFileFactory(settings.FileFolderName, settings.MaxFileCount, settings.FileBufferSize);
        var streamWriter = new StreamWriter(godotFileFactory.CreateFile(), settings.FileBufferSize, settings.FileChannelCapacity);
        var fileLogFormatter = new FileLogFormatter();
        builder.AddLogProcessor(new FileLogProcessor(streamWriter, fileLogFormatter));
    }

    public static void AddGodotConsoleLogProcessor(this ILoggerBuilder builder)
    {
        var godotConsoleLogFormatter = new GodotConsoleLogFormatter();
        builder.AddLogProcessor(new GodotConsoleLogProcessor(godotConsoleLogFormatter));
    }
}
