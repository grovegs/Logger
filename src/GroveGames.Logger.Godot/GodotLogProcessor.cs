using Godot;

namespace GroveGames.Logger;

public sealed class GodotLogProcessor : ILogProcessor
{
    public void ProcessError(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var msg = $"{tag} | {message}";
        GD.PrintErr(msg);
    }

    public void ProcessInfo(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var msg = $"{tag} | {message}";
        GD.Print(msg);
    }

    public void ProcessWarning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var msg = $"{tag} | {message}";
        GD.PrintRich($"[color=yellow]{msg}");
    }
}
