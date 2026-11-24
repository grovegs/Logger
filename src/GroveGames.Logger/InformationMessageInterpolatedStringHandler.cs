using System.Runtime.CompilerServices;

using GroveGames.Globalization;

namespace GroveGames.Logger;

[InterpolatedStringHandler]
public ref struct InformationMessageInterpolatedStringHandler
{
    private readonly bool _isEnabled;
    private TextInterpolatedStringHandler _handler;

    public readonly ReadOnlySpan<char> Written => _handler.Written;

    public InformationMessageInterpolatedStringHandler(int literalLength, int formattedCount, ILogger logger)
    {
        _isEnabled = logger.MinimumLevel <= LogLevel.Information;
        _handler = _isEnabled ? new TextInterpolatedStringHandler(literalLength, formattedCount) : default;
    }

    public void Dispose()
    {
        _handler.Dispose();
    }

    public bool AppendLiteral(ReadOnlySpan<char> value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(ReadOnlySpan<char> value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(string? value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(char value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(int value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(int value, string? format)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value, format);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(long value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(long value, string? format)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value, format);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(float value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(float value, string? format)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value, format);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(double value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted(double value, string? format)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value, format);
        }

        return _isEnabled;
    }

#if NET10_0_OR_GREATER
    public bool AppendFormatted<T>(T value) where T : ISpanFormattable
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted<T>(T value, string? format) where T : ISpanFormattable
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value, format);
        }

        return _isEnabled;
    }
#else
    public bool AppendFormatted<T>(T value)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value);
        }

        return _isEnabled;
    }

    public bool AppendFormatted<T>(T value, string? format)
    {
        if (_isEnabled)
        {
            _handler.AppendFormatted(value, format);
        }

        return _isEnabled;
    }
#endif
}
