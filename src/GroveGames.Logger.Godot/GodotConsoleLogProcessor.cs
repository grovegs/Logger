using Godot;

namespace GroveGames.Logger;

public sealed class GodotConsoleLogProcessor : ILogProcessor
{
    private readonly ILogFormatter _logFormatter;

    public GodotConsoleLogProcessor(ILogFormatter logFormatter)
    {
        _logFormatter = logFormatter;
    }

    public void Process(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _logFormatter.GetBufferSize(level, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];
        _logFormatter.Format(buffer, level, tag, message);
        var log = buffer.ToString();

        switch (level)
        {
            case LogLevel.Debug:
                GD.Print(log);
                break;
            case LogLevel.Information:
                GD.Print(log);
                break;
            case LogLevel.Warning:
                GD.PrintRich(log);
                break;
            case LogLevel.Error:
                GD.PrintErr(log);
                break;
        }
    }
}
