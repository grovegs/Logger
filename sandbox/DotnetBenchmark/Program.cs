using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;

using GroveGames.Logger;

using Microsoft.Extensions.Logging;

using ZLogger;
using ZLogger.Providers;

[MemoryDiagnoser]
[SimpleJob(launchCount: 1, warmupCount: 100, invocationCount: 1000, iterationCount: 1000)]
public class Benchmark
{
    private FileLogger _fileLogger;
    private ILogger<Benchmark> _zlogger;

    [GlobalSetup]
    public void Setup()
    {
        var fileWriter = new FileWriter(new LogFileFactory(Environment.CurrentDirectory).CreateFile());
        _fileLogger = new FileLogger(fileWriter);
        var factory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(LogLevel.Debug);

            builder.AddZLoggerRollingFile(options =>
                {
                    options.UsePlainTextFormatter(formatter =>
                        {
                            formatter.SetPrefixFormatter($"{0}|{1}|", (in MessageTemplate template, in LogInfo info) => template.Format(info.Timestamp, info.LogLevel));
                            formatter.SetExceptionFormatter((writer, ex) => Utf8StringInterpolation.Utf8String.Format(writer, $"{ex.Message}"));
                        });
                    options.FilePathSelector = (timestamp, sequenceNumber) =>
                    $"logs/{timestamp:yyyy-MM-dd}_{sequenceNumber:000}.log";
                    options.RollingInterval = RollingInterval.Day;
                    options.RollingSizeKB = 1024;
                });
        });

        _zlogger = factory.CreateLogger<Benchmark>();
    }

    [Benchmark]
    public void FileLogger()
    {
        _fileLogger.Info("Test Tag", "This is a test message");
    }

    [Benchmark]
    public void ZLogger()
    {
        _zlogger.LogInformation("This is a test message");
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmark>();
    }
}
