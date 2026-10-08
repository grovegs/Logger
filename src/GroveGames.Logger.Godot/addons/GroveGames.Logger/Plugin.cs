#if TOOLS
using Godot;

namespace GroveGames.Logger.Godot;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var resourcePath = LoggerSettingsResource.GetDefaultResourcePath();

        if (!ProjectSettings.HasSetting(LoggerSettingsResource.GetProjectSettingsKey()))
        {
            ProjectSettings.SetSetting(LoggerSettingsResource.GetProjectSettingsKey(), resourcePath);
            ProjectSettings.SetInitialValue(LoggerSettingsResource.GetProjectSettingsKey(), resourcePath);
            ProjectSettings.Save();
        }
    }

    public override void _ExitTree()
    {
    }
}
#endif