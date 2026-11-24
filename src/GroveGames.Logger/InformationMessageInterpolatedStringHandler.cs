using System.Runtime.CompilerServices;

namespace GroveGames.Logger;

[InterpolatedStringHandler]
public ref struct InformationMessageInterpolatedStringHandler
{
    private MessageInterpolatedStringHandler _handler;

    public readonly ReadOnlySpan<char> Written => _handler.Written;

    public InformationMessageInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger)
    {
        _handler = new MessageInterpolatedStringHandler(literalLength, formattedCount, logger.MinimumLevel <= LogLevel.Information);
    }

    public void Dispose()
    {
        _handler.Dispose();
    }

    public bool AppendLiteral(ReadOnlySpan<char> value)
    {
        return _handler.Append(value);
    }

    public bool AppendFormatted(ReadOnlySpan<char> value)
    {
        return _handler.Append(value);
    }

    public bool AppendFormatted(string? value)
    {
        return _handler.Append(value.AsSpan());
    }

    public bool AppendFormatted(char value)
    {
        return _handler.Append(value);
    }

    public bool AppendFormatted(int value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(int value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(long value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(long value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(float value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(float value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(double value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(double value, string? format)
    {
        return _handler.Append(value, format);
    }

#if NET6_0_OR_GREATER
    public bool AppendFormatted<T>(T value) where T : ISpanFormattable
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
    {
        return _handler.Append(value, format);
    }
#else
    public bool AppendFormatted<T>(T value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted<T>(T value, string? format)
    {
        return _handler.Append(value, format);
    }
#endif
}
