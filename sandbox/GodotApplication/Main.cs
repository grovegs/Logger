using Godot;

using GroveGames.Logger;

public partial class Main : Node2D
{
    public override void _Ready()
    {
        GodotLogger.Shared.Info("test", "test");

        for (var i = 0; i< 1000; i++)
        {
             GodotLogger.Shared.Info("test", $"{i}");
        }
    }
}