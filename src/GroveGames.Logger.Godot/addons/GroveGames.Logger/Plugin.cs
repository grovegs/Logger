#if TOOLS
using Godot;

namespace GroveGames.Logger;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        var resourcePath = GodotLoggerSettingsResource.GetDefaultResourcePath();

        if (!ProjectSettings.HasSetting(GodotLoggerSettingsResource.GetProjectSettingsKey()))
        {
            ProjectSettings.SetSetting(GodotLoggerSettingsResource.GetProjectSettingsKey(), resourcePath);
            ProjectSettings.SetInitialValue(GodotLoggerSettingsResource.GetProjectSettingsKey(), resourcePath);
            ProjectSettings.Save();
        }
    }

    public override void _ExitTree()
    {
    }
}
#endif