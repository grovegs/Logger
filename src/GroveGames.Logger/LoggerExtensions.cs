namespace GroveGames.Logger;

public static class LoggerExtensions
{
    public static void LogDebug(this ILogger logger, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Debug, tag, message.Written);
    }

    public static void LogInformation(this ILogger logger, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Information, tag, message.Written);
    }

    public static void Warning(this ILogger logger, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Warning, tag, message.Written);
    }

    public static void Error(this ILogger logger, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Error, tag, message.Written);
    }
}
