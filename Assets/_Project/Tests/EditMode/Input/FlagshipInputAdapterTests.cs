using NUnit.Framework;
using SeaLion.Gameplay.Input;

namespace SeaLion.Tests.EditMode.Input
{
    public sealed class FlagshipInputAdapterTests
    {
        [TestCase(-480f, -1f)]
        [TestCase(-120f, -0.5f)]
        [TestCase(0f, 0f)]
        [TestCase(120f, 0.5f)]
        [TestCase(480f, 1f)]
        public void MapHorizontalDrag_NormalizesAndClamps(float pixels, float expected)
        {
            Assert.That(FlagshipInputAdapter.MapHorizontalDrag(pixels, 240f), Is.EqualTo(expected));
        }

        [Test]
        public void MapHorizontalDrag_InvalidScaleReturnsZero()
        {
            Assert.That(FlagshipInputAdapter.MapHorizontalDrag(100f, 0f), Is.Zero);
        }

        [Test]
        public void PresentationStateStartsNeutral()
        {
            var owner = new UnityEngine.GameObject("flagship-input-test");
            try
            {
                var adapter = owner.AddComponent<FlagshipInputAdapter>();
                Assert.That(adapter.HorizontalIntent, Is.Zero);
                Assert.That(adapter.IsDragging, Is.False);
                Assert.That(adapter.HasSteered, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }

        [Test]
        public void UiSteeringExposesRealIntentAndResetClearsTheAttempt()
        {
            var owner = new UnityEngine.GameObject("flagship-ui-input-test");
            try
            {
                var adapter = owner.AddComponent<FlagshipInputAdapter>();
                adapter.SetUiIntent(-1f);
                Assert.That(adapter.HorizontalIntent, Is.EqualTo(-1f));
                Assert.That(adapter.HasSteered, Is.True);
                adapter.ReleaseUiIntent();
                Assert.That(adapter.HorizontalIntent, Is.EqualTo(-1f));
                adapter.Reset();
                Assert.That(adapter.HorizontalIntent, Is.Zero);
                Assert.That(adapter.HasSteered, Is.False);
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(owner);
            }
        }
    }
}
