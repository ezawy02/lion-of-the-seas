using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Levels;
using UnityEditor;

namespace SeaLion.Tests.EditMode.Levels
{
    public sealed partial class Level01TrialRuntimeTests
    {
        [Test]
        public void LongFrameAndSmallFramesPreserveTheSameSimulationTime()
        {
            var a = CreateRuntime(); var b = CreateRuntime();
            Assert.That(a.Begin() && b.Begin(), Is.True);
            a.Step(3f);
            for (var i = 0; i < 30; i++) b.Step(.1f);
            a.SetTraversalControl(-1f, true); b.SetTraversalControl(-1f, true);
            a.Step(5f);
            for (var i = 0; i < 50; i++) b.Step(.1f);
            Assert.That(a.SimulationTick, Is.EqualTo(b.SimulationTick));
            Assert.That(a.RouteProgress, Is.EqualTo(b.RouteProgress).Within(.00001f));
            Assert.That(a.ForceCount, Is.EqualTo(b.ForceCount));
            Assert.That(a.LastGateAfter, Is.EqualTo(b.LastGateAfter));
        }

        [Test]
        public void GateBoundaryWaitsForLaneAndMissedRescueDoesNotGrantCrew()
        {
            var runtime = CreateRuntime(); runtime.Begin(); runtime.Step(3f);
            runtime.SetTraversalControl(0f, true); runtime.Step(8f);
            Assert.That(runtime.GateCommitted, Is.False);
            Assert.That(runtime.RouteProgress, Is.LessThan(.4f));
            runtime.SetTraversalControl(1f, true); runtime.Step(8f);
            Assert.That(runtime.GateCommitted, Is.True);
            Assert.That(runtime.RescueCollected, Is.False);
        }

        [Test]
        public void DestroyedCraftCannotTransferAndLandingConservesSurvivors()
        {
            var runtime = CreateRuntime(); runtime.Begin(); runtime.Step(3f);
            var before = runtime.ForceCount;
            Assert.That(runtime.DestroyCraft(0), Is.True);
            Assert.That(runtime.ForceCount, Is.LessThan(before));
            Assert.That(runtime.DestroyCraft(0), Is.False);
            runtime.SetTraversalControl(-1f, true); runtime.Step(9.9f);
            var survivors = runtime.ForceCount;
            runtime.Step(.2f);
            while (runtime.CanAssistLanding) { runtime.TryAssistLanding(); runtime.Step(.6f); }
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Assault));
            Assert.That(runtime.ForceCount, Is.LessThanOrEqualTo(survivors));
            Assert.That(runtime.ForceCount, Is.GreaterThan(0));
        }

        [Test]
        public void PausedRuntimeRejectsTimeAndPlayerAttacks()
        {
            var runtime = CreateRuntime(); runtime.Begin(); runtime.Step(3f);
            runtime.SetTraversalControl(-1f, true); runtime.Step(20f);
            runtime.SetPaused(true); var tick = runtime.SimulationTick;
            runtime.Step(5f);
            Assert.That(runtime.SimulationTick, Is.EqualTo(tick));
            Assert.That(runtime.TryPrimaryAttack().Fired, Is.False);
            runtime.SetPaused(false); runtime.Step(.1f);
            Assert.That(runtime.SimulationTick, Is.GreaterThan(tick));
        }

        [Test]
        public void AuthoredLevelConfigurationControlsOpeningAndDisplayBudget()
        {
            var runtime = CreateRuntime();
            var definition = Load<LevelDefinition>("Assets/_Project/Data/Levels/Level01/Level01.asset");
            runtime.ConfigureLevel(definition); runtime.Begin(); runtime.Step(2.1f);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Traversal));
            Assert.That(runtime.DisplayCap, Is.EqualTo(definition.DisplayCap));
        }

        [Test]
        public void StrongerArmyProducesMorePrimaryDamage()
        {
            var strong = CreateRuntime(); var weak = CreateRuntime();
            strong.Begin(); weak.Begin(); strong.Step(3f); weak.Step(3f);
            weak.DestroyCraft(0); weak.DestroyCraft(1);
            strong.SetTraversalControl(-1f, true); weak.SetTraversalControl(-1f, true);
            strong.Step(19.2f); weak.Step(19.2f);
            Assert.That(strong.ForceCount, Is.GreaterThan(weak.ForceCount));
            Assert.That(strong.TryPrimaryAttack().Damage, Is.GreaterThan(weak.TryPrimaryAttack().Damage));
        }
    }
}
