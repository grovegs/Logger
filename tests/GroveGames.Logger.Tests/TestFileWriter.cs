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

public class TestFileWriterAllocation : IFileWriter
{
    public Queue<char> Messages { get; } = new(128);

    public void AddToQueue(ReadOnlySpan<char> message)
    {
        foreach (var ch in message)
        {
            Messages.Enqueue(ch);
        }
    }

    public void Dispose()
    {
    }
}
