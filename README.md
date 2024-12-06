
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

var fileWriter = new FileWriter(new StreamWriter("app.log", append: true));
var logger = new FileLogger(fileWriter);
```

### Logging Messages

Log messages using the `Info`, `Warning`, and `Error` methods:

```csharp
logger.Info("Application", "Application started.");
logger.Warning("Application", "High memory usage detected.");
logger.Error("Application", "Unhandled exception occurred.");
```

### File Rotation and Management

The `FileWriter` automatically manages log files. If the number of log files exceeds the configured limit, the oldest file is deleted.

---

## Godot Integration

The `GodotLogger` class extends the `ILogger` interface to provide seamless integration with the Godot engine. It combines the benefits of `FileLogger` for file-based logging with an additional processing layer for handling Godot-specific logging requirements.

### Features

- **Shared Instance**: Access the `Shared` singleton instance for global logging.
- **Godot Log Processor**: Optionally enable a log processor that hooks into Godot's logging system.
- **File-Based Logging**: Log messages are persisted to a file using the `FileLogger`.

### Setting Up `GodotLogger`

```csharp
using GroveGames.Logger;

// Access the shared instance of GodotLogger
var logger = GodotLogger.Shared;

// Enable Godot-specific log processing (e.g., display logs in Godot console)
logger.EnableGodotProcessor(log => GD.Print(log));
```

### Logging Messages

You can log messages with tags and messages, just like with `FileLogger`:

```csharp
logger.Info("Game".AsSpan(), "Game started.".AsSpan());
logger.Warning("Game".AsSpan(), "Potential performance issue.".AsSpan());
logger.Error("Game".AsSpan(), "Unhandled exception occurred.".AsSpan());
```

### Example Output

In the Godot editor, if `EnableGodotProcessor` is enabled, logs will appear in the Godot output console like this:

```
[INFO] [Game] Game started.
[WARNING] [Game] Potential performance issue.
[ERROR] [Game] Unhandled exception occurred.
```

### File-Based Logging

Logs are also saved to a file. The file location can be configured using the `GodotLogFileFactory`.

---

### Advanced Configuration

#### Enable Custom Log Processing

You can customize the behavior of the Godot-specific log processor by passing a callback function to `EnableGodotProcessor`:

```csharp
logger.EnableGodotProcessor(log =>
{
    // Custom log handling logic
    GD.Print($"[Custom]: {log}");
});
```

#### Default Processor Behavior

If `EnableGodotProcessor` is not called, the logger uses an empty processor (`EmptyLogProcessor`), and no Godot-specific log handling occurs.

---

## Architecture

### Core Components

1. **`ILogger`**: Interface for logging messages.
2. **`FileLogger`**: Main logging implementation that formats log messages and delegates writing to `IFileWriter`.
3. **`IFileWriter`**: Interface for handling file operations.
4. **`FileWriter`**: Writes log messages to files asynchronously using a background thread.
5. **`LogFileFactory`**: Handles file creation and rotation.

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

The library is thoroughly tested using `xUnit` and `Moq`. Tests ensure that:

1. No heap allocation occurs during logging (`FileLoggerAllocationTests`).
2. Log messages are correctly written to files (`FileLoggerTests`).
3. File management functions as expected (`LogFileFactoryTests`).

### Running Tests

Run tests using the .NET CLI:

```bash
dotnet test
```

---

## Extending the Library

### Custom File Writer

Implement `IFileWriter` to customize how logs are written:

```csharp
public class CustomFileWriter : IFileWriter
{
    public void AddToQueue(ReadOnlySpan<char> message)
    {
        // Custom file writing logic
    }

    public void Dispose()
    {
        // Cleanup logic
    }
}
```

---

## Contributing

Contributions are welcome! Please open issues for bugs or feature requests and submit pull requests with new features or fixes.

1. Fork the repository.
2. Create a new branch for your feature.
3. Submit a pull request with a detailed explanation.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contact

For questions or support, please contact [support@grovegames.com](mailto:support@grovegames.com).
