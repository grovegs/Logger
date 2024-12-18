using GroveGames.Logger;

public class TestFileWriterAllocation : IFileWriter
{
    public Queue<char> Messages { get; } = new(128);

    public void AddToQueue(ReadOnlySpan<char> message)
    {
        foreach (var ch in message)
        {
            Messages.Enqueue(ch);
        }

        Messages.Enqueue('\n');
    }

    public void Dispose()
    {
    }
}
