namespace GroveGames.Logger.Tests;

public sealed class FileLogFormatterTests
{
    private readonly FileLogFormatter _formatter = new();

    [Fact]
    public void GetBufferSize_ShouldCalculateCorrectSize()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "TestTag";
        const string message = "Test message";

        // Act
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());

        // Assert
        var expectedSize = 9 + 1 + 1 + 2 + 1 + tag.Length + 2 + message.Length;
        Assert.Equal(expectedSize, bufferSize);
    }

    [Fact]
    public void GetBufferSize_WithEmptyTag_ShouldCalculateCorrectSize()
    {
        // Arrange
        const LogLevel level = LogLevel.Debug;
        var tag = ReadOnlySpan<char>.Empty;
        const string message = "Test message";

        // Act
        var bufferSize = _formatter.GetBufferSize(level, tag, message.AsSpan());

        // Assert
        var expectedSize = 9 + 1 + 1 + 2 + 1 + 0 + 2 + message.Length;
        Assert.Equal(expectedSize, bufferSize);
    }

    [Fact]
    public void GetBufferSize_WithEmptyMessage_ShouldCalculateCorrectSize()
    {
        // Arrange
        const LogLevel level = LogLevel.Warning;
        const string tag = "TestTag";
        var message = ReadOnlySpan<char>.Empty;

        // Act
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message);

        // Assert
        var expectedSize = 9 + 1 + 1 + 2 + 1 + tag.Length + 2 + 0;
        Assert.Equal(expectedSize, bufferSize);
    }

    [Theory]
    [InlineData(LogLevel.Debug, "D")]
    [InlineData(LogLevel.Information, "I")]
    [InlineData(LogLevel.Warning, "W")]
    [InlineData(LogLevel.Error, "E")]
    public void Format_ShouldIncludeCorrectLogLevel(LogLevel level, string expectedLevelChar)
    {
        // Arrange
        const string tag = "Test";
        const string message = "Message";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains($"[{expectedLevelChar}]", formatted);
    }

    [Fact]
    public void Format_ShouldIncludeTimestamp()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "Test";
        const string message = "Message";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} ", formatted);
    }

    [Fact]
    public void Format_ShouldIncludeTagInBrackets()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "MyTag";
        const string message = "Message";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains($"[{tag}]", formatted);
    }

    [Fact]
    public void Format_ShouldIncludeMessage()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "Test";
        const string message = "This is a test message";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.EndsWith(message, formatted);
    }

    [Fact]
    public void Format_ShouldProduceCorrectOverallFormat()
    {
        // Arrange
        const LogLevel level = LogLevel.Warning;
        const string tag = "Logger";
        const string message = "Test warning";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} \[W\] \[Logger\] Test warning$", formatted);
    }

    [Fact]
    public void Format_WithEmptyTag_ShouldFormatCorrectly()
    {
        // Arrange
        const LogLevel level = LogLevel.Error;
        var tag = ReadOnlySpan<char>.Empty;
        const string message = "Error message";
        var bufferSize = _formatter.GetBufferSize(level, tag, message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag, message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} \[E\] \[\] Error message$", formatted);
    }

    [Fact]
    public void Format_WithEmptyMessage_ShouldFormatCorrectly()
    {
        // Arrange
        const LogLevel level = LogLevel.Debug;
        const string tag = "Debug";
        var message = ReadOnlySpan<char>.Empty;
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} \[D\] \[Debug\] $", formatted);
    }

    [Fact]
    public void Format_WithLongTag_ShouldFormatCorrectly()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "VeryLongTagNameForTesting";
        const string message = "Message";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains($"[{tag}]", formatted);
        Assert.EndsWith(message, formatted);
    }

    [Fact]
    public void Format_WithLongMessage_ShouldFormatCorrectly()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "Test";
        const string message = "This is a very long message that should be formatted correctly regardless of its length and content";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains($"[{tag}]", formatted);
        Assert.EndsWith(message, formatted);
    }

    [Fact]
    public void Format_WithSpecialCharacters_ShouldFormatCorrectly()
    {
        // Arrange
        const LogLevel level = LogLevel.Error;
        const string tag = "Test@#$";
        const string message = "Message with special chars: !@#$%^&*()";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains($"[{tag}]", formatted);
        Assert.EndsWith(message, formatted);
    }

    [Fact]
    public void Format_WithUnicodeCharacters_ShouldFormatCorrectly()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "Test🌍";
        const string message = "Unicode message: 世界 🚀";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains($"[{tag}]", formatted);
        Assert.EndsWith(message, formatted);
    }

    [Fact]
    public void Format_ShouldUseUtcTime()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "Test";
        const string message = "Message";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];
        var beforeUtc = DateTime.UtcNow;

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var afterUtc = DateTime.UtcNow;
        var formatted = buffer.ToString();
        var timeString = formatted.Substring(0, 8); // Extract HH:mm:ss

        // Parse the time from the formatted string
        if (TimeSpan.TryParseExact(timeString, @"hh\:mm\:ss", null, out var parsedTime))
        {
            var beforeTime = beforeUtc.TimeOfDay;
            var afterTime = afterUtc.TimeOfDay;

            // Allow for some tolerance due to execution time
            Assert.True(parsedTime >= beforeTime.Subtract(TimeSpan.FromSeconds(1)) &&
                       parsedTime <= afterTime.Add(TimeSpan.FromSeconds(1)));
        }
        else
        {
            Assert.True(false, $"Could not parse time from formatted string: {timeString}");
        }
    }

    [Fact]
    public void Format_WithExactBufferSize_ShouldNotOverflow()
    {
        // Arrange
        const LogLevel level = LogLevel.Information;
        const string tag = "Test";
        const string message = "Message";
        var bufferSize = _formatter.GetBufferSize(level, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        _formatter.Format(buffer, level, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.NotEmpty(formatted);
        Assert.DoesNotContain('\0', formatted);
    }
}