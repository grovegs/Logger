using Godot;

namespace GroveGames.Logger;

public sealed class GodotSetting<[MustBeVariant] T>
{
    private readonly string _name;
    private Variant _defaultValue;

    public string Name => _name;

    public T Value => ProjectSettings.GetSetting(_name, _defaultValue).As<T>();

    public GodotSetting(string name, T defaultValue)
    {
        _name = name;
        _defaultValue = Variant.From(defaultValue);
    }
}
