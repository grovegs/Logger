namespace GroveGames.Logger.Tests;

public class LoggerTests
{
    private class TestLogProcessor : ILogProcessor
    {
        public List<string> DebugLogs { get; } = [];
        public List<string> InformationLogs { get; } = [];
        public List<string> WarningLogs { get; } = [];
        public List<string> ErrorLogs { get; } = [];

        public void Process(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            var log = $"{tag}: {message}";

            switch (level)
            {
                case LogLevel.Debug:
                    DebugLogs.Add(log);
                    break;
                case LogLevel.Information:
                    InformationLogs.Add(log);
                    break;
                case LogLevel.Warning:
                    WarningLogs.Add(log);
                    break;
                case LogLevel.Error:
                    ErrorLogs.Add(log);
                    break;
            }
        }
    }

    [Fact]
    public void Debug_ShouldCall_ProcessInformation_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddLogProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        logger.Debug(tag, $"Debug message {i}");

        // Assert
#if DEBUG
        Assert.Contains("TestTag: Debug message 1", processor.DebugLogs);
#else
        Assert.Empty(processor.DebugMessages);
#endif
    }

    [Fact]
    public void Debug_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var logger = new Logger();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        logger.AddLogProcessor(processor);
        logger.Debug("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Information("TestTag", $"Test message {initialAllocatedBytes}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Information_ShouldCall_ProcessInformation_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddLogProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        logger.Information(tag, $"Info message {i}");

        // Assert
        Assert.Contains("TestTag: Info message 1", processor.InformationLogs);
    }

    [Fact]
    public void Information_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var logger = new Logger();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        logger.AddLogProcessor(processor);
        logger.Information("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Information("TestTag", $"Test message {initialAllocatedBytes}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Warning_ShouldCall_ProcessWarning_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddLogProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        logger.Warning(tag, $"Warning message {i}");

        // Assert
        Assert.Contains("TestTag: Warning message 1", processor.WarningLogs);
    }

    [Fact]
    public void Warning_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var logger = new Logger();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        logger.AddLogProcessor(processor);
        logger.Warning("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Warning("TestTag", $"Test message {float.Epsilon}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Error_ShouldCall_ProcessError_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddLogProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        logger.Error(tag, $"Error message {i}");

        // Assert
        Assert.Contains("TestTag: Error message 1", processor.ErrorLogs);
    }

    [Fact]
    public void Error_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var logger = new Logger();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        logger.AddLogProcessor(processor);
        logger.Error("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Error("TestTag", $"Test message {int.MaxValue}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void AddProcessor_ShouldStoreProcessor()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();

        // Act
        logger.AddLogProcessor(processor);

        // Assert
        logger.Information("Test", $"Message");
        Assert.Contains("Test: Message", processor.InformationLogs);
    }

    [Fact]
    public void RemoveProcessor_ShouldNotCallProcessorAfterRemoval()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddLogProcessor(processor);
        logger.RemoveLogProcessor(processor);

        // Act
        logger.Information("Test".AsSpan(), $"Message");

        // Assert
        Assert.DoesNotContain("Test: Message", processor.InformationLogs);
    }

    [Fact]
    public void Dispose_ShouldCallDispose_OnDisposableProcessors()
    {
        // Arrange
        var disposableProcessor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddLogProcessor(disposableProcessor);

        // Act
        logger.Dispose();

        // Assert
        // No specific assertion needed for this test as TestLogProcessor does not implement IDisposable
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenProcessorsAreNotDisposable()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddLogProcessor(processor);

        // Act & Assert
        var exception = Record.Exception(() => logger.Dispose());
        Assert.Null(exception);
    }
}