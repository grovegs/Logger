
namespace GroveGames.Logger;

public class EmptyLogProcessor : ILogProcessor
{
    public void ProcessLog(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
    }
}
