using GroveGames.Logger;

public class FileWriterTests
{
    private const string LogFilePath = "test_log.txt";

    public FileWriterTests()
    {
        if (File.Exists(LogFilePath))
        {
            File.Delete(LogFilePath);
        }
    }

    [Fact]
    public async Task AddToQueue_ShouldEnqueueMessage()
    {
        // Arrange
        using var streamWriter = new StreamWriter(LogFilePath, append: false);
        var fileWriter = new FileWriter(streamWriter);

        // Act
        fileWriter.AddToQueue("Hello, World!".AsSpan());
        await Task.Delay(1500); // Yazma döngüsünün tamamlanmasını bekle

        // Assert
        fileWriter.Dispose();
        var logContents = File.ReadAllText(LogFilePath);
        Assert.Contains("Hello, World!", logContents);
    }

    [Fact]
    public async Task Dispose_ShouldStopWritingThread()
    {
        // Arrange
        using var streamWriter = new StreamWriter(LogFilePath, append: false);
        var fileWriter = new FileWriter(streamWriter);

        // Act
        fileWriter.AddToQueue("This will not be written.".AsSpan());
        fileWriter.Dispose();
        await Task.Delay(1500); // Yazma döngüsü durmuş olmalı

        // Assert
        var logContents = File.ReadAllText(LogFilePath);
        Assert.DoesNotContain("This will not be written.", logContents);
    }
}
