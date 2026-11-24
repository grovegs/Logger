using System.Buffers;

using Godot;

namespace GroveGames.Logger.Godot;

public sealed class ConsoleLogProcessor : ILogProcessor
{
    private const int StackBufferSize = 1024;

    private readonly ILogFormatter _logFormatter;

    public ConsoleLogProcessor(ILogFormatter logFormatter)
    {
        _logFormatter = logFormatter;
    }

    public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var log = Format(level, tag, message);

        switch (level)
        {
            case LogLevel.Debug:
                GD.Print(log);
                break;
            case LogLevel.Information:
                GD.Print(log);
                break;
            case LogLevel.Warning:
                GD.PrintRich(log);
                break;
            case LogLevel.Error:
                GD.PrintErr(log);
                break;
        }
    }

    private string Format(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _logFormatter.GetBufferSize(level, tag, message);

        if (bufferSize <= StackBufferSize)
        {
            Span<char> buffer = stackalloc char[bufferSize];
            _logFormatter.Format(buffer, level, tag, message);
            return buffer.ToString();
        }

        var rented = ArrayPool<char>.Shared.Rent(bufferSize);

        try
        {
            var buffer = rented.AsSpan(0, bufferSize);
            _logFormatter.Format(buffer, level, tag, message);
            return buffer.ToString();
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rented);
        }
    }
}
