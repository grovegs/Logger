using GroveGames.Logger;

using Xunit;

public class FileLoggerTests
{
    [Fact]
    public void Info_ShouldAddFormattedMessageToQueue()
    {
        // Arrange
        var testFileWriter = new TestFileWriter();
        var logger = new FileLogger(testFileWriter);

        var tag = "TestTag".AsSpan();
        var message = "Test message".AsSpan();

        // Act
        logger.Info(tag, message);

        // Assert
        Assert.Single(testFileWriter.Messages);
        Assert.Contains("INFO | TestTag | Test message", testFileWriter.Messages[0]);
    }

    [Fact]
    public void Warning_ShouldAddFormattedMessageToQueue()
    {
        // Arrange
        var testFileWriter = new TestFileWriter();
        var logger = new FileLogger(testFileWriter);

        var tag = "WarningTag".AsSpan();
        var message = "Warning message".AsSpan();

        // Act
        logger.Warning(tag, message);

        // Assert
        Assert.Single(testFileWriter.Messages);
        Assert.Contains("WARNING | WarningTag | Warning message", testFileWriter.Messages[0]);
    }

    [Fact]
    public void Error_ShouldAddFormattedMessageToQueue()
    {
        // Arrange
        var testFileWriter = new TestFileWriter();
        var logger = new FileLogger(testFileWriter);

        var tag = "ErrorTag".AsSpan();
        var message = "Error message".AsSpan();

        // Act
        logger.Error(tag, message);

        // Assert
        Assert.Single(testFileWriter.Messages);
        Assert.Contains("ERROR | ErrorTag | Error message", testFileWriter.Messages[0]);
    }
}
