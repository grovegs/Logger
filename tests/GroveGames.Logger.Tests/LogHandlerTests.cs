namespace GroveGames.Logger.Tests;

public sealed class LogHandlerTests
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

    private sealed class TestLogHandler : ILogHandler
    {
        private ILogProcessor[] _processors = [];
        public bool IsInitialized { get; private set; }
        public bool IsDisposed { get; private set; }
        public int ProcessorCount => _processors.Length;

        public void Initialize(ILogProcessor[] processors)
        {
            _processors = processors;
            IsInitialized = true;
        }

        public void ProcessLog(LogLevel level, string tag, string message)
        {
            foreach (ILogProcessor processor in _processors)
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
    public void LoggerBuilder_AddLogHandler_InitializesHandler()
    {
        var processor = new TestLogProcessor();
        var handler = new TestLogHandler();
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        builder.AddLogHandler(handler);
        var logger = builder.Build();

        Assert.True(handler.IsInitialized);
        Assert.Equal(1, handler.ProcessorCount);

        logger.Dispose();
    }

    [Fact]
    public void LoggerBuilder_MultipleHandlers_InitializesAll()
    {
        var processor = new TestLogProcessor();
        var handler1 = new TestLogHandler();
        var handler2 = new TestLogHandler();
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        builder.AddLogHandler(handler1);
        builder.AddLogHandler(handler2);
        var logger = builder.Build();

        Assert.True(handler1.IsInitialized);
        Assert.True(handler2.IsInitialized);
        Assert.Equal(1, handler1.ProcessorCount);
        Assert.Equal(1, handler2.ProcessorCount);

        logger.Dispose();
    }

    [Fact]
    public void LoggerBuilder_NoHandlers_DoesNotInitialize()
    {
        var processor = new TestLogProcessor();
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        var logger = builder.Build();

        Assert.NotNull(logger);

        logger.Dispose();
    }

    [Fact]
    public void Logger_Dispose_DisposesHandlers()
    {
        var processor = new TestLogProcessor();
        var handler = new TestLogHandler();
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        builder.AddLogHandler(handler);
        var logger = builder.Build();

        logger.Dispose();

        Assert.True(handler.IsDisposed);
    }

    [Fact]
    public void Logger_Constructor_NullHandler_ThrowsArgumentNullException()
    {
        var processors = new ILogProcessor[] { new TestLogProcessor() };
        var handlers = new ILogHandler[] { null! };

        Assert.Throws<ArgumentNullException>(() => new Logger(processors, handlers, LogLevel.Information));
    }

    [Fact]
    public void LoggerBuilder_AddLogHandler_NullHandler_ThrowsArgumentNullException()
    {
        var builder = new LoggerBuilder();

        Assert.Throws<ArgumentNullException>(() => builder.AddLogHandler(null!));
    }

    [Fact]
    public void Handler_CanProcessLogs_ThroughProcessors()
    {
        var processor = new TestLogProcessor();
        var handler = new TestLogHandler();
        var builder = new LoggerBuilder();

        builder.AddLogProcessor(processor);
        builder.AddLogHandler(handler);
        var logger = builder.Build();

        handler.ProcessLog(LogLevel.Information, "Test", "Message");

        Assert.Equal(1, processor.ProcessLogCallCount);
        Assert.Equal(LogLevel.Information, processor.LastLogLevel);
        Assert.Equal("Test", processor.LastTag);
        Assert.Equal("Message", processor.LastMessage);

        logger.Dispose();
    }
}
