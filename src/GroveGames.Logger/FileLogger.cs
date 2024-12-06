namespace GroveGames.Logger;

public class FileLogger : ILogger
{
    private readonly IFileWriter _fileWriter;

    private static ReadOnlySpan<char> ErrorLevel => "ERROR";
    private static ReadOnlySpan<char> WarningLevel => "WARNING";
    private static ReadOnlySpan<char> InfoLevel => "INFO";

    private const int DateTimeSize = 19;
    private const int SeperatorSize = 9;

    public FileLogger(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    public void Error(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Log(ErrorLevel, tag, message);
    }

    public void Info(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Log(InfoLevel, tag, message);
    }

    public void Warning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Log(WarningLevel, tag, message);
    }

    private void Log(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Span<char> buffer = stackalloc char[level.Length + tag.Length + message.Length + DateTimeSize + SeperatorSize];
        Format(buffer, level, tag, message);
        _fileWriter.AddToQueue(buffer);
    }

    private void Format(Span<char> buffer, ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Span<char> dateTimeBuffer = stackalloc char[DateTimeSize];
        var now = DateTime.UtcNow;
        if (!now.TryFormat(dateTimeBuffer, out int charsWritten, "yyyy-MM-dd HH:mm:ss"))
        {
            //Exception
        }

        ReadOnlySpan<char> seperator = " | ";
        var seperatorLength = seperator.Length;

        var offset = 0;

        var dateTimeLength = dateTimeBuffer.Length;
        dateTimeBuffer.CopyTo(buffer.Slice(offset, dateTimeLength));
        offset += dateTimeLength;

        seperator.CopyTo(buffer.Slice(offset, seperatorLength));
        offset += seperatorLength;

        var levelLength = level.Length;
        level.CopyTo(buffer.Slice(offset, levelLength));
        offset += levelLength;

        seperator.CopyTo(buffer.Slice(offset, seperatorLength));
        offset += seperatorLength;

        var tagLength = tag.Length;
        tag.CopyTo(buffer.Slice(offset, tagLength));
        offset += tagLength;

        seperator.CopyTo(buffer.Slice(offset, seperatorLength));
        offset += seperatorLength;

        var messageLength = message.Length;
        message.CopyTo(buffer.Slice(offset, messageLength));
    }

    public void Dispose()
    {
        _fileWriter.Dispose();
    }
}
