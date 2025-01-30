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
        int a = 5;
        var tag = "TestTag".AsSpan();

        // Act
        logger.Info(tag, $"Test message {a}");

        // Assert
        Assert.Single(testFileWriter.Messages);
        Assert.Contains("INFO | TestTag | Test message 5", testFileWriter.Messages[0]);
    }

    [Fact]
    public void Warning_ShouldAddFormattedMessageToQueue()
    {
        // Arrange
        var testFileWriter = new TestFileWriter();
        var logger = new FileLogger(testFileWriter);

        var tag = "WarningTag".AsSpan();

        // Act
        logger.Warning(tag, $"Warning message");

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

        // Act
        logger.Error(tag, $"Error message");

        // Assert
        Assert.Single(testFileWriter.Messages);
        Assert.Contains("ERROR | ErrorTag | Error message", testFileWriter.Messages[0]);
    }
}
