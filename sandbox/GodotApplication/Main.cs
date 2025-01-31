using Godot;

using GroveGames.Logger;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        var fileFactory = new GodotLogFileFactory();
        var fileWriter = new FileWriter(fileFactory.CreateFile());
        var fileLogProcessor = new FileLogProcessor(fileWriter, new FileLogFormatter());
        Logger.Shared.AddProcessor(fileLogProcessor);
        Logger.Shared.AddProcessor(new GodotConsoleLogProcessor(new GodotConsoleLogFormatter()));
        Logger.Shared.Warning("test", $"test");
        Logger.Shared.Error("test", $"test");

        for (var i = 0; i < 10; i++)
        {
            Logger.Shared.Info("test", $"{i}");
        }
    }
}