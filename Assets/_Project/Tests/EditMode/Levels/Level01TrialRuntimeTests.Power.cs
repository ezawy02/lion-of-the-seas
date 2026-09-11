using NUnit.Framework;
using SeaLion.Gameplay.Levels;

namespace SeaLion.Tests.EditMode.Levels
{
    public sealed partial class Level01TrialRuntimeTests
    {
        [Test]
        public void SafeLaneGrantsShieldFireAndRescueThenEngageSpendsVolley()
        {
            var runtime = ReachAssault(easy: true);
            Assert.That(runtime.RescueCollected, Is.True);
            Assert.That(runtime.ShieldCharges, Is.EqualTo(2));
            Assert.That(runtime.FireRank, Is.EqualTo(3));
            Assert.That(runtime.ShowsPowerStatus, Is.True);
            Assert.That(runtime.FireDensity, Is.GreaterThan(1.4f));
            Assert.That(runtime.TryPrimaryAttack().Fired, Is.True);
            Assert.That(runtime.FireRank, Is.EqualTo(2));
            Assert.That(runtime.Stance, Is.EqualTo(AssaultStance.Engage));
        }

        [Test]
        public void RiskyLaneDropsFireDensityWithoutAShield()
        {
            var runtime = CreateRuntime();
            Assert.That(runtime.Begin(), Is.True);
            Advance(runtime, 3.1f);
            runtime.SetTraversalControl(1f, true);
            Advance(runtime, 4.4f);
            Assert.That(runtime.GateCommitted, Is.True);
            Assert.That(runtime.ChoseEasyGate, Is.False);
            Assert.That(runtime.ShieldCharges, Is.Zero);
            Assert.That(runtime.FireRank, Is.Zero);
            Assert.That(runtime.FireDensity, Is.LessThan(1f));
        }

        [Test]
        public void GuardianTelegraphsAGiantSlamThenShieldsAbsorbTheHit()
        {
            var runtime = ReachAssault(easy: true);
            var shields = runtime.ShieldCharges;
            var force = runtime.ForceCount;
            Advance(runtime, 5.1f);
            Assert.That(runtime.GuardianTelegraphing, Is.True);
            Assert.That(runtime.ObjectiveKey, Is.EqualTo("giantSlam"));
            Advance(runtime, 1.4f);
            Assert.That(runtime.GuardianTelegraphing, Is.False);
            Assert.That(runtime.ShieldCharges, Is.EqualTo(shields - 1));
            Assert.That(runtime.ForceCount, Is.LessThan(force));
            Assert.That(runtime.ForceCount, Is.GreaterThan(force / 2));
        }

        [Test]
        public void LastDefenderReadsAsTheEliteGuardBeat()
        {
            var runtime = ReachAssault(easy: true);
            while (runtime.HostileRemaining > 1 && runtime.IsRunning)
            {
                if (runtime.CanPrimaryAttack) runtime.TryPrimaryAttack();
                runtime.Step(0.1f);
            }
            Assert.That(runtime.HostileRemaining, Is.EqualTo(1));
            Assert.That(runtime.EliteGuardActive, Is.True);
            if (!runtime.GuardianTelegraphing)
                Assert.That(runtime.ObjectiveKey, Is.EqualTo("breakElite"));
        }

        private Level01TrialRuntime ReachAssault(bool easy)
        {
            var runtime = CreateRuntime();
            Assert.That(runtime.Begin(), Is.True);
            Advance(runtime, 3.1f);
            runtime.SetTraversalControl(easy ? -1f : 1f, true);
            Advance(runtime, 10.1f);
            Advance(runtime, 9.1f);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Assault));
            return runtime;
        }
    }
}
