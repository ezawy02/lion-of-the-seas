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
    }
}
