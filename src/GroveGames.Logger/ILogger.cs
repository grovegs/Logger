namespace GroveGames.Logger;

public interface ILogger
{
    LogLevel MinimumLogLevel { get; }
    void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}