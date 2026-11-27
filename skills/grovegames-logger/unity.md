## Unity

- Game code does not call `Debug.Log`, because it allocates. `Debug.LogWarning` and `Debug.LogError` are allowed.
- A file that imports `UnityEngine` aliases the logger: `using ILogger = GroveGames.Logger.ILogger;`.
