namespace GroveGames.Logger;

public interface ILogger : IDisposable
{
    void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void AddLogProcessor(ILogProcessor logProcessor);
    void RemoveLogProcessor(ILogProcessor logProcessor);
}