using NUnit.Framework;
using SeaLion.Core.Simulation;

namespace SeaLion.Tests.EditMode.Simulation
{
    public sealed class SimulationContractTests
    {
        [Test]
        public void ClockOwnsIntegerTicksWithoutFrameTime()
        {
            var clock = new FixedStepClock(60);
            Assert.AreEqual(0, clock.Tick);
            Assert.AreEqual(3, clock.AdvanceTicks(3));
            Assert.AreEqual(3, clock.Tick);
        }

        [Test]
        public void ReplayLogRequiresStableStrictOrdering()
        {
            var log = new ReplayInputLog(new[]
            {
                new ReplayInputRecord(2, 1f, false),
                new ReplayInputRecord(5, -1f, true)
            });
            Assert.IsTrue(log.TryGet(5, out var input));
            Assert.IsTrue(input.AbilityPressed);
            Assert.Throws<System.ArgumentException>(() => new ReplayInputLog(new[]
            {
                new ReplayInputRecord(2, 0f, false), new ReplayInputRecord(2, 0f, false)
            }));
        }

        [Test]
        public void InvalidNumericInputIsRejected()
        {
            Assert.Throws<System.ArgumentOutOfRangeException>(() => new ReplayInputRecord(-1, 0f, false));
            Assert.Throws<System.ArgumentException>(() => new ReplayInputRecord(0, float.NaN, false));
            Assert.IsFalse(DeterministicSeed.TryCreate(-1, out _));
        }
    }
}
