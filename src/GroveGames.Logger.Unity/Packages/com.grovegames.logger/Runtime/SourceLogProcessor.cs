using System;

namespace GroveGames.Logger.Unity
{
    internal sealed class SourceLogProcessor : ILogProcessor
    {
        private readonly ILogProcessor[] _processors;

        public SourceLogProcessor(ILogProcessor[] processors)
        {
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

        public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
        {
            for (var i = 0; i < _processors.Length; i++)
            {
                _processors[i].ProcessLog(level, tag, message);
            }
        }
    }
}
