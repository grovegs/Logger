using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace GroveGames.Logger.Unity.Tests
{
    public sealed class LogHistoryTests
    {
        [Test]
        public void Add_BelowCapacity_KeepsEntriesInOrder()
        {
            var history = new LogHistory(3);
            var entries = new List<LogEntry>();

            history.Add(Entry("a"));
            history.Add(Entry("b"));
            history.CopyTo(entries);

            Assert.That(entries.Count, Is.EqualTo(2));
            Assert.That(entries[0].Message, Is.EqualTo("a"));
            Assert.That(entries[1].Message, Is.EqualTo("b"));
        }

        [Test]
        public void Add_BeyondCapacity_DropsOldestEntries()
        {
            var history = new LogHistory(3);
            var entries = new List<LogEntry>();

            foreach (var message in new[] { "a", "b", "c", "d", "e" })
            {
                history.Add(Entry(message));
            }

            history.CopyTo(entries);

            Assert.That(entries.Count, Is.EqualTo(3));
            Assert.That(entries[0].Message, Is.EqualTo("c"));
            Assert.That(entries[2].Message, Is.EqualTo("e"));
        }

        [Test]
        public void Add_Entry_IncreasesVersion()
        {
            var history = new LogHistory(3);
            var version = history.Version;

            history.Add(Entry("a"));

            Assert.That(history.Version, Is.GreaterThan(version));
        }

        [Test]
        public void Clear_WithEntries_RemovesAllAndIncreasesVersion()
        {
            var history = new LogHistory(3);
            var entries = new List<LogEntry>();
            history.Add(Entry("a"));
            var version = history.Version;

            history.Clear();
            history.CopyTo(entries);

            Assert.That(entries, Is.Empty);
            Assert.That(history.Version, Is.GreaterThan(version));
        }

        [Test]
        public void Constructor_ZeroCapacity_KeepsOneEntry()
        {
            var history = new LogHistory(0);

            Assert.That(history.Capacity, Is.EqualTo(1));
        }

        private static LogEntry Entry(string message)
        {
            return new LogEntry(LogLevel.Information, "Test", message, DateTime.Now);
        }
    }
}
