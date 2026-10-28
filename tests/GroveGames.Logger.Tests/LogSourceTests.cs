namespace GroveGames.Logger.Tests;

public sealed class LogSourceTests
{
    private sealed class TestLogProcessor : ILogProcessor
    {
        private int _processLogCallCount;
        private LogLevel _lastLogLevel;
        private string _lastTag;
        private string _lastMessage;

        public int ProcessLogCallCount => _processLogCallCount;
        public LogLevel LastLogLevel => _lastLogLevel;
        public string LastTag => _lastTag;
        public string LastMessage => _lastMessage;

        public TestLogProcessor()
        {
            _lastMessage = string.Empty;
            _lastTag = string.Empty;
        }

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            _processLogCallCount++;
            _lastLogLevel = level;
            _lastTag = tag.ToString();
            _lastMessage = message.ToString();
        }
    }

    private sealed class TestLogSource : ILogSource
    {
        private readonly ILogProcessor[] _processors;
        private bool _isDisposed;
        public bool IsDisposed => _isDisposed;
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
            _isDisposed = true;
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
