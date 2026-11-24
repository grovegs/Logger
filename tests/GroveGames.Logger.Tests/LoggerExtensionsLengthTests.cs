namespace GroveGames.Logger.Tests;

public sealed class LoggerExtensionsLengthTests
{
    private const int MaxLength = MessageInterpolatedStringHandler.MaxLength;
    private const string Marker = MessageInterpolatedStringHandler.TruncationMarker;

    private sealed class RecordingLogger : ILogger
    {
        private string _message = string.Empty;

        public LogLevel MinimumLevel => LogLevel.Debug;
        public string Message => _message;

        public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            _message = message.ToString();
        }
    }

    [Fact]
    public void LogInformation_ValueLongerThanTheFirstBuffer_LogsTheWholeMessage()
    {
        var logger = new RecordingLogger();
        var value = new string('a', 2000);

        logger.LogInformation("Tag", $"Values: {value}!");

        Assert.Equal("Values: " + value + "!", logger.Message);
    }

    [Fact]
    public void LogInformation_ManyValuesPastTheFirstEstimate_LogsTheWholeMessage()
    {
        var logger = new RecordingLogger();
        var id = new string('b', 40);

        logger.LogInformation("Tag", $"{id},{id},{id},{id}");

        Assert.Equal(string.Join(",", id, id, id, id), logger.Message);
    }

    [Fact]
    public void LogInformation_MessageAtTheLimit_IsNotTruncated()
    {
        var logger = new RecordingLogger();
        var value = new string('a', MaxLength);

        logger.LogInformation("Tag", $"{value}");

        Assert.Equal(value, logger.Message);
    }

    [Fact]
    public void LogInformation_MessagePastTheLimit_IsCutAtTheLimitWithAMarker()
    {
        var logger = new RecordingLogger();
        var value = new string('a', MaxLength * 3);

        logger.LogInformation("Tag", $"Values: {value} end {42}");

        Assert.Equal(MaxLength + Marker.Length, logger.Message.Length);
        Assert.StartsWith("Values: aaa", logger.Message);
        Assert.EndsWith("a" + Marker, logger.Message);
    }

    [Fact]
    public void LogError_NumberThatCrossesTheLimit_IsCutAtTheLimitWithAMarker()
    {
        var logger = new RecordingLogger();
        var value = new string('a', MaxLength - 2);

        logger.LogError("Tag", $"{value}{123456}");

        Assert.Equal(value + "12" + Marker, logger.Message);
    }
}
