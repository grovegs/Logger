## Godot

- Game code does not call `GD.Print`, because it allocates. `GD.PushWarning` and `GD.PushError` are allowed.
