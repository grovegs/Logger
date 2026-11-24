using System.Buffers;

namespace GroveGames.Logger;

public sealed class FileLogProcessor : ILogProcessor, IDisposable
{
    private const int StackBufferSize = 1024;

    private readonly IStreamWriter _writer;
    private readonly ILogFormatter _formatter;

    public FileLogProcessor(IStreamWriter writer, ILogFormatter formatter)
    {
        ArgumentNullException.ThrowIfNull(writer);
        ArgumentNullException.ThrowIfNull(formatter);

        _writer = writer;
        _formatter = formatter;
    }

    public void Dispose()
    {
        _writer.Dispose();
    }

    public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _formatter.GetBufferSize(level, tag, message);

        if (bufferSize <= StackBufferSize)
        {
            Span<char> buffer = stackalloc char[bufferSize];
            _formatter.Format(buffer, level, tag, message);
            _writer.AddEntry(buffer);
            return;
        }

        var rented = ArrayPool<char>.Shared.Rent(bufferSize);

        try
        {
            var buffer = rented.AsSpan(0, bufferSize);
            _formatter.Format(buffer, level, tag, message);
            _writer.AddEntry(buffer);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(rented);
        }
    }
}
