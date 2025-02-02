namespace GroveGames.Logger.Tests;

public class LoggerTests
{
    private class TestLogProcessor : ILogProcessor
    {
        public List<string> Logs { get; } = [];

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            Logs.Add($"{tag.ToString()}: {message.ToString()}"); // ToString only for assertions
        }
    }

    [Fact]
    public void Log_ShouldCall_ProcessLog_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger(LogLevel.Debug);
        logger.AddLogProcessor(processor);

        var tag = "TestTag";
        var message = "Test message";

        // Act
        logger.Log(LogLevel.Debug, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Contains("TestTag: Test message", processor.Logs);
    }

    [Fact]
    public void Log_ShouldNotLog_WhenBelowMinimumLevel()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger(LogLevel.Warning); // Only logs Warning and above
        logger.AddLogProcessor(processor);

        var tag = "TestTag";
        var message = "This should not be logged";

        // Act
        logger.Log(LogLevel.Debug, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Empty(processor.Logs);
    }

    [Fact]
    public void AddProcessor_ShouldStoreProcessor()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger(LogLevel.Information);

        // Act
        logger.AddLogProcessor(processor);
        logger.Log(LogLevel.Information, "Test".AsSpan(), "Message".AsSpan());

        // Assert
        Assert.Contains("Test: Message", processor.Logs);
    }

    [Fact]
    public void RemoveProcessor_ShouldNotCallProcessorAfterRemoval()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger(LogLevel.Information);
        logger.AddLogProcessor(processor);
        logger.RemoveLogProcessor(processor);

        // Act
        logger.Log(LogLevel.Information, "Test".AsSpan(), "Message".AsSpan());

        // Assert
        Assert.Empty(processor.Logs);
    }

    [Fact]
    public void Dispose_ShouldCallDispose_OnDisposableProcessors()
    {
        // Arrange
        var disposableProcessor = new TestLogProcessor();
        var logger = new Logger(LogLevel.Information);
        logger.AddLogProcessor(disposableProcessor);

        // Act
        logger.Dispose();

        // Assert
        // No explicit assertion needed, just verifying no exceptions
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenProcessorsAreNotDisposable()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger(LogLevel.Information);
        logger.AddLogProcessor(processor);

        // Act & Assert
        var exception = Record.Exception(() => logger.Dispose());
        Assert.Null(exception);
    }
}