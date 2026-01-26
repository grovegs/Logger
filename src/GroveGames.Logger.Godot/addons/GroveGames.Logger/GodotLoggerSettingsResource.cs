using Godot;

namespace GroveGames.Logger;

[GlobalClass]
public partial class GodotLoggerSettingsResource : Resource
{
    private const string ResourcePath = "res://addons/GroveGames.Logger/logger_settings.tres";

    [Export] public LogLevel MinLogLevel { get; set; } = LogLevel.Information;
    [Export] public int MaxFileCount { get; set; } = 10;
    [Export] public string FileFolderName { get; set; } = "logs";
    [Export] public int FileBufferSize { get; set; } = 8192;
    [Export] public int FileChannelCapacity { get; set; } = 1000;

    public static GodotLoggerSettingsResource GetOrCreate()
    {
        if (ResourceLoader.Exists(ResourcePath))
        {
            return ResourceLoader.Load<GodotLoggerSettingsResource>(ResourcePath);
        }

        GodotSettings.CreateIfNotExist();
        var settings = new GodotLoggerSettingsResource
        {
            MinLogLevel = GodotSettings.MinLogLevel.Value,
            MaxFileCount = GodotSettings.MaxFileCount.Value,
            FileFolderName = GodotSettings.FileFolderName.Value,
            FileBufferSize = GodotSettings.FileBufferSize.Value,
            FileChannelCapacity = GodotSettings.FileChannelCapacity.Value
        };
        return settings;
    }
}
