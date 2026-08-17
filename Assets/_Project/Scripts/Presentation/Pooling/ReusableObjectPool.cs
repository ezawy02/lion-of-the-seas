using System;
using System.Collections.Generic;

namespace SeaLion.Presentation.Pooling
{
    public interface IReusableObjectPool< T > : IDisposable
    {
        int Capacity { get; }
        int AvailableCount { get; }
        int InUseCount { get; }
        T Rent();
        bool Release(T item);
        void WarmUp(int count);
        void Clear(bool disposeInUse);
    }

    /// <summary>Bounded pool with explicit ownership and duplicate-release protection.</summary>
    public sealed class ReusableObjectPool< T > : IReusableObjectPool< T > where T : class
    {
        private readonly Func<T> create;
        private readonly Action<T> reset;
        private readonly Action<T> dispose;
        private readonly Stack<T> available;
        private readonly HashSet<T> owned;
        private readonly HashSet<T> inUse;
        private bool isDisposed;

        public int Capacity { get; }
        public int AvailableCount { get { return available.Count; } }
        public int InUseCount { get { return inUse.Count; } }

        public ReusableObjectPool(int capacity, Func<T> create, Action<T> reset = null, Action<T> dispose = null)
        {
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            this.create = create ?? throw new ArgumentNullException(nameof(create));
            this.reset = reset;
            this.dispose = dispose;
            Capacity = capacity;
            available = new Stack<T>(capacity);
            owned = new HashSet<T>();
            inUse = new HashSet<T>();
        }

        public void WarmUp(int count)
        {
            EnsureNotDisposed();
            if (count < 0 || count > Capacity) throw new ArgumentOutOfRangeException(nameof(count));
            while (owned.Count < count) AddCreatedToAvailable();
        }

        public T Rent()
        {
            EnsureNotDisposed();
            if (available.Count == 0 && owned.Count < Capacity) AddCreatedToAvailable();
            if (available.Count == 0) throw new InvalidOperationException("Pool capacity exhausted.");
            var item = available.Pop();
            inUse.Add(item);
            return item;
        }

        public bool Release(T item)
        {
            EnsureNotDisposed();
            if (item == null || !owned.Contains(item) || !inUse.Remove(item)) return false;
            reset?.Invoke(item);
            available.Push(item);
            return true;
        }

        public void Clear(bool disposeInUse)
        {
            EnsureNotDisposed();
            if (!disposeInUse && inUse.Count != 0)
                throw new InvalidOperationException("Cannot clear while items are in use.");
            foreach (var item in owned) dispose?.Invoke(item);
            available.Clear();
            owned.Clear();
            inUse.Clear();
        }

        public void Dispose()
        {
            if (isDisposed) return;
            foreach (var item in owned) dispose?.Invoke(item);
            available.Clear();
            owned.Clear();
            inUse.Clear();
            isDisposed = true;
        }

        private void AddCreatedToAvailable()
        {
            var item = create();
            if (item == null) throw new InvalidOperationException("Pool factory returned null.");
            if (!owned.Add(item)) throw new InvalidOperationException("Pool factory returned a duplicate instance.");
            available.Push(item);
        }

        private void EnsureNotDisposed()
        {
            if (isDisposed) throw new ObjectDisposedException(nameof(ReusableObjectPool<T>));
        }
    }
}
