# GroveGames.Logger

A high-performance, zero-allocation logging library for .NET, Unity, and Godot with Native AOT support. Built for game development scenarios where performance is critical.

[![Build Status](https://github.com/grovegs/Logger/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/Logger/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/Logger/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/Logger/actions/workflows/tests.yml)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/Logger)](https://github.com/grovegs/Logger/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.Logger)](https://www.nuget.org/packages/GroveGames.Logger)

---

## Features

- **Zero-Allocation Logging**: Uses interpolated string handlers and `Span<char>` to eliminate heap allocations
- **Native AOT Compatible**: Fully supports ahead-of-time compilation for maximum performance
- **Background File Writing**: Log calls copy into a double buffer that a background thread writes to the file
- **Automatic File Rotation**: Manages log files with configurable limits and automatic cleanup
- **Flexible Log Processing**: Extensible processor system for custom log handling
- **Custom Formatting**: Full control over log output format via `ILogFormatter` interface
- **Unity Integration**: Available as a Unity package
- **Godot Integration**: Available as a Godot addon
- **Bounded Messages**: A message is at most 4096 characters; a longer one is cut there and ends with `... (truncated)`, so buffers and log lines stay bounded
- **Test Double**: `FakeLogger` in the Unity package's editor-only `GroveGames.Logger.Unity.Testing` assembly, for your own tests

## .NET

Install via NuGet:

```bash
dotnet add package GroveGames.Logger
```

### Basic Logging

The core has no console processor; implement `ILogProcessor` to send logs anywhere. The Unity and Godot packages add console processors for their engines.

```csharp
using GroveGames.Logger;

var logger = LoggerFactory.CreateLogger(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information)
           .AddLogProcessor(new ConsoleProcessor());
});

logger.LogInformation("App", $"Application started at {DateTime.UtcNow}");
logger.LogWarning("App", $"Memory usage: {GC.GetTotalMemory(false) / 1024 / 1024}MB");
logger.LogError("App", $"Failed to load configuration");

sealed class ConsoleProcessor : ILogProcessor
{
    public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        Console.Out.WriteLine($"[{level}] [{tag}] {message}");
    }
}
```

A logger with no processors is valid and logs nothing.

### File Logging

```csharp
using GroveGames.Logger;

using StreamWriter = GroveGames.Logger.StreamWriter;

var logger = LoggerFactory.CreateLogger(builder =>
{
    var logFileFactory = new LogFileFactory(
        root: "logs",
        folderName: "app",
        maxFileCount: 10,
        bufferSize: 8192
    );

    var fileStream = logFileFactory.CreateFile();
    var streamWriter = new StreamWriter(fileStream, bufferSize: 8192, channelCapacity: 1000);
    var fileProcessor = new FileLogProcessor(streamWriter, new FileLogFormatter());

    builder.SetMinimumLevel(LogLevel.Debug)
           .AddLogProcessor(fileProcessor);
});
```

`StreamWriter` encodes each entry as UTF-8 straight into one of two buffers and writes full buffers on its own background thread, so logging allocates nothing once the buffers have grown to fit your bursts. An idle writer writes new entries at once, so the last lines before a crash are not left in memory; under load, entries are written in batches. `Flush()` waits up to a second for every entry added so far to reach the stream, so call it before the process may be killed. While the writer is busy, up to `channelCapacity` entries wait, and later ones are dropped. If the stream fails (a full disk, for example), the writer stops and drops later entries instead of throwing, and `Dispose()` never throws. `Dispose()` writes everything accepted, waiting at most 5 seconds for a stuck stream.

`LogFileFactory` names files by their UTC start time, in the invariant culture so names sort the same on every device language, and keeps the newest `maxFileCount` files, deleting the oldest by name. The file it has just opened is never deleted, even when the device clock went backwards.

A processor that throws doesn't stop the log call: the exception is ignored and the remaining processors still run. Processors may be called from any thread, so keep them thread-safe.

### Custom Log Formatters

Implement `ILogFormatter` to create custom formats:

```csharp
using System.Globalization;

public sealed class JsonLogFormatter : ILogFormatter
{
    public int GetBufferSize(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        return 100 + tag.Length + message.Length;
    }

    public void Format(Span<char> buffer, LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        var timestamp = DateTime.UtcNow;
        var position = 0;

        var start = "{\"time\":\"".AsSpan();
        start.CopyTo(buffer[position..]);
        position += start.Length;

        timestamp.TryFormat(buffer[position..], out var written, "O", CultureInfo.InvariantCulture);
        position += written;

        // ... continue building JSON
    }
}

var logger = LoggerFactory.CreateLogger(builder =>
{
    builder.AddLogProcessor(new FileLogProcessor(streamWriter, new JsonLogFormatter()));
});
```

### Performance-Critical Scenarios

The log methods take interpolated string handlers. Below the minimum level, no value in the message is evaluated or formatted. At an enabled level, the message is formatted into a pooled buffer without allocating for `string`, `char`, `bool`, every integer type, `float`, `double`, `decimal`, `DateTime`, `DateTimeOffset`, `TimeSpan` and `Guid`, in the invariant culture:

```csharp
for (int i = 0; i < 1000000; i++)
{
    logger.LogDebug("Loop", $"Iteration {i} of {1000000}");
}
```

Other values are formatted with `ToString`, which allocates; enums box on every target. On Unity's Mono and IL2CPP, a custom date format with `yyyy` allocates; the default date format and `HH:mm:ss` don't. On .NET, a type that implements `ISpanFormattable` is formatted without allocating.

### Core Components

- **`ILogger`**: Core logging interface with minimal API surface
- **`Logger`**: Main implementation with processor management
- **`LoggerFactory`**: Primary entry point for creating loggers with builder configuration
- **`LoggerBuilder`**: Fluent API for logger configuration
- **`ILogProcessor`**: Interface for processing log entries
- **`ILogFormatter`**: Interface for controlling log entry formatting
- **`StreamWriter`**: Zero-allocation double-buffered file writer with a background thread
- **`LogFileFactory`**: Manages log file creation and rotation

## Unity

There are two installation steps required to use it in Unity.

1. Install `GroveGames.Logger` from NuGet using [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). Open Window from NuGet → Manage NuGet Packages, search "GroveGames.Logger" and press Install.

2. Install the `GroveGames.Logger.Unity` package by referencing the git URL:

    ```text
    https://github.com/grovegs/Logger.git?path=src/GroveGames.Logger.Unity/Packages/com.grovegames.logger
    ```

3. Create a `csc.rsp` file in your `Assets/` directory with the following content to enable C# 10 features:

    ```text
    -langversion:10
    -nullable:enable
    ```

With the Unity package, Unity-specific formatters and processors become available for logging in Unity projects.

### Setting Up Unity Logger

`UnityEngine` has its own `Logger` and `ILogger`, so alias the ones from this package:

```csharp
using GroveGames.Logger;
using GroveGames.Logger.Unity;
using UnityEngine;

using Logger = GroveGames.Logger.Logger;

public class GameManager : MonoBehaviour
{
    private Logger _logger;

    private void Awake()
    {
        _logger = UnityLoggerFactory.CreateLogger(builder =>
        {
            builder.AddConsoleLogProcessor();
            builder.AddFileLogProcessor();
        });

        _logger.LogInformation("Game", $"Unity {Application.unityVersion} initialized");
    }

    private void OnDestroy()
    {
        _logger?.Dispose();
    }
}
```

`AddFileLogProcessor()` flushes the log file when the application loses focus, which is when mobile platforms pause it and may kill it. `AddLogSource()` forwards messages that Unity logs from any thread, skipping those below the logger's minimum level, so your own processors must be thread-safe and must not touch Unity objects. History entries are stamped in UTC.

### Project Settings Configuration

Configure logger settings via Edit → Project Settings → GroveGames → Logger. Settings are stored as a ScriptableObject in `Assets/Settings/GroveGames/LoggerSettings.asset` and registered as a preloaded asset in Player Settings, so every build profile ships them. An asset left in the old `Assets/Settings/Resources/GroveGames/` folder is moved there automatically.

| Setting                 | Type       | Default       | Description                                |
| ----------------------- | ---------- | ------------- | ------------------------------------------ |
| `Min Log Level`         | `LogLevel` | `Information` | Minimum level for log output               |
| `Max File Count`        | `int`      | `10`          | Maximum number of log files to retain      |
| `File Folder Name`      | `string`   | `"logs"`      | Folder name for log files                  |
| `File Buffer Size`      | `int`      | `8192`        | Buffer size in bytes for file operations   |
| `File Channel Capacity` | `int`      | `1000`        | Entries that wait while the writer is busy |
| `History Capacity`      | `int`      | `1000`        | Entries kept by `AddLogHistory`            |

For custom configurations or DI scenarios, pass settings directly:

```csharp
var settings = UnityLoggerSettings.GetOrCreate();
var logger = UnityLoggerFactory.CreateLogger(settings, builder => { ... });
```

### Dependency Injection

With [GroveGames.DependencyInjection](https://github.com/grovegs/DependencyInjection) installed, register the logger in your root installer:

```csharp
builder.AddLogger();
```

`AddLogger()` registers `ILogger` with the minimum level from `UnityLoggerSettings`, console output, file output and the Unity log source. To decide per environment, pass the level and whether to write to the console, for example with GroveGames.Platform's `UnityEnvironment`:

```csharp
builder.AddLogger(UnityEnvironment.IsEditor ? LogLevel.Debug : LogLevel.Information, isConsoleEnabled: !UnityEnvironment.IsProduction);
```

For anything else, configure the builder yourself:

```csharp
builder.AddLogger(ConfigureLogger);

private static void ConfigureLogger(ILoggerBuilder logger)
{
    logger.AddConsoleLogProcessor();
    logger.AddFileLogProcessor();
    logger.AddLogSource();
}
```

Each overload builds the logger from `UnityLoggerSettings` and then the given configuration, so code overrides the settings. Disposing the container disposes the logger.

With [GroveGames.DeveloperTools](https://github.com/grovegs/DeveloperTools) installed and `GROVEGAMES_DEVELOPERTOOLS` defined, `AddLogger` also registers the Logger options, a minimum level picker that calls `Logger.SetMinimumLevel`. The level applies to messages logged through `ILogger`; messages Unity logs itself keep the level the logger was built with.

### Unity Components

- **`UnityLoggerFactory`**: Factory with GetOrCreate() pattern for DI-friendly architecture
- **`UnityLoggerSettings`**: ScriptableObject registered as a preloaded asset; implements the core `ILoggerSettings`
- **`LoggerSettingsProvider`**: Project Settings UI using UI Toolkit
- **`ConsoleLogFormatter`**: Formatter for Unity console output (format: `[Tag] Message`)
- **`ConsoleLogProcessor`**: Routes logs to `Debug.Log`, `Debug.LogWarning`, `Debug.LogError`
- **`UnityLogFileFactory`**: File factory using `Application.persistentDataPath`
- **`LogSource`**: Captures Unity's internal logs and forwards them to every processor except the console

## Godot

Install via NuGet:

```bash
dotnet add package GroveGames.Logger.Godot
```

Download the Godot addon from the [latest release](https://github.com/grovegs/Logger/releases/latest) and extract it to your project's `addons` folder. Enable the addon in Project Settings → Plugins.

```text
res://
├── addons/
│   └── GroveGames.Logger/
│       ├── plugin.cfg
│       └── ...
└── ...
```

### Setting Up Godot Logger

Godot 4.5 adds its own `Logger`, so alias the one from this package:

```csharp
using GroveGames.Logger;
using GroveGames.Logger.Godot;
using Godot;

using Logger = GroveGames.Logger.Logger;

public partial class Main : Node
{
    private Logger _logger;

    public override void _Ready()
    {
        _logger = GodotLoggerFactory.CreateLogger(builder =>
        {
            builder.AddConsoleLogProcessor();
            builder.AddFileLogProcessor();
        });

        _logger.LogInformation("Game", $"Godot {Engine.GetVersionInfo()} initialized");
    }

    public override void _ExitTree()
    {
        _logger?.Dispose();
    }
}
```

### Godot Console Formatter

The `ConsoleLogFormatter` provides rich formatting for the Godot editor console:

- Warning messages are highlighted with yellow color; their text is escaped, so `[` in a message prints as written
- Local timestamps in HH:mm:ss format for easy debugging
- Clean tag presentation with brackets

### Project Settings Configuration

Configure logger settings via a Resource stored at `res://addons/GroveGames.Logger/LoggerSettings.tres`. The resource path is registered in project settings and automatically included in builds.

| Setting                 | Type       | Default       | Description                                |
| ----------------------- | ---------- | ------------- | ------------------------------------------ |
| `Min Log Level`         | `LogLevel` | `Information` | Minimum level for log output               |
| `Max File Count`        | `int`      | `10`          | Maximum number of log files to retain      |
| `File Folder Name`      | `string`   | `"logs"`      | Folder name for log files                  |
| `File Buffer Size`      | `int`      | `8192`        | Buffer size in bytes for file operations   |
| `File Channel Capacity` | `int`      | `1000`        | Entries that wait while the writer is busy |

For custom configurations or DI scenarios, reference the settings resource:

```csharp
public partial class GameManager : Node
{
    [Export] private LoggerSettingsResource _loggerSettings;

    public override void _Ready()
    {
        var logger = GodotLoggerFactory.CreateLogger(_loggerSettings, builder => { ... });
    }
}
```

### Godot Components

- **`GodotLoggerFactory`**: Factory with GetOrCreate() pattern for DI-friendly architecture
- **`LoggerSettingsResource`**: Resource with ProjectSettings integration; implements the core `ILoggerSettings`
- **`ConsoleLogFormatter`**: Rich formatting for Godot's editor console
- **`ConsoleLogProcessor`**: Processor optimized for Godot's output methods
- **`GodotLogFileFactory`**: File factory using `OS.GetUserDataDir()`

## Architecture

### Performance Optimizations

1. **Zero-Allocation String Handling**: Uses `Span<char>` and stackalloc throughout
2. **Interpolated String Handlers**: Compile-time optimization for string formatting
3. **Double-Buffered File Writes**: Log calls copy into one buffer under a short lock while a background thread writes the other
4. **Batched Writing**: Reduces I/O operations by batching multiple log entries
5. **Native AOT**: Full compatibility with ahead-of-time compilation

---

## Agent Skills

The package ships an agent skill, `grovegames-logger`, with the rules for game code that uses it.

- **Unity:** the package's `Skills~` folder.
- **Godot:** the addon's `skills` folder.
- **.NET:** `skills/grovegames-logger/SKILL.md` in the NuGet package.

## Testing

Run tests:

```bash
dotnet test
```

---

## Contributing

Contributions are welcome! Please:

1. Fork the repository
2. Create a feature branch
3. Write tests for new functionality
4. Submit a pull request

---

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
