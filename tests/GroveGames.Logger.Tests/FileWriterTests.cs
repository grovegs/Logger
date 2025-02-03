using System.Text;

namespace GroveGames.Logger.Tests;

public class FileWriterTests
{
    private const int WriteInterval = 10;
    private const int QueueSize = 100;

    [Fact]
    public void AddEntry_ShouldQueueCharacters()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8) { AutoFlush = true };
        using var fileWriter = new FileWriter(streamWriter, WriteInterval, QueueSize);
        var entry = "TestEntry";

        // Act
        fileWriter.AddEntry(entry.AsSpan());
        Thread.Sleep(WriteInterval * 2);
        streamWriter.Flush();
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(memoryStream, Encoding.UTF8);
        var writtenText = reader.ReadToEnd();

        // Assert
        Assert.Contains(entry, writtenText);
    }

    [Fact]
    public void Dispose_ShouldFlushAndDisposeWriter()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8);
        var fileWriter = new FileWriter(streamWriter, WriteInterval, QueueSize);

        // Act
        fileWriter.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => streamWriter.Write("Test"));
    }

    [Fact]
    public void Dispose_ShouldStopWriting()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8) { AutoFlush = true };
        using var fileWriter = new FileWriter(streamWriter, WriteInterval, QueueSize);
        fileWriter.AddEntry("Entry1".AsSpan());
        Thread.Sleep(WriteInterval * 2);

        // Act
        memoryStream.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(memoryStream, Encoding.UTF8);
        var writtenTextBeforeDispose = reader.ReadToEnd();
        fileWriter.Dispose();
        fileWriter.AddEntry("Entry2".AsSpan());
        Thread.Sleep(WriteInterval * 2);

        // Assert
        Assert.Contains("Entry1", writtenTextBeforeDispose);
        Assert.DoesNotContain("Entry2", writtenTextBeforeDispose);
    }
}