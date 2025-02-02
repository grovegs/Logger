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

    public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _logFormatter.GetBufferSize(level, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];
        _logFormatter.Format(buffer, level, tag, message);
        _fileWriter.AddToQueue(buffer);
    }
}
