using System.Runtime.CompilerServices;

namespace GroveGames.Logger;

public static class LoggerExtensions
{
    public static void LogDebug(this ILogger logger, ReadOnlySpan<char> tag, [InterpolatedStringHandlerArgument("logger")] DebugMessageInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Debug, tag, message.Written);
    }

    public static void LogInformation(this ILogger logger, ReadOnlySpan<char> tag, [InterpolatedStringHandlerArgument("logger")] InformationMessageInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Information, tag, message.Written);
    }

    public static void LogWarning(this ILogger logger, ReadOnlySpan<char> tag, [InterpolatedStringHandlerArgument("logger")] WarningMessageInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Warning, tag, message.Written);
    }

    public static void LogError(this ILogger logger, ReadOnlySpan<char> tag, [InterpolatedStringHandlerArgument("logger")] ErrorMessageInterpolatedStringHandler message)
    {
        logger.Log(LogLevel.Error, tag, message.Written);
    }
}
