namespace GroveGames.Logger;

public interface ILogProcessor
{
    public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}
