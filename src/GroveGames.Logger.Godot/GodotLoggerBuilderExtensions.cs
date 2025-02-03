using Godot;

namespace GroveGames.Logger;

public static class GodotLoggerBuilderExtensions
{
    public static void AddGodotFileLogProcessor(this ILoggerBuilder builder)
    {
        var fileFolderName = GodotSettings.FileFolderName.Value;
        var maxFileCount = GodotSettings.MaxFileCount.Value;
        var fileWriteInterval = GodotSettings.FileWriteInterval.Value;
        var fileCharacterQueueSize = GodotSettings.FileCharacterQueueSize.Value;
        var godotFileFactory = new GodotLogFileFactory(fileFolderName, maxFileCount);
        var fileWriter = new FileWriter(godotFileFactory.CreateFile(), fileWriteInterval, fileCharacterQueueSize);
        var fileLogFormatter = new FileLogFormatter();
        builder.AddLogProcessor(new FileLogProcessor(fileWriter, fileLogFormatter));
    }

    public static void AddGodotConsoleLogProcessor(this ILoggerBuilder builder)
    {
        var godotConsoleLogFormatter = new GodotConsoleLogFormatter();
        builder.AddLogProcessor(new GodotConsoleLogProcessor(godotConsoleLogFormatter));
    }
}
