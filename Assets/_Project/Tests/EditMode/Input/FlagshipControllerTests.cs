using NUnit.Framework;
using SeaLion.Gameplay.Flagship;

namespace SeaLion.Tests.EditMode.Input
{
    public sealed class FlagshipControllerTests
    {
        [TestCase(-10f, -2f, 3f, -2f)]
        [TestCase(10f, -2f, 3f, 3f)]
        [TestCase(0.5f, 3f, -2f, 0.5f)]
        public void ClampPosition_HandlesBoundsInEitherOrder(float position, float first, float second, float expected)
        {
            Assert.That(FlagshipController.ClampPosition(position, first, second), Is.EqualTo(expected));
        }

        [Test]
        public void SmoothNormalized_IsFrameRateIndependentAndClamped()
        {
            var oneFrame = FlagshipController.SmoothNormalized(0f, 1f, 10f, 1f / 60f);
            var twoHalfFrames = FlagshipController.SmoothNormalized(
                FlagshipController.SmoothNormalized(0f, 1f, 10f, 1f / 120f), 1f, 10f, 1f / 120f);
            Assert.That(oneFrame, Is.EqualTo(twoHalfFrames).Within(0.002f));
            Assert.That(FlagshipController.SmoothNormalized(2f, 2f, 10f, 1f), Is.EqualTo(1f));
        }

        [Test]
        public void SmoothNormalized_ZeroDeltaDoesNotJump()
        {
            Assert.That(FlagshipController.SmoothNormalized(-0.4f, 1f, 20f, 0f), Is.EqualTo(-0.4f));
        }

        [Test]
        public void InvalidNumbersCannotPoisonMovementState()
        {
            Assert.That(FlagshipController.SmoothNormalized(float.NaN, float.PositiveInfinity, 10f, .1f), Is.Zero);
            Assert.That(FlagshipController.ClampPosition(float.NaN, -5f, 5f), Is.Zero);
            Assert.That(FlagshipController.ClampPosition(2f, float.NaN, 5f), Is.EqualTo(2f));
        }
    }
}
