using System.Runtime.CompilerServices;

namespace GroveGames.Logger;

[InterpolatedStringHandler]
public ref struct DebugMessageInterpolatedStringHandler
{
    private MessageInterpolatedStringHandler _handler;

    public readonly ReadOnlySpan<char> Written => _handler.Written;

    public DebugMessageInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger)
    {
        _handler = new MessageInterpolatedStringHandler(literalLength, formattedCount, logger.MinimumLevel <= LogLevel.Debug);
    }

    public DebugMessageInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger, out bool isEnabled)
    {
        isEnabled = logger.MinimumLevel <= LogLevel.Debug;
        _handler = new MessageInterpolatedStringHandler(literalLength, formattedCount, isEnabled);
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

    public bool AppendFormatted(bool value)
    {
        return _handler.Append(value);
    }

    public bool AppendFormatted(uint value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(uint value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(ulong value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(ulong value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(short value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(short value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(ushort value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(ushort value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(byte value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(byte value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(sbyte value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(sbyte value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(decimal value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(decimal value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(DateTime value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(DateTime value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(DateTimeOffset value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(DateTimeOffset value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(TimeSpan value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(TimeSpan value, string? format)
    {
        return _handler.Append(value, format);
    }

    public bool AppendFormatted(Guid value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(Guid value, string? format)
    {
        return _handler.Append(value, format);
    }

#if NET6_0_OR_GREATER
    public bool AppendFormatted(object? value)
    {
        return _handler.Append(value, null);
    }

    public bool AppendFormatted(object? value, string? format)
    {
        return _handler.Append(value, format);
    }

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
