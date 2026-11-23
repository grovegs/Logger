namespace GroveGames.Logger.Tests;

public sealed class LoggerSettingsTests
{
    [Fact]
    public void Constructor_NullFileFolderName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new LoggerSettings(LogLevel.Debug, 10, null!, 8192, 1000));
    }

    [Fact]
    public void Constructor_ValidArguments_ExposesThem()
    {
        var settings = new LoggerSettings(LogLevel.Error, 5, "files", 4096, 200);

        Assert.Equal(LogLevel.Error, settings.MinLogLevel);
        Assert.Equal(5, settings.MaxFileCount);
        Assert.Equal("files", settings.FileFolderName);
        Assert.Equal(4096, settings.FileBufferSize);
        Assert.Equal(200, settings.FileChannelCapacity);
    }
}
