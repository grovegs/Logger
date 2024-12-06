namespace GroveGames.Logger;

public interface ILogProcessor
{
    void ProcessInfo(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void ProcessWarning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void ProcessError(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}