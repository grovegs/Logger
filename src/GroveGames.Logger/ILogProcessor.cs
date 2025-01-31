namespace GroveGames.Logger;

public interface ILogProcessor
{
    void Process(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}