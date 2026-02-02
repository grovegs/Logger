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
        string result = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal($"{testEntry}{Environment.NewLine}", result);
    }

    [Fact]
    public void AddEntry_WritesMultipleEntriesToStream()
    {
        // Arrange
        using var stream = new MemoryStream();
        string[] entries = new[] { "Entry1", "Entry2", "Entry3" };

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            foreach (string? entry in entries)
            {
                writer.AddEntry(entry);
            }
        }

        // Assert
        string result = Encoding.UTF8.GetString(stream.ToArray());
        string expected = string.Join(Environment.NewLine, entries) + Environment.NewLine;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AddEntry_HandlesLargeEntryExceedingBufferSize()
    {
        // Arrange
        using var stream = new MemoryStream();
        string largeEntry = new string('X', 100);

        // Act
        using (var writer = new StreamWriter(stream, 50, 10))
        {
            writer.AddEntry(largeEntry);
        }

        // Assert
        string result = Encoding.UTF8.GetString(stream.ToArray());
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
        string result = Encoding.UTF8.GetString(stream.ToArray());
        Assert.Equal($"{unicodeEntry}{Environment.NewLine}", result);
    }

    [Fact]
    public void AddEntry_BatchesMultipleSmallEntries()
    {
        // Arrange
        using var stream = new MemoryStream();
        string[] entries = Enumerable.Range(1, 10).Select(i => $"Entry{i}").ToArray();

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            foreach (string? entry in entries)
            {
                writer.AddEntry(entry);
            }
        }

        // Assert
        string result = Encoding.UTF8.GetString(stream.ToArray());
        string expected = string.Join(Environment.NewLine, entries) + Environment.NewLine;
        Assert.Equal(expected, result);
    }

    [Fact]
    public void AddEntry_ThrowsObjectDisposedException_WhenDisposed()
    {
        // Arrange
        using var stream = new MemoryStream();
        var writer = new StreamWriter(stream, 1024, 100);
        writer.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => writer.AddEntry("Test"));
    }

    [Fact]
    public void Flush_ThrowsObjectDisposedException_WhenDisposed()
    {
        // Arrange
        using var stream = new MemoryStream();
        var writer = new StreamWriter(stream, 1024, 100);
        writer.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(writer.Flush);
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
        string[] entries = Enumerable.Range(1, 100).Select(i => $"Entry{i}").ToArray();

        // Act
        using (var writer = new StreamWriter(stream, 1024, 10))
        {
            foreach (string? entry in entries)
            {
                writer.AddEntry(entry);
            }
        }

        // Assert
        string result = Encoding.UTF8.GetString(stream.ToArray());
        Assert.NotEmpty(result);
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
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
        string result = Encoding.UTF8.GetString(stream.ToArray());
        string expected = $"{Environment.NewLine}NonEmpty{Environment.NewLine}{Environment.NewLine}";
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
            Task[] tasks = Enumerable.Range(0, threadCount).Select(threadId => Task.Run(() =>
            {
                for (int j = 0; j < entriesPerThread; j++)
                {
                    writer.AddEntry($"Thread{threadId}-Entry{j}");
                }
            })).ToArray();

            await Task.WhenAll(tasks);
        }

        // Assert
        string result = Encoding.UTF8.GetString(stream.ToArray());
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

        Assert.NotEmpty(lines);
        foreach (string line in lines)
        {
            Assert.Matches(@"Thread\d+-Entry\d+", line);
        }
    }

    [Fact]
    public void BatchBuffer_FlushesBatchWhenThreeQuartersFull()
    {
        // Arrange
        using var stream = new MemoryStream();
        string entry = new string('X', 20);

        // Act
        using (var writer = new StreamWriter(stream, 100, 50))
        {
            for (int i = 0; i < 4; i++)
            {
                writer.AddEntry(entry);
            }
        }

        // Assert
        string result = Encoding.UTF8.GetString(stream.ToArray());
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(4, lines.Length);
    }

    [Fact]
    public void Dispose_EnsuresAllEntriesAreWritten()
    {
        // Arrange
        using var stream = new MemoryStream();
        int expectedEntries = 10;

        // Act
        using (var writer = new StreamWriter(stream, 1024, 100))
        {
            for (int i = 0; i < expectedEntries; i++)
            {
                writer.AddEntry($"Entry{i}");
            }
        }

        // Assert
        string result = Encoding.UTF8.GetString(stream.ToArray());
        string[] lines = result.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        Assert.Equal(expectedEntries, lines.Length);
    }
}
