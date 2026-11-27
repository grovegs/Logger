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
            var recorder = new FakeLogProcessor();
            var console = new ConsoleLogProcessor(new ConsoleLogFormatter());
            using var source = new LogSource(new ILogProcessor[] { console, recorder });

            console.ProcessLog(LogLevel.Information, "Test", "From the logger");

            Assert.AreEqual(0, recorder.Messages.Count);
        }

        [Test]
        public void DebugLog_FromUnity_ReachesProcessors()
        {
            var recorder = new FakeLogProcessor();
            using var source = new LogSource(new ILogProcessor[] { recorder });

            Debug.Log("From Unity");

            Assert.AreEqual(1, recorder.Messages.Count);
            Assert.AreEqual("From Unity", recorder.Messages[0]);
        }

        [Test]
        public void DebugLog_BelowMinimumLevel_IsSkipped()
        {
            var recorder = new FakeLogProcessor();
            using var source = new LogSource(new ILogProcessor[] { recorder }, "Unity", LogLevel.Warning);

            Debug.Log("Below the level");
            Debug.LogWarning("At the level");

            Assert.AreEqual(1, recorder.Messages.Count);
            Assert.AreEqual("At the level", recorder.Messages[0]);
        }

        [Test]
        public void AddLogSource_BuilderMinimumLevel_FiltersUnityLogs()
        {
            var recorder = new FakeLogProcessor();
            var builder = new LoggerBuilder();
            builder.AddLogProcessor(recorder);
            builder.AddLogSource();
            builder.SetMinimumLevel(LogLevel.Error);
            using var logger = builder.Build();

            Debug.Log("Below the level");

            Assert.AreEqual(0, recorder.Messages.Count);
        }

        [Test]
        public void LogException_FromUnity_IncludesStackTrace()
        {
            var recorder = new FakeLogProcessor();
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

        [Test]
        public void DebugLog_ProcessorThrows_StillReachesTheOtherProcessors()
        {
            var recorder = new FakeLogProcessor();
            using var source = new LogSource(new ILogProcessor[] { new FakeThrowingLogProcessor(), recorder });

            Debug.Log("From Unity");

            Assert.AreEqual(1, recorder.Messages.Count);
        }

        private sealed class FakeThrowingLogProcessor : ILogProcessor
        {
            public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
            {
                throw new InvalidOperationException("Broken processor");
            }
        }

        private sealed class FakeLogProcessor : ILogProcessor
        {
            private readonly List<string> _messages;

            public List<string> Messages => _messages;

            public FakeLogProcessor()
            {
                _messages = new();
            }

            public void ProcessLog(LogLevel level, ReadOnlySpan<char> tag, ReadOnlySpan<char> message)
            {
                Messages.Add(message.ToString());
            }
        }
    }
}
