using Godot;

namespace GroveGames.Logger;

public sealed class GodotLogProcessor : ILogProcessor
{
    private readonly Action<string> _onLogReceived;

    public GodotLogProcessor(Action<string> onLogReceived = null)
    {
        _onLogReceived = onLogReceived;
    }

    public void ProcessLog(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var msg = $"{level} | {tag} | {message}";
        _onLogReceived?.Invoke(msg);
    }
}
