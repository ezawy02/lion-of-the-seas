using NUnit.Framework;
using SeaLion.Presentation.Pooling;

namespace SeaLion.Tests.EditMode.Presentation.Pooling
{
    public sealed class ReusableObjectPoolTests
    {
        private sealed class Item { public int ResetCount; }

        [Test]
        public void WarmUpAndReleaseReuseWithoutCreatingBeyondCapacity()
        {
            var created = 0;
            using (var pool = new ReusableObjectPool<Item>(2, () => { created++; return new Item(); }, i => i.ResetCount++))
            {
                pool.WarmUp(2);
                var first = pool.Rent();
                Assert.That(pool.Release(first), Is.True);
                Assert.That(pool.Release(first), Is.False);
                Assert.That(pool.Rent(), Is.SameAs(first));
                Assert.That(created, Is.EqualTo(2));
            }
        }

        [Test]
        public void ClearRequiresExplicitInUseDisposal()
        {
            var disposed = 0;
            var pool = new CraftPool<Item>(1, () => new Item(), null, i => disposed++);
            pool.Rent();
            Assert.Throws<System.InvalidOperationException>(() => pool.Clear(false));
            pool.Clear(true);
            Assert.That(disposed, Is.EqualTo(1));
            pool.Dispose();
        }
    }
}
