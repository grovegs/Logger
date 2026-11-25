using UnityEngine;

namespace GroveGames.Logger.Unity
{
    internal sealed class LogFileFlusher : ILogSource
    {
        private readonly IStreamWriter _writer;
        private bool _disposed;

        public LogFileFlusher(IStreamWriter writer)
        {
            _writer = writer;
            _disposed = false;
            Application.focusChanged += OnFocusChanged;
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            Application.focusChanged -= OnFocusChanged;
        }

        internal void OnFocusChanged(bool hasFocus)
        {
            if (!hasFocus && !_disposed)
            {
                _writer.Flush();
            }
        }
    }
}
