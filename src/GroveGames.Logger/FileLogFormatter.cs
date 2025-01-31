namespace GroveGames.Logger;

public sealed class FileLogFormatter : ILogFormatter
{
    private const int DateTimeSize = 8; // HH:mm:ss
    private const int BracketsAndSpaces = 7; // " [" + "] [" + "] " = 2+3+2=7

    public int GetBufferSize(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        return DateTimeSize + BracketsAndSpaces + level.Length + tag.Length + message.Length;
    }

    public void Format(Span<char> buffer, ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Span<char> dateBuffer = stackalloc char[DateTimeSize];
        DateTime.UtcNow.TryFormat(dateBuffer, out _, "HH:mm:ss");

        ReadOnlySpan<char> openBracket = " [";
        ReadOnlySpan<char> closeOpenBracket = "] [";
        ReadOnlySpan<char> closeBracket = "] ";

        int offset = 0;

        // Copy datetime (8 chars)
        dateBuffer.CopyTo(buffer.Slice(offset, dateBuffer.Length));
        offset += dateBuffer.Length;

        // Copy " [" (2 chars)
        openBracket.CopyTo(buffer.Slice(offset, 2));
        offset += 2;

        // Copy level
        level.CopyTo(buffer.Slice(offset, level.Length));
        offset += level.Length;

        // Copy "] [" (3 chars)
        closeOpenBracket.CopyTo(buffer.Slice(offset, 3));
        offset += 3;

        // Copy tag
        tag.CopyTo(buffer.Slice(offset, tag.Length));
        offset += tag.Length;

        // Copy "] " (2 chars)
        closeBracket.CopyTo(buffer.Slice(offset, 2));
        offset += 2;

        // Copy message
        message.CopyTo(buffer.Slice(offset, message.Length));
    }
}
