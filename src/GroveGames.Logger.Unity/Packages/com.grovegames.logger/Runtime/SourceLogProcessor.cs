using System;

namespace GroveGames.Logger.Unity
{
    internal sealed class SourceLogProcessor : ILogProcessor
    {
        private readonly ILogProcessor[] _processors;
        private readonly LogLevel _minimumLevel;

        public SourceLogProcessor(ILogProcessor[] processors, LogLevel minimumLevel)
        {
            _minimumLevel = minimumLevel;

            var count = 0;
            for (var i = 0; i < processors.Length; i++)
            {
                if (processors[i] is not ConsoleLogProcessor)
                {
                    count++;
                }
            }

            _processors = new ILogProcessor[count];
            var index = 0;
            for (var i = 0; i < processors.Length; i++)
            {
                if (processors[i] is not ConsoleLogProcessor)
                {
                    _processors[index++] = processors[i];
                }
            }
        }

        public bool IsEnabled(LogLevel level)
        {
            return level >= _minimumLevel && _minimumLevel != LogLevel.None;
        }

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            if (!IsEnabled(level))
            {
                return;
            }

            for (var i = 0; i < _processors.Length; i++)
            {
                try
                {
                    _processors[i].ProcessLog(level, tag, message);
                }
                catch (Exception)
                {
                }
            }
        }
    }
}
