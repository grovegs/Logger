#if !NET9_0_OR_GREATER

namespace System.Threading;

internal sealed class Lock
{
    private readonly object _syncRoot = new();

    public Scope EnterScope()
    {
        Monitor.Enter(_syncRoot);
        return new Scope(this);
    }

    public ref struct Scope
    {
        private Lock? _lock;

        internal Scope(Lock @lock)
        {
            _lock = @lock;
        }

        public void Dispose()
        {
            var @lock = _lock;
            if (@lock is not null)
            {
                _lock = null;
                Monitor.Exit(@lock._syncRoot);
            }
        }
    }
}

#endif
