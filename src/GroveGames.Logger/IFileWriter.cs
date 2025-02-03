namespace GroveGames.Logger;

public interface IFileWriter : IDisposable
{
    void AddEntry(ReadOnlySpan<char> entry);
}
