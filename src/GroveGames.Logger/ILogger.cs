using System;

namespace GroveGames.Logger;

public interface ILogger : IDisposable
{
    void Info(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void Error(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
    void Warning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message);
}
