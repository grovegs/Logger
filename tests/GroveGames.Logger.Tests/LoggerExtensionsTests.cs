namespace GroveGames.Logger.Tests;

public class LoggerExtensionsTests
{
    private class TestLogger : ILogger
    {
        public List<(LogLevel Level, string Tag, string Message)> Logs { get; } = new();
        public LogLevel MinimumLevel { get; set; } = LogLevel.Debug;

        public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            if (level < MinimumLevel) return;
            Logs.Add((level, tag.ToString(), message.ToString()));
        }
    }

    [Fact]
    public void LogDebug_RecordsCorrectLevelTagAndMessage()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        LoggerExtensions.LogDebug(logger, "DEBUG", $"Test message");

        // Assert
        var log = Assert.Single(logger.Logs);
        Assert.Equal(LogLevel.Debug, log.Level);
        Assert.Equal("DEBUG", log.Tag);
        Assert.Equal("Test message", log.Message);
    }

    [Fact]
    public void LogInformation_RecordsCorrectLevelTagAndMessage()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        LoggerExtensions.LogInformation(logger, "INFO", $"System status OK");

        // Assert
        var log = Assert.Single(logger.Logs);
        Assert.Equal(LogLevel.Information, log.Level);
        Assert.Equal("INFO", log.Tag);
        Assert.Equal("System status OK", log.Message);
    }

    [Fact]
    public void LogWarning_RecordsCorrectLevelTagAndMessage()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        LoggerExtensions.LogWarning(logger, "WARN", $"Disk space low");

        // Assert
        var log = Assert.Single(logger.Logs);
        Assert.Equal(LogLevel.Warning, log.Level);
        Assert.Equal("WARN", log.Tag);
        Assert.Equal("Disk space low", log.Message);
    }

    [Fact]
    public void LogError_RecordsCorrectLevelTagAndMessage()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        LoggerExtensions.LogError(logger, "ERROR", $"Failed to save data");

        // Assert
        var log = Assert.Single(logger.Logs);
        Assert.Equal(LogLevel.Error, log.Level);
        Assert.Equal("ERROR", log.Tag);
        Assert.Equal("Failed to save data", log.Message);
    }

    [Fact]
    public void MinimumLevelWarning_IgnoresDebugAndInfoLogs()
    {
        // Arrange
        var logger = new TestLogger { MinimumLevel = LogLevel.Warning };

        // Act
        LoggerExtensions.LogDebug(logger, "TEST", $"Debug message");
        LoggerExtensions.LogInformation(logger, "TEST", $"Info message");
        LoggerExtensions.LogWarning(logger, "TEST", $"Warning message");
        LoggerExtensions.LogError(logger, "TEST", $"Error message");

        // Assert
        Assert.Equal(2, logger.Logs.Count);
        Assert.Contains(logger.Logs, l => l.Level == LogLevel.Warning);
        Assert.Contains(logger.Logs, l => l.Level == LogLevel.Error);
    }

    [Fact]
    public void AllLevels_HandleEmptyTagCorrectly()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        LoggerExtensions.LogDebug(logger, "", $"Debug");
        LoggerExtensions.LogInformation(logger, "", $"Info");
        LoggerExtensions.LogWarning(logger, "", $"Warning");
        LoggerExtensions.LogError(logger, "", $"Error");

        // Assert
        Assert.All(logger.Logs, l => Assert.Equal("", l.Tag));
    }

    [Fact]
    public void AllLevels_HandleNullFormattedValues()
    {
        // Arrange
        var logger = new TestLogger();
        string? nullValue = null;

        // Act
        LoggerExtensions.LogDebug(logger, "NULL", $"{nullValue}");
        LoggerExtensions.LogInformation(logger, "NULL", $"{nullValue}");
        LoggerExtensions.LogWarning(logger, "NULL", $"{nullValue}");
        LoggerExtensions.LogError(logger, "NULL", $"{nullValue}");

        // Assert
        Assert.All(logger.Logs, l => Assert.Equal("", l.Message));
    }

    [Fact]
    public void AllLevels_HandleLargeMessages()
    {
        // Arrange
        var logger = new TestLogger();
        var largeMessage = new string('X', 16);

        // Act
        LoggerExtensions.LogDebug(logger, "LARGE", $"{largeMessage}");
        LoggerExtensions.LogInformation(logger, "LARGE", $"{largeMessage}");
        LoggerExtensions.LogWarning(logger, "LARGE", $"{largeMessage}");
        LoggerExtensions.LogError(logger, "LARGE", $"{largeMessage}");

        // Assert
        Assert.All(logger.Logs, l => Assert.Equal(largeMessage, l.Message));
    }

    [Fact]
    public void ComplexMessages_FormatCorrectlyAcrossLevels()
    {
        // Arrange
        var logger = new TestLogger();
        var exception = new InvalidOperationException("Error");
        var timestamp = DateTime.UtcNow.ToString("O");

        // Act
        LoggerExtensions.LogDebug(logger, "APP",
            $"Startup at {timestamp}, Version: {Environment.Version}");

        LoggerExtensions.LogError(logger, "APP",
            $"Crash at {timestamp}. Error: {exception.Message}");

        // Assert
        Assert.Equal(2, logger.Logs.Count);
        Assert.Contains(timestamp, logger.Logs[0].Message);
        Assert.Contains(exception.Message, logger.Logs[1].Message);
    }

    [Fact]
    public void MixedLevelLogging_PreservesOrder()
    {
        // Arrange
        var logger = new TestLogger();

        // Act
        LoggerExtensions.LogDebug(logger, "1", $"First");
        LoggerExtensions.LogInformation(logger, "2", $"Second");
        LoggerExtensions.LogWarning(logger, "3", $"Third");
        LoggerExtensions.LogError(logger, "4", $"Fourth");

        // Assert
        Assert.Collection(logger.Logs,
            item => Assert.Equal("First", item.Message),
            item => Assert.Equal("Second", item.Message),
            item => Assert.Equal("Third", item.Message),
            item => Assert.Equal("Fourth", item.Message));
    }

    [Fact]
    public void NumericFormatting_HandlesDifferentNumberTypes()
    {
        // Arrange
        var logger = new TestLogger();
        var intValue = 42;
        var doubleValue = 3.14159;
        var decimalValue = 19.99m;

        // Act
        LoggerExtensions.LogInformation(logger, "NUM",
            $"Int: {intValue}, Double: {doubleValue:N3}, Decimal: {decimalValue:C}");

        // Assert
        var log = Assert.Single(logger.Logs);
        Assert.Contains("Int: 42", log.Message);
        Assert.Contains("Double: 3.142", log.Message);
        Assert.Contains("Decimal: ¤19.99", log.Message);
    }
}