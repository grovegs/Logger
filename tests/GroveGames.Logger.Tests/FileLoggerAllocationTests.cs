using System.Buffers;
using System.Numerics;

namespace GroveGames.Logger.Tests;

public class FileLoggerAllocationTests
{
    [Fact]
    public void Info_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var testFileWriter = new TestFileWriterAllocation();
        var logger = new FileLogger(testFileWriter);
        logger.Info("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Info("TestTag", $"Test message {initialAllocatedBytes}");

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
        logger.Info("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Warning("TestTag", $"Test message {float.Epsilon}");

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
        logger.Info("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Error("TestTag", $"Test message {int.MaxValue}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }
}
