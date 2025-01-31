namespace GroveGames.Logger;

public sealed class LogFileFactory : ILogFileFactory
{
    private const string FolderName = "logs";
    private readonly string _root;
    private const int MaxLogFiles = 10;

    public LogFileFactory(string root)
    {
        _root = root;
    }

    public StreamWriter CreateFile()
    {
        var path = Path.Combine(_root, FolderName);
        var fileName = $"{DateTime.UtcNow:yyyyMMdd_HHmmss}.log";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        var directoryInfo = new DirectoryInfo(path);

        var files = directoryInfo.GetFiles();

        if (files.Length >= MaxLogFiles)
        {
            var oldestFile = files.OrderBy(f => f.CreationTime).First();
            oldestFile.Delete();
        }

        return new StreamWriter(Path.Combine(path, fileName), true);
    }
}
