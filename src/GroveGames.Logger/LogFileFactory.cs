namespace GroveGames.Logger;

public sealed class LogFileFactory : ILogFileFactory
{
    private readonly string _root;
    private readonly string _folderName;
    private readonly int _maxFileCount;
    private readonly int _bufferSize;

    public LogFileFactory(string root, string folderName, int maxFileCount, int bufferSize)
    {
        _root = root;
        _folderName = folderName;
        _maxFileCount = maxFileCount;
        _bufferSize = bufferSize;
    }

    public FileStream CreateFile()
    {
        var path = Path.Combine(_root, _folderName);
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}.log";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var directoryInfo = new DirectoryInfo(path);
        var files = directoryInfo.GetFiles();

        if (files.Length >= _maxFileCount)
        {
            var oldestFile = files.OrderBy(file => file.CreationTime).First();
            oldestFile.Delete();
        }

        return new FileStream(
            Path.Combine(path, fileName),
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            _bufferSize,
            true
        );
    }
}
