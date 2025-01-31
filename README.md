
# GroveGames.Logger

A high-performance, low-allocation logging library designed for .NET applications. The `GroveGames.Logger` library offers efficient and customizable logging capabilities using modern .NET features like `Span<char>` and `ReadOnlySpan<char>` to minimize heap allocations.


[![Build Status](https://github.com/grovegs/Logger/actions/workflows/release.yml/badge.svg)](https://github.com/grovegs/Logger/actions/workflows/release.yml)
[![Tests](https://github.com/grovegs/Logger/actions/workflows/tests.yml/badge.svg)](https://github.com/grovegs/Logger/actions/workflows/tests.yml)
[![Latest Release](https://img.shields.io/github/v/release/grovegs/Logger)](https://github.com/grovegs/Logger/releases/latest)
[![NuGet](https://img.shields.io/nuget/v/GroveGames.Logger)](https://www.nuget.org/packages/GroveGames.Logger)

---

## Features

- **Low Allocation Logging**: Uses stack-allocated buffers (`Span<char>`) to avoid unnecessary heap allocations.
- **Log Levels**: Supports `Info`, `Warning`, and `Error` log levels.
- **Asynchronous File Writing**: Logs are written to files asynchronously using a background thread.
- **Customizable File Management**: Automatically rotates log files and removes old ones based on a configurable limit.
- **Mockable Interfaces**: Provides interfaces like `ILogger` and `IFileWriter` for unit testing and flexibility.
- **Godot Integration**: Includes `GodotLogger` for seamless logging in Godot projects.

---

## Installation

Add the library to your .NET project via NuGet:

```bash
dotnet add package GroveGames.Logger
```

For Godot:

```bash
dotnet add package GroveGames.Logger.Godot
```

---

## Usage

### Setting Up a Logger

To use the logger, initialize a `FileLogger` instance with a custom `IFileWriter`:

```csharp
using GroveGames.Logger;

// Access the shared instance of GodotFileLogger
var fileWriter = new FileWriter(new StreamWriter("app.log", append: true));
var logger = new FileLogger(fileWriter);
```

### Logging Messages

Log messages with tags and messages:

```csharp
logger.Info("Application", "Application started.");
logger.Warning("Application", "High memory usage detected.");
logger.Error("Application", "Unhandled exception occurred.");
```

### File Rotation and Management

The `FileWriter` automatically manages log files. If the number of log files exceeds the configured limit, the oldest file is deleted.

---

### Processor-Based Logging

The `ILogProcessor` interface allows you to extend the logging system by processing logs as they are generated. Processors can be dynamically added or removed at runtime.

#### Adding a Processor

##### Custom Log Processor

Implement the `ILogProcessor` interface to define custom log processing behavior:

```csharp
public class CustomLogProcessor : ILogProcessor
{
    public void ProcessLog(ReadOnlySpan<char> level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
    {
        // Custom log handling logic
        Console.WriteLine($"[{level}] [{tag}]: {message}");
    }
}
```

#### Adding Your Processor

Add your custom processor to the logger:

```csharp
logger.AddProcessor(new CustomLogProcessor());
```

#### Removing a Processor

Processors can be removed when they are no longer needed:

```csharp
logger.RemoveProcessor(customProcessor);
```
---

## Godot Integration

The `GodotFileLogger` class extends the `ILogger` interface and integrates seamlessly with the Godot Engine. Logs are saved to files while also being processed for the Godot console.

### Enabling Godot Logging

```csharp
using GroveGames.Logger;

// Access the shared instance of GodotFileLogger
var logger = GodotFileLogger.Shared;

// Add a processor to print logs to the Godot output console
logger.AddProcessor(new GodotLogProcessor(s => GD.Print(s)));

logger.Info("Game", "Game started.");
logger.Warning("Game", "Potential performance issue.");
logger.Error("Game", "Unhandled exception occurred.");
```

### Example Log Output in Godot

In the Godot editor, logs processed by `GodotLogProcessor` will appear in the output console like this:

```
[INFO] [Game] Game started.
[WARNING] [Game] Potential performance issue.
[ERROR] [Game] Unhandled exception occurred.
```

---

## Architecture

### Core Components

1. **`ILogger`**: Interface defining core logging functionality.
2. **`GodotFileLogger`**: Combines file-based logging with processor-based extensibility for Godot Engine.
3. **`ILogProcessor`**: Interface for implementing custom log processors.
4. **`FileLogger`**: Handles core file-based logging operations.
5. **`FileWriter`**: Writes log messages to files asynchronously using a background thread.
6. **`LogFileFactory`**: Manages log file creation and rotation.
7. **`GodotLogProcessor`**: A processor for displaying logs in the Godot console.

### Logging Format

Logs are formatted as:
```
[DateTime] | [LogLevel] | [Tag] | [Message]
```

Example:
```
2024-12-05 14:23:45 | INFO | Application | Application started.
```

---

## Testing

The library is tested with `xUnit` and `Moq` to ensure reliability and performance. Tests cover:

1. File-based logging functionality.
2. Processor-based log handling.
3. Log file creation and rotation.

### Running Tests

Run tests using the .NET CLI:

```bash
dotnet test
```

---

## Contributing

Contributions are welcome! Please open issues for bugs or feature requests and submit pull requests with new features or fixes.

1. Fork the repository.
2. Create a new branch for your feature.
3. Submit a pull request with a detailed explanation.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
