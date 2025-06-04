namespace GroveGames.Logger;

public sealed class GodotConsoleLogFormatter : ILogFormatter
{
    private static ReadOnlySpan<char> WarningTag => "[color=yellow]⚠️ ";
    private static ReadOnlySpan<char> TimeFormat => "HH:mm:ss ";
    private static ReadOnlySpan<char> LeftBracket => "[";
    private static ReadOnlySpan<char> RightBracket => "] ";

    public int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = 9 + 1 + tag.Length + 2 + message.Length;

        if (level == LogLevel.Warning)
        {
            bufferSize += 17;
        }

        return bufferSize;
    }

    public void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        int currentPosition = 0;

        if (level == LogLevel.Warning)
        {
            WarningTag.CopyTo(buffer[currentPosition..]);
            currentPosition += WarningTag.Length;
        }

        var timeFormat = TimeFormat;
        Span<char> timeBuffer = stackalloc char[timeFormat.Length];

        if (!DateTime.UtcNow.TryFormat(timeBuffer, out int charsWritten, timeFormat))
        {
            throw new FormatException("Failed to format DateTime");
        }

        timeBuffer.CopyTo(buffer[currentPosition..]);
        currentPosition += timeFormat.Length;

        LeftBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += LeftBracket.Length;

        tag.CopyTo(buffer[currentPosition..]);
        currentPosition += tag.Length;

        RightBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += RightBracket.Length;

        message.CopyTo(buffer[currentPosition..]);
    }
}