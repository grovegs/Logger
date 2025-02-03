namespace GroveGames.Logger.Tests;

public class TestFileWriter : IFileWriter
{
    public List<string> Messages { get; } = new(24);

    public void AddEntry(ReadOnlySpan<char> message)
    {
        Messages.Add(message.ToString());
    }

    public void Dispose()
    {
    }
}