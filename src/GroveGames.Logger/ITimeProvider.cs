namespace GroveGames.Logger;

public interface ITimeProvider
{
    public DateTimeOffset GetUtcNow();
}
