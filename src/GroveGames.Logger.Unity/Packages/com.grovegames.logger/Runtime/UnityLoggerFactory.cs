using System;

namespace GroveGames.Logger.Unity
{
    public static class UnityLoggerFactory
    {
        public static Logger CreateLogger(Action<ILoggerBuilder> configure)
        {
            var settings = UnityLoggerSettings.Load();
            var builder = new LoggerBuilder();
            builder.SetMinimumLevel(settings.MinLogLevel);
            configure(builder);
            return builder.Build();
        }

        public static Logger CreateLogger(UnityLoggerSettings settings, Action<ILoggerBuilder> configure)
        {
            var builder = new LoggerBuilder();
            builder.SetMinimumLevel(settings.MinLogLevel);
            configure(builder);
            return builder.Build();
        }
    }
}
