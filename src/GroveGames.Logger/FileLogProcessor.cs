namespace GroveGames.Logger;

public sealed class FileLogProcessor : ILogProcessor
{
    private readonly IFileWriter _fileWriter;
    private readonly ILogFormatter _formatter;

    public FileLogProcessor(IFileWriter fileWriter, ILogFormatter formatter)
    {
        _fileWriter = fileWriter;
        _formatter = formatter;
    }

    public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var bufferSize = _formatter.GetBufferSize(level, tag, message);
        Span<char> buffer = stackalloc char[bufferSize];
        _formatter.Format(buffer, level, tag, message);
        _fileWriter.AddEntry(buffer);
    }
}
