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

    public bool Append(bool value)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;
        return value.TryFormat(text, out var written)
            ? Append(text.AsSpan(0, written))
            : Append(value ? bool.TrueString.AsSpan() : bool.FalseString.AsSpan());
    }

    public bool Append(uint value, string? format)
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

    public bool Append(ulong value, string? format)
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

    public bool Append(short value, string? format)
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

    public bool Append(ushort value, string? format)
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

    public bool Append(byte value, string? format)
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

    public bool Append(sbyte value, string? format)
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

    public bool Append(decimal value, string? format)
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

    public bool Append(DateTime value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;

        if (format is null)
        {
            return Append(text.AsSpan(0, WriteInvariant(text, value)));
        }

        return value.TryFormat(text, out var written, format, CultureInfo.InvariantCulture)
            ? Append(text.AsSpan(0, written))
            : Append(value.ToString(format, CultureInfo.InvariantCulture).AsSpan());
    }

    public bool Append(DateTimeOffset value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;

        if (format is null)
        {
            return Append(text.AsSpan(0, WriteInvariant(text, value)));
        }

        return value.TryFormat(text, out var written, format, CultureInfo.InvariantCulture)
            ? Append(text.AsSpan(0, written))
            : Append(value.ToString(format, CultureInfo.InvariantCulture).AsSpan());
    }

    public bool Append(TimeSpan value, string? format)
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

    public bool Append(Guid value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = Scratch;
        return value.TryFormat(text, out var written, format)
            ? Append(text.AsSpan(0, written))
            : Append(value.ToString(format).AsSpan());
    }

#if NET6_0_OR_GREATER
    public bool Append(object? value, string? format)
    {
        if (!_isOpen)
        {
            return false;
        }

        var text = value is IFormattable formattable ? formattable.ToString(format, CultureInfo.InvariantCulture) : value?.ToString();
        return Append(text.AsSpan());
    }

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

    // The invariant "G" form, written digit by digit because Mono and IL2CPP allocate a string for every "yyyy".
    private static int WriteInvariant(Span<char> text, DateTime value)
    {
        var position = WriteDigits(text, 0, value.Month, 2);
        text[position++] = '/';
        position = WriteDigits(text, position, value.Day, 2);
        text[position++] = '/';
        position = WriteDigits(text, position, value.Year, 4);
        text[position++] = ' ';
        position = WriteDigits(text, position, value.Hour, 2);
        text[position++] = ':';
        position = WriteDigits(text, position, value.Minute, 2);
        text[position++] = ':';
        return WriteDigits(text, position, value.Second, 2);
    }

    private static int WriteInvariant(Span<char> text, DateTimeOffset value)
    {
        var position = WriteInvariant(text, value.DateTime);
        var offset = (int)value.Offset.TotalMinutes;
        text[position++] = ' ';
        text[position++] = offset < 0 ? '-' : '+';
        offset = Math.Abs(offset);
        position = WriteDigits(text, position, offset / 60, 2);
        text[position++] = ':';
        return WriteDigits(text, position, offset % 60, 2);
    }

    private static int WriteDigits(Span<char> text, int position, int value, int count)
    {
        for (var i = count - 1; i >= 0; i--)
        {
            text[position + i] = (char)('0' + (value % 10));
            value /= 10;
        }

        return position + count;
    }
}
