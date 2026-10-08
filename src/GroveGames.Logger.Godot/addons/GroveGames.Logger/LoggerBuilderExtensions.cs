using Godot;

namespace GroveGames.Logger.Godot;

public static class LoggerBuilderExtensions
{
    public static void AddFileLogProcessor(this ILoggerBuilder builder)
    {
        var settings = LoggerSettingsResource.GetOrCreate();
        var godotFileFactory = new GodotLogFileFactory(settings.FileFolderName, settings.MaxFileCount, settings.FileBufferSize);
        var streamWriter = new StreamWriter(godotFileFactory.CreateFile(), settings.FileBufferSize, settings.FileChannelCapacity);
        var fileLogFormatter = new FileLogFormatter();
        builder.AddLogProcessor(new FileLogProcessor(streamWriter, fileLogFormatter));
    }

    public static void AddConsoleLogProcessor(this ILoggerBuilder builder)
    {
        var godotConsoleLogFormatter = new ConsoleLogFormatter();
        builder.AddLogProcessor(new ConsoleLogProcessor(godotConsoleLogFormatter));
    }
}
