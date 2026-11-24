using System;
using System.Buffers;
using UnityEngine;

namespace GroveGames.Logger.Unity
{
    public sealed class ConsoleLogProcessor : ILogProcessor
    {
        private const int StackBufferSize = 1024;

        [ThreadStatic]
        private static bool s_isWriting;

        private readonly ILogFormatter _logFormatter;

        internal static bool IsWriting => s_isWriting;

        public ConsoleLogProcessor(ILogFormatter logFormatter)
        {
            _logFormatter = logFormatter;
        }

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            var log = Format(level, tag, message);
            s_isWriting = true;

            try
            {
                Write(level, log);
            }
            finally
            {
                s_isWriting = false;
            }
        }

        private string Format(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            var bufferSize = _logFormatter.GetBufferSize(level, tag, message);

            if (bufferSize <= StackBufferSize)
            {
                Span<char> buffer = stackalloc char[bufferSize];
                _logFormatter.Format(buffer, level, tag, message);
                return buffer.ToString();
            }

            var rented = ArrayPool<char>.Shared.Rent(bufferSize);

            try
            {
                var buffer = rented.AsSpan(0, bufferSize);
                _logFormatter.Format(buffer, level, tag, message);
                return buffer.ToString();
            }
            finally
            {
                ArrayPool<char>.Shared.Return(rented);
            }
        }

        private static void Write(LogLevel level, string log)
        {
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
