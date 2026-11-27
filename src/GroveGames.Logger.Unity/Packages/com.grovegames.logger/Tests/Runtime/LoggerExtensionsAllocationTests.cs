using System;
using NUnit.Framework;
using UnityEngine.TestTools.Constraints;
using Is = UnityEngine.TestTools.Constraints.Is;

namespace GroveGames.Logger.Unity.Tests
{
    public sealed class LoggerExtensionsAllocationTests
    {
        [Test]
        public void LogInformation_ValueTypes_DoesNotAllocate()
        {
            var logger = new FakeCountingLogger(LogLevel.Debug);
            var count = 7u;
            var time = new DateTime(2024, 1, 15, 10, 30, 45);
            var id = new Guid("0f8fad5b-d9cb-469f-a165-70867728950e");
            var flag = true;
            var offset = new DateTimeOffset(time, TimeSpan.Zero);
            var amount = 1.5m;
            var duration = TimeSpan.FromSeconds(90);
            var stage = 12;
            var ratio = 1.5f;
            TestDelegate log = () => logger.LogInformation("Tag", $"{count} {time} {time:HH:mm:ss} {offset} {id} {flag} {stage} {ratio} {amount} {duration}");

            // The first calls JIT the delegate and grow the pooled message buffer.
            log();
            log();

            Assert.That(log, Is.Not.AllocatingGCMemory());
            Assert.Greater(logger.CharacterCount, 0);
        }

        [Test]
        public void LogDebug_LevelDisabled_DoesNotEvaluateTheMessage()
        {
            var logger = new FakeCountingLogger(LogLevel.Error);
            var calls = 0;

            logger.LogDebug("Tag", $"{Count(ref calls)} items");

            Assert.AreEqual(0, calls);
        }

        private static int Count(ref int calls)
        {
            calls++;
            return calls;
        }

        private sealed class FakeCountingLogger : ILogger
        {
            private readonly LogLevel _minimumLevel;
            private int _characterCount;

            public FakeCountingLogger(LogLevel minimumLevel)
            {
                _minimumLevel = minimumLevel;
            }

            public LogLevel MinimumLevel => _minimumLevel;
            public int CharacterCount => _characterCount;

            public void Log(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
            {
                _characterCount += message.Length;
            }
        }
    }
}
