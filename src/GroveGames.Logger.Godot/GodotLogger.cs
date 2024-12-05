using Godot;

namespace GroveGames.Logger;

public class GodotLogger : ILogger
{
    public static readonly GodotLogger Shared = new();
    private readonly FileLogger _logger;

    public GodotLogger()
    {
        var logFileFactory = new GodotLogFileFactory();
        _logger = new FileLogger(logFileFactory.CreateFile());
    }

    public void Info(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        _logger.Info(tag, message);
        GD.Print(message.ToString());
    }

    public void Warning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        _logger.Warning(tag, message);
    }

    public void Error(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        _logger.Error(tag, message);
    }

    public void Dispose()
    {
        _logger.Dispose();
    }
}
