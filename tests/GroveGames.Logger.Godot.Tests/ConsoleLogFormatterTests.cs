namespace GroveGames.Logger.Godot.Tests;

public sealed class GodotConsoleLogFormatterTests
{
    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

    [Fact]
    public void Constructor_NullTimeProvider_UsesSystemTimeProvider()
    {
        // Act
        var formatter = new GodotConsoleLogFormatter();

        // Assert
        Assert.NotNull(formatter);
    }

    [Fact]
    public void Constructor_WithTimeProvider_UsesProvidedTimeProvider()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));

        // Act
        var formatter = new GodotConsoleLogFormatter(timeProvider);

        // Assert
        Assert.NotNull(formatter);
    }

    [Fact]
    public void GetBufferSize_InfoLevel_ReturnsCorrectSize()
    {
        // Arrange
        var formatter = new GodotConsoleLogFormatter();

        // Act
        var size = formatter.GetBufferSize(LogLevel.Information, "TestTag", "Test message");

        // Assert
        Assert.Equal(31, size);
    }

    [Fact]
    public void GetBufferSize_WarningLevel_ReturnsCorrectSizeWithWarningTag()
    {
        // Arrange
        var formatter = new GodotConsoleLogFormatter();

        // Act
        var size = formatter.GetBufferSize(LogLevel.Warning, "TestTag", "Test message");

        // Assert
        Assert.Equal(48, size);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Error)]
    public void GetBufferSize_NonWarningLevels_ReturnsBasicSize(LogLevel level)
    {
        // Arrange
        var formatter = new GodotConsoleLogFormatter();

        // Act
        var size = formatter.GetBufferSize(level, "Tag", "Message");

        // Assert
        Assert.Equal(22, size);
    }

    [Fact]
    public void GetBufferSize_EmptyTagAndMessage_ReturnsMinimumSize()
    {
        // Arrange
        var formatter = new GodotConsoleLogFormatter();

        // Act
        var size = formatter.GetBufferSize(LogLevel.Information, ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty);

        // Assert
        Assert.Equal(12, size);
    }

    [Fact]
    public void GetBufferSize_EmptyTagAndMessageWarning_ReturnsMinimumSizeWithWarningTag()
    {
        // Arrange
        var formatter = new GodotConsoleLogFormatter();

        // Act
        var size = formatter.GetBufferSize(LogLevel.Warning, ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty);

        // Assert
        Assert.Equal(29, size);
    }

    [Fact]
    public void Format_InfoLevel_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var buffer = new char[50];

        // Act
        formatter.Format(buffer, LogLevel.Information, "TestTag", "Test message");
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal("14:30:45 [TestTag] Test message", result);
    }

    [Fact]
    public void Format_WarningLevel_IncludesWarningTag()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var buffer = new char[60];

        // Act
        formatter.Format(buffer, LogLevel.Warning, "WarnTag", "Warning message");
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal("[color=yellow]⚠️ 14:30:45 [WarnTag] Warning message", result);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Error)]
    public void Format_NonWarningLevels_NoWarningTag(LogLevel level)
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var buffer = new char[50];

        // Act
        formatter.Format(buffer, level, "Tag", "Message");
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.DoesNotContain("[color=yellow]", result);
        Assert.Equal("14:30:45 [Tag] Message", result);
    }

    [Fact]
    public void Format_EmptyTag_FormatsWithEmptyBrackets()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var buffer = new char[30];

        // Act
        formatter.Format(buffer, LogLevel.Information, ReadOnlySpan<char>.Empty, "Message");
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal("14:30:45 [] Message", result);
    }

    [Fact]
    public void Format_EmptyMessage_FormatsWithoutMessage()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var buffer = new char[30];

        // Act
        formatter.Format(buffer, LogLevel.Information, "Tag", ReadOnlySpan<char>.Empty);
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal("14:30:45 [Tag] ", result);
    }

    [Fact]
    public void Format_EmptyTagAndMessage_FormatsMinimal()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var buffer = new char[20];

        // Act
        formatter.Format(buffer, LogLevel.Information, ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty);
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal("14:30:45 [] ", result);
    }

    [Fact]
    public void Format_LongTagAndMessage_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var longTag = "VeryLongTagNameForTesting";
        var longMessage = "This is a very long message that contains a lot of text to test the formatter";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, longTag, longMessage);
        var buffer = new char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, longTag, longMessage);
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal($"14:30:45 [{longTag}] {longMessage}", result);
    }

    [Fact]
    public void Format_ExactBufferSize_NoOverflow()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var tag = "Tag";
        var message = "Message";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, tag, message);
        var buffer = new char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, tag, message);
        var result = new string(buffer);

        // Assert
        Assert.DoesNotContain('\0', result);
        Assert.Equal("14:30:45 [Tag] Message", result);
    }

    [Fact]
    public void Format_MidnightTime_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 0, 0, 0, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var buffer = new char[50];

        // Act
        formatter.Format(buffer, LogLevel.Information, "Tag", "Message");
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal("00:00:00 [Tag] Message", result);
    }

    [Fact]
    public void Format_SingleDigitTime_FormatsWithLeadingZeros()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 5, 7, 9, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var buffer = new char[50];

        // Act
        formatter.Format(buffer, LogLevel.Information, "Tag", "Message");
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal("05:07:09 [Tag] Message", result);
    }

    [Fact]
    public void Format_SpecialCharactersInMessage_FormatsCorrectly()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var specialMessage = "Message with 特殊文字 and emojis 🎮🎯";
        var bufferSize = formatter.GetBufferSize(LogLevel.Information, "Tag", specialMessage);
        var buffer = new char[bufferSize];

        // Act
        formatter.Format(buffer, LogLevel.Information, "Tag", specialMessage);
        var result = new string(buffer).TrimEnd('\0');

        // Assert
        Assert.Equal($"14:30:45 [Tag] {specialMessage}", result);
    }

    [Fact]
    public void GetBufferSize_MatchesActualFormattedLength()
    {
        // Arrange
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 14, 30, 45, TimeSpan.Zero));
        var formatter = new GodotConsoleLogFormatter(timeProvider);
        var testCases = new[]
        {
            (LogLevel.Information, "Short", "Quick test"),
            (LogLevel.Warning, "Medium", "A bit longer message here"),
            (LogLevel.Error, "", ""),
            (LogLevel.Debug, "VeryLongTagName", "Very long message with lots of content")
        };

        foreach (var (level, tag, message) in testCases)
        {
            // Act
            var expectedSize = formatter.GetBufferSize(level, tag, message);
            var buffer = new char[expectedSize + 10]; // Add extra space
            formatter.Format(buffer, level, tag, message);
            var result = new string(buffer).TrimEnd('\0');

            // Assert
            Assert.Equal(expectedSize, result.Length);
        }
    }
}