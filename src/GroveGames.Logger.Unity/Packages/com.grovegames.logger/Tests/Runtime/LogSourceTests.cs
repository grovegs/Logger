using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GroveGames.Logger.Unity.Tests
{
    public sealed class LogSourceTests
    {
        [Test]
        public void ConsoleLogProcessor_ProcessLog_IsNotEchoedToOtherProcessors()
        {
            var recorder = new TestLogProcessor();
            var console = new ConsoleLogProcessor(new ConsoleLogFormatter());
            using var source = new LogSource(new ILogProcessor[] { console, recorder });

            console.ProcessLog(LogLevel.Information, "Test", "From the logger");

            Assert.AreEqual(0, recorder.Messages.Count);
        }

        [Test]
        public void DebugLog_FromUnity_ReachesProcessors()
        {
            var recorder = new TestLogProcessor();
            using var source = new LogSource(new ILogProcessor[] { recorder });

            Debug.Log("From Unity");

            Assert.AreEqual(1, recorder.Messages.Count);
            Assert.AreEqual("From Unity", recorder.Messages[0]);
        }

        [Test]
        public void LogException_FromUnity_IncludesStackTrace()
        {
            var recorder = new TestLogProcessor();
            using var source = new LogSource(new ILogProcessor[] { recorder });
            LogAssert.ignoreFailingMessages = true;

            try
            {
                Throw();
            }
            catch (InvalidOperationException exception)
            {
                Debug.LogException(exception);
            }
            finally
            {
                LogAssert.ignoreFailingMessages = false;
            }

            var message = recorder.Messages.Find(logged => logged.StartsWith("InvalidOperationException: Broken", StringComparison.Ordinal));
            Assert.IsNotNull(message);
            StringAssert.Contains(nameof(LogException_FromUnity_IncludesStackTrace), message);
        }

        private static void Throw()
        {
            throw new InvalidOperationException("Broken");
        }

        private sealed class TestLogProcessor : ILogProcessor
        {
            public List<string> Messages { get; } = new();

            public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
            {
                Messages.Add(message.ToString());
            }
        }
    }
}
