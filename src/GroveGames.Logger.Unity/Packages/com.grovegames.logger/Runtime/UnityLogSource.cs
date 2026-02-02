using System;
using System.Threading;
using UnityEngine;

namespace GroveGames.Logger.Unity;

public sealed class UnityLogSource : ILogSource
{
    private readonly ILogProcessor _logProcessor;
    private readonly string _tag;
    private volatile int _disposed;

    public UnityLogSource(ILogProcessor[] processors, string tag = "Unity")
    {
        _logProcessor = new UnitySourceLogProcessor(processors);
        _tag = tag;
        Application.logMessageReceived += OnLogMessageReceived;
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        if (_disposed == 1)
        {
            return;
        }

        var level = ConvertLogType(type);
        _logProcessor.ProcessLog(level, _tag.AsSpan(), condition.AsSpan());
    }

    private static LogLevel ConvertLogType(LogType type)
    {
        return type switch
        {
            LogType.Error => LogLevel.Error,
            LogType.Assert => LogLevel.Error,
            LogType.Exception => LogLevel.Error,
            LogType.Warning => LogLevel.Warning,
            LogType.Log => LogLevel.Information,
            _ => LogLevel.Debug
        };
    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        Application.logMessageReceived -= OnLogMessageReceived;
    }
}
