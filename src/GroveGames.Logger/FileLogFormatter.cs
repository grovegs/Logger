namespace GroveGames.Logger;

public sealed class FileLogFormatter : ILogFormatter
{
    private static ReadOnlySpan<char> LevelCharacter(LogLevel level) => level switch
    {
        LogLevel.Debug => "D",
        LogLevel.Information => "I",
        LogLevel.Warning => "W",
        LogLevel.Error => "E",
        _ => "N"
    };
    private static ReadOnlySpan<char> TimeFormat => "HH:mm:ss ";
    private static ReadOnlySpan<char> LeftBracket => "[";
    private static ReadOnlySpan<char> RightBracket => "] ";

    private readonly ITimeProvider _timeProvider;

    public FileLogFormatter(ITimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? new SystemTimeProvider();
    }

    public int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        int bufferSize = 9 + 1 + 1 + 2 + 1 + tag.Length + 2 + message.Length;
        return bufferSize;
    }

    public void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        int currentPosition = 0;
        Span<char> timeBuffer = stackalloc char[TimeFormat.Length];
        DateTime currentTime = _timeProvider.GetUtcNow().DateTime;

        if (!currentTime.TryFormat(timeBuffer, out int charsWritten, TimeFormat))
        {
            "??:??:?? ".AsSpan().CopyTo(timeBuffer);
        }

        ReadOnlySpan<char> leftBracket = LeftBracket;
        ReadOnlySpan<char> rightBracket = RightBracket;

        timeBuffer.CopyTo(buffer[currentPosition..]);
        currentPosition += TimeFormat.Length;

        leftBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += leftBracket.Length;

        ReadOnlySpan<char> levelCharacter = LevelCharacter(level);
        levelCharacter.CopyTo(buffer[currentPosition..]);
        currentPosition += levelCharacter.Length;

        rightBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += rightBracket.Length;

        leftBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += leftBracket.Length;

        tag.CopyTo(buffer[currentPosition..]);
        currentPosition += tag.Length;

        rightBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += rightBracket.Length;

        message.CopyTo(buffer[currentPosition..]);
    }
}
