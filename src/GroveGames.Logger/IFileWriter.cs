namespace GroveGames.Logger;

public interface IFileWriter : IDisposable
{
    void AddToQueue(ReadOnlySpan<char> message);
}
