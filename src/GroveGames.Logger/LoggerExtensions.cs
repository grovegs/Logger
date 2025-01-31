using System.Diagnostics;

namespace GroveGames.Logger;

public static class LoggerExtensions
{
    [Conditional("DEBUG")]
    public static void Debug(this ILogger logger, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Debug, tag, message);
    }

    public static void Information(this ILogger logger, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Information, tag, message);
    }

    public static void Warning(this ILogger logger, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Warning, tag, message);
    }

    public static void Error(this ILogger logger, ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Error, tag, message);
    }
}
