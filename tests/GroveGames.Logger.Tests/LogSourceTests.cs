namespace GroveGames.Logger.Tests;

public sealed class LogSourceTests
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

    private sealed class TestLogSource : ILogSource
    {
        private readonly ILogProcessor[] _processors;
        public bool IsDisposed { get; private set; }
        public int ProcessorCount => _processors.Length;

        public TestLogSource(ILogProcessor[] processors)
        {
            _processors = processors;
        }

        public void ProcessLog(LogLevel level, string tag, string message)
        {
            foreach (var processor in _processors)
            {
                processor.ProcessLog(level, tag.AsSpan(), message.AsSpan());
            }
        }

        public void Dispose()
        {
            IsDisposed = true;
        }
    }

    [Fact]
    public void LoggerBuilder_AddLogSource_InitializesSource()
    {
        var processor = new TestLogProcessor();
        TestLogSource? source = null;
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        builder.AddLogSource(processors => source = new TestLogSource(processors));
        var logger = builder.Build();

        Assert.NotNull(source);
        Assert.Equal(1, source.ProcessorCount);

        logger.Dispose();
    }

    [Fact]
    public void LoggerBuilder_MultipleSources_InitializesAll()
    {
        var processor = new TestLogProcessor();
        TestLogSource? source1 = null;
        TestLogSource? source2 = null;
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        builder.AddLogSource(processors => source1 = new TestLogSource(processors));
        builder.AddLogSource(processors => source2 = new TestLogSource(processors));
        var logger = builder.Build();

        Assert.NotNull(source1);
        Assert.NotNull(source2);
        Assert.Equal(1, source1.ProcessorCount);
        Assert.Equal(1, source2.ProcessorCount);

        logger.Dispose();
    }

    [Fact]
    public void LoggerBuilder_NoSources_DoesNotInitialize()
    {
        var processor = new TestLogProcessor();
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        var logger = builder.Build();

        Assert.NotNull(logger);

        logger.Dispose();
    }

    [Fact]
    public void Logger_Dispose_DisposesSources()
    {
        var processor = new TestLogProcessor();
        TestLogSource? source = null;
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        builder.AddLogSource(processors => source = new TestLogSource(processors));
        var logger = builder.Build();

        logger.Dispose();

        Assert.NotNull(source);
        Assert.True(source.IsDisposed);
    }

    [Fact]
    public void Logger_Constructor_NullSource_ThrowsArgumentNullException()
    {
        var processors = new ILogProcessor[] { new TestLogProcessor() };
        var sources = new ILogSource[] { null! };

        Assert.Throws<ArgumentNullException>(() => new Logger(processors, sources, LogLevel.Information));
    }

    [Fact]
    public void LoggerBuilder_AddLogSource_NullSource_ThrowsArgumentNullException()
    {
        var builder = new LoggerBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.AddLogSource(null!));
    }

    [Fact]
    public void Source_CanProcessLogs_ThroughProcessors()
    {
        var processor = new TestLogProcessor();
        TestLogSource? source = null;
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        builder.AddLogSource(processors => source = new TestLogSource(processors));
        var logger = builder.Build();

        Assert.NotNull(source);
        source.ProcessLog(LogLevel.Information, "Test", "Message");

        Assert.Equal(1, processor.ProcessLogCallCount);
        Assert.Equal(LogLevel.Information, processor.LastLogLevel);
        Assert.Equal("Test", processor.LastTag);
        Assert.Equal("Message", processor.LastMessage);

        logger.Dispose();
    }
}
