namespace GroveGames.Logger;

public interface ILoggerSettings
{
    public LogLevel MinLogLevel { get; }
    public int MaxFileCount { get; }
    public string FileFolderName { get; }
    public int FileBufferSize { get; }
    public int FileChannelCapacity { get; }
}
