using GroveGames.Logger;

public class TestFileWriter : IFileWriter
{
    public List<string> Messages { get; } = new(24);

    public void AddToQueue(ReadOnlySpan<char> message)
    {
        Messages.Add(message.ToString());
    }

    public void Dispose()
    {
    }
}
