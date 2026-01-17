namespace GroveGames.Logger;

public interface ITimeProvider
{
    DateTimeOffset GetUtcNow();
}
