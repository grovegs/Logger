using Godot;

namespace GroveGames.Logger.Godot;

public static class LoggerBuilderExtensions
{
    public static void AddFileLogProcessor(this ILoggerBuilder builder)
    {
        AddFileLogProcessor(builder, LoggerSettingsResource.GetOrCreate());
    }

    public static void AddFileLogProcessor(this ILoggerBuilder builder, ILoggerSettings settings)
    {
        if (string.IsNullOrEmpty(settings.FileFolderName) || settings.MaxFileCount <= 0 || settings.FileBufferSize <= 0 || settings.FileChannelCapacity <= 0)
        {
            GD.PushError($"File logging is off: the logger settings need a folder name and a positive file count, buffer size and channel capacity, but they are '{settings.FileFolderName}', {settings.MaxFileCount}, {settings.FileBufferSize} and {settings.FileChannelCapacity}.");
            return;
        }

        System.IO.Stream? stream = null;

        try
        {
            var godotFileFactory = new GodotLogFileFactory(settings.FileFolderName, settings.MaxFileCount, settings.FileBufferSize);
            stream = godotFileFactory.CreateFile();
            var streamWriter = new StreamWriter(stream, settings.FileBufferSize, settings.FileChannelCapacity);
            builder.AddLogProcessor(new FileLogProcessor(streamWriter, new FileLogFormatter()));
        }
        catch (System.Exception exception)
        {
            stream?.Dispose();
            GD.PushError($"File logging is off: the log file could not be created. {exception}");
        }
    }

    public static void AddConsoleLogProcessor(this ILoggerBuilder builder)
    {
        var godotConsoleLogFormatter = new ConsoleLogFormatter();
        builder.AddLogProcessor(new ConsoleLogProcessor(godotConsoleLogFormatter));
    }
}
