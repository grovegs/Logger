namespace GroveGames.Logger.Tests;

public sealed class LogFileFactoryTests
{
    private sealed class TestFileSystem : IFileSystem
    {
        private bool _directoryExistsResult;
        private IEnumerable<FileInfo> _getFilesResult;
        private Stream _createFileStreamResult;
        private bool _createDirectoryCalled;
        private string? _createDirectoryPath;
        private bool _deleteFileCalled;
        private string? _deleteFilePath;
        private string? _createFileStreamPath;
        private FileMode _createFileStreamMode;
        private FileAccess _createFileStreamAccess;
        private FileShare _createFileStreamShare;
        private int _createFileStreamBufferSize;
        private bool _createFileStreamUseAsync;

        public bool DirectoryExistsResult => _directoryExistsResult;
        public IEnumerable<FileInfo> GetFilesResult => _getFilesResult;
        public Stream CreateFileStreamResult => _createFileStreamResult;

        public bool CreateDirectoryCalled => _createDirectoryCalled;
        public string? CreateDirectoryPath => _createDirectoryPath;

        public bool DeleteFileCalled => _deleteFileCalled;
        public string? DeleteFilePath => _deleteFilePath;

        public string? CreateFileStreamPath => _createFileStreamPath;
        public FileMode CreateFileStreamMode => _createFileStreamMode;
        public FileAccess CreateFileStreamAccess => _createFileStreamAccess;
        public FileShare CreateFileStreamShare => _createFileStreamShare;
        public int CreateFileStreamBufferSize => _createFileStreamBufferSize;
        public bool CreateFileStreamUseAsync => _createFileStreamUseAsync;

        public TestFileSystem()
        {
            _createFileStreamResult = new MemoryStream();
            _getFilesResult = Array.Empty<FileInfo>();
        }

        public void SetCreateFileStreamResult(Stream value)
        {
            _createFileStreamResult = value;
        }

        public void SetGetFilesResult(IEnumerable<FileInfo> value)
        {
            _getFilesResult = value;
        }

        public void SetDirectoryExistsResult(bool value)
        {
            _directoryExistsResult = value;
        }

        public bool DirectoryExists(string path) => DirectoryExistsResult;

        public void CreateDirectory(string path)
        {
            _createDirectoryCalled = true;
            _createDirectoryPath = path;
        }

        public Stream CreateFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            _createFileStreamPath = path;
            _createFileStreamMode = mode;
            _createFileStreamAccess = access;
            _createFileStreamShare = share;
            _createFileStreamBufferSize = bufferSize;
            _createFileStreamUseAsync = useAsync;
            return CreateFileStreamResult;
        }

        public IEnumerable<FileInfo> GetFiles(string path, string searchPattern) => GetFilesResult;

