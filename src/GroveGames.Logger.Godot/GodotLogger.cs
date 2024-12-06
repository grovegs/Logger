using Godot;

namespace GroveGames.Logger;

public class GodotLogger : ILogger
{
    public static readonly GodotLogger Shared = new();
    private readonly FileLogger _logger;

    public GodotLogger()
    {
        var logFileFactory = new GodotLogFileFactory();
        var fileWriter = new FileWriter(logFileFactory.CreateFile());
        _logger = new FileLogger(fileWriter);
    }

    public void Info(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        _logger.Info(tag, message);
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
