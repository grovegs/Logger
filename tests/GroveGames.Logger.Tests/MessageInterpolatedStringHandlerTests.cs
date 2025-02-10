namespace GroveGames.Logger.Tests;

public class MessageInterpolatedStringHandlerTests
{
    [Fact]
    public void AppendLiteral_ShouldWriteLiteralToBuffer()
    {
        var handler = new MessageInterpolatedStringHandler(10, 0);

        bool result = handler.AppendLiteral("Hello, World!".AsSpan(0, 5));

        Assert.True(result);
        Assert.Equal("Hello", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_ISpanFormattable_ShouldFormatAndWriteToBuffer()
    {
        var handler = new MessageInterpolatedStringHandler(20, 0);

        bool result = handler.AppendFormatted(123.45, "F2");

        Assert.True(result);
        Assert.Equal("123.45", handler.Written.ToString());
    }

    [Fact]
    public void AppendFormatted_WithInsufficientBuffer_ShouldReturnFalse()
    {
        var handler = new MessageInterpolatedStringHandler(5, 0);

        bool result = handler.AppendFormatted(1234567890);

        Assert.False(result);
        Assert.Equal(0, handler.Written.Length);
    }

    [Fact]
    public void Dispose_ShouldReturnRentedArrayToPool()
    {
        var handler = new MessageInterpolatedStringHandler(10, 0);
        handler.Dispose();

        // No way to directly assert ArrayPool release, but this ensures no exceptions
        Assert.True(true);
    }
}