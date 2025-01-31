namespace GroveGames.Logger.Tests;

public class LoggerTests
{
    private class TestLogProcessor : ILogProcessor
    {
        public List<string> DebugMessages { get; } = [];
        public List<string> InfoMessages { get; } = [];
        public List<string> WarningMessages { get; } = [];
        public List<string> ErrorMessages { get; } = [];

        public void ProcessDebug(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            DebugMessages.Add($"{tag}: {message}");
        }

        public void ProcessInfo(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            InfoMessages.Add($"{tag}: {message}");
        }

        public void ProcessWarning(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            WarningMessages.Add($"{tag}: {message}");
        }

        public void ProcessError(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            ErrorMessages.Add($"{tag}: {message}");
        }
    }

    [Fact]
    public void Debug_ShouldCall_ProcessInfo_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        logger.Debug(tag, $"Debug message {i}");

        // Assert
        Assert.Contains("TestTag: Debug message 1", processor.DebugMessages);
    }

    [Fact]
    public void Debug_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var logger = new Logger();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        logger.AddProcessor(processor);
        logger.Debug("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Info("TestTag", $"Test message {initialAllocatedBytes}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Info_ShouldCall_ProcessInfo_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        logger.Info(tag, $"Info message {i}");

        // Assert
        Assert.Contains("TestTag: Info message 1", processor.InfoMessages);
    }

    [Fact]
    public void Info_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var logger = new Logger();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        logger.AddProcessor(processor);
        logger.Info("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        logger.Info("TestTag", $"Test message {initialAllocatedBytes}");

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
        logger.AddProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        logger.Warning(tag, $"Warning message {i}");

        // Assert
        Assert.Contains("TestTag: Warning message 1", processor.WarningMessages);
    }

    [Fact]
    public void Warning_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var logger = new Logger();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        logger.AddProcessor(processor);
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
        logger.AddProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        logger.Error(tag, $"Error message {i}");

        // Assert
        Assert.Contains("TestTag: Error message 1", processor.ErrorMessages);
    }

    [Fact]
    public void Error_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var logger = new Logger();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        logger.AddProcessor(processor);
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
        logger.AddProcessor(processor);

        // Assert
        logger.Info("Test", $"Message");
        Assert.Contains("Test: Message", processor.InfoMessages);
    }

    [Fact]
    public void RemoveProcessor_ShouldNotCallProcessorAfterRemoval()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddProcessor(processor);
        logger.RemoveProcessor(processor);

        // Act
        logger.Info("Test".AsSpan(), $"Message");

        // Assert
        Assert.DoesNotContain("Test: Message", processor.InfoMessages);
    }

    [Fact]
    public void Dispose_ShouldCallDispose_OnDisposableProcessors()
    {
        // Arrange
        var disposableProcessor = new TestLogProcessor();
        var logger = new Logger();
        logger.AddProcessor(disposableProcessor);

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
        logger.AddProcessor(processor);

        // Act & Assert
        var exception = Record.Exception(() => logger.Dispose());
        Assert.Null(exception);
    }
}