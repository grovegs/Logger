using GroveGames.Logger;

using System.IO;
using System.Threading.Tasks;

using Xunit;

public class FileWriterTests
{
    [Fact]
    public async Task AddToQueue_ShouldEnqueueMessage()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        var fileWriter = new FileWriter(streamWriter);

        // Act
        fileWriter.AddToQueue("Hello, World!".AsSpan());
        await Task.Delay(1500); // Allow the background thread to write the log

        // Assert
        memoryStream.Seek(0, SeekOrigin.Begin); // Reset the stream position
        using var reader = new StreamReader(memoryStream);
        var logContents = await reader.ReadToEndAsync();
        Assert.Contains("Hello, World!", logContents);

        fileWriter.Dispose();
    }

    [Fact]
    public async Task Dispose_ShouldStopWritingThread()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream);
        var fileWriter = new FileWriter(streamWriter);

        // İlk mesajı ekleyip işlenmesini bekleyelim
        fileWriter.AddToQueue("First message".AsSpan());
        await Task.Delay(1500);

        memoryStream.Seek(0, SeekOrigin.Begin); // Stream'i başa sar
        using var reader = new StreamReader(memoryStream);
        var logContents = await reader.ReadToEndAsync();

        Assert.Contains("First message", logContents);

        // Dispose çağrısı yapıldıktan sonra log eklemeye çalışalım
        fileWriter.Dispose();
        fileWriter.AddToQueue("Second message".AsSpan());
        Assert.DoesNotContain("Second message", logContents);
    }


}
