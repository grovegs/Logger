namespace GroveGames.Logger;

public sealed class LoggerSettings : ILoggerSettings
{
    private readonly LogLevel _minLogLevel;
    private readonly int _maxFileCount;
    private readonly string _fileFolderName;
    private readonly int _fileBufferSize;
    private readonly int _fileChannelCapacity;

    public LogLevel MinLogLevel => _minLogLevel;
    public int MaxFileCount => _maxFileCount;
    public string FileFolderName => _fileFolderName;
    public int FileBufferSize => _fileBufferSize;
    public int FileChannelCapacity => _fileChannelCapacity;

    public LoggerSettings(LogLevel minLogLevel, int maxFileCount, string fileFolderName, int fileBufferSize, int fileChannelCapacity)
    {
        ArgumentNullException.ThrowIfNull(fileFolderName);

        _minLogLevel = minLogLevel;
        _maxFileCount = maxFileCount;
        _fileFolderName = fileFolderName;
        _fileBufferSize = fileBufferSize;
        _fileChannelCapacity = fileChannelCapacity;
    }
}
