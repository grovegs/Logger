using Godot;

namespace GroveGames.Logger;

[GlobalClass]
public partial class GodotLoggerSettingsResource : Resource
{
    private const string ProjectSettingsKey = "grove_games/logger/settings_resource";
    private const string DefaultResourcePath = "res://addons/GroveGames.Logger/LoggerSettings.tres";

    [Export] public LogLevel MinLogLevel { get; set; } = LogLevel.Information;
    [Export] public int MaxFileCount { get; set; } = 10;
    [Export] public string FileFolderName { get; set; } = "logs";
    [Export] public int FileBufferSize { get; set; } = 8192;
    [Export] public int FileChannelCapacity { get; set; } = 1000;

    public static GodotLoggerSettingsResource GetOrCreate()
    {
        if (ProjectSettings.HasSetting(ProjectSettingsKey))
        {
            var resourcePath = ProjectSettings.GetSetting(ProjectSettingsKey).AsString();
            if (ResourceLoader.Exists(resourcePath))
            {
                return ResourceLoader.Load<GodotLoggerSettingsResource>(resourcePath);
            }
        }

        if (ResourceLoader.Exists(DefaultResourcePath))
        {
            return ResourceLoader.Load<GodotLoggerSettingsResource>(DefaultResourcePath);
        }

        return new();
    }

    internal static string GetProjectSettingsKey() => ProjectSettingsKey;
    internal static string GetDefaultResourcePath() => DefaultResourcePath;
}
