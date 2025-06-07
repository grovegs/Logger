namespace GroveGames.Logger.Tests;

public sealed class LogLevelTests
{
    [Fact]
    public void LogLevel_ShouldHaveExpectedValues()
    {
        // Assert
        Assert.Equal(0, (int)LogLevel.Debug);
        Assert.Equal(1, (int)LogLevel.Information);
        Assert.Equal(2, (int)LogLevel.Warning);
        Assert.Equal(3, (int)LogLevel.Error);
        Assert.Equal(4, (int)LogLevel.None);
    }

    [Fact]
    public void LogLevel_ShouldSupportComparison()
    {
        // Assert
        Assert.True(LogLevel.Debug < LogLevel.Information);
        Assert.True(LogLevel.Debug < LogLevel.Warning);
        Assert.True(LogLevel.Debug < LogLevel.Error);
        Assert.True(LogLevel.Debug < LogLevel.None);

        Assert.True(LogLevel.Information < LogLevel.Warning);
        Assert.True(LogLevel.Information < LogLevel.Error);
        Assert.True(LogLevel.Information < LogLevel.None);

        Assert.True(LogLevel.Warning < LogLevel.Error);
        Assert.True(LogLevel.Warning < LogLevel.None);

        Assert.True(LogLevel.Error < LogLevel.None);
    }

    [Fact]
    public void LogLevel_ShouldSupportEquality()
    {
        // Assert
        Assert.Equal(LogLevel.Debug, LogLevel.Debug);
        Assert.Equal(LogLevel.Information, LogLevel.Information);
        Assert.Equal(LogLevel.Warning, LogLevel.Warning);
        Assert.Equal(LogLevel.Error, LogLevel.Error);
        Assert.Equal(LogLevel.None, LogLevel.None);

        Assert.NotEqual(LogLevel.Debug, LogLevel.Information);
        Assert.NotEqual(LogLevel.Information, LogLevel.Warning);
        Assert.NotEqual(LogLevel.Warning, LogLevel.Error);
        Assert.NotEqual(LogLevel.Error, LogLevel.None);
    }

    [Theory]
    [InlineData(LogLevel.Debug, "Debug")]
    [InlineData(LogLevel.Information, "Information")]
    [InlineData(LogLevel.Warning, "Warning")]
    [InlineData(LogLevel.Error, "Error")]
    [InlineData(LogLevel.None, "None")]
    public void LogLevel_ToString_ShouldReturnExpectedString(LogLevel level, string expected)
    {
        // Act
        var result = level.ToString();

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void LogLevel_ShouldBeDefinedForAllValues()
    {
        // Assert
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Debug));
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Information));
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Warning));
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.Error));
        Assert.True(Enum.IsDefined(typeof(LogLevel), LogLevel.None));
    }

    [Fact]
    public void LogLevel_ShouldHaveCorrectCount()
    {
        // Act
        var values = Enum.GetValues<LogLevel>();

        // Assert
        Assert.Equal(5, values.Length);
        Assert.Contains(LogLevel.Debug, values);
        Assert.Contains(LogLevel.Information, values);
        Assert.Contains(LogLevel.Warning, values);
        Assert.Contains(LogLevel.Error, values);
        Assert.Contains(LogLevel.None, values);
    }

    [Fact]
    public void LogLevel_MinimumLevelFiltering_ShouldWorkCorrectly()
    {
        // Arrange & Act & Assert

        Assert.True(LogLevel.Information >= LogLevel.Debug);
        Assert.True(LogLevel.Warning >= LogLevel.Debug);
        Assert.True(LogLevel.Error >= LogLevel.Debug);
        Assert.True(LogLevel.None >= LogLevel.Debug);

        Assert.False(LogLevel.Debug >= LogLevel.Information);
        Assert.True(LogLevel.Warning >= LogLevel.Information);
        Assert.True(LogLevel.Error >= LogLevel.Information);
        Assert.True(LogLevel.None >= LogLevel.Information);

        Assert.False(LogLevel.Debug >= LogLevel.Warning);
        Assert.False(LogLevel.Information >= LogLevel.Warning);
        Assert.True(LogLevel.Error >= LogLevel.Warning);
        Assert.True(LogLevel.None >= LogLevel.Warning);

        Assert.False(LogLevel.Debug >= LogLevel.Error);
        Assert.False(LogLevel.Information >= LogLevel.Error);
        Assert.False(LogLevel.Warning >= LogLevel.Error);
        Assert.True(LogLevel.None >= LogLevel.Error);

        Assert.False(LogLevel.Debug >= LogLevel.None);
        Assert.False(LogLevel.Information >= LogLevel.None);
        Assert.False(LogLevel.Warning >= LogLevel.None);
        Assert.False(LogLevel.Error >= LogLevel.None);
    }

    [Theory]
    [InlineData("Debug", LogLevel.Debug)]
    [InlineData("Information", LogLevel.Information)]
    [InlineData("Warning", LogLevel.Warning)]
    [InlineData("Error", LogLevel.Error)]
    [InlineData("None", LogLevel.None)]
    public void LogLevel_Parse_ShouldWorkCorrectly(string input, LogLevel expected)
    {
        // Act
        var result = Enum.Parse<LogLevel>(input);

        // Assert
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("debug", true, LogLevel.Debug)]
    [InlineData("DEBUG", true, LogLevel.Debug)]
    [InlineData("information", true, LogLevel.Information)]
    [InlineData("INFORMATION", true, LogLevel.Information)]
    [InlineData("InvalidLevel", false, default(LogLevel))]
    public void LogLevel_TryParse_ShouldWorkCorrectly(string input, bool expectedSuccess, LogLevel expectedValue)
    {
        // Act
        var success = Enum.TryParse<LogLevel>(input, ignoreCase: true, out var result);

        // Assert
        Assert.Equal(expectedSuccess, success);
        if (expectedSuccess)
        {
            Assert.Equal(expectedValue, result);
        }
    }
}