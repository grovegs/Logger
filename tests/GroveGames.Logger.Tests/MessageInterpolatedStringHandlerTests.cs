using System.Globalization;

namespace GroveGames.Logger.Tests;

public sealed class MessageInterpolatedStringHandlerTests
{
    [Fact]
    public void Empty_ReturnsEmptyHandler()
    {
        // Act
        MessageInterpolatedStringHandler handler = MessageInterpolatedStringHandler.Empty;

        // Assert
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void Constructor_ZeroLengthAndCount_CreatesValidHandler()
    {
        // Act
        using var handler = new MessageInterpolatedStringHandler(0, 0);

        // Assert
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void Constructor_ValidLengthAndCount_CreatesValidHandler()
    {
        // Act
        using var handler = new MessageInterpolatedStringHandler(10, 2);

        // Assert
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void Written_InitialState_ReturnsEmptySpan()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(10, 1);

        // Act
        ReadOnlySpan<char> result = handler.Written;

        // Assert
        Assert.True(result.IsEmpty);
    }

    [Fact]
    public void AppendLiteral_SimpleString_ReturnsTrue()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(10, 0);

        // Act
        bool result = handler.AppendLiteral("Hello");

        // Assert
        Assert.True(result);
        Assert.Equal("Hello", handler.Written.ToString());
    }

    [Fact]
    public void AppendLiteral_EmptyString_ReturnsTrue()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(10, 0);

        // Act
        bool result = handler.AppendLiteral("");

        // Assert
        Assert.True(result);
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void AppendLiteral_ExceedsBuffer_ReturnsFalse()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(5, 0);
        string longString = "This string is longer than 5 characters";

        // Act
        bool result = handler.AppendLiteral(longString);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void AppendLiteral_MultipleCalls_AppendsCorrectly()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(20, 0);

        // Act
        bool result1 = handler.AppendLiteral("Hello");
        bool result2 = handler.AppendLiteral(" ");
        bool result3 = handler.AppendLiteral("World");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.Equal("Hello World", handler.Written.ToString());
    }

    [Theory]
    [InlineData(42)]
    [InlineData(-123)]
    [InlineData(0)]
    public void AppendFormatted_Integer_ReturnsTrue(int value)
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(20, 1);

        // Act
        bool result = handler.AppendFormatted(value);

