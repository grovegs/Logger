using System;
using GroveGames.DependencyInjection;
#if GROVEGAMES_DEVELOPERTOOLS
using GroveGames.DeveloperTools.Unity;
using GroveGames.Logger.Unity.DeveloperTools;
#endif

namespace GroveGames.Logger.Unity
{
    public static class LoggerContainerBuilderExtensions
    {
        public static IContainerBuilder AddLogger(this IContainerBuilder builder)
        {
            return builder.AddLogger(UnityLoggerSettings.GetOrCreate().MinLogLevel, true);
        }

        public static IContainerBuilder AddLogger(this IContainerBuilder builder, LogLevel minimumLevel, bool isConsoleEnabled)
        {
            return builder.AddLogger(logger =>
            {
                logger.SetMinimumLevel(minimumLevel);

                if (isConsoleEnabled)
                {
                    logger.AddConsoleLogProcessor();
                }

                logger.AddFileLogProcessor();
                logger.AddLogSource();
            });
        }

        public static IContainerBuilder AddLogger(this IContainerBuilder builder, Action<ILoggerBuilder> configure)
        {
#if GROVEGAMES_DEVELOPERTOOLS
            var history = new LogHistory(UnityLoggerSettings.GetOrCreate().HistoryCapacity);
            builder.AddSingleton<ILogHistory>(history);
            builder.AddDeveloperToolsPage<LogsPage>(LogsPage.PageOrder);
            builder.AddSingleton(_ => UnityLoggerFactory.CreateLogger(logger =>
            {
                configure(logger);
                logger.AddLogHistory(history);
            }));
            builder.AddDeveloperOptions<LoggerDeveloperOptions>();
            return builder.AddSingleton<ILogger>(resolver => resolver.Resolve<Logger>());
#else
            return builder.AddSingleton<ILogger>(_ => UnityLoggerFactory.CreateLogger(configure));
#endif
        }
    }
}
