# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

GroveGames.Logger is a high-performance, zero-allocation logging library for .NET, Unity, and Godot with Native AOT support. The library uses interpolated string handlers and `Span<char>` to eliminate heap allocations, making it ideal for game development scenarios where performance is critical. It targets .NET 10.0 for modern features and netstandard2.1 for Unity compatibility.

## Project Structure

The repository contains three main project types:

- **Core Library** (`src/GroveGames.Logger/`): The main .NET library targeting both `net10.0` and `netstandard2.1`
- **Godot Integration** (`src/GroveGames.Logger.Godot/`): Godot-specific formatters, processors, and plugin
- **Unity Integration** (`src/GroveGames.Logger.Unity/`): Unity-specific package structure

The architecture is built around several key abstractions:

1. **Core Interfaces**: `ILogger`, `ILogProcessor`, `ILogFormatter` define the logging contracts
2. **Interpolated String Handlers**: Zero-allocation message formatting via compile-time handlers
3. **Processors**: `FileLogProcessor`, `ConsoleLogProcessor` for different output targets
4. **Formatters**: `FileLogFormatter`, `ConsoleLogFormatter` for customizable output formats
5. **Factory Pattern**: `LoggerFactory` and `LoggerBuilder` for easy logger configuration

## Development Commands

### Building

```bash
dotnet build                           # Build all projects
dotnet build -c Release               # Release build
```

### Testing

```bash
dotnet test                           # Run all tests
dotnet test tests/GroveGames.Logger.Tests/        # Core library tests
```

### Formatting

```bash
dotnet format                         # Format all code according to .editorconfig
dotnet format --verify-no-changes    # Check if code is properly formatted (CI/CD)
dotnet format whitespace             # Format whitespace only
dotnet format style                  # Apply code style fixes
```

### Packaging

```bash
dotnet pack -c Release                # Create NuGet packages
```

## Code Style & Formatting

The project uses automated formatting via GitHub Actions and comprehensive configuration files:

### Configuration Structure

The project uses a layered configuration approach with platform-specific settings:

#### Root Configuration (Core .NET Library)

- **EditorConfig** (`.editorconfig`): Core C# coding standards, 4-space indentation, naming conventions (private fields: `_camelCase`, static: `s_camelCase`, interfaces: `IPascalCase`)
- **Git Configuration** (`.gitignore`/`.gitattributes`): Focused on .NET build outputs, NuGet packages, IDE files, and basic OS artifacts
- **VS Code Settings** (`.vscode/settings.json`): C# formatting and save actions

#### Unity-Specific Configuration (`sandbox/UnityApplication/`)

- **EditorConfig** (`.editorconfig`): Inherits core C# settings, same coding standards as main library
- **Git Configuration** (`.gitignore`/`.gitattributes`): Unity build artifacts, temp files, auto-generated content, Git LFS for large assets, Unity YAML merge drivers
- **Project Files**: Unity-specific project configuration and package management

#### Godot-Specific Configuration (`sandbox/GodotApplication/`)

- **EditorConfig** (`.editorconfig`): Inherits core C# settings plus Godot scene files (`.tscn`/`.tres`) with 2-space indentation
- **Git Configuration** (`.gitignore`/`.gitattributes`): Godot-specific files (`.godot/`, `.import/`), C# project artifacts, binary asset handling
- **Plugin Configuration** (`src/GroveGames.Logger.Godot/addons/GroveGames.Logger/plugin.cfg`): Godot plugin metadata

### Target Frameworks & Features

- **Multi-targeting**: `net10.0` (with AOT support) and `netstandard2.1` (for Unity compatibility)
- **Nullable Reference Types**: Enabled across the project
- **AOT Compatibility**: The `net10.0` target includes AOT analyzers and trimming support
- **Polyfills via Extension Members**: Custom polyfills using C# 14 extension members (`extension(Type)` syntax) in `Polyfills/` folder provide backward compatibility for netstandard2.1. These use `#if !NET6_0_OR_GREATER` preprocessor directives and are placed in the `System` namespace for seamless usage:
  - `CallerArgumentExpressionAttribute` for parameter name capture
  - `ArgumentNullException.ThrowIfNull` static method
  - `ArgumentException.ThrowIfNullOrEmpty` static method
  - `ArgumentOutOfRangeException.ThrowIfNegative`, `ThrowIfNegativeOrZero`, `ThrowIfGreaterThan` static methods
  - `ObjectDisposedException.ThrowIf` static method
  - `Lock` class for thread synchronization
  - `InterpolatedStringHandlerAttribute` and `InterpolatedStringHandlerArgumentAttribute` for interpolated string handlers
