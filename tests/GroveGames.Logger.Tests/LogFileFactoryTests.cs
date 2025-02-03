namespace GroveGames.Logger.Tests;

public class LogFileFactoryTests : IDisposable
{
    private const string DefaultFolderName = "logs";
    private const int DefaultMaxFileCount = 10;

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
        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, DefaultMaxFileCount);
        var expectedDirectory = Path.Combine(_testDirectory, DefaultFolderName);

        // Act
        using (var stream = factory.CreateFile())
        {
            // Assert
            Assert.True(Directory.Exists(expectedDirectory));
            Assert.NotNull(stream);
        }
    }

    [Fact]
    public void CreateFile_ShouldDeleteOldestFile_WhenLimitExceeded()
    {
        // Arrange
        var directory = Path.Combine(_testDirectory, DefaultFolderName);
        Directory.CreateDirectory(directory);

        for (int i = 0; i < DefaultMaxFileCount; i++)
        {
            using (File.Create(Path.Combine(directory, $"log_{i}.log"))) { }
        }

        var factory = new LogFileFactory(_testDirectory, DefaultFolderName, DefaultMaxFileCount);

        // Act
        factory.CreateFile();

        // Assert
        var files = Directory.GetFiles(directory);
        Assert.Equal(DefaultMaxFileCount, files.Length);
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