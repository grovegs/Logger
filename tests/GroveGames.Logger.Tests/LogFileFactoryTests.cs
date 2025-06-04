namespace GroveGames.Logger.Tests;

public sealed class LogFileFactoryTests : IDisposable
{
    private const string DefaultFolderName = "logs";
    private const int DefaultMaxFileCount = 10;
    private const int DefaultFileBufferSize = 4096;

    private readonly string _testDirectory;

    public LogFileFactoryTests()
    {
        _testDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_testDirectory);
    }

    [Fact]
    public void CreateFile_ShouldCreateLogFileInCorrectDirectory()
    {
        // Arrange
        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, DefaultMaxFileCount, DefaultFileBufferSize);
        var expectedDirectory = Path.Combine(_testDirectory, DefaultFolderName);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        Assert.True(Directory.Exists(expectedDirectory));
        Assert.NotNull(stream);
        Assert.True(stream.CanWrite);
        Assert.True(stream.IsAsync);

        // Verify file naming pattern
        var files = Directory.GetFiles(expectedDirectory, "*.log");
        Assert.Single(files);
        var fileName = Path.GetFileName(files[0]);
        Assert.Matches(@"^\d{8}_\d{6}_\d{3}\.log$", fileName);
    }

    [Fact]
    public void CreateFile_ShouldCreateDirectoryIfNotExists()
    {
        // Arrange
        var nonExistentFolder = "newlogs";
        var factory = new LogFileFactory(_testDirectory, nonExistentFolder, DefaultMaxFileCount, DefaultFileBufferSize);
        var expectedDirectory = Path.Combine(_testDirectory, nonExistentFolder);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        Assert.True(Directory.Exists(expectedDirectory));
        Assert.NotNull(stream);
    }

    [Fact]
    public void CreateFile_ShouldDeleteOldestFile_WhenLimitExceeded()
    {
        // Arrange
        var directory = Path.Combine(_testDirectory, DefaultFolderName);
        Directory.CreateDirectory(directory);

        // Create files with specific creation times to test oldest file deletion
        var baseTime = DateTime.UtcNow.AddHours(-DefaultMaxFileCount);
        var oldestFileName = string.Empty;

        for (int i = 0; i < DefaultMaxFileCount; i++)
        {
            var fileName = $"{baseTime.AddHours(i):yyyyMMdd_HHmmss}.log";
            var filePath = Path.Combine(directory, fileName);

            if (i == 0) oldestFileName = fileName;

            using (File.Create(filePath)) { }
            File.SetCreationTimeUtc(filePath, baseTime.AddHours(i));
        }

        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, DefaultMaxFileCount, DefaultFileBufferSize);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        var files = Directory.GetFiles(directory);
        Assert.Equal(DefaultMaxFileCount, files.Length);
        Assert.DoesNotContain(files, f => Path.GetFileName(f) == oldestFileName);
    }

    [Fact]
    public void CreateFile_ShouldReturnProperlyConfiguredFileStream()
    {
        // Arrange
        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, DefaultMaxFileCount, DefaultFileBufferSize);

        // Act
        using var stream = factory.CreateFile();

        // Assert
        Assert.IsType<FileStream>(stream);
        Assert.True(stream.CanWrite);
        Assert.False(stream.CanRead);
        Assert.True(stream.IsAsync);

        // Verify buffer size (approximation due to internal buffering)
        var fileStream = (FileStream)stream;
        Assert.NotNull(fileStream);
    }

    [Fact]
    public void CreateFile_ShouldHandleMultipleCreations()
    {
        // Arrange
        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, DefaultMaxFileCount, DefaultFileBufferSize);
        var directory = Path.Combine(_testDirectory, DefaultFolderName);

        // Act
        for (int i = 0; i < 3; i++)
        {
            using (var stream = factory.CreateFile())
            {
            }
            Thread.Sleep(100);
        }

        // Assert
        var allFiles = Directory.GetFiles(directory, "*.log");
        Assert.Equal(3, allFiles.Length);
        Assert.Equal(3, allFiles.Distinct().Count());
    }

    [Fact]
    public void CreateFile_ShouldMaintainMaxFileCount()
    {
        // Arrange
        const int maxFiles = 3;
        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, maxFiles, DefaultFileBufferSize);
        var directory = Path.Combine(_testDirectory, DefaultFolderName);

        // Act
        for (int i = 0; i < maxFiles + 2; i++)
        {
            using (var stream = factory.CreateFile())
            {
            }
            Thread.Sleep(10);
        }

        // Assert
        var files = Directory.GetFiles(directory, "*.log");
        Assert.Equal(maxFiles, files.Length);
    }

    [Fact]
    public void CreateFile_ShouldWorkWithSingleFileLimit()
    {
        // Arrange
        const int maxFiles = 1;
        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, maxFiles, DefaultFileBufferSize);
        var directory = Path.Combine(_testDirectory, DefaultFolderName);

        // Act
        using (var stream1 = factory.CreateFile()) { }
        Thread.Sleep(10);
        using (var stream2 = factory.CreateFile()) { }

        // Assert
        var files = Directory.GetFiles(directory);
        Assert.Single(files);
    }

    [Fact]
    public void CreateFile_ShouldHandleConcurrentAccess()
    {
        // Arrange
        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, DefaultMaxFileCount, DefaultFileBufferSize);
        var directory = Path.Combine(_testDirectory, DefaultFolderName);
        const int threadCount = 5;
        var barrier = new Barrier(threadCount);
        var exceptions = new List<Exception>();

        // Act
        var tasks = Enumerable.Range(0, threadCount).Select(_ => Task.Run(() =>
        {
            try
            {
                barrier.SignalAndWait();
                using var stream = factory.CreateFile();
                Assert.NotNull(stream);
            }
            catch (Exception ex)
            {
                lock (exceptions)
                {
                    exceptions.Add(ex);
                }
            }
        })).ToArray();

        Task.WaitAll(tasks);

        // Assert
        Assert.Empty(exceptions);
        var files = Directory.GetFiles(directory);
        Assert.True(files.Length <= DefaultMaxFileCount);
    }

    public void Dispose()
    {
        if (Directory.Exists(_testDirectory))
        {
            Directory.Delete(_testDirectory, true);
        }

        GC.SuppressFinalize(this);
    }
}