using System;
using GroveGames.DependencyInjection;
#if GROVEGAMES_DEVELOPERTOOLS
using GroveGames.DeveloperTools.Unity;
#endif

namespace GroveGames.Logger.Unity
{
    public static class LoggerContainerBuilderExtensions
    {
        public static IContainerBuilder AddLogger(this IContainerBuilder builder, Action<ILoggerBuilder> configure)
        {
#if GROVEGAMES_DEVELOPERTOOLS
            var history = new LogHistory(LoggerSettings.GetOrCreate().HistoryCapacity);
            builder.AddSingleton<ILogHistory>(history);
            builder.AddDeveloperToolsPage<LogsPage>(LogsPage.PageOrder);
            return builder.AddSingleton<ILogger>(_ => UnityLoggerFactory.CreateLogger(logger =>
            {
                configure(logger);
                logger.AddLogHistory(history);
            }));
#else
            return builder.AddSingleton<ILogger>(_ => UnityLoggerFactory.CreateLogger(configure));
#endif
        }
    }
}
