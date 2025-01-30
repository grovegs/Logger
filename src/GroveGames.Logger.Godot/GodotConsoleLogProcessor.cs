using Godot;

namespace GroveGames.Logger;

public sealed class GodotConsoleLogProcessor : ILogProcessor
{
    private readonly ILogFormatter _logFormatter;

    public GodotConsoleLogProcessor(ILogFormatter logFormatter)
    {
        _logFormatter = logFormatter;
    }

    public void ProcessInfo(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _logFormatter.GetBufferSize(LogLevel.Info, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];
        _logFormatter.Format(buffer, LogLevel.Info, tag, message);
        GD.Print(buffer.ToString());
    }

    public void ProcessWarning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _logFormatter.GetBufferSize(LogLevel.Warning, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];
        _logFormatter.Format(buffer, LogLevel.Warning, tag, message);
        GD.PrintRich(buffer.ToString());
    }

    public void ProcessError(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _logFormatter.GetBufferSize(LogLevel.Error, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];
        _logFormatter.Format(buffer, LogLevel.Error, tag, message);
        GD.PrintErr(buffer.ToString());
    }
}
