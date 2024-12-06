namespace GroveGames.Logger;

public sealed class GodotFileLogger : ILogger
{
    public static readonly GodotFileLogger Shared = new();
    private readonly FileLogger _logger;

    public GodotFileLogger()
    {
        var logFileFactory = new GodotLogFileFactory();
        var fileWriter = new FileWriter(logFileFactory.CreateFile());
        _logger = new FileLogger(fileWriter);
    }

    public void SetProcessor(ILogProcessor processor)
    {
        _logger.SetProcessor(processor);
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
