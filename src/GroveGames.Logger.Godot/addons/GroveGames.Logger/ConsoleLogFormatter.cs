using System.Globalization;

namespace GroveGames.Logger.Godot;

public sealed class ConsoleLogFormatter : ILogFormatter
{
    private static ReadOnlySpan<char> WarningTag => "[color=yellow]⚠️ ";
    private static ReadOnlySpan<char> WarningEnd => "[/color]";
    private static ReadOnlySpan<char> EscapedBracket => "[lb]";
    private static ReadOnlySpan<char> TimeFormat => "HH:mm:ss ";
    private static ReadOnlySpan<char> LeftBracket => "[";
    private static ReadOnlySpan<char> RightBracket => "] ";

    private readonly ITimeProvider _timeProvider;

    public ConsoleLogFormatter(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider is null ? new SystemTimeProvider() : new TimeProviderAdapter(timeProvider);
    }

    public int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = TimeFormat.Length + LeftBracket.Length + tag.Length + RightBracket.Length + message.Length;

        if (level == LogLevel.Warning)
        {
            // Warnings go through PrintRich, so every '[' of the text is written as "[lb]".
            var escapes = (Count(tag, '[') + Count(message, '[')) * (EscapedBracket.Length - 1);
            bufferSize += WarningTag.Length + WarningEnd.Length + escapes + (EscapedBracket.Length - 1);
        }

        return bufferSize;
    }

    public void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var isRich = level == LogLevel.Warning;
        var currentPosition = 0;

        if (isRich)
        {
            WarningTag.CopyTo(buffer[currentPosition..]);
            currentPosition += WarningTag.Length;
        }

        Span<char> timeBuffer = stackalloc char[TimeFormat.Length];
        var currentTime = _timeProvider.GetUtcNow().LocalDateTime;

        if (!currentTime.TryFormat(timeBuffer, out _, TimeFormat, CultureInfo.InvariantCulture))
        {
            "??:??:?? ".AsSpan().CopyTo(timeBuffer);
        }

        timeBuffer.CopyTo(buffer[currentPosition..]);
        currentPosition += TimeFormat.Length;

        currentPosition = Write(buffer, currentPosition, LeftBracket, isRich);
        currentPosition = Write(buffer, currentPosition, tag, isRich);
        RightBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += RightBracket.Length;
        currentPosition = Write(buffer, currentPosition, message, isRich);

        if (isRich)
        {
            WarningEnd.CopyTo(buffer[currentPosition..]);
            currentPosition += WarningEnd.Length;
        }
    }

    private static int Write(Span<char> buffer, int position, ReadOnlySpan<char> text, bool escape)
    {
        if (!escape)
        {
            text.CopyTo(buffer[position..]);
            return position + text.Length;
        }

        foreach (var character in text)
        {
            if (character == '[')
            {
                EscapedBracket.CopyTo(buffer[position..]);
                position += EscapedBracket.Length;
            }
            else
            {
                buffer[position++] = character;
            }
        }

        return position;
    }

    private static int Count(ReadOnlySpan<char> text, char value)
    {
        var count = 0;

        foreach (var character in text)
        {
            if (character == value)
            {
                count++;
            }
        }

        return count;
    }

    private sealed class TimeProviderAdapter : ITimeProvider
    {
        private readonly TimeProvider _timeProvider;

        public TimeProviderAdapter(TimeProvider timeProvider)
        {
            _timeProvider = timeProvider;
        }

        public DateTimeOffset GetUtcNow() => _timeProvider.GetUtcNow();
    }
}
