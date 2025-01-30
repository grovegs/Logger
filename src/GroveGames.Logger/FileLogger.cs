namespace GroveGames.Logger;

public sealed class FileLogger : ILogger
{
    private readonly IFileWriter _fileWriter;

    private readonly List<ILogProcessor> _processors;

    private const int DateTimeSize = 19;
    private const int SeperatorSize = 9;

    public FileLogger(IFileWriter fileWriter)
    {
        _fileWriter = fileWriter;
        _processors = [];
    }

    public void Info(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        Log(LogLevel.Info, tag, message.Written);

        for (var i = 0; i < _processors.Count; i++)
        {
            _processors[i].ProcessInfo(tag, message.Written);
        }
    }

    public void Warning(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        Log(LogLevel.Warning, tag, message.Written);

        for (var i = 0; i < _processors.Count; i++)
        {
            _processors[i].ProcessWarning(tag, message.Written);
        }
    }

    public void Error(ReadOnlySpan<char> tag, LogInterpolatedStringHandler message)
    {
        Log(LogLevel.Error, tag, message.Written);

        for (var i = 0; i < _processors.Count; i++)
        {
            _processors[i].ProcessError(tag, message.Written);
        }
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

    public void AddProcessor(ILogProcessor processor)
    {
        _processors.Add(processor);
    }

    public void RemoveProcessor(ILogProcessor processor)
    {
        _processors.Remove(processor);
    }

    public void Dispose()
    {
        _fileWriter.Dispose();
    }
}
