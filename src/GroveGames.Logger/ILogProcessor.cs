namespace GroveGames.Logger;

public interface ILogProcessor
{
    void ProcessDebug(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void ProcessInformation(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void ProcessWarning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void ProcessError(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}