namespace GroveGames.Logger;

public sealed class LogFileFactory : ILogFileFactory
{
    private readonly string _root;
    private readonly string _folderName;
    private readonly int _maxFileCount;

    public LogFileFactory(string root, string folderName, int maxfileCount)
    {
        _root = root;
        _folderName = folderName;
        _maxFileCount = maxfileCount;
    }

    public StreamWriter CreateFile()
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

        return new StreamWriter(Path.Combine(path, fileName), true);
    }
}
