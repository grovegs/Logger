namespace GroveGames.Logger.Tests;

public class LogFileFactoryTests : IDisposable
{
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
        var factory = new LogFileFactory(_testDirectory);
        var expectedDirectory = Path.Combine(_testDirectory, "logs");

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
        var directory = Path.Combine(_testDirectory, "logs");
        Directory.CreateDirectory(directory);

        for (int i = 0; i < 10; i++)
        {
            using (File.Create(Path.Combine(directory, $"log_{i}.log"))) { }
        }

        var factory = new LogFileFactory(_testDirectory);

        // Act
        factory.CreateFile();

        // Assert
        var files = Directory.GetFiles(directory);
        Assert.Equal(10, files.Length);
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