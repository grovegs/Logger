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
- **Asynchronous File Writing**: Non-blocking log writes using channels and background processing
- **Automatic File Rotation**: Manages log files with configurable limits and automatic cleanup
- **Flexible Log Processing**: Extensible processor system for custom log handling
- **Custom Formatting**: Full control over log output format via `ILogFormatter` interface
- **Unity Integration**: Available as a Unity package
- **Godot Integration**: Available as a Godot addon with project settings integration

## .NET

Install via NuGet:

```bash
dotnet add package GroveGames.Logger
```

### Basic Console Logging

```csharp
using GroveGames.Logger;

var logger = LoggerFactory.CreateLogger(builder =>
{
    builder.SetMinimumLevel(LogLevel.Information)
           .AddLogProcessor(new ConsoleLogProcessor(new ConsoleLogFormatter()));
});

logger.LogInformation("App", $"Application started at {DateTime.UtcNow}");
logger.LogWarning("App", $"Memory usage: {GC.GetTotalMemory(false) / 1024 / 1024}MB");
logger.LogError("App", $"Failed to load configuration");
```

### File Logging

```csharp
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

### Custom Log Formatters

Implement `ILogFormatter` to create custom formats:

```csharp
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

        "{\"time\":\"".AsSpan().CopyTo(buffer[position..]);
        position += 10;

        timestamp.TryFormat(buffer[position..], out var written, "O");
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

The logger uses interpolated string handlers that only allocate when the log level is active:

```csharp
for (int i = 0; i < 1000000; i++)
{
    logger.LogDebug("Loop", $"Iteration {i} of {1000000}");
}
```

### Core Components

- **`ILogger`**: Core logging interface with minimal API surface
- **`Logger`**: Main implementation with processor management
- **`LoggerFactory`**: Primary entry point for creating loggers with builder configuration
- **`LoggerBuilder`**: Fluent API for logger configuration
- **`ILogProcessor`**: Interface for processing log entries
- **`ILogFormatter`**: Interface for controlling log entry formatting
- **`StreamWriter`**: High-performance async file writer with batching
- **`LogFileFactory`**: Manages log file creation and rotation

## Unity

There are two installation steps required to use it in Unity.

1. Install `GroveGames.Logger` from NuGet using [NuGetForUnity](https://github.com/GlitchEnzo/NuGetForUnity). Open Window from NuGet → Manage NuGet Packages, search "GroveGames.Logger" and press Install.

2. Install the `GroveGames.Logger.Unity` package by referencing the git URL:

```
https://github.com/grovegs/Logger.git?path=src/GroveGames.Logger.Unity/Packages/com.grovegames.logger
```

With the Unity package, Unity-specific formatters and processors become available for logging in Unity projects.

### Unity Components

- **Unity-specific formatters**: Optimized for Unity's console output
- **Unity-specific processors**: Integration with Unity's logging system

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

```csharp
using GroveGames.Logger;
using Godot;

public partial class Main : Node
{
    private Logger _logger;

    public override void _Ready()
    {
        GodotSettings.CreateIfNotExist();

        _logger = GodotLoggerFactory.CreateLogger(builder =>
        {
            builder.AddLogProcessor(new GodotConsoleLogProcessor(
                text => GD.Print(text),
                new GodotConsoleLogFormatter()
            ));

            var fileStream = CreateLogFileStream();
            var streamWriter = new StreamWriter(fileStream, 8192, 1000);
            builder.AddLogProcessor(new FileLogProcessor(
                streamWriter,
                new FileLogFormatter()
            ));
        });

        _logger.LogInformation("Game", $"Godot {Engine.GetVersionInfo()} initialized");
    }
}
```

### Godot Console Formatter

The `GodotConsoleLogFormatter` provides rich formatting for the Godot editor console:

- Warning messages are highlighted with yellow color
- Timestamps in HH:mm:ss format for easy debugging
- Clean tag presentation with brackets

### Project Settings Configuration

The addon automatically adds settings to your Godot project settings under `grove_games/logger/`:

| Setting                 | Type       | Default       | Description                               |
| ----------------------- | ---------- | ------------- | ----------------------------------------- |
| `min_log_level`         | `LogLevel` | `Information` | Minimum level for log output              |
| `max_file_count`        | `int`      | `10`          | Maximum number of log files to retain     |
| `file_folder_name`      | `string`   | `"logs"`      | Folder name for log files                 |
| `file_buffer_size`      | `int`      | `8192`        | Buffer size in bytes for file operations  |
| `file_channel_capacity` | `int`      | `1000`        | Channel capacity for async log processing |

### Godot Components

- **`GodotLoggerFactory`**: Factory that reads configuration from Godot project settings
- **`GodotConsoleLogFormatter`**: Rich formatting for Godot's editor console
- **`GodotSettings`**: Integration with Godot's project settings system
- **`GodotConsoleLogProcessor`**: Processor optimized for Godot's output methods

## Architecture

### Performance Optimizations

1. **Zero-Allocation String Handling**: Uses `Span<char>` and stackalloc throughout
2. **Interpolated String Handlers**: Compile-time optimization for string formatting
3. **Channel-Based Async I/O**: Lock-free producer-consumer pattern for file writes
4. **Batched Writing**: Reduces I/O operations by batching multiple log entries
5. **Native AOT**: Full compatibility with ahead-of-time compilation

---

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
