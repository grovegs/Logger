namespace GroveGames.Logger;

public interface IStreamWriter : IDisposable
{
    void AddEntry(ReadOnlySpan<char> entry);
    void Flush();
}
