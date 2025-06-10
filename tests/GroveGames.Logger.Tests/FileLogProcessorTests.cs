namespace GroveGames.Logger.Tests;

public sealed class FileLogProcessorTests
{
    private sealed class TestStreamWriter : IStreamWriter
    {
        public List<string> Entries { get; } = [];
        public bool IsDisposed { get; private set; }
        public bool IsFlushed { get; private set; }

        public void AddEntry(ReadOnlySpan<char> entry)
        {
            Entries.Add(entry.ToString().TrimEnd('\0'));
        }

        public void Flush()
        {
            IsFlushed = true;
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    private sealed class TestLogFormatter : ILogFormatter
    {
        public List<(LogLevel level, string tag, string message)> GetBufferSizeCalls { get; } = [];
        public List<(LogLevel level, string tag, string message)> FormatCalls { get; } = [];

        public int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            GetBufferSizeCalls.Add((level, tag.ToString(), message.ToString()));
            var formatted = $"{level}|{tag.ToString()}|{message.ToString()}";
            return formatted.Length;
        }

        public void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            FormatCalls.Add((level, tag.ToString(), message.ToString()));
            var formatted = $"{level}|{tag.ToString()}|{message.ToString()}";
            formatted.AsSpan().CopyTo(buffer[..formatted.Length]);
        }
    }

    [Fact]
    public void Constructor_NullWriter_ThrowsArgumentNullException()
    {
        // Arrange
        var formatter = new TestLogFormatter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileLogProcessor(null!, formatter));
    }

    [Fact]
    public void Constructor_NullFormatter_ThrowsArgumentNullException()
    {
        // Arrange
        var writer = new TestStreamWriter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileLogProcessor(writer, null!));
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();

        // Act
        using var processor = new FileLogProcessor(writer, formatter);

        // Assert
        Assert.NotNull(processor);
    }

    [Fact]
    public void ProcessLog_ValidInput_CallsFormatterGetBufferSize()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Information, "API".AsSpan(), "Request completed".AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Equal((LogLevel.Information, "API", "Request completed"), formatter.GetBufferSizeCalls[0]);
    }

    [Fact]
    public void ProcessLog_ValidInput_CallsFormatterFormat()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Warning, "DB".AsSpan(), "Connection timeout".AsSpan());

        // Assert
        Assert.Single(formatter.FormatCalls);
        Assert.Equal((LogLevel.Warning, "DB", "Connection timeout"), formatter.FormatCalls[0]);
    }

    [Fact]
    public void ProcessLog_ValidInput_CallsWriterAddEntry()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Error, "AUTH".AsSpan(), "Login failed".AsSpan());

        // Assert
        Assert.Single(writer.Entries);
        Assert.Equal("Error|AUTH|Login failed", writer.Entries[0]);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void ProcessLog_DifferentLogLevels_ProcessesCorrectly(LogLevel level)
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(level, "TEST".AsSpan(), "message".AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Single(formatter.FormatCalls);
        Assert.Single(writer.Entries);
        Assert.Equal(level, formatter.GetBufferSizeCalls[0].level);
        Assert.Equal(level, formatter.FormatCalls[0].level);
    }

    [Fact]
    public void ProcessLog_EmptyTag_ProcessesCorrectly()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Information, ReadOnlySpan<char>.Empty, "No tag message".AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Equal((LogLevel.Information, "", "No tag message"), formatter.GetBufferSizeCalls[0]);
        Assert.Single(writer.Entries);
        Assert.Equal("Information||No tag message", writer.Entries[0]);
    }

    [Fact]
    public void ProcessLog_EmptyMessage_ProcessesCorrectly()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Debug, "EMPTY".AsSpan(), ReadOnlySpan<char>.Empty);

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Equal((LogLevel.Debug, "EMPTY", ""), formatter.GetBufferSizeCalls[0]);
        Assert.Single(writer.Entries);
        Assert.Equal("Debug|EMPTY|", writer.Entries[0]);
    }

    [Fact]
    public void ProcessLog_EmptyTagAndMessage_ProcessesCorrectly()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Warning, ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty);

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Equal((LogLevel.Warning, "", ""), formatter.GetBufferSizeCalls[0]);
        Assert.Single(writer.Entries);
        Assert.Equal("Warning||", writer.Entries[0]);
    }

    [Fact]
    public void ProcessLog_LongTagAndMessage_ProcessesCorrectly()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);
        var longTag = new string('A', 50);
        var longMessage = new string('B', 200);

        // Act
        processor.ProcessLog(LogLevel.Information, longTag.AsSpan(), longMessage.AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Equal((LogLevel.Information, longTag, longMessage), formatter.GetBufferSizeCalls[0]);
        Assert.Single(writer.Entries);
        Assert.Contains(longTag, writer.Entries[0]);
        Assert.Contains(longMessage, writer.Entries[0]);
    }

    [Fact]
    public void ProcessLog_MultipleEntries_ProcessesAll()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Information, "API".AsSpan(), "First".AsSpan());
        processor.ProcessLog(LogLevel.Warning, "DB".AsSpan(), "Second".AsSpan());
        processor.ProcessLog(LogLevel.Error, "AUTH".AsSpan(), "Third".AsSpan());

        // Assert
        Assert.Equal(3, formatter.GetBufferSizeCalls.Count);
        Assert.Equal(3, formatter.FormatCalls.Count);
        Assert.Equal(3, writer.Entries.Count);
        Assert.Equal("Information|API|First", writer.Entries[0]);
        Assert.Equal("Warning|DB|Second", writer.Entries[1]);
        Assert.Equal("Error|AUTH|Third", writer.Entries[2]);
    }

    [Fact]
    public void ProcessLog_SpecialCharacters_ProcessesCorrectly()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Information, "HTTP/2".AsSpan(), "Status: 200 OK".AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Equal((LogLevel.Information, "HTTP/2", "Status: 200 OK"), formatter.GetBufferSizeCalls[0]);
        Assert.Single(writer.Entries);
        Assert.Equal("Information|HTTP/2|Status: 200 OK", writer.Entries[0]);
    }

    [Fact]
    public void Dispose_CallsWriterDispose()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.Dispose();

        // Assert
        Assert.True(writer.IsDisposed);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.Dispose();
        processor.Dispose();
        processor.Dispose();

        // Assert
        Assert.True(writer.IsDisposed);
    }

    [Fact]
    public void ProcessLog_AllocatesCorrectBufferSize()
    {
        // Arrange
        var writer = new TestStreamWriter();
        var formatter = new TestLogFormatter();
        using var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.ProcessLog(LogLevel.Information, "TEST".AsSpan(), "message".AsSpan());

        // Assert
        Assert.Single(writer.Entries);
        var entry = writer.Entries[0];
        Assert.Equal("Information|TEST|message", entry);
    }
}