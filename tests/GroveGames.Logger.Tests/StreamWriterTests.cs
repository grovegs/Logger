using System.Text;

namespace GroveGames.Logger.Tests;

public sealed class StreamWriterTests : IDisposable
{
    private sealed class TestMemoryStream : MemoryStream
    {
        public int WriteCount { get; private set; }
        public int FlushCount { get; private set; }

        public override void Write(byte[] buffer, int offset, int count)
        {
            WriteCount++;
            base.Write(buffer, offset, count);
        }

        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            WriteCount++;
            return base.WriteAsync(buffer, offset, count, cancellationToken);
        }

        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default)
        {
            WriteCount++;
            return base.WriteAsync(buffer, cancellationToken);
        }

        public override void Flush()
        {
            FlushCount++;
            base.Flush();
        }
    }

    private const int DefaultBufferSize = 1024;
    private const int DefaultChannelCapacity = 100;

    [Fact]
    public void AddEntry_ShouldWriteToStream()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);
        const string entry = "TestEntry";

        // Act
        streamWriter.AddEntry(entry.AsSpan());

        // Dispose immediately to ensure all pending writes complete
        streamWriter.Dispose();

        // Read the content after disposal
        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);

        // Assert
        Assert.Contains(entry, writtenText);
        Assert.EndsWith(Environment.NewLine, writtenText);
    }

    [Fact]
    public void AddEntry_AfterDispose_ShouldThrowException()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);
        streamWriter.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => streamWriter.AddEntry("Test".AsSpan()));
    }

    [Fact]
    public void Flush_AfterDispose_ShouldThrowException()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);
        streamWriter.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => streamWriter.Flush());
    }

    [Fact]
    public void Dispose_ShouldCompleteAllPendingWrites()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);

        // Add multiple entries
        ReadOnlySpan<string> entries = ["Entry0", "Entry1", "Entry2", "Entry3", "Entry4",
                                        "Entry5", "Entry6", "Entry7", "Entry8", "Entry9"];

        foreach (var entry in entries)
        {
            streamWriter.AddEntry(entry.AsSpan());
        }

        // Act - Dispose immediately (this will wait for all writes to complete)
        streamWriter.Dispose();

        // Assert - All entries should be written
        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);

        foreach (var entry in entries)
        {
            Assert.Contains(entry, writtenText);
        }
    }

    [Fact]
    public void Constructor_WithInvalidParameters_ShouldThrowException()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StreamWriter(null!, DefaultBufferSize, DefaultChannelCapacity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWriter(new MemoryStream(), 0, DefaultChannelCapacity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWriter(new MemoryStream(), -1, DefaultChannelCapacity));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWriter(new MemoryStream(), DefaultBufferSize, 0));
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWriter(new MemoryStream(), DefaultBufferSize, -1));
    }

    [Fact]
    public async Task ShouldBatchSmallWrites()
    {
        // Arrange
        using var memoryStream = new TestMemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, 4096, DefaultChannelCapacity);

        // Write enough small entries to trigger batching
        const int messageCount = 100;
        for (int i = 0; i < messageCount; i++)
        {
            streamWriter.AddEntry($"Small message {i}".AsSpan());
        }

        // Allow time for async processing
        await Task.Delay(500);

        // Dispose to ensure all pending writes complete
        streamWriter.Dispose();

        // Assert
        Assert.True(memoryStream.WriteCount < messageCount,
            $"Expected fewer than {messageCount} writes due to batching, but got {memoryStream.WriteCount}");
        Assert.True(memoryStream.WriteCount > 0, "Expected at least one write");
    }

    [Fact]
    public async Task ShouldHandleLargeEntries()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, 1024, DefaultChannelCapacity);

        // Create an entry larger than the buffer size
        var largeEntry = new string('X', 2048);

        // Act
        streamWriter.AddEntry(largeEntry.AsSpan());

        // Allow time for async processing
        await Task.Delay(100);

        // Dispose to ensure all pending writes complete
        streamWriter.Dispose();

        // Read the content after disposal
        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);

        // Assert
        Assert.Contains(largeEntry, writtenText);
    }

    [Fact]
    public async Task ShouldHandleChannelCapacityWithDropNewest()
    {
        // Arrange
        const int channelCapacity = 5;
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, channelCapacity);

        // Block the processing to fill up the channel
        var largeEntry = new string('X', DefaultBufferSize * 2);
        streamWriter.AddEntry(largeEntry.AsSpan());

        // Try to add more entries than channel capacity
        for (int i = 0; i < channelCapacity + 5; i++)
        {
            streamWriter.AddEntry($"Entry{i}".AsSpan());
        }

        // Allow processing to catch up
        await Task.Delay(500);

        // Dispose to ensure all pending writes complete
        streamWriter.Dispose();

        // Read the content after disposal
        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);

        // Should contain the large entry
        Assert.Contains(largeEntry, writtenText);

        // Should have dropped some entries due to channel capacity
        var lines = writtenText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length <= channelCapacity + 2); // +1 for large entry, +1 for possible in-flight
    }

    [Fact]
    public void Flush_ShouldForceStreamFlush()
    {
        // Arrange
        using var memoryStream = new TestMemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);

        // Act
        streamWriter.Flush();

        // Assert
        Assert.True(memoryStream.FlushCount > 0);
    }

    [Fact]
    public async Task ShouldHandleConcurrentWrites()
    {
        // Arrange
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);
        const int threadCount = 10;
        const int entriesPerThread = 100;
        var barrier = new Barrier(threadCount);

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
        {
            barrier.SignalAndWait();
            for (int i = 0; i < entriesPerThread; i++)
            {
                streamWriter.AddEntry($"Thread{threadId}_Entry{i}".AsSpan());
            }
        })).ToArray();

        await Task.WhenAll(tasks);

        // Allow time for async processing
        await Task.Delay(500);

        // Dispose the stream writer (this will wait for all writes to complete)
        streamWriter.Dispose();

        // Assert
        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);
        var lines = writtenText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        // Should have written a reasonable number of entries (some may be dropped due to channel capacity)
        Assert.True(lines.Length > 0, "Should have written at least some entries");

        // Check that entries from multiple threads were written
        var threadsWithEntries = lines
            .Select(line => line.Split('_')[0])
            .Where(threadPrefix => threadPrefix.StartsWith("Thread"))
            .Distinct()
            .Count();

        Assert.True(threadsWithEntries > 1, $"Expected entries from multiple threads, but only found {threadsWithEntries}");
    }

    [Fact]
    public async Task ShouldTriggerBatchWriteAt75PercentCapacity()
    {
        // Arrange
        const int bufferSize = 100;
        using var memoryStream = new TestMemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, bufferSize, DefaultChannelCapacity);

        // Calculate entry size to fill exactly 75% of buffer
        const string baseEntry = "Entry";
        var newLineBytes = Encoding.UTF8.GetByteCount(Environment.NewLine);
        var entryBytes = Encoding.UTF8.GetByteCount(baseEntry);
        var totalEntrySize = entryBytes + newLineBytes;
        var entriesFor75Percent = (bufferSize * 3 / 4) / totalEntrySize;

        // Act - Add entries to reach 75% capacity
        for (int i = 0; i < entriesFor75Percent + 1; i++)
        {
            streamWriter.AddEntry(baseEntry.AsSpan());
        }

        // Allow time for async processing
        await Task.Delay(200);

        // Assert - Should have triggered at least one write
        Assert.True(memoryStream.WriteCount > 0, "Expected write to be triggered at 75% capacity");
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);

        // Act & Assert - Multiple disposes should not throw
        streamWriter.Dispose();
        streamWriter.Dispose();
        streamWriter.Dispose();
    }

    public void Dispose()
    {
        // Cleanup if needed
        GC.SuppressFinalize(this);
    }
}