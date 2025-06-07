using static GroveGames.Logger.LoggerExtensions;

namespace GroveGames.Logger.Tests;

public sealed class FileLogProcessorTests : IDisposable
{
    [Fact]
    public void Constructor_WithNullWriter_ShouldThrowArgumentNullException()
    {
        // Arrange
        var formatter = new TestLogFormatter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileLogProcessor(null!, formatter));
    }

    [Fact]
    public void Constructor_WithNullFormatter_ShouldThrowArgumentNullException()
    {
        // Arrange
        var writer = new TestStreamWriter();

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new FileLogProcessor(writer, null!));
    }

    [Fact]
    public void ProcessLog_ShouldCallFormatterWithCorrectParameters()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        using var processor = new FileLogProcessor(writer, formatter);
        const LogLevel level = LogLevel.Information;
        const string tag = "TestTag";
        const string message = "Test message";

        // Act
        processor.ProcessLog(level, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        var call = formatter.GetBufferSizeCalls[0];
        Assert.Equal(level, call.Level);
        Assert.Equal(tag, call.Tag);
        Assert.Equal(message, call.Message);
    }

    [Fact]
    public void ProcessLog_ShouldCallFormatterFormatWithCorrectParameters()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        using var processor = new FileLogProcessor(writer, formatter);
        const LogLevel level = LogLevel.Warning;
        const string tag = "TestTag";
        const string message = "Test message";

        // Act
        processor.ProcessLog(level, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Single(formatter.FormatCalls);
        var call = formatter.FormatCalls[0];
        Assert.Equal(level, call.Level);
        Assert.Equal(tag, call.Tag);
        Assert.Equal(message, call.Message);
    }

    [Fact]
    public void ProcessLog_ShouldPassFormattedMessageToWriter()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        using var processor = new FileLogProcessor(writer, formatter);
        const LogLevel level = LogLevel.Error;
        const string tag = "TestTag";
        const string message = "Test message";

        // Act
        processor.ProcessLog(level, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Single(writer.Messages);
        Assert.Equal(formatter.FormattedOutput, writer.Messages[0]);
    }

    [Fact]
    public void ProcessLog_WithEmptyTag_ShouldProcessCorrectly()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        using var processor = new FileLogProcessor(writer, formatter);
        const LogLevel level = LogLevel.Debug;
        var tag = ReadOnlySpan<char>.Empty;
        const string message = "Test message";

        // Act
        processor.ProcessLog(level, tag, message.AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Single(formatter.FormatCalls);
        Assert.Single(writer.Messages);
    }

    [Fact]
    public void ProcessLog_WithEmptyMessage_ShouldProcessCorrectly()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        using var processor = new FileLogProcessor(writer, formatter);
        const LogLevel level = LogLevel.Information;
        const string tag = "TestTag";
        var message = ReadOnlySpan<char>.Empty;

        // Act
        processor.ProcessLog(level, tag.AsSpan(), message);

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Single(formatter.FormatCalls);
        Assert.Single(writer.Messages);
    }

    [Fact]
    public void ProcessLog_WithLargeMessage_ShouldProcessCorrectly()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        using var processor = new FileLogProcessor(writer, formatter);
        const LogLevel level = LogLevel.Warning;
        const string tag = "TestTag";
        var message = new string('X', 10000); // Large message

        // Act
        processor.ProcessLog(level, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Single(formatter.FormatCalls);
        Assert.Single(writer.Messages);
        Assert.Equal(formatter.FormattedOutput, writer.Messages[0]);
    }

    [Fact]
    public void ProcessLog_WithUnicodeCharacters_ShouldProcessCorrectly()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        using var processor = new FileLogProcessor(writer, formatter);
        const LogLevel level = LogLevel.Information;
        const string tag = "测试🌍";
        const string message = "Unicode message: 世界 🚀";

        // Act
        processor.ProcessLog(level, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Single(formatter.FormatCalls);
        Assert.Single(writer.Messages);

        var formatCall = formatter.FormatCalls[0];
        Assert.Equal(tag, formatCall.Tag);
        Assert.Equal(message, formatCall.Message);
    }

    [Fact]
    public void ProcessLog_MultipleMessages_ShouldProcessEachCorrectly()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        using var processor = new FileLogProcessor(writer, formatter);

        var messages = new[]
        {
            (LogLevel.Debug, "Tag1", "Message1"),
            (LogLevel.Information, "Tag2", "Message2"),
            (LogLevel.Warning, "Tag3", "Message3"),
            (LogLevel.Error, "Tag4", "Message4")
        };

        // Act
        foreach (var (level, tag, message) in messages)
        {
            processor.ProcessLog(level, tag.AsSpan(), message.AsSpan());
        }

        // Assert
        Assert.Equal(4, formatter.GetBufferSizeCalls.Count);
        Assert.Equal(4, formatter.FormatCalls.Count);
        Assert.Equal(4, writer.Messages.Count);

        for (int i = 0; i < messages.Length; i++)
        {
            var (expectedLevel, expectedTag, expectedMessage) = messages[i];
            var getBufferCall = formatter.GetBufferSizeCalls[i];
            var formatCall = formatter.FormatCalls[i];

            Assert.Equal(expectedLevel, getBufferCall.Level);
            Assert.Equal(expectedTag, getBufferCall.Tag);
            Assert.Equal(expectedMessage, getBufferCall.Message);

            Assert.Equal(expectedLevel, formatCall.Level);
            Assert.Equal(expectedTag, formatCall.Tag);
            Assert.Equal(expectedMessage, formatCall.Message);
        }
    }

    [Fact]
    public void Dispose_ShouldDisposeWriter()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.Dispose();

        // Assert
        Assert.True(writer.IsDisposed);
    }

    [Fact]
    public void Dispose_ShouldBeIdempotent()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        var processor = new FileLogProcessor(writer, formatter);

        // Act & Assert
        processor.Dispose();
        processor.Dispose();
        processor.Dispose();

        Assert.True(writer.IsDisposed);
    }

    [Fact]
    public void ProcessLog_AfterDispose_ShouldStillWork()
    {
        // Arrange
        var formatter = new TestLogFormatter();
        var writer = new TestStreamWriter();
        var processor = new FileLogProcessor(writer, formatter);

        // Act
        processor.Dispose();
        processor.ProcessLog(LogLevel.Information, "Tag".AsSpan(), "Message".AsSpan());

        // Assert
        Assert.Single(formatter.GetBufferSizeCalls);
        Assert.Single(formatter.FormatCalls);
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    private sealed class TestLogFormatter : ILogFormatter
    {
        public List<(LogLevel Level, string Tag, string Message)> GetBufferSizeCalls { get; } = [];
        public List<(LogLevel Level, string Tag, string Message)> FormatCalls { get; } = [];
        public string FormattedOutput => "FORMATTED_OUTPUT";

        public int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            GetBufferSizeCalls.Add((level, tag.ToString(), message.ToString()));
            return FormattedOutput.Length;
        }

        public void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            FormatCalls.Add((level, tag.ToString(), message.ToString()));
            FormattedOutput.AsSpan().CopyTo(buffer);
        }
    }

    private sealed class TestStreamWriter : IStreamWriter
    {
        public List<string> Messages { get; } = new();
        public bool IsDisposed { get; private set; }

        public void AddEntry(ReadOnlySpan<char> message)
        {
            Messages.Add(message.ToString());
        }

        public void Dispose()
        {
            IsDisposed = true;
        }

        public void Flush()
        {
        }
    }
}