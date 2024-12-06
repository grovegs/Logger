using GroveGames.Logger;

public class FileLoggerAllocationTests
{
    [Fact]
    public void Info_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var streamWriter = new StreamWriter(Stream.Null);
        var logger = new FileLogger(streamWriter);

        var tag = "TestTag".AsSpan();
        var message = "Test message".AsSpan();

        // Başlangıç tahsisi
        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Info(tag, message);

        // Tahsis sonrası
        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes); // Heap allocation olmamalı
    }

    [Fact]
    public void Error_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var streamWriter = new StreamWriter(Stream.Null);
        var logger = new FileLogger(streamWriter);

        var tag = "ErrorTag".AsSpan();
        var message = "Error message".AsSpan();

        // Başlangıç tahsisi
        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Error(tag, message);

        // Tahsis sonrası
        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes); // Heap allocation olmamalı
    }

    [Fact]
    public void Warning_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var streamWriter = new StreamWriter(Stream.Null);
        var logger = new FileLogger(streamWriter);

        var tag = "WarningTag".AsSpan();
        var message = "Warning message".AsSpan();

        // Başlangıç tahsisi
        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Warning(tag, message);

        // Tahsis sonrası
        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes); // Heap allocation olmamalı
    }
}
