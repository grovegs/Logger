using System;
using GroveGames.DependencyInjection;

namespace GroveGames.Logger.Unity
{
    public static class LoggerContainerBuilderExtensions
    {
        public static IContainerBuilder AddLogger(this IContainerBuilder builder, Action<ILoggerBuilder> configure)
        {
            return builder.AddSingleton<ILogger>(_ => UnityLoggerFactory.CreateLogger(configure));
        }
    }
}
