namespace GroveGames.Logger;

public interface ILogProcessor
{
    void ProcessLog(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}