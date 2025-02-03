#if TOOLS
using Godot;

namespace GroveGames.Logger;

[Tool]
public partial class Plugin : EditorPlugin
{
    public override void _EnterTree()
    {
        GodotSettings.CreateIfNotExist();
    }

    public override void _ExitTree()
    {
    }
}
#endif