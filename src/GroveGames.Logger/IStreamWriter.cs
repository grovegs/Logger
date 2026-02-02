namespace GroveGames.Logger;

public interface IStreamWriter : IDisposable
{
    public void AddEntry(ReadOnlySpan<char> entry);
    public void Flush();
}
