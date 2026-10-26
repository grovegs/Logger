using System.Collections.Generic;
using NUnit.Framework;

namespace GroveGames.Logger.Unity.Tests
{
    public sealed class LogHistoryProcessorTests
    {
        [Test]
        public void ProcessLog_Message_AddsEntryWithLevelTagAndMessage()
        {
            var history = new LogHistory(4);
            var processor = new LogHistoryProcessor(history);
            var entries = new List<LogEntry>();

            processor.ProcessLog(LogLevel.Warning, "Economy", "Currency 5 is not defined");
            history.CopyTo(entries);

            Assert.That(entries.Count, Is.EqualTo(1));
            Assert.That(entries[0].Level, Is.EqualTo(LogLevel.Warning));
            Assert.That(entries[0].Tag, Is.EqualTo("Economy"));
            Assert.That(entries[0].Message, Is.EqualTo("Currency 5 is not defined"));
        }
    }
}
