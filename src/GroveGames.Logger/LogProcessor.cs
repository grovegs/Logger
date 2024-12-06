
namespace GroveGames.Logger;

public abstract class LogProcessor : ILogProcessor
{
    public static readonly EmptyLogProcessor Empty = new();

    public void ProcessLog(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
    }
}
