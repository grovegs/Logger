using Godot;

using GroveGames.Logger;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        GodotFileLogger.Shared.SetProcessor(new GodotLogProcessor(s => GD.Print(s)));
        GodotFileLogger.Shared.Info("test", "test");

        for (var i = 0; i< 1000; i++)
        {
             GodotFileLogger.Shared.Info("test", $"{i}");
        }
    }
}