namespace GroveGames.Logger.Tests;

public sealed class LevelSpecificInterpolatedStringHandlerTests : IDisposable
{
    [Fact]
    public void DebugHandler_WhenLoggerMinimumLevelIsDebug_ShouldCreateActiveHandler()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Debug);

        // Act
        var handler = new DebugMessageInterpolatedStringHandler(10, 1, logger);

        // Assert
        // Should not be empty (active)
        handler.AppendLiteral("Test");
        Assert.Equal("Test", handler.Written.ToString());
    }

    [Fact]
    public void DebugHandler_WhenLoggerMinimumLevelIsInformation_ShouldCreateEmptyHandler()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Information);

        // Act
        var handler = new DebugMessageInterpolatedStringHandler(10, 1, logger);

        // Assert
        // Should be empty (inactive) - operations should be no-op
        handler.AppendLiteral("Test");
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void DebugHandler_WhenLoggerMinimumLevelIsWarning_ShouldCreateEmptyHandler()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Warning);

        // Act
        var handler = new DebugMessageInterpolatedStringHandler(10, 1, logger);

        // Assert
        handler.AppendLiteral("Test");
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void DebugHandler_WhenLoggerMinimumLevelIsError_ShouldCreateEmptyHandler()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Error);

        // Act
        var handler = new DebugMessageInterpolatedStringHandler(10, 1, logger);

        // Assert
        handler.AppendLiteral("Test");
        Assert.True(handler.Written.IsEmpty);
    }

    [Theory]
    [InlineData(LogLevel.Debug, true)]
    [InlineData(LogLevel.Information, true)]
    [InlineData(LogLevel.Warning, false)]
    [InlineData(LogLevel.Error, false)]
    public void InformationHandler_ShouldBeActiveBasedOnMinimumLevel(LogLevel minimumLevel, bool shouldBeActive)
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(minimumLevel);

        // Act
        var handler = new InformationMessageInterpolatedStringHandler(10, 1, logger);
        handler.AppendLiteral("Test");

        // Assert
        if (shouldBeActive)
        {
            Assert.Equal("Test", handler.Written.ToString());
        }
        else
        {
            Assert.True(handler.Written.IsEmpty);
        }
    }

    [Theory]
    [InlineData(LogLevel.Debug, true)]
    [InlineData(LogLevel.Information, true)]
    [InlineData(LogLevel.Warning, true)]
    [InlineData(LogLevel.Error, false)]
    public void WarningHandler_ShouldBeActiveBasedOnMinimumLevel(LogLevel minimumLevel, bool shouldBeActive)
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(minimumLevel);

        // Act
        var handler = new WarningMessageInterpolatedStringHandler(10, 1, logger);
        handler.AppendLiteral("Test");

        // Assert
        if (shouldBeActive)
        {
            Assert.Equal("Test", handler.Written.ToString());
        }
        else
        {
            Assert.True(handler.Written.IsEmpty);
        }
    }

    [Theory]
    [InlineData(LogLevel.Debug, true)]
    [InlineData(LogLevel.Information, true)]
    [InlineData(LogLevel.Warning, true)]
    [InlineData(LogLevel.Error, true)]
    public void ErrorHandler_ShouldBeActiveBasedOnMinimumLevel(LogLevel minimumLevel, bool shouldBeActive)
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(minimumLevel);

        // Act
        var handler = new ErrorMessageInterpolatedStringHandler(10, 1, logger);
        handler.AppendLiteral("Test");

        // Assert
        if (shouldBeActive)
        {
            Assert.Equal("Test", handler.Written.ToString());
        }
        else
        {
            Assert.True(handler.Written.IsEmpty);
        }
    }

    [Fact]
    public void DebugHandler_AppendLiteral_WhenActive_ShouldDelegateToBaseHandler()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Debug);
        var handler = new DebugMessageInterpolatedStringHandler(20, 0, logger);

        // Act
        var result1 = handler.AppendLiteral("Hello ");
        var result2 = handler.AppendLiteral("World");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.Equal("Hello World", handler.Written.ToString());
    }

    [Fact]
    public void DebugHandler_AppendFormatted_WhenActive_ShouldDelegateToBaseHandler()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Debug);
        var handler = new DebugMessageInterpolatedStringHandler(10, 2, logger);

        // Act
        handler.AppendLiteral("Value: ");
        var result = handler.AppendFormatted(42);

        // Assert
        Assert.True(result);
        Assert.Equal("Value: 42", handler.Written.ToString());
    }

    [Fact]
    public void DebugHandler_AppendFormattedWithFormat_WhenActive_ShouldDelegateToBaseHandler()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Debug);
        var handler = new DebugMessageInterpolatedStringHandler(15, 1, logger);

        // Act
        handler.AppendLiteral("Value: ");
        var result = handler.AppendFormatted(123.456, "F2");

        // Assert
        Assert.True(result);
        Assert.Equal("Value: 123.46", handler.Written.ToString());
    }

    [Fact]
    public void DebugHandler_AppendFormattedSpan_WhenActive_ShouldDelegateToBaseHandler()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Debug);
        var handler = new DebugMessageInterpolatedStringHandler(20, 1, logger);

        // Act
        handler.AppendLiteral("Text: ");
        var result = handler.AppendFormatted("Hello World".AsSpan());

        // Assert
        Assert.True(result);
        Assert.Equal("Text: Hello World", handler.Written.ToString());
    }

    [Fact]
    public void DebugHandler_WhenInactive_ShouldReturnTrueButNotModifyOutput()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Information);
        var handler = new DebugMessageInterpolatedStringHandler(10, 1, logger);

        // Act
        var literalResult = handler.AppendLiteral("Test");
        var formattedResult = handler.AppendFormatted(42);
        var formattedWithFormatResult = handler.AppendFormatted(123.456, "F2");
        var spanResult = handler.AppendFormatted("span".AsSpan());

        // Assert
        Assert.True(literalResult);
        Assert.True(formattedResult);
        Assert.True(formattedWithFormatResult);
        Assert.True(spanResult);
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void InformationHandler_ComplexInterpolation_WhenActive_ShouldFormatCorrectly()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Debug);
        var handler = new InformationMessageInterpolatedStringHandler(50, 3, logger);

        // Act
        handler.AppendLiteral("User ");
        handler.AppendFormatted("John");
        handler.AppendLiteral(" has ");
        handler.AppendFormatted(5);
        handler.AppendLiteral(" items worth $");
        handler.AppendFormatted(123.45, "F2");

        // Assert
        Assert.Equal("User John has 5 items worth $123.45", handler.Written.ToString());
    }

    [Fact]
    public void WarningHandler_ComplexInterpolation_WhenInactive_ShouldRemainEmpty()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Error);
        var handler = new WarningMessageInterpolatedStringHandler(50, 3, logger);

        // Act
        handler.AppendLiteral("Warning: ");
        handler.AppendFormatted("Something");
        handler.AppendLiteral(" happened at ");
        handler.AppendFormatted(DateTime.Now, "yyyy-MM-dd");

        // Assert
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void ErrorHandler_WhenActive_ShouldHandleNullValues()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Debug);
        var handler = new ErrorMessageInterpolatedStringHandler(20, 1, logger);

        // Act
        handler.AppendLiteral("Error: ");
        handler.AppendFormatted((string?)null);

        // Assert
        Assert.Equal("Error: ", handler.Written.ToString());
    }

    [Fact]
    public void AllHandlers_ShouldExposeWrittenProperty()
    {
        // Arrange
        var logger = CreateLoggerWithMinimumLevel(LogLevel.Debug);

        // Act & Assert
        var debugHandler = new DebugMessageInterpolatedStringHandler(10, 0, logger);
        var infoHandler = new InformationMessageInterpolatedStringHandler(10, 0, logger);
        var warningHandler = new WarningMessageInterpolatedStringHandler(10, 0, logger);
        var errorHandler = new ErrorMessageInterpolatedStringHandler(10, 0, logger);

        debugHandler.AppendLiteral("Debug");
        infoHandler.AppendLiteral("Info");
        warningHandler.AppendLiteral("Warning");
        errorHandler.AppendLiteral("Error");

        Assert.Equal("Debug", debugHandler.Written.ToString());
        Assert.Equal("Info", infoHandler.Written.ToString());
        Assert.Equal("Warning", warningHandler.Written.ToString());
        Assert.Equal("Error", errorHandler.Written.ToString());
    }

    private static ILogger CreateLoggerWithMinimumLevel(LogLevel minimumLevel)
    {
        var builder = new LoggerBuilder();
        builder.SetMinimumLevel(minimumLevel);
        builder.AddLogProcessor(new TestLogProcessor());
        return builder.Build();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }

    // Test helper class
    private sealed class TestLogProcessor : ILogProcessor
    {
        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
        }

        public void Dispose()
        {
        }
    }
}