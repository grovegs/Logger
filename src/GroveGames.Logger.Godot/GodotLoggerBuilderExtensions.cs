using Godot;

namespace GroveGames.Logger;

public static class GodotLoggerBuilderExtensions
{
    public static void AddGodotFileLogProcessor(this ILoggerBuilder builder)
    {
        var fileFolderName = GodotSettings.FileFolderName.Value;
        var maxFileCount = GodotSettings.MaxFileCount.Value;
        var fileWriteInterval = GodotSettings.FileWriteInterval.Value;
        var fileBufferSize = GodotSettings.FileCharacterQueueSize.Value;
        var godotFileFactory = new GodotLogFileFactory(fileFolderName, maxFileCount, fileBufferSize);
        var streamWriter = new StreamWriter(godotFileFactory.CreateFile(), fileWriteInterval);
        var fileLogFormatter = new FileLogFormatter();
        builder.AddLogProcessor(new FileLogProcessor(streamWriter, fileLogFormatter));
    }

    public static void AddGodotConsoleLogProcessor(this ILoggerBuilder builder)
    {
        var godotConsoleLogFormatter = new GodotConsoleLogFormatter();
        builder.AddLogProcessor(new GodotConsoleLogProcessor(godotConsoleLogFormatter));
    }
}
