using System;
using NUnit.Framework;

namespace GroveGames.Logger.Unity.Tests
{
    public sealed class LogFileFlusherTests
    {
        [Test]
        public void OnFocusChanged_FocusLost_FlushesWriter()
        {
            var writer = new FakeStreamWriter();
            using var flusher = new LogFileFlusher(writer);

            flusher.OnFocusChanged(false);

            Assert.AreEqual(1, writer.FlushCount);
        }

        [Test]
        public void OnFocusChanged_FocusGained_DoesNotFlush()
        {
            var writer = new FakeStreamWriter();
            using var flusher = new LogFileFlusher(writer);

            flusher.OnFocusChanged(true);

            Assert.AreEqual(0, writer.FlushCount);
        }

        [Test]
        public void OnFocusChanged_AfterDispose_DoesNotFlush()
        {
            var writer = new FakeStreamWriter();
            var flusher = new LogFileFlusher(writer);
            flusher.Dispose();

            flusher.OnFocusChanged(false);

            Assert.AreEqual(0, writer.FlushCount);
        }

        private sealed class FakeStreamWriter : IStreamWriter
        {
            public int FlushCount { get; private set; }

            public void AddEntry(ReadOnlySpan<char> entry)
            {
            }

            public void Flush()
            {
                FlushCount++;
            }

            public void Dispose()
            {
            }
        }
    }
}
