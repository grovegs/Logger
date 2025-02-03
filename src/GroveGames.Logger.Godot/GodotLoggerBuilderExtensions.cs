using Godot;

namespace GroveGames.Logger;

public static class GodotLoggerBuilderExtensions
{
    public static void AddGodotFileLogProcessor(this ILoggerBuilder builder)
    {
        var fileFolderName = ProjectSettings.GetSetting(GodotProjectSettingsKey.FileFolderName).AsString();
        var maxFileCount = ProjectSettings.GetSetting(GodotProjectSettingsKey.MaxFileCount).AsInt32();
        var fileWriteInterval = ProjectSettings.GetSetting(GodotProjectSettingsKey.FileWriteInterval).AsInt32();
        var fileCharacterQueueSize = ProjectSettings.GetSetting(GodotProjectSettingsKey.FileCharacterQueueSize).AsInt32();
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
