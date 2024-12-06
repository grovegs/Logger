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

        var tag = "TestTag".AsSpan();
        var message = "Test message".AsSpan();

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Info(tag, message);

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

        var tag = "ErrorTag".AsSpan();
        var message = "Error message".AsSpan();

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Error(tag, message);

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

        var tag = "WarningTag".AsSpan();
        var message = "Warning message".AsSpan();

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Warning(tag, message);

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }
}
