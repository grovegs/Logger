using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace GroveGames.Logger;

[InterpolatedStringHandler]
public ref struct MessageInterpolatedStringHandler
{
    public static MessageInterpolatedStringHandler Empty => new([]);

    private readonly Span<char> _buffer;
    private readonly char[] _rentedArray;
    private int _position;

    public readonly ReadOnlySpan<char> Written => _buffer[.._position];

    private MessageInterpolatedStringHandler(Span<char> buffer)
    {
        _buffer = buffer;
        _rentedArray = [];
        _position = 0;
    }

    public MessageInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        var formattedLength = formattedCount * 16;
        var bufferSize = literalLength + formattedLength;
        var rentedArray = ArrayPool<char>.Shared.Rent(bufferSize);
        _buffer = rentedArray.AsSpan(0, bufferSize);
        _rentedArray = rentedArray;
        _position = 0;
    }

    public readonly void Dispose()
    {
        if (_rentedArray.Length > 0)
        {
            ArrayPool<char>.Shared.Return(_rentedArray);
        }
    }

    public bool AppendLiteral(ReadOnlySpan<char> value)
    {
        return !_buffer.IsEmpty && TryWrite(value);
    }

    public bool AppendFormatted<T>(T value) where T : ISpanFormattable
    {
        return !_buffer.IsEmpty && value.TryFormat(_buffer[_position..], out int written, default, CultureInfo.InvariantCulture) && Advance(written);
    }

    public bool AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
    {
        return !_buffer.IsEmpty && value.TryFormat(_buffer[_position..], out int written, format, CultureInfo.InvariantCulture) && Advance(written);
    }

    public bool AppendFormatted(ReadOnlySpan<char> value)
    {
        return !_buffer.IsEmpty && TryWrite(value);
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