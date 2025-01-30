namespace GroveGames.Logger;

public sealed class GodotLogger : ILogger
{
    public static readonly GodotLogger Shared = new();
    private readonly Logger _logger;

    public GodotLogger()
    {
        _logger = new Logger();
        var fileFactory = new GodotLogFileFactory();
        var fileWriter = new FileWriter(fileFactory.CreateFile());
        _logger.AddProcessor(new FileLogProcessor(fileWriter, new FileLogFormatter()));
        _logger.AddProcessor(new GodotConsoleLogProcessor(new GodotConsoleLogFormatter()));
    }

    public void Info(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        _logger.Info(tag, message);
    }

    public void Warning(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        _logger.Warning(tag, message);
    }

    public void Error(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        _logger.Error(tag, message);
    }

    public void AddProcessor(ILogProcessor processor)
    {
        _logger.AddProcessor(processor);
    }

    public void RemoveProcessor(ILogProcessor processor)
    {
        _logger.RemoveProcessor(processor);
    }

    public void Dispose()
    {
        _logger.Dispose();
    }
}
