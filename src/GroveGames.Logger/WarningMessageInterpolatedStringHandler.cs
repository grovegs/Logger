using System.Runtime.CompilerServices;

namespace GroveGames.Logger;

[InterpolatedStringHandler]
public readonly ref struct WarningMessageInterpolatedStringHandler
{
    private readonly MessageInterpolatedStringHandler _handler;

    public readonly ReadOnlySpan<char> Written => _handler.Written;

    public WarningMessageInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger)
    {
        if (logger.MinimumLogLevel > LogLevel.Warning)
        {
            _handler = default;
            return;
        }

        _handler = new MessageInterpolatedStringHandler(literalLength, formattedCount);
    }

    public bool AppendLiteral(ReadOnlySpan<char> value)
    {
        return _handler.AppendLiteral(value);
    }

    public bool AppendFormatted<T>(T value) where T : ISpanFormattable
    {
        return _handler.AppendFormatted(value);
    }

    public bool AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
    {
        return _handler.AppendFormatted(value, format);
    }

    public bool AppendFormatted(ReadOnlySpan<char> value)
    {
        return _handler.AppendFormatted(value);
    }
}
