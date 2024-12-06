using Godot;

namespace GroveGames.Logger;

public class GodotLogProcessor : ILogProcessor
{
    private readonly Action<string> _onLogReceived;

    public GodotLogProcessor(Action<string> onLogReceived = null)
    {
        _onLogReceived = onLogReceived;
    }

    public void ProcessLog(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var msg = $"{level} | {tag} | {message}";
        GD.Print(msg);
        _onLogReceived?.Invoke(msg);
    }
}
