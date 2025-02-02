using Godot;

using GroveGames.Logger;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        var logger = new Logger(LogLevel.Information);
        logger.AddGodotFileLogProcessor();
        logger.AddGodotConsoleLogProcessor();

        logger.LogDebug("debug", $"test");

        for (var i = 0; i < 10; i++)
        {
            logger.LogInformation("information", $"{i}");
        }

        logger.LogWarning("warning", $"test");
        logger.LogError("error", $"test");
    }
}