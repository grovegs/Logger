using System;
using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public static class UnityLoggerFactory
    {
        public static Logger CreateLogger(Action<ILoggerBuilder> configure)
        {
            return CreateLogger(LoggerSettings.GetOrCreate(), configure);
        }

        public static Logger CreateLogger(LoggerSettings settings, Action<ILoggerBuilder> configure)
        {
            if (settings == null)
            {
                Debug.LogError("LoggerSettings cannot be null");
                settings = ScriptableObject.CreateInstance<LoggerSettings>();
            }

            var builder = new LoggerBuilder();
            builder.SetMinimumLevel(settings.MinLogLevel);
            configure(builder);
            return builder.Build();
        }
    }
}
