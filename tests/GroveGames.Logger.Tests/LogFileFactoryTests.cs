using GroveGames.Logger;

public class LogFileFactoryTests
{
    [Fact]
    public void CreateFile_ShouldCreateLogFileInCorrectDirectory()
    {
        // Arrange
        string rootPath = "C:\\Logs";
        var factory = new LogFileFactory(rootPath);
        var expectedDirectory = Path.Combine(rootPath, "Logs");

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
        var factory = new LogFileFactory(Environment.CurrentDirectory);
        var directory = Path.Combine(Environment.CurrentDirectory, "ApplicationLogs");

        Directory.Delete(directory, true);
        Directory.CreateDirectory(directory);

        // Create 10 dummy files
        for (int i = 0; i < 10; i++)
        {
            File.Create(Path.Combine(directory, $"log_{i}.log")).Close();
        }

        // Act
        factory.CreateFile();

        // Assert
        var files = Directory.GetFiles(directory);
        Assert.Equal(10, files.Length);
    }
}
