## Unity

- Game code does not call `Debug.Log`, because it allocates. `Debug.LogWarning` and `Debug.LogError` are allowed.
- A file that imports `UnityEngine` aliases the logger: `using ILogger = GroveGames.Logger.ILogger;`.
- Register with `builder.AddLogger()`, or with `builder.AddLogger(level, isConsoleEnabled)` when the level or console output differs per environment; the game decides the environment with GroveGames.Platform's `UnityEnvironment`, never the package.
