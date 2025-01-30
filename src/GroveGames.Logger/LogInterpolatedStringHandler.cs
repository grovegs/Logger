using System.Buffers;
using System.Runtime.CompilerServices;

namespace GroveGames.Logger;

[InterpolatedStringHandler]
public ref struct LogInterpolatedStringHandler
{
    private readonly Span<char> _buffer;
    private int _position;

    public readonly ReadOnlySpan<char> Written => _buffer[.._position];

    public LogInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        var array = ArrayPool<char>.Shared.Rent(literalLength + formattedCount);
        _buffer = array.AsSpan();
        ArrayPool<char>.Shared.Return(array);
        _position = 0;
    }

    public bool AppendLiteral(ReadOnlySpan<char> value)
    {
        return TryWrite(value);
    }

    public bool AppendFormatted<T>(T value) where T : ISpanFormattable
    {
        return value.TryFormat(_buffer[_position..], out int written, default, default) && Advance(written);
    }

    public bool AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
    {
        return value.TryFormat(_buffer[_position..], out int written, format, default) && Advance(written);
    }

    public bool AppendFormatted(ReadOnlySpan<char> value)
    {
        return TryWrite(value);
    }

    private bool TryWrite(ReadOnlySpan<char> value)
    {
        if (_buffer.Length - _position < value.Length)
        {
            return false;
        }

        value.CopyTo(_buffer[_position..]);
        _position += value.Length;
        return true;
    }

    private bool Advance(int count)
    {
        if (_buffer.Length - _position < count)
        {
            return false;
        }

        _position += count;
        return true;
    }
}