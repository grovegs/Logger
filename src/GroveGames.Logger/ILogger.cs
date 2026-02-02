namespace GroveGames.Logger;

public interface ILogger
{
    public LogLevel MinimumLevel { get; }
    public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}