        // Assert
        Assert.True(result);
        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), handler.Written.ToString());
    }

    [Theory]
    [InlineData(3.14159)]
    [InlineData(-2.5)]
    [InlineData(0.0)]
    public void AppendFormatted_Double_ReturnsTrue(double value)
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(30, 1);

        // Act
        bool result = handler.AppendFormatted(value);

        // Assert
        Assert.True(result);
        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_IntegerWithFormat_ReturnsTrue()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(20, 1);

        // Act
        bool result = handler.AppendFormatted(42, "X");

        // Assert
        Assert.True(result);
        Assert.Equal("2A", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_DoubleWithFormat_ReturnsTrue()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(20, 1);

        // Act
        bool result = handler.AppendFormatted(3.14159, "F2");

        // Assert
        Assert.True(result);
        Assert.Equal("3.14", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_ReadOnlySpan_ReturnsTrue()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(20, 1);
        ReadOnlySpan<char> span = "Test".AsSpan();

        // Act
        bool result = handler.AppendFormatted(span);

        // Assert
        Assert.True(result);
        Assert.Equal("Test", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_EmptySpan_ReturnsTrue()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(20, 1);
        ReadOnlySpan<char> span = [];

        // Act
        bool result = handler.AppendFormatted(span);

        // Assert
        Assert.True(result);
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void AppendFormatted_ExceedsBuffer_ReturnsFalse()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(1, 0);
        int value = 12345;

        // Act
        bool result = handler.AppendFormatted(value);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void MixedOperations_LiteralAndFormatted_WorksCorrectly()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(50, 2);

        // Act
        bool result1 = handler.AppendLiteral("Value: ");
        bool result2 = handler.AppendFormatted(42);
        bool result3 = handler.AppendLiteral(", Double: ");
        bool result4 = handler.AppendFormatted(3.14, "F1");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        Assert.True(result4);
        Assert.Equal("Value: 42, Double: 3.1", handler.Written.ToString());
    }

    [Fact]
    public void BufferOverflow_PartialSuccess_MaintainsValidState()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(5, 0);

        // Act
        bool result1 = handler.AppendLiteral("Hello");
        bool result2 = handler.AppendLiteral("X");

        // Assert
        Assert.True(result1);
        Assert.False(result2);
        Assert.Equal("Hello", handler.Written.ToString());
    }

    [Fact]
    public void Written_AfterOperations_ReturnsCorrectSpan()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(30, 1);

        // Act
        handler.AppendLiteral("Hello ");
        handler.AppendFormatted(123);
        ReadOnlySpan<char> written = handler.Written;

        // Assert
        Assert.Equal("Hello 123", written.ToString());
        Assert.Equal(9, written.Length);
    }

    [Fact]
    public void BufferSizeCalculation_CorrectAllocation()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(10, 3);
        string testString58 = new string('A', 58);

        // Act
        bool result58 = handler.AppendLiteral(testString58);
        bool result59 = handler.AppendLiteral("B");

        // Assert
        Assert.True(result58);
        Assert.False(result59);
        Assert.Equal(58, handler.Written.Length);
    }

    [Fact]
    public void Dispose_ReleasesArrayPoolBuffer()
    {
        // Arrange
        var handler = new MessageInterpolatedStringHandler(100, 5);
        handler.AppendLiteral("Test");

        // Act & Assert
        handler.Dispose();
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var handler = new MessageInterpolatedStringHandler(50, 2);

        // Act & Assert
        handler.Dispose();
        handler.Dispose();
        handler.Dispose();
    }

    [Fact]
    public void Empty_BufferOperations_AlwaysReturnFalse()
    {
        // Arrange
        MessageInterpolatedStringHandler handler = MessageInterpolatedStringHandler.Empty;

        // Act
        bool result1 = handler.AppendLiteral("Any text");
        bool result2 = handler.AppendFormatted(12345);
        bool result3 = handler.AppendFormatted("test".AsSpan());

        // Assert
        Assert.False(result1);
        Assert.False(result2);
        Assert.False(result3);
        Assert.True(handler.Written.IsEmpty);
    }

    [Fact]
    public void LargeBuffer_HandlesLargeContent()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(1000, 10);
        string largeString = new string('X', 500);

        // Act
        bool result1 = handler.AppendLiteral(largeString);
        bool result2 = handler.AppendFormatted(999999);
        bool result3 = handler.AppendLiteral("End");

        // Assert
        Assert.True(result1);
        Assert.True(result2);
        Assert.True(result3);
        string written = handler.Written.ToString();
        Assert.StartsWith(largeString, written);
        Assert.Contains("999999", written);
        Assert.EndsWith("End", written);
    }

    [Theory]
    [InlineData(true, "True")]
    [InlineData(false, "False")]
    public void AppendFormatted_BooleanAsSpan_ReturnsTrue(bool value, string expected)
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(20, 1);

        // Act
        bool result = handler.AppendFormatted(value.ToString().AsSpan());

        // Assert
        Assert.True(result);
        Assert.Equal(expected, handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_Guid_ReturnsTrue()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(50, 1);
        var guid = Guid.NewGuid();

        // Act
        bool result = handler.AppendFormatted(guid);

        // Assert
        Assert.True(result);
        Assert.Equal(guid.ToString(), handler.Written.ToString());
    }

    [Fact]
    public void InvariantCulture_NumberFormatting_UsesInvariantCulture()
    {
        // Arrange
        using var handler = new MessageInterpolatedStringHandler(20, 1);
        double value = 1234.56;

        // Act
        bool result = handler.AppendFormatted(value);

        // Assert
        Assert.True(result);
        Assert.Contains(".", handler.Written.ToString());
        Assert.Equal(value.ToString(CultureInfo.InvariantCulture), handler.Written.ToString());
    }
}
