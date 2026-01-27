using System;

namespace GroveGames.Logger.Unity
{
    public static class UnityLoggerFactory
    {
        public static Logger CreateLogger(Action<ILoggerBuilder> configure)
        {
            return CreateLogger(UnityLoggerSettings.GetOrCreate(), configure);
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
