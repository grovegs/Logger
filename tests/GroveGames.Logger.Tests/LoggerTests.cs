namespace GroveGames.Logger.Tests;

public sealed class LoggerTests
{
    private sealed class TestLogProcessor : ILogProcessor
    {
        public int ProcessLogCallCount { get; private set; }
        public LogLevel LastLogLevel { get; private set; }
        public string LastTag { get; private set; } = string.Empty;
        public string LastMessage { get; private set; } = string.Empty;

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            ProcessLogCallCount++;
            LastLogLevel = level;
            LastTag = tag.ToString();
            LastMessage = message.ToString();
        }
    }

    private sealed class DisposableTestLogProcessor : ILogProcessor, IDisposable
    {
        public bool IsDisposed { get; private set; }
        public int DisposeCallCount { get; private set; }

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
        }

        public void Dispose()
        {
            DisposeCallCount++;
            IsDisposed = true;
        }
    }

    private sealed class ConcurrentTestLogProcessor : ILogProcessor
    {
        private int _callCount;

        public int ProcessLogCallCount => _callCount;

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            Interlocked.Increment(ref _callCount);
        }
    }

    [Fact]
    public void Constructor_NullProcessors_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new Logger(null!, [], LogLevel.Information));
    }

    [Fact]
    public void Constructor_EmptyProcessors_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => new Logger([], [], LogLevel.Information));
    }

    [Fact]
    public void Constructor_NullProcessorInArray_ThrowsArgumentNullException()
    {
        // Arrange
        var processors = new ILogProcessor[] { new TestLogProcessor(), null!, new TestLogProcessor() };

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => new Logger(processors, [], LogLevel.Information));
    }

    [Fact]
    public void Constructor_ValidParameters_CreatesInstance()
    {
        // Arrange
        var processors = new ILogProcessor[] { new TestLogProcessor() };

        // Act
        using var logger = new Logger(processors, [], LogLevel.Information);

        // Assert
        Assert.NotNull(logger);
    }

    [Fact]
    public void MinimumLevel_ReturnsConstructorValue()
    {
        // Arrange
        var processors = new ILogProcessor[] { new TestLogProcessor() };

        // Act
        using var logger = new Logger(processors, [], LogLevel.Warning);

        // Assert
        Assert.Equal(LogLevel.Warning, logger.MinimumLevel);
    }

    [Fact]
    public void Log_AfterDispose_ThrowsObjectDisposedException()
    {
        // Arrange
        var processors = new ILogProcessor[] { new TestLogProcessor() };
        var logger = new Logger(processors, [], LogLevel.Information);
        logger.Dispose();

        // Act & Assert
        Assert.Throws<ObjectDisposedException>(() => logger.Log(LogLevel.Information, "tag", "message"));
    }

    [Fact]
    public void Log_BelowMinimumLevel_DoesNotProcessLog()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var processors = new ILogProcessor[] { processor };
        using var logger = new Logger(processors, [], LogLevel.Warning);

        // Act
        logger.Log(LogLevel.Information, "tag", "message");

        // Assert
        Assert.Equal(0, processor.ProcessLogCallCount);
    }

    [Fact]
    public void Log_AtMinimumLevel_ProcessesLog()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var processors = new ILogProcessor[] { processor };
        using var logger = new Logger(processors, [], LogLevel.Warning);

        // Act
        logger.Log(LogLevel.Warning, "tag", "message");

        // Assert
        Assert.Equal(1, processor.ProcessLogCallCount);
        Assert.Equal(LogLevel.Warning, processor.LastLogLevel);
        Assert.Equal("tag", processor.LastTag);
        Assert.Equal("message", processor.LastMessage);
    }

    [Fact]
    public void Log_AboveMinimumLevel_ProcessesLog()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var processors = new ILogProcessor[] { processor };
        using var logger = new Logger(processors, [], LogLevel.Information);

        // Act
        logger.Log(LogLevel.Error, "tag", "message");

        // Assert
        Assert.Equal(1, processor.ProcessLogCallCount);
        Assert.Equal(LogLevel.Error, processor.LastLogLevel);
    }

    [Fact]
    public void Log_MultipleProcessors_CallsAllProcessors()
    {
        // Arrange
        var processor1 = new TestLogProcessor();
        var processor2 = new TestLogProcessor();
        var processor3 = new TestLogProcessor();
        var processors = new ILogProcessor[] { processor1, processor2, processor3 };
        using var logger = new Logger(processors, [], LogLevel.Debug);

        // Act
        logger.Log(LogLevel.Information, "tag", "message");

        // Assert
        Assert.Equal(1, processor1.ProcessLogCallCount);
        Assert.Equal(1, processor2.ProcessLogCallCount);
        Assert.Equal(1, processor3.ProcessLogCallCount);
    }

    [Fact]
    public void Log_EmptySpans_ProcessesSuccessfully()
    {
        // Arrange
        var processor = new TestLogProcessor();
        var processors = new ILogProcessor[] { processor };
        using var logger = new Logger(processors, [], LogLevel.Debug);

        // Act
        logger.Log(LogLevel.Information, ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty);

        // Assert
        Assert.Equal(1, processor.ProcessLogCallCount);
        Assert.Equal(string.Empty, processor.LastTag);
        Assert.Equal(string.Empty, processor.LastMessage);
    }

    [Fact]
    public void Dispose_DisposableProcessors_DisposesAll()
    {
        // Arrange
        var processor1 = new DisposableTestLogProcessor();
        var processor2 = new TestLogProcessor(); // Not disposable
        var processor3 = new DisposableTestLogProcessor();
        var processors = new ILogProcessor[] { processor1, processor2, processor3 };
        var logger = new Logger(processors, [], LogLevel.Information);

        // Act
        logger.Dispose();

        // Assert
        Assert.True(processor1.IsDisposed);
        Assert.True(processor3.IsDisposed);
    }

    [Fact]
    public void Dispose_CalledMultipleTimes_DoesNotThrow()
    {
        // Arrange
        var processor = new DisposableTestLogProcessor();
        var processors = new ILogProcessor[] { processor };
        var logger = new Logger(processors, [], LogLevel.Information);

        // Act
        logger.Dispose();
        logger.Dispose();
        logger.Dispose();

        // Assert
        Assert.Equal(1, processor.DisposeCallCount);
    }

    [Fact]
    public void Dispose_NoDisposableProcessors_CompletesSuccessfully()
    {
        // Arrange
        var processors = new ILogProcessor[] { new TestLogProcessor(), new TestLogProcessor() };
        var logger = new Logger(processors, [], LogLevel.Information);

        // Act
        logger.Dispose();

        // Assert
        Assert.Throws<ObjectDisposedException>(() => logger.Log(LogLevel.Information, "tag", "message"));
    }

    [Theory]
    [InlineData(LogLevel.Debug)]
    [InlineData(LogLevel.Information)]
    [InlineData(LogLevel.Warning)]
    [InlineData(LogLevel.Error)]
    public void Log_AllLogLevels_ProcessesCorrectly(LogLevel level)
    {
        // Arrange
        var processor = new TestLogProcessor();
        var processors = new ILogProcessor[] { processor };
        using var logger = new Logger(processors, [], LogLevel.Debug);

        // Act
        logger.Log(level, "tag", "message");

        // Assert
        Assert.Equal(1, processor.ProcessLogCallCount);
        Assert.Equal(level, processor.LastLogLevel);
    }

    [Fact]
    public async Task Log_ConcurrentCalls_ProcessesAllLogs()
    {
        // Arrange
        var processor = new ConcurrentTestLogProcessor();
        var processors = new ILogProcessor[] { processor };
        using var logger = new Logger(processors, [], LogLevel.Debug);

        // Act
        var tasks = Enumerable.Range(0, 100).Select(i => Task.Run(() =>
            logger.Log(LogLevel.Information, $"tag{i}", $"message{i}")
        )).ToArray();

        await Task.WhenAll(tasks);

        // Assert
        Assert.Equal(100, processor.ProcessLogCallCount);
    }
}