- **Code Formatting**:
  - Automatic formatting on save configured in VS Code settings
  - `dotnet format` command respects all `.editorconfig` settings
  - GitHub Actions enforce formatting via `--verify-no-changes` flag
  - Supports whitespace, style, and analyzer-based formatting

## Platform Integration Structure

### Unity Package Structure (`src/GroveGames.Logger.Unity/Packages/com.grovegames.logger/`)

Standard Unity Package Manager (UPM) layout:

- `Runtime/` - Runtime scripts with `.asmdef`
- `Editor/` - Editor scripts with `.asmdef`
- `Tests/Runtime/` - Unity Test Framework tests with `.asmdef`
- `package.json` - UPM package manifest
- `LICENSE` → symlink to root LICENSE
- `README.md` → symlink to root README.md

Unity-specific components:

- `UnityLoggerSettings` - Settings class with JSON persistence in `ProjectSettings/GroveGamesLoggerSettings.json`
- `UnityLoggerSettingsProvider` - SettingsProvider for Edit > Project Settings > Grove Games > Logger
- `UnityLoggerFactory` - Factory that reads settings and creates loggers
- `UnityLoggerBuilderExtensions` - Extension methods (`AddUnityFileLogProcessor`, `AddUnityConsoleLogProcessor`)
- `UnityConsoleLogFormatter` - Formatter for Unity console output (format: `[Tag] Message`)
- `UnityConsoleLogProcessor` - Processor that routes logs to Unity's `Debug.Log`, `Debug.LogWarning`, `Debug.LogError`
- `UnityLogFileFactory` - File factory using `Application.persistentDataPath` for log file location
- `UnityLogHandler` - Subscribes to `Application.logMessageReceived` to capture Unity's internal logs and forward them to file logging

The Unity package requires the core NuGet package (`GroveGames.Logger`) installed via NuGetForUnity, then the Unity package via git URL.

### Godot Addon Structure (`src/GroveGames.Logger.Godot/addons/GroveGames.Logger/`)

Standard Godot addon layout:

- `plugin.cfg` - Godot plugin configuration
- `Plugin.cs` - Plugin entry point (EditorPlugin)
- `LICENSE` → symlink to root LICENSE
- `README.md` → symlink to root README.md

The Godot addon requires the NuGet package (`GroveGames.Logger.Godot`) plus the addon files extracted to project's `addons/` folder.

## Testing Framework

- **Test Framework**: xUnit v3
- **Test Projects**:
  - `GroveGames.Logger.Tests` (core functionality)
- **Test Configuration**: Uses `xunit.runner.json` for xUnit configuration

## Build Configuration

Key build configurations:

- **Multi-targeting**: Projects support both modern .NET and legacy .NET Standard
- **AOT Support**: Native AOT compilation enabled for `net10.0` target
- **Documentation**: XML documentation generation enabled for all projects
- **Package Properties**: Centralized in `Directory.Build.props`

## SDK Version

The project targets .NET 10.0 SDK (see `global.json`). When working with this codebase, ensure you have .NET 10.0 SDK installed.

## GitHub Workflows

The project uses reusable workflows from `grovegs/workflows`:

- **Tests** (`tests.yml`): Runs on pushes/PRs to main/develop branches
- **Format** (`format.yml`): Validates code formatting
- **Release** (`release.yml`): Manual workflow for creating releases and publishing NuGet packages

## Development Sandbox

The `sandbox/` directory contains sample applications for testing and development:

- **ConsoleApplication**: Basic .NET console app for testing core functionality
- **UnityApplication**: Full Unity project for testing Unity integration
- **GodotApplication**: Godot project with the logger addon for testing Godot integration
- **Benchmark**: Performance benchmarking application

### Unity Sandbox Setup

The Unity sandbox project uses a symlink to reference the Unity package locally:

