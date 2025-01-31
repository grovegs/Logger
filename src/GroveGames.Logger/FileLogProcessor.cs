namespace GroveGames.Logger;

public sealed class FileLogProcessor : ILogProcessor
{
    private readonly IFileWriter _fileWriter;
    private readonly ILogFormatter _logFormatter;

    public FileLogProcessor(IFileWriter fileWriter, ILogFormatter logFormatter)
    {
        _fileWriter = fileWriter;
        _logFormatter = logFormatter;
    }

    public void ProcessInfo(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        ProcessLog(LogLevel.Info, tag, message);
    }

    public void ProcessWarning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        ProcessLog(LogLevel.Warning, tag, message);
    }

    public void ProcessError(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        ProcessLog(LogLevel.Error, tag, message);
    }

    private void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _logFormatter.GetBufferSize(level, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];
        _logFormatter.Format(buffer, level, tag, message);
        _fileWriter.AddToQueue(buffer);
    }
}
