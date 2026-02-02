namespace GroveGames.Logger;

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow();
}
