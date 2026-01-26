using Godot;

namespace GroveGames.Logger;

[GlobalClass]
public partial class GodotLoggerSettingsResource : Resource
{
    [Export] public LogLevel MinLogLevel { get; set; } = LogLevel.Information;
    [Export] public int MaxFileCount { get; set; } = 10;
    [Export] public string FileFolderName { get; set; } = "logs";
    [Export] public int FileBufferSize { get; set; } = 8192;
    [Export] public int FileChannelCapacity { get; set; } = 1000;
}
