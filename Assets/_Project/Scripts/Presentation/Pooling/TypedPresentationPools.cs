using System;

namespace SeaLion.Presentation.Pooling
{
    public abstract class TypedPresentationPool<T> : IDisposable where T : class
    {
        protected readonly ReusableObjectPool<T> Pool;
        protected TypedPresentationPool(int capacity, Func<T> create, Action<T> reset = null, Action<T> dispose = null)
        { Pool = new ReusableObjectPool<T>(capacity, create, reset, dispose); }
        public int Capacity { get { return Pool.Capacity; } }
        public int AvailableCount { get { return Pool.AvailableCount; } }
        public int InUseCount { get { return Pool.InUseCount; } }
        public T Rent() { return Pool.Rent(); }
        public bool Release(T item) { return Pool.Release(item); }
        public void WarmUp(int count) { Pool.WarmUp(count); }
        public void Clear(bool disposeInUse) { Pool.Clear(disposeInUse); }
        public void Dispose() { Pool.Dispose(); }
    }

    public sealed class CraftPool<T> : TypedPresentationPool<T> where T : class { public CraftPool(int c, Func<T> f, Action<T> r = null, Action<T> d = null) : base(c, f, r, d) { } }
    public sealed class ProjectilePool<T> : TypedPresentationPool<T> where T : class { public ProjectilePool(int c, Func<T> f, Action<T> r = null, Action<T> d = null) : base(c, f, r, d) { } }
    public sealed class VfxPool<T> : TypedPresentationPool<T> where T : class { public VfxPool(int c, Func<T> f, Action<T> r = null, Action<T> d = null) : base(c, f, r, d) { } }
    public sealed class DebrisPool<T> : TypedPresentationPool<T> where T : class { public DebrisPool(int c, Func<T> f, Action<T> r = null, Action<T> d = null) : base(c, f, r, d) { } }
    public sealed class UiNumberPool<T> : TypedPresentationPool<T> where T : class { public UiNumberPool(int c, Func<T> f, Action<T> r = null, Action<T> d = null) : base(c, f, r, d) { } }
    public sealed class AudioSourcePool<T> : TypedPresentationPool<T> where T : class { public AudioSourcePool(int c, Func<T> f, Action<T> r = null, Action<T> d = null) : base(c, f, r, d) { } }
}