- `Packages/com.grovegames.logger` → symlink to `src/GroveGames.Logger.Unity/Packages/com.grovegames.logger`
- `Assets/Editor/PluginBuilder/` → separate assembly that auto-builds `GroveGames.Logger.dll` to `Assets/Plugins`
- `Packages/manifest.json` → includes NuGetForUnity via OpenUPM scoped registry
- `Assets/packages.config` → NuGet package dependencies

The PluginBuilder is in a separate assembly (`PluginBuilder.Editor.asmdef`) with no dependencies, allowing it to compile even when the Unity package has errors due to a missing DLL. On first Unity open, it builds the DLL automatically, then refreshes so the package can compile.

### Godot Sandbox Setup

The Godot sandbox project uses a symlink to reference the Godot addon locally:

- `addons/GroveGames.Logger` → symlink to `src/GroveGames.Logger.Godot/addons/GroveGames.Logger`

These sandbox projects are useful for:

- Testing changes across different platforms
- Demonstrating usage examples
- Performance testing and benchmarking
- Integration validation

## Key Dependencies

- **Microsoft.SourceLink.GitHub**: For source linking in packages

Note: Custom polyfill extensions in the `System` namespace provide backward compatibility for netstandard2.1.

## Template Patterns for Other Packages

This project structure serves as a template for other GroveGames packages. Key patterns to follow:

### Interface Abstraction for External Dependencies

When using types from NuGet packages that are only available in newer .NET versions, wrap them in custom interfaces to avoid exposing them in the public API. This prevents Unity/Godot users from needing to reference those packages directly.

Example with `TimeProvider` (from `Microsoft.Bcl.TimeProvider`):

```csharp
public interface ITimeProvider
{
    DateTimeOffset GetUtcNow();
}

public sealed class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset GetUtcNow() => TimeProvider.System.GetUtcNow();
}

public LogFileFactory(..., ITimeProvider? timeProvider = null)
{
    _timeProvider = timeProvider ?? new SystemTimeProvider();
}
```

Current interface abstractions:
- `ITimeProvider` / `SystemTimeProvider` - Wraps `TimeProvider` for time operations
- `IFileSystem` / `FileSystem` - Wraps file system operations for testability

This pattern ensures:

- Public API only exposes types defined in the library
- External dependencies are implementation details
- Unity/Godot projects compile without needing polyfill packages in their project
- Easy unit testing via mock implementations

### Platform-Specific Project Settings

Both Unity and Godot integrations provide project settings for configuration:

**Unity** (via `SettingsProvider`):
- Settings stored in `ProjectSettings/GroveGamesLoggerSettings.json`
- UI at Edit > Project Settings > Grove Games > Logger
- `UnityLoggerSettings` class with `Load()` and `Save()` methods
- `UnityLoggerSettingsProvider` for Editor UI

**Godot** (via `ProjectSettings`):
- Settings stored in Godot's project settings under `grove_games/logger/`
- `GodotSettings` static class with `GodotSetting<T>` typed accessors
- `CreateIfNotExist()` to initialize defaults

Both provide factory classes (`UnityLoggerFactory`, `GodotLoggerFactory`) and builder extensions for easy setup.

### Backward Compatibility via Static Extensions

Use C# 14 extension members with preprocessor directives for .NET API polyfills:

```csharp
#if !NET6_0_OR_GREATER
namespace System;

internal static class ArgumentNullExceptionExtensions
{
    extension(ArgumentNullException)
    {
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                throw new ArgumentNullException(paramName);
            }
        }
    }
}
#endif
```

### README Structure

Consistent platform sections with installation + usage + components:

1. **Features** - Brief feature list
2. **.NET** - NuGet install, usage examples, core/concurrent components
3. **Unity** - NuGetForUnity + git URL install, Unity-specific usage, Unity components
4. **Godot** - NuGet + addon install, Godot-specific usage
5. **Architecture** - Performance optimizations
6. **Testing/Contributing/License**

### Sandbox Applications

Always include sandbox projects for testing:

- `sandbox/ConsoleApplication/` - Basic .NET testing
- `sandbox/UnityApplication/` - Unity integration testing (symlink to Unity package, isolated PluginBuilder assembly for auto DLL build)
- `sandbox/GodotApplication/` - Godot integration testing (symlink to Godot addon)
- `sandbox/Benchmark/` - Performance benchmarking
