namespace GroveGames.Logger;

public interface ILogFormatter
{
    int GetBufferSize(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void Format(Span<char> buffer, ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}