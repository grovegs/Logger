using System.Diagnostics;
using System.Text;

namespace GroveGames.Logger.Tests;

public sealed class StreamWriterTests
{
    [Fact]
    public void Constructor_ThrowsArgumentNullException_WhenStreamIsNull()
    {
        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new StreamWriter(null!, 1024, 100));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenBufferSizeIsInvalid(int bufferSize)
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWriter(stream, bufferSize, 100));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ThrowsArgumentOutOfRangeException_WhenChannelCapacityIsInvalid(int channelCapacity)
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act & Assert
        Assert.Throws<ArgumentOutOfRangeException>(() => new StreamWriter(stream, 1024, channelCapacity));
    }

    [Fact]
    public void AddEntry_WritesSimpleEntryToStream()
    {
        // Arrange
        using var stream = new MemoryStream();
        const string testEntry = "Test log entry";

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            writer.AddEntry(testEntry);
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal($"{testEntry}{Environment.NewLine}", result);
    }

    [Fact]
    public void AddEntry_WritesMultipleEntriesToStream()
    {
        // Arrange
        using var stream = new MemoryStream();
        var entries = new[] { "Entry1", "Entry2", "Entry3" };

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            foreach (var entry in entries)
            {
                writer.AddEntry(entry);
            }
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        var expected = string.Join(Environment.NewLine, entries) + Environment.NewLine;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AddEntry_HandlesLargeEntryExceedingBufferSize()
    {
        // Arrange
        using var stream = new MemoryStream();
        var largeEntry = new string('X', 100);

        // Act
        using (var writer = new StreamWriter(stream, 50, 10))
        {
            writer.AddEntry(largeEntry);
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal($"{largeEntry}{Environment.NewLine}", result);
    }

    [Fact]
    public void AddEntry_HandlesUnicodeCharacters()
    {
        // Arrange
        using var stream = new MemoryStream();
        const string unicodeEntry = "Hello 世界 🌍 Привет";

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            writer.AddEntry(unicodeEntry);
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal($"{unicodeEntry}{Environment.NewLine}", result);
    }

    [Fact]
    public void AddEntry_BatchesMultipleSmallEntries()
    {
        // Arrange
        using var stream = new MemoryStream();
        var entries = Enumerable.Range(1, 10).Select(i => $"Entry{i}").ToArray();

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            foreach (var entry in entries)
            {
                writer.AddEntry(entry);
            }
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        var expected = string.Join(Environment.NewLine, entries) + Environment.NewLine;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AddEntry_WhenDisposed_DoesNothing()
    {
        using var stream = new MemoryStream();
        var writer = new StreamWriter(stream, 1024, 100);
        writer.Dispose();

        var exception = Record.Exception(() => writer.AddEntry("Test"));

        Assert.Null(exception);
    }

    [Fact]
    public void Flush_WhenDisposed_DoesNothing()
    {
        using var stream = new MemoryStream();
        var writer = new StreamWriter(stream, 1024, 100);
        writer.Dispose();

        var exception = Record.Exception(writer.Flush);

        Assert.Null(exception);
    }

    [Fact]
    public void Flush_DoesNotThrowWhenCalled()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, 1024, 100);

        // Act & Assert
        writer.Flush();
        writer.AddEntry("Test");
        writer.Flush();
    }

    [Fact]
    public void Dispose_CanBeCalledMultipleTimes()
    {
        // Arrange
        using var stream = new MemoryStream();
        var writer = new StreamWriter(stream, 1024, 100);

        // Act & Assert
        writer.Dispose();
        writer.Dispose();
    }

    [Fact]
    public void AddEntry_HandlesChannelCapacityLimit()
    {
        // Arrange
        using var stream = new MemoryStream();
        var entries = Enumerable.Range(1, 100).Select(i => $"Entry{i}").ToArray();

        // Act
        using (var writer = new StreamWriter(stream, 1024, 10))
        {
            foreach (var entry in entries)
            {
                writer.AddEntry(entry);
            }
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        Assert.NotEmpty(result);
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.True(lines.Length <= entries.Length);
    }

    [Fact]
    public void AddEntry_HandlesEmptyEntries()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            writer.AddEntry("");
            writer.AddEntry("NonEmpty");
            writer.AddEntry("");
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        var expected = $"{Environment.NewLine}NonEmpty{Environment.NewLine}{Environment.NewLine}";
        Assert.Equal(expected, result);
    }

    [Fact]
    public async Task AddEntry_HandlesConcurrentWrites()
    {
        // Arrange
        using var stream = new MemoryStream();
        const int threadCount = 10;
        const int entriesPerThread = 100;

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            var tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
            {
                for (var j = 0; j < entriesPerThread; j++)
                {
                    writer.AddEntry($"Thread{threadId}-Entry{j}");
                }
            })).ToArray();

            await Task.WhenAll(tasks);
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.NotEmpty(lines);
        foreach (var line in lines)
        {
            Assert.Matches(@"Thread\d+-Entry\d+", line);
        }
    }

    [Fact]
    public void BatchBuffer_FlushesBatchWhenThreeQuartersFull()
    {
        // Arrange
        using var stream = new MemoryStream();
        var entry = new string('X', 20);

        // Act
        using (var writer = new StreamWriter(stream, 100, 50))
        {
            for (var i = 0; i < 4; i++)
            {
                writer.AddEntry(entry);
            }
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, lines.Length);
    }

    [Fact]
    public void Dispose_EnsuresAllEntriesAreWritten()
    {
        // Arrange
        using var stream = new MemoryStream();
        var expectedEntries = 10;

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            for (var i = 0; i < expectedEntries; i++)
            {
                writer.AddEntry($"Entry{i}");
            }
        }

        // Assert
        var result = Encoding.UTF8.GetString(stream.ToArray());
        var lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(expectedEntries, lines.Length);
    }

    [Fact]
    public void Flush_QueuedEntries_WritesThemBeforeReturning()
    {
        // Arrange
        using var stream = new MemoryStream();
        using var writer = new StreamWriter(stream, 1024, 100);
        writer.AddEntry("first");
        writer.AddEntry("second");

        // Act
        writer.Flush();

        // Assert
        Assert.Equal($"first{Environment.NewLine}second{Environment.NewLine}", Encoding.UTF8.GetString(stream.ToArray()));
    }

    [Fact]
    public void Dispose_StreamFailsToWrite_DoesNotThrow()
    {
        // Arrange
        var writer = new StreamWriter(new FakeFailingStream(), 1024, 100);
        writer.AddEntry("entry");
        writer.Flush();

        // Act
        var exception = Record.Exception(writer.Dispose);

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void AddEntry_AfterStreamFailed_DoesNotThrow()
    {
        // Arrange
        using var writer = new StreamWriter(new FakeFailingStream(), 1024, 100);
        writer.AddEntry("entry");
        writer.Flush();

        // Act
        var exception = Record.Exception(() => writer.AddEntry("later"));

        // Assert
        Assert.Null(exception);
    }

    [Fact]
    public void AddEntry_WriterBusyWithinCapacity_KeepsEveryEntry()
    {
        // Arrange
        var stream = new FakeGatedStream();
        using var writer = new StreamWriter(stream, 64, 100);

        // Act
        for (var i = 0; i < 50; i++)
        {
            writer.AddEntry($"entry {i}");
        }

        stream.Open();
        writer.Flush();

        // Assert
        Assert.Equal(50, stream.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length);
    }

    [Fact]
    public void AddEntry_WriterBusyBeyondCapacity_DropsTheRest()
    {
        // Arrange
        var stream = new FakeGatedStream();
        using var writer = new StreamWriter(stream, 64, 5);

        // Act
        for (var i = 0; i < 50; i++)
        {
            writer.AddEntry($"entry {i}");
        }

        stream.Open();
        writer.Flush();

        // Assert
        Assert.InRange(stream.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length, 5, 14);
    }

    [Fact]
    public void AddEntry_WithoutFlush_IsWrittenSoon()
    {
        // Arrange
        var stream = new FakeGatedStream();
        stream.Open();
        using var writer = new StreamWriter(stream, 1024, 100);
        var stopwatch = Stopwatch.StartNew();

        // Act
        writer.AddEntry("idle");

        // Assert
        while (stream.Text.Length == 0 && stopwatch.ElapsedMilliseconds < 2000)
        {
            Thread.Sleep(10);
        }

        Assert.Equal($"idle{Environment.NewLine}", stream.Text);
    }

    [Fact]
    public void AddEntry_AfterWarmup_DoesNotAllocate()
    {
        // Arrange
        using var writer = new StreamWriter(Stream.Null, 1024, 100);
        for (var i = 0; i < 10_000; i++)
        {
            writer.AddEntry("[INF] [Gameplay] Player collected 12 coins");
        }

        writer.Flush();
        var before = GC.GetAllocatedBytesForCurrentThread();

        // Act
        for (var i = 0; i < 10_000; i++)
        {
            writer.AddEntry("[INF] [Gameplay] Player collected 12 coins");
        }

        // Assert
        // A buffer may still double once if the writer thread runs late, so allow that bounded growth, which is far below one allocation per entry.
        Assert.InRange(GC.GetAllocatedBytesForCurrentThread() - before, 0, 32 * 1024);
    }

    [Fact]
    public void Flush_WriterStuck_ReturnsAfterTheTimeout()
    {
        // Arrange
        var stream = new FakeGatedStream();
        using var writer = new StreamWriter(stream, 1024, 100);
        writer.AddEntry("stuck");
        var stopwatch = Stopwatch.StartNew();

        // Act
        writer.Flush();

        // Assert
        stopwatch.Stop();
        stream.Open();
        Assert.InRange(stopwatch.ElapsedMilliseconds, 900, 5000);
    }

    [Fact]
    public void AddEntry_EntryManyTimesTheBufferSize_WritesItWhole()
    {
        // Arrange
        using var stream = new MemoryStream();
        var entry = new string('x', 100);

        // Act
        using (var writer = new StreamWriter(stream, 16, 100))
        {
            writer.AddEntry(entry);
        }

        // Assert
        Assert.Equal($"{entry}{Environment.NewLine}", Encoding.UTF8.GetString(stream.ToArray()));
    }

    [Fact]
    public void AddEntry_InvalidSurrogate_WritesReplacementCharacter()
    {
        // Arrange
        using var stream = new MemoryStream();

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            writer.AddEntry("a\uD800b");
        }

        // Assert
        Assert.Equal($"a\uFFFDb{Environment.NewLine}", Encoding.UTF8.GetString(stream.ToArray()));
    }

    [Fact]
    public void Flush_AnotherThreadKeepsLogging_ReturnsWithoutWaitingForTheTimeout()
    {
        // Arrange
        var stream = new FakeSlowStream();
        using var writer = new StreamWriter(stream, 1024, int.MaxValue);
        using var stop = new CancellationTokenSource();
        var logger = new Thread(() =>
        {
            while (!stop.IsCancellationRequested)
            {
                writer.AddEntry("background");
                Thread.Sleep(1);
            }
        });
        logger.Start();
        Thread.Sleep(50);
        var slowest = 0L;
        var missing = 0;

        // Act
        for (var i = 0; i < 10; i++)
        {
            var mine = $"mine {i}{Environment.NewLine}";
            writer.AddEntry($"mine {i}");
            var stopwatch = Stopwatch.StartNew();
            writer.Flush();
            slowest = Math.Max(slowest, stopwatch.ElapsedMilliseconds);
            missing += stream.Text.Contains(mine, StringComparison.Ordinal) ? 0 : 1;
        }

        // Assert
        stop.Cancel();
        logger.Join();
        Assert.Equal(0, missing);
        Assert.True(slowest < 500, $"The slowest Flush took {slowest} ms");
    }

    [Fact]
    public void Dispose_WriterBusy_WritesEveryAcceptedEntry()
    {
        // Arrange
        var stream = new FakeGatedStream();
        var writer = new StreamWriter(stream, 64, 100);
        for (var i = 0; i < 20; i++)
        {
            writer.AddEntry($"entry {i}");
        }

        var opener = new Thread(() =>
        {
            Thread.Sleep(50);
            stream.Open();
        });
        opener.Start();

        // Act
        writer.Dispose();

        // Assert
        opener.Join();
        Assert.Equal(20, stream.Text.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length);
    }

    private sealed class FakeSlowStream : Stream
    {
        private readonly MemoryStream _inner;

        public FakeSlowStream()
        {
            _inner = new MemoryStream();
        }

        public string Text
        {
            get
            {
                lock (_inner)
                {
                    return Encoding.UTF8.GetString(_inner.ToArray());
                }
            }
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get => 0; set { } }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            Thread.Sleep(5);

            lock (_inner)
            {
                _inner.Write(buffer, offset, count);
            }
        }
    }

    private sealed class FakeFailingStream : Stream
    {
        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get => 0; set { } }

        public override void Flush()
        {
            throw new IOException("Disk full");
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new IOException("Disk full");
        }
    }

    [Fact]
    public void Flush_AfterQueueWasFull_ReturnsOnceLaterEntriesAreWritten()
    {
        // Arrange
        var stream = new FakeGatedStream();
        using var writer = new StreamWriter(stream, 1024, 1);
        for (var i = 0; i < 10; i++)
        {
            writer.AddEntry("dropped or written");
        }

        stream.Open();
        writer.Flush();
        writer.AddEntry("last");
        var stopwatch = Stopwatch.StartNew();

        // Act
        writer.Flush();

        // Assert
        stopwatch.Stop();
        Assert.True(stopwatch.ElapsedMilliseconds < 500, $"Flush took {stopwatch.ElapsedMilliseconds} ms");
        Assert.EndsWith($"last{Environment.NewLine}", stream.Text);
    }

    private sealed class FakeGatedStream : Stream
    {
        private readonly ManualResetEventSlim _gate;
        private readonly MemoryStream _inner;

        public FakeGatedStream()
        {
            _gate = new ManualResetEventSlim(false);
            _inner = new MemoryStream();
        }

        public string Text
        {
            get
            {
                lock (_inner)
                {
                    return Encoding.UTF8.GetString(_inner.ToArray());
                }
            }
        }

        public override bool CanRead => false;
        public override bool CanSeek => false;
        public override bool CanWrite => true;
        public override long Length => 0;
        public override long Position { get => 0; set { } }

        public void Open()
        {
            _gate.Set();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _gate.Dispose();
                _inner.Dispose();
            }

            base.Dispose(disposing);
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            _gate.Wait();

            lock (_inner)
            {
                _inner.Write(buffer, offset, count);
            }
        }
    }
}
