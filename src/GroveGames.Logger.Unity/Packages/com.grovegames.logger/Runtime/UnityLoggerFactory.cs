using System;
using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public static class UnityLoggerFactory
    {
        public static Logger CreateLogger(Action<ILoggerBuilder> configure)
        {
            return CreateLogger(UnityLoggerSettings.GetOrCreate(), configure);
        }

        public static Logger CreateLogger(ILoggerSettings settings, Action<ILoggerBuilder> configure)
        {
            if (settings == null)
            {
                Debug.LogError("Settings cannot be null");
                settings = ScriptableObject.CreateInstance<UnityLoggerSettings>();
            }

            if (configure == null)
            {
                Debug.LogError("The logger configuration cannot be null, so the logger writes nowhere.");
                configure = static _ => { };
            }

            return LoggerFactory.CreateLogger(settings, configure);
        }
    }
}
