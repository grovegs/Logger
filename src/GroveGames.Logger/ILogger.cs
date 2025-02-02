namespace GroveGames.Logger;

public interface ILogger : IDisposable
{
    LogLevel MinimumLogLevel { get; }
    void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void AddLogProcessor(ILogProcessor logProcessor);
    void RemoveLogProcessor(ILogProcessor logProcessor);
}