namespace GroveGames.Logger.Tests;

public sealed class LoggerExtensionsAllocationTests
{
    private const int CallCount = 1000;

    private sealed class CountingLogger : ILogger
    {
        private readonly LogLevel _minimumLevel;
        private int _characterCount;

        public LogLevel MinimumLevel => _minimumLevel;
        public int CharacterCount => _characterCount;

        public CountingLogger(LogLevel minimumLevel)
        {
            _minimumLevel = minimumLevel;
        }

        public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            _characterCount += message.Length;
        }
    }

    [Fact]
    public void LogInformation_Numbers_DoesNotAllocate()
    {
        var logger = new CountingLogger(LogLevel.Debug);
        var stage = 12;
        var ratio = 1.5f;
        Log(logger, stage, ratio);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < CallCount; i++)
        {
            Log(logger, stage, ratio);
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0L, allocated);
        Assert.True(logger.CharacterCount > 0);
    }

    [Fact]
    public void LogDebug_LevelDisabled_DoesNotAllocateOrFormat()
    {
        var logger = new CountingLogger(LogLevel.Error);
        var stage = 12;
        logger.LogDebug("Tag", $"Reach Stage {stage}");

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < CallCount; i++)
        {
            logger.LogDebug("Tag", $"Reach Stage {stage}");
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0L, allocated);
        Assert.Equal(0, logger.CharacterCount);
    }

    private static void Log(ILogger logger, int stage, float ratio)
    {
        logger.LogInformation("Tag", $"Reach Stage {stage} at {ratio}");
    }
}
