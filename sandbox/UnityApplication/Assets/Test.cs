
using GroveGames.Logger;
using GroveGames.Logger.Unity;

using UnityEngine;

using ILogger = GroveGames.Logger.ILogger;

public class Test : MonoBehaviour
{
    private ILogger _logger;

    private void Awake()
    {
        _logger = UnityLoggerFactory.CreateLogger(builder =>
        {
            builder.AddUnityConsoleLogProcessor();
            builder.AddUnityFileLogProcessor();
        });
        _logger.LogInformation("Test log message from Unity Logger", $"Awake");
    }
}
