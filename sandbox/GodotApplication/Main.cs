using Godot;

using GroveGames.Logger;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        GodotFileLogger.Shared.AddProcessor(new GodotLogProcessor());
        GodotFileLogger.Shared.Warning("test", "test");
        GodotFileLogger.Shared.Error("test", "test");

        for (var i = 0; i< 10; i++)
        {
             GodotFileLogger.Shared.Info("test", $"{i}");
        }
    }
}