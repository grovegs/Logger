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

        public override async Task FlushAsync(CancellationToken cancellationToken)
        {
            FlushCount++;
            await base.FlushAsync(cancellationToken);
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
        streamWriter.Dispose();

        // Assert
        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);
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
        ReadOnlySpan<string> entries = ["Entry0", "Entry1", "Entry2", "Entry3", "Entry4",
                                        "Entry5", "Entry6", "Entry7", "Entry8", "Entry9"];

        foreach (var entry in entries)
        {
            streamWriter.AddEntry(entry.AsSpan());
        }

        // Act
        streamWriter.Dispose();

        // Assert
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
        const int messageCount = 100;

        // Act
        for (int i = 0; i < messageCount; i++)
        {
            streamWriter.AddEntry($"Small message {i}".AsSpan());
        }

        await Task.Delay(500);
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
        var largeEntry = new string('X', 2048);

        // Act
        streamWriter.AddEntry(largeEntry.AsSpan());
        await Task.Delay(100);
        streamWriter.Dispose();

        // Assert
        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);
        Assert.Contains(largeEntry, writtenText);
    }

    [Fact]
    public void AddEntry_WithFullChannel_ShouldNotBlock_DueToDropNewest()
    {
        // Arrange
        const int channelCapacity = 5;
        var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, channelCapacity);

        // Act
        const int entriesToAdd = 1000;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        for (int i = 0; i < entriesToAdd; i++)
        {
            streamWriter.AddEntry($"Entry{i}".AsSpan());
        }

        stopwatch.Stop();

        // Assert
        Assert.True(stopwatch.ElapsedMilliseconds < 100,
            $"Adding {entriesToAdd} entries took {stopwatch.ElapsedMilliseconds}ms - channel might be blocking");

        streamWriter.Dispose();

        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);
        var lines = writtenText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.True(lines.Length > 0, "Should have written at least some entries");
        Assert.True(lines.Length <= entriesToAdd, "Shouldn't write more entries than were added");
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
        await Task.Delay(500);
        streamWriter.Dispose();

        // Assert
        var writtenBytes = memoryStream.ToArray();
        var writtenText = Encoding.UTF8.GetString(writtenBytes);
        var lines = writtenText.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.True(lines.Length > 0, "Should have written at least some entries");

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
        const string baseEntry = "Entry";
        var newLineBytes = Encoding.UTF8.GetByteCount(Environment.NewLine);
        var entryBytes = Encoding.UTF8.GetByteCount(baseEntry);
        var totalEntrySize = entryBytes + newLineBytes;
        var entriesFor75Percent = (bufferSize * 3 / 4) / totalEntrySize;

        // Act
        for (int i = 0; i < entriesFor75Percent + 1; i++)
        {
            streamWriter.AddEntry(baseEntry.AsSpan());
        }

        await Task.Delay(200);

        // Assert
        Assert.True(memoryStream.WriteCount > 0, "Expected write to be triggered at 75% capacity");
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Arrange
        using var memoryStream = new MemoryStream();
        var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);

        // Act & Assert
        streamWriter.Dispose();
        streamWriter.Dispose();
        streamWriter.Dispose();
    }

    [Fact]
    public async Task ShouldFlushAfterProcessingBatch()
    {
        // Arrange
        using var memoryStream = new TestMemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);

        // Act
        streamWriter.AddEntry("Test entry".AsSpan());
        await Task.Delay(200);

        // Assert
        Assert.True(memoryStream.WriteCount >= 1, $"Expected at least 1 write, got {memoryStream.WriteCount}");
        Assert.True(memoryStream.FlushCount >= 1, $"Expected at least 1 flush, got {memoryStream.FlushCount}");
        Assert.Equal(memoryStream.WriteCount, memoryStream.FlushCount);
    }

    [Fact]
    public async Task ShouldBatchEntriesBeforeFlush()
    {
        // Arrange
        const int bufferSize = 1000;
        using var memoryStream = new TestMemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, bufferSize, DefaultChannelCapacity);
        const int entryCount = 10;

        // Act
        for (int i = 0; i < entryCount; i++)
        {
            streamWriter.AddEntry($"Entry{i}".AsSpan());
        }

        await Task.Delay(200);

        // Assert
        Assert.True(memoryStream.WriteCount < entryCount,
            $"Expected fewer writes than entries due to batching. Writes: {memoryStream.WriteCount}, Entries: {entryCount}");
        Assert.Equal(memoryStream.WriteCount, memoryStream.FlushCount);

        streamWriter.Dispose();
        var writtenText = Encoding.UTF8.GetString(memoryStream.ToArray());
        for (int i = 0; i < entryCount; i++)
        {
            Assert.Contains($"Entry{i}", writtenText);
        }
    }

    [Fact]
    public async Task ShouldProcessEntriesInBatches()
    {
        // Arrange
        using var memoryStream = new TestMemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, DefaultBufferSize, DefaultChannelCapacity);

        // Act
        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0; i < 10; i++)
            {
                streamWriter.AddEntry($"Wave{wave}_Entry{i}".AsSpan());
            }
            await Task.Delay(100);
        }

        streamWriter.Dispose();

        // Assert
        Assert.True(memoryStream.WriteCount >= 3, $"Expected at least 3 writes, got {memoryStream.WriteCount}");
        Assert.True(memoryStream.FlushCount >= 3, $"Expected at least 3 flushes, got {memoryStream.FlushCount}");

        var writtenText = Encoding.UTF8.GetString(memoryStream.ToArray());
        for (int wave = 0; wave < 3; wave++)
        {
            for (int i = 0; i < 10; i++)
            {
                Assert.Contains($"Wave{wave}_Entry{i}", writtenText);
            }
        }
    }

    [Fact]
    public void ShouldBatchAllEntriesAddedSynchronously()
    {
        // Arrange
        using var memoryStream = new TestMemoryStream();
        using var streamWriter = new StreamWriter(memoryStream, 4096, DefaultChannelCapacity);
        const int entryCount = 50;

        // Act
        for (int i = 0; i < entryCount; i++)
        {
            streamWriter.AddEntry($"SyncEntry{i}".AsSpan());
        }

        streamWriter.Dispose();

        // Assert
        Assert.True(memoryStream.WriteCount < entryCount / 2,
            $"Expected significant batching (less than {entryCount / 2} writes for {entryCount} entries), got {memoryStream.WriteCount}");
        Assert.Equal(memoryStream.WriteCount, memoryStream.FlushCount);

        var writtenText = Encoding.UTF8.GetString(memoryStream.ToArray());
        for (int i = 0; i < entryCount; i++)
        {
            Assert.Contains($"SyncEntry{i}", writtenText);
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}