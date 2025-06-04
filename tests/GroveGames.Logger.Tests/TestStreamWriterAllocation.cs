namespace GroveGames.Logger.Tests;

public class TestStreamWriterAllocation : IStreamWriter
{
    public Queue<char> Messages { get; } = new(1024);

    public void AddEntry(ReadOnlySpan<char> message)
    {
        foreach (var ch in message)
        {
            Messages.Enqueue(ch);
        }

        Messages.Enqueue('\n');
    }

    public void Flush()
    {
    }

    public void Dispose()
    {
    }
}