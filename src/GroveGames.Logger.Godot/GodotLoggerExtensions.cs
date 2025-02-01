namespace GroveGames.Logger;

public static class GodotLoggerExtensions
{
    public static void AddGodotFileLogProcessor(this ILogger logger)
    {
        var godotFileFactory = new GodotLogFileFactory();
        var fileWriter = new FileWriter(godotFileFactory.CreateFile());
        var fileLogFormatter = new FileLogFormatter();
        logger.AddLogProcessor(new FileLogProcessor(fileWriter, fileLogFormatter));
    }

    public static void AddGodotConsoleLogProcessor(this ILogger logger)
    {
        var godotConsoleLogFormatter = new GodotConsoleLogFormatter();
        logger.AddLogProcessor(new GodotConsoleLogProcessor(godotConsoleLogFormatter));
    }
}
