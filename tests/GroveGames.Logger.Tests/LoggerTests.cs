namespace GroveGames.Logger.Tests;

public class LoggerTests
{
    private class TestLogProcessor : ILogProcessor
    {
        public List<string> Logs { get; } = [];

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            Logs.Add($"{tag.ToString()}: {message.ToString()}");
        }
    }

    [Fact]
    public void Log_ShouldCall_ProcessLog_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger([processor], LogLevel.Debug);

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
        var logger = new Logger([processor], LogLevel.Warning);

        var tag = "TestTag";
        var message = "This should not be logged";

        // Act
        logger.Log(LogLevel.Debug, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Empty(processor.Logs);
    }

    [Fact]
    public void Dispose_ShouldCallDispose_OnDisposableProcessors()
    {
        // Arrange
        var disposableProcessor = new TestLogProcessor();
        var logger = new Logger([disposableProcessor], LogLevel.Information);

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
        var logger = new Logger([processor], LogLevel.Information);

        // Act & Assert
        var exception = Record.Exception(() => logger.Dispose());
        Assert.Null(exception);
    }
}
