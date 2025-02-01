namespace GroveGames.Logger;

public sealed class SharedLogger
{
    private static readonly Lazy<Logger> LazyInstance = new(() => new Logger());

    public static Logger Instance => LazyInstance.Value;

    private SharedLogger() { }
}
