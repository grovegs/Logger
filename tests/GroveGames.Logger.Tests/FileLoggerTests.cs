using GroveGames.Logger;

using System;
using System.IO;
using System.Threading.Tasks;

using Xunit;

public class FileLoggerTests
{

    [Fact]
    public async Task Info_ShouldWriteMessageToLogFile()
    {
        // Arrange
        using var streamWriter = new StreamWriter("test_info.txt", append: false);
        var logger = new FileLogger(streamWriter);

        // Act
        logger.Info("TestTag".AsSpan(), "Test message".AsSpan());
        await Task.Delay(2500); // Yazma döngüsünü tamamlaması için bekle

        // Assert
        logger.Dispose();
        var logContents = File.ReadAllText("test_info.txt");
        Assert.Contains("INFO | TestTag | Test message", logContents);
    }

    [Fact]
    public async Task Warning_ShouldWriteMessageToLogFile()
    {
        // Arrange
        using var streamWriter = new StreamWriter("test_warning.txt", append: false);
        var logger = new FileLogger(streamWriter);

        // Act
        logger.Warning("TestTag".AsSpan(), "Test warning".AsSpan());
        await Task.Delay(2500); // Yazma döngüsünü tamamlaması için bekle

        // Assert
        logger.Dispose();
        var logContents = File.ReadAllText("test_warning.txt");
        Assert.Contains("WARNING | TestTag | Test warning", logContents);
    }

    [Fact]
    public async Task Error_ShouldWriteMessageToLogFile()
    {
        // Arrange
        using var streamWriter = new StreamWriter("test_error.txt", append: false);
        var logger = new FileLogger(streamWriter);

        // Act
        logger.Error("TestTag".AsSpan(), "Test error".AsSpan());
        await Task.Delay(2500); // Yazma döngüsünü tamamlaması için bekle

        // Assert
        logger.Dispose();
        var logContents = File.ReadAllText("test_error.txt");
        Assert.Contains("ERROR | TestTag | Test error", logContents);
    }
}
