namespace GroveGames.Logger.Tests;

public sealed class FileLogFormatterTests
{
    [Fact]
    public void GetBufferSize_EmptyTagAndMessage_ReturnsCorrectSize()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = ReadOnlySpan<char>.Empty;
        var message = ReadOnlySpan<char>.Empty;

        // Act
        var result = formatter.GetBufferSize(LogLevel.Information, tag, message);

        // Assert
        Assert.Equal(16, result);
    }

    [Theory]
    [InlineData("API", "Request completed", 36)]
    [InlineData("DB", "Connection established", 40)]
    [InlineData("Cache", "Hit", 24)]
    public void GetBufferSize_VariousTagsAndMessages_ReturnsCorrectSize(string tag, string message, int expectedSize)
    {
        // Arrange
        var formatter = new FileLogFormatter();

        // Act
        var result = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        Assert.Equal(expectedSize, result);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void GetBufferSize_DifferentLogLevels_ReturnsSameSize(LogLevel level)
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = "TEST".AsSpan();
        var message = "message".AsSpan();

        // Act
        var result = formatter.GetBufferSize(level, tag, message);

        // Assert
        Assert.Equal(27, result);
    }

    [Fact]
    public void GetBufferSize_LargeTagAndMessage_ReturnsCorrectSize()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = new string('A', 100).AsSpan();
        var message = new string('B', 500).AsSpan();

        // Act
        var result = formatter.GetBufferSize(LogLevel.Information, tag, message);

        // Assert
        Assert.Equal(616, result);
    }

    [Theory]
    [InlineData(LogLevel.Debug, 'D')]
    [InlineData(LogLevel.Information, 'I')]
    [InlineData(LogLevel.Warning, 'W')]
    [InlineData(LogLevel.Error, 'E')]
    public void Format_DifferentLogLevels_ProducesCorrectLevelCharacter(LogLevel level, char expectedChar)
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = "TEST".AsSpan();
        var message = "msg".AsSpan();
        var bufferSize = formatter.GetBufferSize(level, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, level, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains($"[{expectedChar}]", formatted);
    }

    [Fact]
    public void Format_UnknownLogLevel_ProducesNCharacter()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = "TEST".AsSpan();
        var message = "msg".AsSpan();
        var unknownLevel = (LogLevel)999;
        var bufferSize = formatter.GetBufferSize(unknownLevel, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, unknownLevel, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains("[N]", formatted);
    }

    [Fact]
    public void Format_ValidInput_ProducesCorrectStructure()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = "API".AsSpan();
        var message = "Request completed".AsSpan();
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} \[I\] \[API\] Request completed$", formatted);
    }

    [Fact]
    public void Format_EmptyTag_ProducesCorrectFormat()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = ReadOnlySpan<char>.Empty;
        var message = "Test message".AsSpan();
        var bufferSize = formatter.GetBufferSize(LogLevel.Warning, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Warning, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} \[W\] \[\] Test message$", formatted);
    }

    [Fact]
    public void Format_EmptyMessage_ProducesCorrectFormat()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = "ERROR".AsSpan();
        var message = ReadOnlySpan<char>.Empty;
        var bufferSize = formatter.GetBufferSize(LogLevel.Error, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Error, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} \[E\] \[ERROR\] $", formatted);
    }

    [Fact]
    public void Format_EmptyTagAndMessage_ProducesCorrectFormat()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = ReadOnlySpan<char>.Empty;
        var message = ReadOnlySpan<char>.Empty;
        var bufferSize = formatter.GetBufferSize(LogLevel.Debug, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Debug, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} \[D\] \[\] $", formatted);
    }

    [Fact]
    public void Format_LongTagAndMessage_FormatsCorrectly()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = "VeryLongTagName".AsSpan();
        var message = "This is a very long message that should be formatted correctly".AsSpan();
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains("[VeryLongTagName]", formatted);
        Assert.Contains("This is a very long message that should be formatted correctly", formatted);
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} \[I\] \[VeryLongTagName\] This is a very long message that should be formatted correctly$", formatted);
    }

    [Fact]
    public void Format_SpecialCharactersInTagAndMessage_FormatsCorrectly()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = "HTTP/2".AsSpan();
        var message = "Status: 200 OK - Content-Type: application/json".AsSpan();
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Contains("[HTTP/2]", formatted);
        Assert.Contains("Status: 200 OK - Content-Type: application/json", formatted);
    }

    [Fact]
    public void Format_ExactBufferSize_DoesNotOverrun()
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var tag = "TEST".AsSpan();
        var message = "message".AsSpan();
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag, message);

        // Assert
        var formatted = buffer.ToString();
        Assert.Equal(bufferSize, formatted.Length);
        Assert.DoesNotContain('\0', formatted);
    }

    [Theory]
    [InlineData("A", "B")]
    [InlineData("Logger", "Started")]
    [InlineData("", "Empty tag test")]
    [InlineData("Tag", "")]
    public void Format_VariousInputs_TimeFormatIsCorrect(string tag, string message)
    {
        // Arrange
        var formatter = new FileLogFormatter();
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag.AsSpan(), message.AsSpan());
        Span<char> buffer = stackalloc char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag.AsSpan(), message.AsSpan());

        // Assert
        var formatted = buffer.ToString();
        Assert.Matches(@"^\d{2}:\d{2}:\d{2} ", formatted);
    }
}