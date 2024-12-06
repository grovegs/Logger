using GroveGames.Logger;

public class FileWriterTests
{
    [Fact]
    public void AddToQueue_ShouldEnqueueMessages()
    {
        // Arrange
        var mockStreamWriter = new Mock<StreamWriter>(Stream.Null);
        var writer = new FileWriter(mockStreamWriter.Object);
        string testMessage = "Hello, World!";

        // Act
        writer.AddToQueue(testMessage.AsSpan());

        // Assert
        // Unfortunately, internal queues are hard to test directly. Focus on behavior:
        mockStreamWriter.Verify(w => w.WriteAsync(It.IsAny<char>()), Times.AtLeastOnce);
    }

    [Fact]
    public void Dispose_ShouldStopWritingThread()
    {
        // Arrange
        var mockStreamWriter = new Mock<StreamWriter>(Stream.Null);
        var writer = new FileWriter(mockStreamWriter.Object);

        // Act
        writer.Dispose();

        // Assert
        mockStreamWriter.Verify(w => w.Close(), Times.Once);
    }
}
