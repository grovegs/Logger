namespace GroveGames.Logger.Tests;

public sealed class LoggerBuilderTests : IDisposable
{
    [Fact]
    public void Constructor_ShouldInitializeEmptyBuilder()
    {
        // Act
        var builder = new LoggerBuilder();
        var logger = builder.Build();

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void AddLogProcessor_WithValidProcessor_ShouldReturnSameBuilder()
    {
        // Arrange
        var builder = new LoggerBuilder();
        var processor = new TestLogProcessor();

        // Act
        var result = builder.AddLogProcessor(processor);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void AddLogProcessor_WithMultipleProcessors_ShouldChainCorrectly()
    {
        // Arrange
        var builder = new LoggerBuilder();
        var processor1 = new TestLogProcessor();
        var processor2 = new TestLogProcessor();
        var processor3 = new TestLogProcessor();

        // Act
        var result = builder
            .AddLogProcessor(processor1)
            .AddLogProcessor(processor2)
            .AddLogProcessor(processor3);

        // Assert
        Assert.Same(builder, result);
        var logger = builder.Build();
        logger.Log(LogLevel.Information, "Test", "Test message");

        Assert.Single(processor1.ProcessedLogs);
        Assert.Single(processor2.ProcessedLogs);
        Assert.Single(processor3.ProcessedLogs);
    }

    [Fact]
    public void SetMinimumLevel_WithValidLevel_ShouldReturnSameBuilder()
    {
        // Arrange
        var builder = new LoggerBuilder();

        // Act
        var result = builder.SetMinimumLevel(LogLevel.Warning);

        // Assert
        Assert.Same(builder, result);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void SetMinimumLevel_WithDifferentLevels_ShouldConfigureLoggerCorrectly(LogLevel minimumLevel)
    {
        // Arrange
        var builder = new LoggerBuilder();
        var processor = new TestLogProcessor();

        // Act
        builder.SetMinimumLevel(minimumLevel);
        builder.AddLogProcessor(processor);
        var logger = builder.Build();
        logger.Log(LogLevel.Debug, "Test", "Debug message");
        logger.Log(LogLevel.Information, "Test", "Info message");
        logger.Log(LogLevel.Warning, "Test", "Warning message");
        logger.Log(LogLevel.Error, "Test", "Error message");

        // Assert
        var expectedLogs = new List<LogLevel>();
        if (minimumLevel <= LogLevel.Debug) expectedLogs.Add(LogLevel.Debug);
        if (minimumLevel <= LogLevel.Information) expectedLogs.Add(LogLevel.Information);
        if (minimumLevel <= LogLevel.Warning) expectedLogs.Add(LogLevel.Warning);
        if (minimumLevel <= LogLevel.Error) expectedLogs.Add(LogLevel.Error);

        Assert.Equal(expectedLogs.Count, processor.ProcessedLogs.Count);
        for (int i = 0; i < expectedLogs.Count; i++)
        {
            Assert.Equal(expectedLogs[i], processor.ProcessedLogs[i].Level);
        }
    }

    [Fact]
    public void Build_WithNoProcessors_ShouldReturnValidLogger()
    {
        // Arrange
        var builder = new LoggerBuilder();

        // Act
        var logger = builder.Build();

        // Assert
        Assert.NotNull(logger);

        // Should not throw when logging
        logger.Log(LogLevel.Information, "Test", "Test message");
    }

    [Fact]
    public void Build_WithSingleProcessor_ShouldReturnLoggerWithProcessor()
    {
        // Arrange
        var builder = new LoggerBuilder();
        var processor = new TestLogProcessor();

        // Act
        builder.AddLogProcessor(processor);
        var logger = builder.Build();

        // Assert
        logger.Log(LogLevel.Information, "Test", "Test message");
        Assert.Single(processor.ProcessedLogs);
        Assert.Equal(LogLevel.Information, processor.ProcessedLogs[0].Level);
        Assert.Equal("Test", processor.ProcessedLogs[0].Tag);
        Assert.Equal("Test message", processor.ProcessedLogs[0].Message);
    }

    [Fact]
    public void Build_WithMultipleProcessors_ShouldReturnLoggerWithAllProcessors()
    {
        // Arrange
        var builder = new LoggerBuilder();
        var processor1 = new TestLogProcessor();
        var processor2 = new TestLogProcessor();
        var processor3 = new TestLogProcessor();

        // Act
        builder.AddLogProcessor(processor1);
        builder.AddLogProcessor(processor2);
        builder.AddLogProcessor(processor3);
        var logger = builder.Build();

        // Assert
        logger.Log(LogLevel.Warning, "TestTag", "Warning message");

        Assert.Single(processor1.ProcessedLogs);
        Assert.Single(processor2.ProcessedLogs);
        Assert.Single(processor3.ProcessedLogs);

        foreach (var processor in new[] { processor1, processor2, processor3 })
        {
            Assert.Equal(LogLevel.Warning, processor.ProcessedLogs[0].Level);
            Assert.Equal("TestTag", processor.ProcessedLogs[0].Tag);
            Assert.Equal("Warning message", processor.ProcessedLogs[0].Message);
        }
    }

    [Fact]
    public void Build_CalledMultipleTimes_ShouldReturnIndependentLoggers()
    {
        // Arrange
        var builder = new LoggerBuilder();
        var processor = new TestLogProcessor();
        builder.AddLogProcessor(processor);

        // Act
        var logger1 = builder.Build();
        var logger2 = builder.Build();

        // Assert
        Assert.NotSame(logger1, logger2);

        // Test that they work independently
        logger1.LogInformation("Tag1", $"Message1");
        logger2.LogInformation("Tag2", $"Message2");

        Assert.Equal(2, processor.ProcessedLogs.Count);
        Assert.Equal("Message1", processor.ProcessedLogs[0].Message);
        Assert.Equal("Message2", processor.ProcessedLogs[1].Message);
    }

    [Fact]
    public void FluentInterface_ShouldAllowMethodChaining()
    {
        // Arrange
        var processor1 = new TestLogProcessor();
        var processor2 = new TestLogProcessor();

        // Act
        var builder = new LoggerBuilder();
        builder.SetMinimumLevel(LogLevel.Information);
        builder.AddLogProcessor(processor1);
        builder.AddLogProcessor(processor2);
        builder.SetMinimumLevel(LogLevel.Warning);
        var logger = builder.Build();

        // Assert
        logger.Log(LogLevel.Information, "Test", "Info message"); // Should be filtered out
        logger.Log(LogLevel.Warning, "Test", "Warning message"); // Should be processed

        Assert.Single(processor1.ProcessedLogs);
        Assert.Single(processor2.ProcessedLogs);
        Assert.Equal(LogLevel.Warning, processor1.ProcessedLogs[0].Level);
        Assert.Equal(LogLevel.Warning, processor2.ProcessedLogs[0].Level);
    }

    [Fact]
    public void AddLogProcessor_AfterBuild_ShouldNotAffectPreviouslyBuiltLoggers()
    {
        // Arrange
        var builder = new LoggerBuilder();
        var processor1 = new TestLogProcessor();
        var processor2 = new TestLogProcessor();

        // Act
        builder.AddLogProcessor(processor1);
        var logger1 = builder.Build();

        builder.AddLogProcessor(processor2);
        var logger2 = builder.Build();

        logger1.Log(LogLevel.Information, "Test", "Message for logger1");
        logger2.Log(LogLevel.Information, "Test", "Message for logger2");

        // Assert
        Assert.Equal(2, processor1.ProcessedLogs.Count);
        Assert.Equal("Message for logger1", processor1.ProcessedLogs[0].Message);
        Assert.Equal("Message for logger2", processor1.ProcessedLogs[1].Message);

        Assert.Single(processor2.ProcessedLogs);
        Assert.Equal("Message for logger2", processor2.ProcessedLogs[0].Message);
    }

    [Fact]
    public void SetMinimumLevel_DefaultValue_ShouldBeDebug()
    {
        // Arrange
        var builder = new LoggerBuilder();
        var processor = new TestLogProcessor();

        // Act
        builder.AddLogProcessor(processor);
        var logger = builder.Build(); // Don't set minimum level explicitly

        // Assert
        logger.Log(LogLevel.Debug, "Test", "Debug message");
        Assert.Single(processor.ProcessedLogs);
        Assert.Equal(LogLevel.Debug, processor.ProcessedLogs[0].Level);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // Test helper class
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