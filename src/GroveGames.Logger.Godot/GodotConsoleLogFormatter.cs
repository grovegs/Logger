namespace GroveGames.Logger;

public sealed class GodotConsoleLogFormatter : ILogFormatter
{
    private const int WarningStyleSize = 16; // "[color=yellow]⚠️ "
    private const int DateTimeSize = 8; // HH:mm:ss
    private const int BracketsAndSpaces = 4; // " [" + "] " = 2+2=4

    public int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        if (level == LogLevel.Warning)
        {
            return WarningStyleSize + DateTimeSize + BracketsAndSpaces + tag.Length + message.Length;
        }

        return DateTimeSize + BracketsAndSpaces + tag.Length + message.Length;
    }

    public void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Span<char> dateBuffer = stackalloc char[DateTimeSize];
        DateTime.UtcNow.TryFormat(dateBuffer, out _, "HH:mm:ss");

        ReadOnlySpan<char> openBracket = " [";
        ReadOnlySpan<char> closeBracket = "] ";

        int offset = 0;

        // Copy warning style
        if (level == LogLevel.Warning)
        {
            ReadOnlySpan<char> warningStyle = "[color=yellow]⚠️ ";
            warningStyle.CopyTo(buffer.Slice(offset, warningStyle.Length));
            offset += warningStyle.Length;
        }

        // Copy datetime (8 chars)
        dateBuffer.CopyTo(buffer.Slice(offset, dateBuffer.Length));
        offset += dateBuffer.Length;

        // Copy " [" (2 chars)
        openBracket.CopyTo(buffer.Slice(offset, 2));
        offset += 2;

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
