namespace GroveGames.Logger;

public sealed class LogFileFactory : ILogFileFactory
{
    private const string FolderName = "Logs";
    private readonly string _root;

    public LogFileFactory(string root)
    {
        _root = root;
    }

    public StreamWriter CreateFile()
    {
        var path = Path.Combine(_root, FolderName);
        var fileName = DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".log";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            return new StreamWriter(Path.Combine(path, fileName), true);
        }

        var directoryInfo = new DirectoryInfo(path);

        var files = directoryInfo.GetFiles();

        if (files.Length >= 10)
        {
            var sortedFiles = files.OrderByDescending(f => f.CreationTime);
            sortedFiles.Last().Delete();
        }

        return new StreamWriter(Path.Combine(path, fileName), true);
    }
}
