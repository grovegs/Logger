namespace GroveGames.Logger;

public sealed class LogFileFactory : ILogFileFactory
{
    private readonly string _root;
    private readonly string _folderName;
    private readonly int _maxFileCount;
    private readonly int _bufferSize;
    private readonly IFileSystem _fileSystem;
    private readonly ITimeProvider _timeProvider;

    public LogFileFactory(string root, string folderName, int maxFileCount, int bufferSize, IFileSystem? fileSystem = null, ITimeProvider? timeProvider = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        ArgumentException.ThrowIfNullOrEmpty(folderName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxFileCount);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(bufferSize);

        _root = root;
        _folderName = folderName;
        _maxFileCount = maxFileCount;
        _bufferSize = bufferSize;
        _fileSystem = fileSystem ?? new FileSystem();
        _timeProvider = timeProvider ?? new SystemTimeProvider();
    }

    public Stream CreateFile()
    {
        var path = Path.Combine(_root, _folderName);
        var fileName = GenerateFileName();
        var fullPath = Path.Combine(path, fileName);

        EnsureDirectoryExists(path);
        var fileStream = CreateFileStream(fullPath);
        CleanupOldFiles(path, fileName);

        return fileStream;
    }

    private string GenerateFileName()
    {
        return $"{_timeProvider.GetUtcNow():yyyyMMdd_HHmmss_fff}.log";
    }

    private void EnsureDirectoryExists(string path)
    {
        if (!_fileSystem.DirectoryExists(path))
        {
            _fileSystem.CreateDirectory(path);
        }
    }

    private Stream CreateFileStream(string fullPath)
    {
        return _fileSystem.CreateFileStream(
            fullPath,
            FileMode.Create,
            FileAccess.Write,
            FileShare.Read,
            _bufferSize,
            false
        );
    }

    private void CleanupOldFiles(string path, string currentFileName)
    {
        var files = _fileSystem.GetFiles(path, "*.log").ToArray();

        if (files.Length <= _maxFileCount)
        {
            return;
        }

        Array.Sort(files, static (left, right) => string.CompareOrdinal(left.FullPath, right.FullPath));
        var toDelete = files.Length - _maxFileCount;

        for (var i = 0; i < files.Length && toDelete > 0; i++)
        {
            if (string.Equals(Path.GetFileName(files[i].FullPath), currentFileName, StringComparison.Ordinal))
            {
                continue;
            }

            toDelete--;

            try
            {
                _fileSystem.DeleteFile(files[i].FullPath);
            }
            catch (Exception)
            {
            }
        }
    }
}
