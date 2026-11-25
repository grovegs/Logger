using System;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace GroveGames.Logger.Unity.Tests
{
    public sealed class LoggerBuilderExtensionsTests
    {
        [Test]
        public void AddFileLogProcessor_InvalidSettings_LogsErrorAndAddsNoProcessor()
        {
            var builder = new FakeLoggerBuilder();
            LogAssert.Expect(LogType.Error, new Regex("File logging is off: the logger settings"));

            builder.AddFileLogProcessor(new FakeLoggerSettings("Logs", 0));

            Assert.That(builder.ProcessorCount, Is.Zero);
        }

        [Test]
        public void AddFileLogProcessor_FolderCannotBeCreated_LogsErrorAndAddsNoProcessor()
        {
            var folderName = "LoggerBuilderExtensionsTests_" + Guid.NewGuid().ToString("N");
            var blocker = Path.Combine(Application.persistentDataPath, folderName);
            File.WriteAllText(blocker, string.Empty);
            var builder = new FakeLoggerBuilder();
            LogAssert.Expect(LogType.Error, new Regex("File logging is off: the log file could not be created"));

            try
            {
                builder.AddFileLogProcessor(new FakeLoggerSettings(folderName, 3));
            }
            finally
            {
                File.Delete(blocker);
            }

            Assert.That(builder.ProcessorCount, Is.Zero);
        }

        private sealed class FakeLoggerBuilder : ILoggerBuilder
        {
            public int ProcessorCount { get; private set; }

            public ILoggerBuilder AddLogProcessor(ILogProcessor processor)
            {
                ProcessorCount++;
                return this;
            }

            public ILoggerBuilder SetMinimumLevel(LogLevel level)
            {
                return this;
            }

            public ILoggerBuilder AddLogSource(Func<ILogProcessor[], ILogSource> sourceFactory)
            {
                return this;
            }
        }

        private sealed class FakeLoggerSettings : ILoggerSettings
        {
            private readonly string _fileFolderName;
            private readonly int _maxFileCount;

            public FakeLoggerSettings(string fileFolderName, int maxFileCount)
            {
                _fileFolderName = fileFolderName;
                _maxFileCount = maxFileCount;
            }

            public LogLevel MinLogLevel => LogLevel.Information;
            public int MaxFileCount => _maxFileCount;
            public string FileFolderName => _fileFolderName;
            public int FileBufferSize => 4096;
            public int FileChannelCapacity => 16;
        }
    }
}
