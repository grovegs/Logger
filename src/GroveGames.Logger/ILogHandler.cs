namespace GroveGames.Logger;

public interface ILogHandler : IDisposable
{
    void Initialize(ILogProcessor[] processors);
}
