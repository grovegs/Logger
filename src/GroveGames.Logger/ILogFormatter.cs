namespace GroveGames.Logger;

public interface ILogFormatter
{
    int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}
