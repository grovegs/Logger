namespace GroveGames.Logger.Tests;

public class LogTests
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

        public void ProcessInformation(ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
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
    public void Debug_ShouldCall_ProcessInformation_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var log = new Log();
        log.AddProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        log.Debug(tag, $"Debug message {i}");

        // Assert
#if DEBUG
        Assert.Contains("TestTag: Debug message 1", processor.DebugMessages);
#else
        Assert.Empty(processor.DebugMessages);
#endif
    }

    [Fact]
    public void Debug_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var log = new Log();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        log.AddProcessor(processor);
        log.Debug("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        log.Information("TestTag", $"Test message {initialAllocatedBytes}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Information_ShouldCall_ProcessInformation_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var log = new Log();
        log.AddProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        log.Information(tag, $"Info message {i}");

        // Assert
        Assert.Contains("TestTag: Info message 1", processor.InfoMessages);
    }

    [Fact]
    public void Information_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var log = new Log();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        log.AddProcessor(processor);
        log.Information("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        log.Information("TestTag", $"Test message {initialAllocatedBytes}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Warning_ShouldCall_ProcessWarning_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var log = new Log();
        log.AddProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        log.Warning(tag, $"Warning message {i}");

        // Assert
        Assert.Contains("TestTag: Warning message 1", processor.WarningMessages);
    }

    [Fact]
    public void Warning_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var log = new Log();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        log.AddProcessor(processor);
        log.Warning("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        log.Warning("TestTag", $"Test message {float.Epsilon}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void Error_ShouldCall_ProcessError_OnAllProcessors()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var log = new Log();
        log.AddProcessor(processor);

        var tag = "TestTag";
        var i = 1;

        // Act
        log.Error(tag, $"Error message {i}");

        // Assert
        Assert.Contains("TestTag: Error message 1", processor.ErrorMessages);
    }

    [Fact]
    public void Error_ShouldNotCauseHeapAllocation()
    {
        // Arrange
        var log = new Log();
        var testFileWriter = new TestFileWriterAllocation();
        var processor = new FileLogProcessor(testFileWriter, new FileLogFormatter());
        log.AddProcessor(processor);
        log.Error("Warmup", $"Warmup message {42}");

        long initialAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Act
        log.Error("TestTag", $"Test message {int.MaxValue}");

        long finalAllocatedBytes = GC.GetAllocatedBytesForCurrentThread();

        // Assert
        Assert.Equal(initialAllocatedBytes, finalAllocatedBytes);
    }

    [Fact]
    public void AddProcessor_ShouldStoreProcessor()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var log = new Log();

        // Act
        log.AddProcessor(processor);

        // Assert
        log.Information("Test", $"Message");
        Assert.Contains("Test: Message", processor.InfoMessages);
    }

    [Fact]
    public void RemoveProcessor_ShouldNotCallProcessorAfterRemoval()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var log = new Log();
        log.AddProcessor(processor);
        log.RemoveProcessor(processor);

        // Act
        log.Information("Test".AsSpan(), $"Message");

        // Assert
        Assert.DoesNotContain("Test: Message", processor.InfoMessages);
    }

    [Fact]
    public void Dispose_ShouldCallDispose_OnDisposableProcessors()
    {
        // Arrange
        var disposableProcessor = new TestLogProcessor();
        var log = new Log();
        log.AddProcessor(disposableProcessor);

        // Act
        log.Dispose();

        // Assert
        // No specific assertion needed for this test as TestLogProcessor does not implement IDisposable
    }

    [Fact]
    public void Dispose_ShouldNotThrow_WhenProcessorsAreNotDisposable()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var log = new Log();
        log.AddProcessor(processor);

        // Act & Assert
        var exception = Record.Exception(() => log.Dispose());
        Assert.Null(exception);
    }
}