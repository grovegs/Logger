
namespace GroveGames.Logger;

public sealed class EmptyLogProcessor : ILogProcessor
{
    public void ProcessLog(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
    }
}
