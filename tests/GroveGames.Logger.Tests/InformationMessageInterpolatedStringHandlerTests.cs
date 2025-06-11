namespace GroveGames.Logger.Tests;

public class InformationMessageInterpolatedStringHandlerTests
{
    private class TestLogger : ILogger
    {
        public LogLevel MinimumLevel { get; }

        public TestLogger(LogLevel minimumLevel)
        {
            MinimumLevel = minimumLevel;
        }

        public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message) { }
    }

    [Fact]
    public void Constructor_LoggerMinimumLevelInformation_CreatesHandler()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);

        // Act
        var handler = new InformationMessageInterpolatedStringHandler(10, 2, logger);

        // Assert
        Assert.Equal(0, handler.Written.Length);
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    public void Constructor_LoggerMinimumLevelLowerThanInformation_CreatesHandler(LogLevel level)
    {
        // Arrange
        var logger = new TestLogger(level);

        // Act
        var handler = new InformationMessageInterpolatedStringHandler(10, 2, logger);
        handler.AppendLiteral("test");

        // Assert
        Assert.Equal("test", handler.Written.ToString());
    }

    [Fact]
    public void Constructor_LoggerLevelDebug_AllowsAppending()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Debug);
        var handler = new InformationMessageInterpolatedStringHandler(50, 2, logger);

        // Act
        handler.AppendLiteral("Information: ");
        handler.AppendFormatted(999);

        // Assert
        Assert.Equal("Information: 999", handler.Written.ToString());
    }

    [Fact]
    public void Constructor_LoggerLevelInformation_AllowsAppending()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(50, 2, logger);

        // Act
        handler.AppendLiteral("Information: ");
        handler.AppendFormatted(999);

        // Assert
        Assert.Equal("Information: 999", handler.Written.ToString());
    }

    [Fact]
    public void Constructor_LoggerLevelWarning_BlocksAppending()
    {
        var logger = new TestLogger(LogLevel.Warning);
        var handler = new DebugMessageInterpolatedStringHandler(50, 2, logger);
        handler.AppendLiteral("Debug: ");
        handler.AppendFormatted(123);
        Assert.Equal(0, handler.Written.Length);
    }

    [Fact]
    public void Constructor_LoggerLevelError_BlocksAppending()
    {
        var logger = new TestLogger(LogLevel.Error);
        var handler = new DebugMessageInterpolatedStringHandler(50, 2, logger);
        handler.AppendLiteral("Debug: ");
        handler.AppendFormatted(123);
        Assert.Equal(0, handler.Written.Length);
    }

    [Fact]
    public void AppendLiteral_ValidString_AppendsToWritten()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(20, 0, logger);

        // Act
        handler.AppendLiteral("Hello World");

        // Assert
        Assert.Equal("Hello World", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_IntValue_AppendsToWritten()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(10, 1, logger);

        // Act
        handler.AppendFormatted(42);

        // Assert
        Assert.Equal("42", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_IntWithFormat_AppendsFormattedToWritten()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(10, 1, logger);

        // Act
        handler.AppendFormatted(42, "X");

        // Assert
        Assert.Equal("2A", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_ReadOnlySpan_AppendsToWritten()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(10, 1, logger);

        // Act
        handler.AppendFormatted("test".AsSpan());

        // Assert
        Assert.Equal("test", handler.Written.ToString());
    }

    [Fact]
    public void AppendMethods_MixedCalls_CombinesAllParts()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(30, 3, logger);

        // Act
        handler.AppendLiteral("Value: ");
        handler.AppendFormatted(42);
        handler.AppendLiteral(", Hex: ");
        handler.AppendFormatted(42, "X");
        handler.AppendLiteral(", Text: ");
        handler.AppendFormatted("test".AsSpan());

        // Assert
        Assert.Equal("Value: 42, Hex: 2A, Text: test", handler.Written.ToString());
    }

    [Fact]
    public void AppendLiteral_EmptySpan_HandlesCorrectly()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(0, 0, logger);

        // Act
        handler.AppendLiteral(ReadOnlySpan<char>.Empty);

        // Assert
        Assert.Equal(0, handler.Written.Length);
    }

    [Fact]
    public void AppendFormatted_EmptySpan_HandlesCorrectly()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(0, 0, logger);

        // Act
        handler.AppendFormatted(ReadOnlySpan<char>.Empty);

        // Assert
        Assert.Equal(0, handler.Written.Length);
    }

    [Fact]
    public void AppendFormatted_NullFormat_UsesDefaultFormat()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(10, 1, logger);

        // Act
        handler.AppendFormatted(42, null);

        // Assert
        Assert.Equal("42", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_Double_FormatsCorrectly()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(30, 1, logger);
        var value = 123.456789;

        // Act
        handler.AppendFormatted(value, "F3");

        // Assert
        Assert.Equal("123.457", handler.Written.ToString());
    }

    [Fact]
    public void Constructor_NoLoggerLevelRestriction_AppendMethodsWork()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Debug);
        var handler = new InformationMessageInterpolatedStringHandler(50, 2, logger);

        // Act
        handler.AppendLiteral("Information: ");
        handler.AppendFormatted(404);
        handler.AppendLiteral(" - Not Found");

        // Assert
        Assert.Equal("Information: 404 - Not Found", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_LargeNumber_FormatsCorrectly()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(30, 1, logger);
        var value = 1234567890L;

        // Act
        handler.AppendFormatted(value, "N0");

        // Assert
        Assert.Equal("1,234,567,890", handler.Written.ToString());
    }

    [Fact]
    public void AppendMethods_MultipleFormats_AppliesEachCorrectly()
    {
        // Arrange
        var logger = new TestLogger(LogLevel.Information);
        var handler = new InformationMessageInterpolatedStringHandler(100, 4, logger);

        // Act
        handler.AppendLiteral("Int: ");
        handler.AppendFormatted(255, "X2");
        handler.AppendLiteral(", Double: ");
        handler.AppendFormatted(3.14159, "F2");
        handler.AppendLiteral(", Date: ");
        handler.AppendFormatted(new DateTime(2024, 12, 25), "yyyy-MM-dd");
        handler.AppendLiteral(", Span: ");
        handler.AppendFormatted("Text".AsSpan());

        // Assert
        Assert.Equal("Int: FF, Double: 3.14, Date: 2024-12-25, Span: Text", handler.Written.ToString());
    }
}