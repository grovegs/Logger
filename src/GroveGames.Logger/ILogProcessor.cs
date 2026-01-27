namespace GroveGames.Logger;

public interface ILogProcessor
{
    void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}
