using Godot;

using GroveGames.Logger;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        GodotLogger.Shared.Warning("test", $"test");
        GodotLogger.Shared.Error("test", $"test");

        for (var i = 0; i < 10; i++)
        {
            GodotLogger.Shared.Info("test", $"{i}");
        }
    }
}