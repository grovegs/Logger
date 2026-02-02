using System;

namespace GroveGames.Logger.Unity;

public sealed class UnityConsoleLogFormatter : ILogFormatter
{
    private static ReadOnlySpan<char> LeftBracket => "[";
    private static ReadOnlySpan<char> RightBracket => "] ";

    public int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        return 1 + tag.Length + 2 + message.Length;
    }

    public void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var currentPosition = 0;

        LeftBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += LeftBracket.Length;

        tag.CopyTo(buffer[currentPosition..]);
        currentPosition += tag.Length;

        RightBracket.CopyTo(buffer[currentPosition..]);
        currentPosition += RightBracket.Length;

        message.CopyTo(buffer[currentPosition..]);
    }
}
