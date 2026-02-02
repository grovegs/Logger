using System;
using NUnit.Framework;

namespace GroveGames.Logger.Unity.Tests
{
    public sealed class UnityConsoleLogFormatterTests
    {
        [Test]
        public void GetBufferSize_ReturnsCorrectSize()
        {
            var formatter = new UnityConsoleLogFormatter();

            var size = formatter.GetBufferSize(LogLevel.Information, "TestTag", "Test message");

            Assert.AreEqual(22, size);
        }

        [Test]
        public void GetBufferSize_EmptyTagAndMessage_ReturnsMinimumSize()
        {
            var formatter = new UnityConsoleLogFormatter();

            var size = formatter.GetBufferSize(LogLevel.Information, ReadOnlySpan<char>.Empty, ReadOnlySpan<char>.Empty);

            Assert.AreEqual(3, size);
        }

        [Test]
        public void Format_FormatsCorrectly()
        {
            var formatter = new UnityConsoleLogFormatter();
            var buffer = new char[50];

            formatter.Format(buffer, LogLevel.Information, "TestTag", "Test message");
            var result = new string(buffer).TrimEnd('\0');

            Assert.AreEqual("[TestTag] Test message", result);
        }

        [Test]
        public void Format_AllLevels_SameFormat()
        {
            var formatter = new UnityConsoleLogFormatter();
            var levels = new[] { LogLevel.Debug, LogLevel.Information, LogLevel.Warning, LogLevel.Error };

            foreach (var level in levels)
            {
                var buffer = new char[50];
                formatter.Format(buffer, level, "Tag", "Message");
                var result = new string(buffer).TrimEnd('\0');

                Assert.AreEqual("[Tag] Message", result);
            }
        }

        [Test]
        public void Format_EmptyTag_FormatsWithEmptyBrackets()
        {
            var formatter = new UnityConsoleLogFormatter();
            var buffer = new char[30];

            formatter.Format(buffer, LogLevel.Information, ReadOnlySpan<char>.Empty, "Message");
            var result = new string(buffer).TrimEnd('\0');

            Assert.AreEqual("[] Message", result);
        }

        [Test]
        public void Format_EmptyMessage_FormatsWithoutMessage()
        {
            var formatter = new UnityConsoleLogFormatter();
            var buffer = new char[30];

            formatter.Format(buffer, LogLevel.Information, "Tag", ReadOnlySpan<char>.Empty);
            var result = new string(buffer).TrimEnd('\0');

            Assert.AreEqual("[Tag] ", result);
        }

        [Test]
        public void GetBufferSize_MatchesActualFormattedLength()
        {
            var formatter = new UnityConsoleLogFormatter();
            var testCases = new[]
            {
                (LogLevel.Information, "Short", "Quick test"),
                (LogLevel.Warning, "Medium", "A bit longer message here"),
                (LogLevel.Error, "", ""),
                (LogLevel.Debug, "VeryLongTagName", "Very long message with lots of content")
            };

            foreach (var (level, tag, message) in testCases)
            {
                var expectedSize = formatter.GetBufferSize(level, tag, message);
                var buffer = new char[expectedSize + 10];
                formatter.Format(buffer, level, tag, message);
                var result = new string(buffer).TrimEnd('\0');

                Assert.AreEqual(expectedSize, result.Length);
            }
        }
    }
}
