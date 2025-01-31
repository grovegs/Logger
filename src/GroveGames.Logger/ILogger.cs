namespace GroveGames.Logger;

public interface ILogger : IDisposable
{
#if DEBUG
    void Debug(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message);
#else
    void Debug(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
#endif
    void Info(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message);
    void Warning(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message);
    void Error(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message);
}