using System;

using Godot;

using GroveGames.Logger;

public partial class Main : Node2D
{
    private ILogger _logger;

    public override void _EnterTree()
    {
        GD.Print("EnterTree");

        _logger = GodotLoggerFactory.CreateLogger(builder =>
        {
            builder.AddGodotFileLogProcessor();
            builder.AddGodotConsoleLogProcessor();
        });
    }

    public override void _Ready()
    {
        GD.Print("Ready");
        _logger.LogDebug("debug", $"test");

        for (var i = 0; i < 10; i++)
        {
            _logger.LogInformation("information", $"{i}");
        }

        _logger.LogWarning("warning", $"test");
        _logger.LogError("error", $"test");
    }

    public override void _ExitTree()
    {
        GD.Print("ExitTree");

        if (_logger is IDisposable disposable)
        {
            GD.Print("Disposed");
            disposable.Dispose();
        }
    }
}