        public void DeleteFile(string path)
        {
            _deleteFileCalled = true;
            _deleteFilePath = path;
        }
    }

    private sealed class TestTimeProvider : ITimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public DateTimeOffset GetUtcNow() => _utcNow;
    }

    private sealed class ConcurrentTestFileSystem : IFileSystem
    {
        private int _callCount;

        public int CreateFileStreamCallCount => _callCount;

        public bool DirectoryExists(string path) => true;
        public void CreateDirectory(string path) { }
        public Stream CreateFileStream(string path, FileMode mode, FileAccess access, FileShare share, int bufferSize, bool useAsync)
        {
            Interlocked.Increment(ref _callCount);
            return new MemoryStream();
        }
        public IEnumerable<FileInfo> GetFiles(string path, string searchPattern) => Array.Empty<FileInfo>();
        public void DeleteFile(string path) { }
    }

    private sealed class ConcurrentTestTimeProvider : ITimeProvider
    {
        private int _counter;

        public DateTimeOffset GetUtcNow()
        {
            var milliseconds = Interlocked.Increment(ref _counter);
            return new DateTimeOffset(2024, 1, 15, 10, 30, 45, 0, TimeSpan.Zero).AddMilliseconds(milliseconds);
        }
    }

    [Fact]
    public void Constructor_NullRoot_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => new LogFileFactory(null!, "logs", 10, 1024));
    }

    [Fact]
    public void Constructor_EmptyRoot_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new LogFileFactory(string.Empty, "logs", 10, 1024));
    }

    [Fact]
    public void Constructor_NullFolderName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentNullException>(() => new LogFileFactory("/root", null!, 10, 1024));
    }

    [Fact]
    public void Constructor_EmptyFolderName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new LogFileFactory("/root", string.Empty, 10, 1024));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidMaxFileCount_ThrowsArgumentOutOfRangeException(int maxFileCount)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LogFileFactory("/root", "logs", maxFileCount, 1024));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_InvalidBufferSize_ThrowsArgumentOutOfRangeException(int bufferSize)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new LogFileFactory("/root", "logs", 10, bufferSize));
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Act
        var factory = new LogFileFactory("/root", "logs", 10, 1024);

        // Assert
        Assert.NotNull(factory);
    }

    [Fact]
    public void CreateFile_DirectoryDoesNotExist_CreatesDirectory()
    {
        // Arrange
        var fileSystem = new TestFileSystem();
        fileSystem.SetDirectoryExistsResult(false);
        fileSystem.SetGetFilesResult(Array.Empty<FileInfo>());
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero));
        var factory = new LogFileFactory("/root", "logs", 10, 1024, fileSystem, timeProvider);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        Assert.True(fileSystem.CreateDirectoryCalled);
        Assert.Equal("/root/logs", fileSystem.CreateDirectoryPath);
    }

    [Fact]
    public void CreateFile_DirectoryExists_DoesNotCreateDirectory()
    {
        // Arrange
        var fileSystem = new TestFileSystem();
        fileSystem.SetDirectoryExistsResult(true);
        fileSystem.SetGetFilesResult(Array.Empty<FileInfo>());
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero));
        var factory = new LogFileFactory("/root", "logs", 10, 1024, fileSystem, timeProvider);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        Assert.False(fileSystem.CreateDirectoryCalled);
    }

    [Fact]
    public void CreateFile_GeneratesCorrectFileName()
    {
        // Arrange
        var fileSystem = new TestFileSystem();
        fileSystem.SetDirectoryExistsResult(true);
        fileSystem.SetGetFilesResult(Array.Empty<FileInfo>());
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero));
        var factory = new LogFileFactory("/root", "logs", 10, 1024, fileSystem, timeProvider);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        Assert.Equal("/root/logs/20240115_103045_123.log", fileSystem.CreateFileStreamPath);
        Assert.Equal(FileMode.Create, fileSystem.CreateFileStreamMode);
        Assert.Equal(FileAccess.Write, fileSystem.CreateFileStreamAccess);
        Assert.Equal(FileShare.Read, fileSystem.CreateFileStreamShare);
        Assert.Equal(1024, fileSystem.CreateFileStreamBufferSize);
        Assert.True(fileSystem.CreateFileStreamUseAsync);
    }

    [Fact]
    public void CreateFile_FileCountBelowMax_DoesNotDeleteFiles()
    {
        // Arrange
        var existingFiles = new[]
        {
            new FileInfo("/root/logs/20240101_120000_000.log", new DateTime(2024, 1, 1)),
            new FileInfo("/root/logs/20240102_120000_000.log", new DateTime(2024, 1, 2))
        };
        var fileSystem = new TestFileSystem();
        fileSystem.SetDirectoryExistsResult(true);
        fileSystem.SetGetFilesResult(existingFiles);
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero));
        var factory = new LogFileFactory("/root", "logs", 5, 1024, fileSystem, timeProvider);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        Assert.False(fileSystem.DeleteFileCalled);
    }

    [Fact]
    public void CreateFile_FileCountExceedsMax_DeletesOldestFile()
    {
        // Arrange
        var existingFiles = new[]
        {
            new FileInfo("/root/logs/20240101_120000_000.log", new DateTime(2024, 1, 1)),
            new FileInfo("/root/logs/20240103_120000_000.log", new DateTime(2024, 1, 3)),
            new FileInfo("/root/logs/20240102_120000_000.log", new DateTime(2024, 1, 2))
        };
        var fileSystem = new TestFileSystem();
        fileSystem.SetDirectoryExistsResult(true);
        fileSystem.SetGetFilesResult(existingFiles);
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero));
        var factory = new LogFileFactory("/root", "logs", 2, 1024, fileSystem, timeProvider);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        Assert.True(fileSystem.DeleteFileCalled);
        Assert.Equal("/root/logs/20240101_120000_000.log", fileSystem.DeleteFilePath);
    }

    [Fact]
    public void CreateFile_ReturnsValidStream()
    {
        // Arrange
        var expectedStream = new MemoryStream();
        var fileSystem = new TestFileSystem();
        fileSystem.SetDirectoryExistsResult(true);
        fileSystem.SetGetFilesResult(Array.Empty<FileInfo>());
        fileSystem.SetCreateFileStreamResult(expectedStream);
        var timeProvider = new TestTimeProvider(new DateTimeOffset(2024, 1, 15, 10, 30, 45, 123, TimeSpan.Zero));
        var factory = new LogFileFactory("/root", "logs", 10, 1024, fileSystem, timeProvider);

        // Act
        var stream = factory.CreateFile();

        // Assert
        Assert.Same(expectedStream, stream);
        stream.Dispose();
    }

    [Fact]
    public async Task CreateFile_ConcurrentCalls_CreatesMultipleFiles()
    {
        // Arrange
        var fileSystem = new ConcurrentTestFileSystem();
        var timeProvider = new ConcurrentTestTimeProvider();
        var factory = new LogFileFactory("/root", "logs", 100, 1024, fileSystem, timeProvider);

        // Act
        var tasks = Enumerable.Range(0, 10).Select(_ => Task.Run(() =>
        {
            using var stream = factory.CreateFile();
        })).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(10, fileSystem.CreateFileStreamCallCount);
    }
}
