using System.Runtime.CompilerServices;

namespace GroveGames.Logger;

[InterpolatedStringHandler]
public ref struct DebugMessageInterpolatedStringHandler
{
    private MessageInterpolatedStringHandler _handler;

    public readonly ReadOnlySpan<char> Written => _handler.Written;

    public DebugMessageInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger)
    {
        if (logger.MinimumLevel > LogLevel.Debug)
        {
            _handler = MessageInterpolatedStringHandler.Empty;
            return;
        }

        _handler = new MessageInterpolatedStringHandler(literalLength, formattedCount);
    }

    public bool AppendLiteral(ReadOnlySpan<char> value)
    {
        return _handler.AppendLiteral(value);
    }

#if NET6_0_OR_GREATER
    public bool AppendFormatted<T>(T value) where T : ISpanFormattable
    {
        return _handler.AppendFormatted(value);
    }

    public bool AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
    {
        return _handler.AppendFormatted(value, format);
    }
#else
    public bool AppendFormatted<T>(T value)
    {
        return _handler.AppendFormatted(value);
    }

    public bool AppendFormatted<T>(T value, string? format)
    {
        return _handler.AppendFormatted(value, format);
    }
#endif

    public bool AppendFormatted(ReadOnlySpan<char> value)
    {
        return _handler.AppendFormatted(value);
    }
}
