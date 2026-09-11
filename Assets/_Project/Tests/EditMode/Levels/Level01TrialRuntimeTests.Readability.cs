using NUnit.Framework;
using SeaLion.Gameplay.Levels;

namespace SeaLion.Tests.EditMode.Levels
{
    public sealed partial class Level01TrialRuntimeTests
    {
        [Test]
        public void AfterGateCommitTheObjectiveSendsTheFleetToShore()
        {
            var runtime = CreateRuntime();
            Assert.That(runtime.Begin(), Is.True);
            Advance(runtime, 3.1f);
            Assert.That(runtime.ObjectiveKey, Is.EqualTo("steerToChoose"));
            runtime.SetTraversalControl(-1f, true);
            Advance(runtime, 4.4f);
            Assert.That(runtime.GateCommitted, Is.True);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Traversal));
            Assert.That(runtime.ObjectiveKey, Is.EqualTo("sailToShore"));
            Assert.That(runtime.ForceCount, Is.GreaterThan(8));
            Assert.That(runtime.LastGateAfter, Is.GreaterThan(runtime.LastGateBefore));
            Assert.That(runtime.ForceCount, Is.EqualTo(runtime.LastGateAfter));
            Assert.That(runtime.LastForceDelta, Is.EqualTo(runtime.LastGateAfter - runtime.LastGateBefore));
            Assert.That(runtime.ShowsForceDelta, Is.True);
            Assert.That(runtime.PeakForce, Is.GreaterThanOrEqualTo(runtime.ForceCount));
        }

        [Test]
        public void LandingKeepsReadableForceWhileCrowdShowsLandedOnly()
        {
            var runtime = CreateRuntime();
            Assert.That(runtime.Begin(), Is.True);
            Advance(runtime, 3.1f);
            runtime.SetTraversalControl(-1f, true);
            Advance(runtime, 10.1f);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Landing));
            var readable = runtime.ForceCount;
            Assert.That(readable, Is.GreaterThan(8));
            Assert.That(runtime.DisplayedForceCount, Is.Zero);
            Assert.That(runtime.ShowsLandingCount, Is.True);
            Assert.That(runtime.LandedCraftCount, Is.Zero);
            Assert.That(runtime.LandingCraftTotal, Is.GreaterThan(0));
            Assert.That(runtime.ObjectiveKey, Is.EqualTo("landing"));

            Advance(runtime, 2.2f);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Landing));
            Assert.That(runtime.ForceCount, Is.EqualTo(readable));
            Assert.That(runtime.DisplayedForceCount, Is.GreaterThan(0));
            Assert.That(runtime.DisplayedForceCount, Is.LessThan(runtime.ForceCount));
            Assert.That(runtime.LandedCraftCount, Is.GreaterThan(0));
            Assert.That(runtime.LandedCraftCount, Is.LessThan(runtime.LandingCraftTotal));
        }
    }
}
