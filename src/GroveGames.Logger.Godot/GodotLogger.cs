namespace GroveGames.Logger;

public class GodotLogger : ILogger
{
    public static readonly GodotLogger Shared = new();
    private readonly FileLogger _logger;

    private ILogProcessor _logProcessor;

    public GodotLogger()
    {
        var logFileFactory = new GodotLogFileFactory();
        var fileWriter = new FileWriter(logFileFactory.CreateFile());
        _logger = new FileLogger(fileWriter);
        _logProcessor = new EmptyLogProcessor();
    }

    public void EnableGodotProcessor(Action<string> onLogReceived = null)
    {
        _logProcessor = new GodotLogProcessor(onLogReceived);
    }

    public void Info(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        _logger.Info(tag, message);
        _logProcessor.ProcessLog("INFO", tag, message);
    }

    public void Warning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        _logger.Warning(tag, message);
        _logProcessor.ProcessLog("WARNING", tag, message);
    }

    public void Error(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        _logger.Error(tag, message);
        _logProcessor.ProcessLog("ERROR", tag, message);
    }

    public void Dispose()
    {
        _logger.Dispose();
    }
}
