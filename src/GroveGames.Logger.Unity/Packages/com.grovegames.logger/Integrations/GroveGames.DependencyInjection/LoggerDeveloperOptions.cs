#if GROVEGAMES_DEVELOPERTOOLS
using GroveGames.DeveloperTools;

namespace GroveGames.Logger.Unity.DeveloperTools
{
    public sealed class LoggerDeveloperOptions : IDeveloperOptions
    {
        private readonly Logger _logger;

        [DeveloperOption(Header = "Level", Tooltip = "Messages logged through ILogger below this level are dropped. Messages Unity logs itself keep the level the logger was built with.")] private LogLevel _minimumLevel;

        public string Category => "Logger";

        public LoggerDeveloperOptions(Logger logger)
        {
            _logger = logger;
            _minimumLevel = logger.MinimumLevel;
        }

        private void OnValidate()
        {
            _logger.SetMinimumLevel(_minimumLevel);
        }
    }
}
#endif
