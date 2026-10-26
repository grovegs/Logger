using GroveGames.DependencyInjection;
using GroveGames.DeveloperTools.Unity;

namespace GroveGames.Logger.Unity
{
    public static class LoggerDeveloperToolsContainerBuilderExtensions
    {
        public static IContainerBuilder AddLogsPage(this IContainerBuilder builder, ILogHistory history)
        {
            builder.AddSingleton(history);
            return builder.AddDeveloperToolsPage<LogsPage>();
        }
    }
}
