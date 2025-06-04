namespace GroveGames.Logger.Tests;

public class TestStreamWriter : IStreamWriter
{
    public List<string> Messages { get; } = new(24);

    public void AddEntry(ReadOnlySpan<char> message)
    {
        Messages.Add(message.ToString());
    }

    public void Dispose()
    {
    }

    public void Flush()
    {
    }
}