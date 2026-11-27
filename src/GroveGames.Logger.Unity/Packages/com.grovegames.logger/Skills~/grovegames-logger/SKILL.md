---
name: grovegames-logger
description: Rules for logging in game code with GroveGames.Logger, covering which level to pick, how to tag and what never to log. Use when writing or reviewing code that calls ILogger, adds a LogTag constant, reports an error, or adds or removes log lines.
---

# Using GroveGames.Logger

Rules for game code that logs with this package. They hold in every game. The API is in the package's `README.md`.

- A class that logs takes `ILogger` through its constructor and has a `LogTag` constant for its feature: `_logger.LogError(LogTag, $"...")`.
- Pick the level by what happened:
    - `LogError`: a broken invariant or unexpected failure, such as an unknown id, a missing view or a failed load.
    - `LogWarning`: something recoverable but suspicious, such as a corrupt save value that was reset or a fallback that was used.
    - `LogInformation`: a player-visible state change, once per event, such as a purchase, a level cleared or failed, an option changed or a scene loaded. Include the values that matter (level, cost, reward).
    - `LogDebug`: only temporarily, while tracking down a bug. Remove these logs before committing. Committed code has no `LogDebug` calls.
- Expected outcomes stay silent, such as a tap that hits nothing or a purchase the player can't afford.
- Never log per frame, per projectile or per hit.
- Use interpolated strings (`$"..."`). Below the minimum level, no value in the message is evaluated.
- Log numbers, `bool`, `DateTime`, `TimeSpan`, `Guid` and strings as they are; they format without allocating. An enum or another struct allocates, so log a string constant for it instead.

## Unity

- Game code does not call `Debug.Log`, because it allocates. `Debug.LogWarning` and `Debug.LogError` are allowed.
- A file that imports `UnityEngine` aliases the logger: `using ILogger = GroveGames.Logger.ILogger;`.
