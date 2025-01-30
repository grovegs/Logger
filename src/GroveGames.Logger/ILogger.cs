namespace GroveGames.Logger;

public interface ILogger : IDisposable
{
    void Info(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message);
    void Error(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message);
    void Warning(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message);
    void AddProcessor(ILogProcessor processor);
    void RemoveProcessor(ILogProcessor processor);
}
