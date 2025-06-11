namespace GroveGames.Logger.Tests;

public sealed class FileLogFormatterTests
{
    private class TestTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => new(2024, 1, 1, 12, 34, 56, TimeSpan.Zero);
    }

    [Fact]
    public void Constructor_NullTimeProvider_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new FileLogFormatter(null!));
    }

    [Fact]
    public void Constructor_Default_UsesSystemTimeProvider()
    {
        var formatter = new FileLogFormatter();
        Assert.NotNull(formatter);
    }

    [Fact]
    public void Constructor_ValidTimeProvider_CreatesInstance()
    {
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        Assert.NotNull(formatter);
    }

    [Theory]
    [InlineData(LogLevel.Debug, "Tag", "Message", 26)]
    [InlineData(LogLevel.Information, "Tag", "Message", 26)]
    [InlineData(LogLevel.Warning, "Tag", "Message", 26)]
    [InlineData(LogLevel.Error, "Tag", "Message", 26)]
    [InlineData(LogLevel.None, "Tag", "Message", 26)]
    public void GetBufferSize_VariousLogLevels_ReturnsCorrectSize(LogLevel level, string tag, string message, int expected)
    {
        // Arrange
        var formatter = new FileLogFormatter();

        // Act
        var size = formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal(expected, size);
    }

    [Theory]
    [InlineData("", "Message", 23)]
    [InlineData("Tag", "", 19)]
    [InlineData("", "", 16)]
    [InlineData("LongTagName", "LongMessageContent", 45)]
    public void GetBufferSize_VariousTagAndMessageLengths_ReturnsCorrectSize(string tag, string message, int expected)
    {
        // Arrange
        var formatter = new FileLogFormatter();

        // Act
        var size = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal(expected, size);
    }

    [Fact]
    public void Format_DebugLevel_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "Test";
        var message = "Debug message";
        var bufferSize = formatter.GetBufferSize(LogLevel.Debug, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Debug, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [D] [Test] Debug message", buffer.ToString());
    }

    [Fact]
    public void Format_InformationLevel_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "Test";
        var message = "Info message";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [I] [Test] Info message", buffer.ToString());
    }

    [Fact]
    public void Format_WarningLevel_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "Test";
        var message = "Warning message";
        var bufferSize = formatter.GetBufferSize(LogLevel.Warning, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Warning, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [W] [Test] Warning message", buffer.ToString());
    }

    [Fact]
    public void Format_ErrorLevel_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "Test";
        var message = "Error message";
        var bufferSize = formatter.GetBufferSize(LogLevel.Error, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Error, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [E] [Test] Error message", buffer.ToString());
    }

    [Fact]
    public void Format_UnknownLevel_FormatsWithN()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "Test";
        var message = "Unknown message";
        var bufferSize = formatter.GetBufferSize((LogLevel)999, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, (LogLevel)999, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [N] [Test] Unknown message", buffer.ToString());
    }

    [Fact]
    public void Format_EmptyTag_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "";
        var message = "Message";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [I] [] Message", buffer.ToString());
    }

    [Fact]
    public void Format_EmptyMessage_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "Tag";
        var message = "";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [I] [Tag] ", buffer.ToString());
    }

    [Fact]
    public void Format_EmptyTagAndMessage_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "";
        var message = "";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [I] [] ", buffer.ToString());
    }

    [Fact]
    public void Format_LongTagAndMessage_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "VeryLongTagName";
        var message = "This is a very long message with lots of content";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [I] [VeryLongTagName] This is a very long message with lots of content", buffer.ToString());
    }

    [Fact]
    public void Format_ExactBufferSize_FitsExactly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider();
        var formatter = new FileLogFormatter(timeProvider);
        var tag = "Tag";
        var message = "Message";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal("12:34:56 [I] [Tag] Message", buffer.ToString());
    }
}
