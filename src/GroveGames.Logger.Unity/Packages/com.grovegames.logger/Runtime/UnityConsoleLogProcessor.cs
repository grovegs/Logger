using System;
using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public sealed class UnityConsoleLogProcessor : ILogProcessor
    {
        private readonly ILogFormatter _logFormatter;

        public UnityConsoleLogProcessor(ILogFormatter logFormatter)
        {
            _logFormatter = logFormatter;
        }

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            var bufferSize = _logFormatter.GetBufferSize(level, tag, message);
            Span<char> buffer = stackalloc char[bufferSize];
            _logFormatter.Format(buffer, level, tag, message);
            var log = buffer.ToString();

            switch (level)
            {
                case LogLevel.Debug:
                case LogLevel.Information:
                    Debug.Log(log);
                    break;
                case LogLevel.Warning:
                    Debug.LogWarning(log);
                    break;
                case LogLevel.Error:
                    Debug.LogError(log);
                    break;
            }
        }
    }
}
