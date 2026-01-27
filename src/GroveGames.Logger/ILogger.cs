namespace GroveGames.Logger;

public interface ILogger
{
    LogLevel MinimumLevel { get; }
    void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}
