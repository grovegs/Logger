namespace GroveGames.Logger;

public sealed class FileLogProcessor : ILogProcessor
{
    private const int DateTimeSize = 19;
    private const int SeperatorSize = 9;

    private readonly IFileWriter _fileWriter;

    public FileLogProcessor(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter;
    }

    public void ProcessInfo(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Log(LogLevel.Info, tag, message);
    }

    public void ProcessWarning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Log(LogLevel.Warning, tag, message);
    }

    public void ProcessError(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Log(LogLevel.Error, tag, message);
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
        now.TryFormat(dateTimeBuffer, out _, "yyyy-MM-dd HH:mm:ss");

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
}
