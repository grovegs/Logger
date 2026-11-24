namespace GroveGames.Logger.Tests;

public sealed class LoggerFactoryTests
{
    private sealed class FakeLogProcessor : ILogProcessor
    {
        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
        }
    }

    [Fact]
    public void CreateLogger_NullSettings_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => LoggerFactory.CreateLogger(null!, _ => { }));
    }

    [Fact]
    public void CreateLogger_NullConfigure_ThrowsArgumentNullException()
    {
        var settings = new LoggerSettings(LogLevel.Warning, 10, "logs", 8192, 1000);

        Assert.Throws<ArgumentNullException>(() => LoggerFactory.CreateLogger(settings, null!));
    }

    [Fact]
    public void CreateLogger_WithSettings_UsesTheirMinimumLevel()
    {
        var settings = new LoggerSettings(LogLevel.Warning, 10, "logs", 8192, 1000);

        using var logger = LoggerFactory.CreateLogger(settings, builder => builder.AddLogProcessor(new FakeLogProcessor()));

        Assert.Equal(LogLevel.Warning, logger.MinimumLevel);
    }

    [Fact]
    public void CreateLogger_ConfigureSetsLevel_OverridesSettings()
    {
        var settings = new LoggerSettings(LogLevel.Warning, 10, "logs", 8192, 1000);

        using var logger = LoggerFactory.CreateLogger(settings, builder => builder.AddLogProcessor(new FakeLogProcessor()).SetMinimumLevel(LogLevel.Error));

        Assert.Equal(LogLevel.Error, logger.MinimumLevel);
    }
}
