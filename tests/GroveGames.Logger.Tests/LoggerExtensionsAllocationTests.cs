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

    [Fact]
    public void LogInformation_LongValue_DoesNotAllocate()
    {
        var logger = new CountingLogger(LogLevel.Debug);
        var value = new string('a', 2000);
        logger.LogInformation("Tag", $"Values: {value}");

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < CallCount; i++)
        {
            logger.LogInformation("Tag", $"Values: {value}");
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0L, allocated);
    }

    private static void Log(ILogger logger, int stage, float ratio)
    {
        logger.LogInformation("Tag", $"Reach Stage {stage} at {ratio}");
    }

    [Fact]
    public void LogDebug_LevelDisabled_SkipsTheFirstHole()
    {
        var logger = new CountingLogger(LogLevel.Error);
        var calls = 0;

        logger.LogDebug("Tag", $"{Count(ref calls)} items");

        Assert.Equal(0, calls);
    }

    [Fact]
    public void LogInformation_ValueTypes_DoesNotAllocate()
    {
        var logger = new CountingLogger(LogLevel.Debug);
        var count = 7u;
        var time = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var id = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");
        var flag = true;
        LogValues(logger, count, time, id, flag);

        var before = GC.GetAllocatedBytesForCurrentThread();
        for (var i = 0; i < CallCount; i++)
        {
            LogValues(logger, count, time, id, flag);
        }

        var allocated = GC.GetAllocatedBytesForCurrentThread() - before;

        Assert.Equal(0L, allocated);
    }

    [Fact]
    public void LogInformation_ValueTypes_FormatsInvariantly()
    {
        var logger = new RecordingLogger();
        var time = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);

        logger.LogInformation("Tag", $"{7u} {(byte)3} {1.5m} {true} {time:yyyy-MM-dd} {TimeSpan.FromSeconds(90)} {(int?)5} {(object)"text"}");

        Assert.Equal("7 3 1.5 True 2024-01-15 00:01:30 5 text", logger.Message);
    }

    [Fact]
    public void LogInformation_DateTimeWithoutFormat_MatchesInvariantToString()
    {
        var logger = new RecordingLogger();
        var time = new DateTime(2024, 1, 15, 10, 30, 45, DateTimeKind.Utc);
        var offset = new DateTimeOffset(time);

        logger.LogInformation("Tag", $"{time}|{offset}");

        Assert.Equal(time.ToString(System.Globalization.CultureInfo.InvariantCulture) + "|" + offset.ToString(System.Globalization.CultureInfo.InvariantCulture), logger.Message);
    }

    private static int Count(ref int calls)
    {
        calls++;
        return calls;
    }

    private static void LogValues(ILogger logger, uint count, DateTime time, Guid id, bool flag)
    {
        logger.LogInformation("Tag", $"{count} {time:HH:mm:ss} {id} {flag}");
    }

    private sealed class RecordingLogger : ILogger
    {
        private string _message;

        public RecordingLogger()
        {
            _message = string.Empty;
        }

        public LogLevel MinimumLevel => LogLevel.Debug;
        public string Message => _message;

        public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            _message = message.ToString();
        }
    }
}
