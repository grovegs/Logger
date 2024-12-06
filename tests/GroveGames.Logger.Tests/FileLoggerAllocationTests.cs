using GroveGames.Logger;

using Xunit;

public class FileLoggerAllocationTests
{
    [Fact]
    public void Info_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var testFileWriter = new TestFileWriterAllocation();
        var logger = new FileLogger(testFileWriter);

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Info("TestTag", "Test message");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Error_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var testFileWriter = new TestFileWriterAllocation();
        var logger = new FileLogger(testFileWriter);

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Error("TestTag", "Test message");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Warning_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var testFileWriter = new TestFileWriterAllocation();
        var logger = new FileLogger(testFileWriter);

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Warning("TestTag", "Test message");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }
}
