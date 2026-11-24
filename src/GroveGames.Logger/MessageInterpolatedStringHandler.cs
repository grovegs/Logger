using System.Globalization;

using GroveGames.Globalization;

namespace GroveGames.Logger;

internal ref struct MessageInterpolatedStringHandler
{
    public const int MaxLength = 4096;
    public const string TruncationMarker = "... (truncated)";

    private const int ScratchLength = 64;

    [ThreadStatic]
    private static char[]? t_scratch;

    private TextInterpolatedStringHandler _handler;
    private bool _isOpen;

    public readonly ReadOnlySpan<char> Written => _handler.Written;

    private static char[] Scratch => t_scratch ??= new char[ScratchLength];

    public MessageInterpolatedStringHandler(int literalLength, int formattedCount, bool isEnabled)
    {
        _isOpen = isEnabled;
        _handler = isEnabled ? new TextInterpolatedStringHandler(Math.Min(literalLength, MaxLength), formattedCount) : default;
    }

    public void Dispose()
    {
        _handler.Dispose();
    }

    public bool Append(ReadOnlySpan<char> value)
    {
        if (!_isOpen)
        {
            return false;
        }

        var remaining = MaxLength - _handler.Written.Length;

        if (value.Length <= remaining)
        {
            _handler.AppendFormatted(value);
            return true;
        }

        _handler.AppendFormatted(value.Slice(0, remaining));
        _handler.AppendLiteral(TruncationMarker);
        _isOpen = false;
        return false;
    }

    public bool Append(char value)
    {
        var text = Scratch;
        text[0] = value;
        return Append(text.AsSpan(0, 1));
    }

    public bool Append(int value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;
        return value.TryFormat(text, out var written, format, CultureInfo.InvariantCulture)
            ? Append(text.AsSpan(0, written))
            : Append(value.ToString(format, CultureInfo.InvariantCulture).AsSpan());
    }

    public bool Append(long value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;
        return value.TryFormat(text, out var written, format, CultureInfo.InvariantCulture)
            ? Append(text.AsSpan(0, written))
            : Append(value.ToString(format, CultureInfo.InvariantCulture).AsSpan());
    }

    public bool Append(float value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;
        return value.TryFormat(text, out var written, format, CultureInfo.InvariantCulture)
            ? Append(text.AsSpan(0, written))
            : Append(value.ToString(format, CultureInfo.InvariantCulture).AsSpan());
    }

    public bool Append(double value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;
        return value.TryFormat(text, out var written, format, CultureInfo.InvariantCulture)
            ? Append(text.AsSpan(0, written))
            : Append(value.ToString(format, CultureInfo.InvariantCulture).AsSpan());
    }

#if NET6_0_OR_GREATER
    public bool Append<T>(T value, string? format) where T : ISpanFormattable
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;
        return value.TryFormat(text, out var written, format, CultureInfo.InvariantCulture)
            ? Append(text.AsSpan(0, written))
            : Append(value.ToString(format, CultureInfo.InvariantCulture).AsSpan());
    }
#else
    public bool Append<T>(T value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = value switch
        {
            IFormattable formattable => formattable.ToString(format, CultureInfo.InvariantCulture),
            _ => value?.ToString()
        };
        return Append(text.AsSpan());
    }
#endif
}
