using static GroveGames.Logger.LoggerExtensions;

namespace GroveGames.Logger.Tests;

public sealed class LoggerFactoryTests : IDisposable
{
    [Fact]
    public void CreateLogger_WithValidConfiguration_ShouldReturnConfiguredLogger()
    {
        // Arrange
        var processor = new TestLogProcessor();

        // Act
        var logger = LoggerFactory.CreateLogger(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Information)
                   .AddLogProcessor(processor);
        });

        // Assert
        Assert.NotNull(logger);
        logger.Log(LogLevel.Debug, "Test", "Debug message");
        logger.Log(LogLevel.Information, "Test", "Info message");
        Assert.Single(processor.ProcessedLogs);
        Assert.Equal(LogLevel.Information, processor.ProcessedLogs[0].Level);
        Assert.Equal("Info message", processor.ProcessedLogs[0].Message);
    }

    [Fact]
    public void CreateLogger_WithEmptyConfiguration_ShouldReturnLoggerWithDefaults()
    {
        // Act
        var logger = LoggerFactory.CreateLogger(builder => { });

        // Assert
        Assert.NotNull(logger);
        logger.Log(LogLevel.Information, "Test", "Test message");
    }

    [Fact]
    public void CreateLogger_WithMultipleProcessors_ShouldConfigureAllProcessors()
    {
        // Arrange
        var processor1 = new TestLogProcessor();
        var processor2 = new TestLogProcessor();
        var processor3 = new TestLogProcessor();

        // Act
        var logger = LoggerFactory.CreateLogger(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Warning)
                   .AddLogProcessor(processor1)
                   .AddLogProcessor(processor2)
                   .AddLogProcessor(processor3);
        });

        // Assert
        logger.Log(LogLevel.Warning, "Test", "Warning message");
        Assert.Single(processor1.ProcessedLogs);
        Assert.Single(processor2.ProcessedLogs);
        Assert.Single(processor3.ProcessedLogs);

        foreach (var processor in new[] { processor1, processor2, processor3 })
        {
            Assert.Equal(LogLevel.Warning, processor.ProcessedLogs[0].Level);
            Assert.Equal("Warning message", processor.ProcessedLogs[0].Message);
        }
    }

    [Fact]
    public void CreateLogger_WithComplexConfiguration_ShouldApplyAllSettings()
    {
        // Arrange
        var debugProcessor = new TestLogProcessor();
        var errorProcessor = new TestLogProcessor();

        // Act
        var logger = LoggerFactory.CreateLogger(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug)
                   .AddLogProcessor(debugProcessor)
                   .SetMinimumLevel(LogLevel.Information)
                   .AddLogProcessor(errorProcessor);
        });

        // Assert
        logger.Log(LogLevel.Debug, "Test", "Debug message");
        logger.Log(LogLevel.Information, "Test", "Info message");
        logger.Log(LogLevel.Error, "Test", "Error message");
        Assert.Equal(2, debugProcessor.ProcessedLogs.Count);
        Assert.Equal(2, errorProcessor.ProcessedLogs.Count);
        Assert.Equal(LogLevel.Information, debugProcessor.ProcessedLogs[0].Level);
        Assert.Equal(LogLevel.Error, debugProcessor.ProcessedLogs[1].Level);
        Assert.Equal(LogLevel.Information, errorProcessor.ProcessedLogs[0].Level);
        Assert.Equal(LogLevel.Error, errorProcessor.ProcessedLogs[1].Level);
    }

    [Fact]
    public void CreateLogger_CalledMultipleTimes_ShouldReturnIndependentLoggers()
    {
        // Arrange
        var processor1 = new TestLogProcessor();
        var processor2 = new TestLogProcessor();

        // Act
        var logger1 = LoggerFactory.CreateLogger(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug)
                   .AddLogProcessor(processor1);
        });

        var logger2 = LoggerFactory.CreateLogger(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Error)
                   .AddLogProcessor(processor2);
        });

        // Assert
        Assert.NotSame(logger1, logger2);
        logger1.LogDebug("Test1", $"Debug message");
        logger2.LogDebug("Test2", $"Debug message");
        logger2.LogError("Test2", $"Error message");
        Assert.Single(processor1.ProcessedLogs);
        Assert.Equal("Debug message", processor1.ProcessedLogs[0].Message);
        Assert.Single(processor2.ProcessedLogs);
        Assert.Equal("Error message", processor2.ProcessedLogs[0].Message);
    }

    [Fact]
    public void CreateLogger_WithConfigurationThatThrows_ShouldPropagateException()
    {
        // Act & Assert
        var exception = Assert.Throws<InvalidOperationException>(() =>
            LoggerFactory.CreateLogger(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Information);
                throw new InvalidOperationException("Configuration error");
            }));

        Assert.Equal("Configuration error", exception.Message);
    }

    [Fact]
    public void CreateLogger_WithBuilderModifications_ShouldCreateNewBuilderEachTime()
    {
        // Arrange
        var processor1 = new TestLogProcessor();
        var processor2 = new TestLogProcessor();

        // Act
        var logger1 = LoggerFactory.CreateLogger(builder =>
        {
            builder.AddLogProcessor(processor1);
        });

        var logger2 = LoggerFactory.CreateLogger(builder =>
        {
            builder.AddLogProcessor(processor2);
        });

        // Assert
        logger1.LogInformation("Test", $"Message1");
        logger2.LogInformation("Test", $"Message2");
        Assert.Single(processor1.ProcessedLogs);
        Assert.Equal("Message1", processor1.ProcessedLogs[0].Message);
        Assert.Single(processor2.ProcessedLogs);
        Assert.Equal("Message2", processor2.ProcessedLogs[0].Message);
    }

    [Fact]
    public void CreateLogger_WithFluentConfiguration_ShouldSupportMethodChaining()
    {
        // Arrange
        var processor = new TestLogProcessor();

        // Act
        var logger = LoggerFactory.CreateLogger(builder =>
            builder.SetMinimumLevel(LogLevel.Warning)
                   .AddLogProcessor(processor)
                   .SetMinimumLevel(LogLevel.Information));

        // Assert
        logger.Log(LogLevel.Information, "Test", "Info message");
        logger.Log(LogLevel.Debug, "Test", "Debug message");
        Assert.Single(processor.ProcessedLogs);
        Assert.Equal(LogLevel.Information, processor.ProcessedLogs[0].Level);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void CreateLogger_WithDifferentMinimumLevels_ShouldRespectEachLevel(LogLevel minimumLevel)
    {
        // Arrange
        var processor = new TestLogProcessor();

        // Act
        var logger = LoggerFactory.CreateLogger(builder =>
        {
            builder.SetMinimumLevel(minimumLevel)
                   .AddLogProcessor(processor);
        });

        logger.LogDebug("Test", $"Debug");
        logger.LogInformation("Test", $"Info");
        logger.LogWarning("Test", $"Warning");
        logger.LogError("Test", $"Error");

        // Assert
        var expectedCount = minimumLevel switch
        {
            LogLevel.Debug => 4,
            LogLevel.Information => 3,
            LogLevel.Warning => 2,
            LogLevel.Error => 1,
            _ => 0
        };

        Assert.Equal(expectedCount, processor.ProcessedLogs.Count);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private sealed class TestLogProcessor : ILogProcessor
    {
        public List<(LogLevel Level, string Tag, string Message)> ProcessedLogs { get; } = [];

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            ProcessedLogs.Add((level, tag.ToString(), message.ToString()));
        }

        public void Dispose()
        {
        }
    }
}