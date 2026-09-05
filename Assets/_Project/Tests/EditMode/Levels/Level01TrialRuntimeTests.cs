using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Levels;
using UnityEditor;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Levels
{
    public sealed class Level01TrialRuntimeTests
    {
        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();
        private string savePath;

        [TearDown]
        public void TearDown()
        {
            for (var index = spawned.Count - 1; index >= 0; index--)
                if (spawned[index] != null) UnityEngine.Object.DestroyImmediate(spawned[index]);
            Delete(savePath);
            Delete(savePath + ".bak");
            Delete(savePath + ".tmp");
        }

        [Test]
        public void EasyRouteRunsOpeningGateLandingAssaultVictoryRewardAndRetry()
        {
            var runtime = CreateRuntime();
            Assert.That(runtime.Begin(), Is.True);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Opening));

            Advance(runtime, 3.1f);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Traversal));
            runtime.SetTraversalControl(-1f, true);
            Advance(runtime, 10.1f);
            Assert.That(runtime.GateCommitted, Is.True);
            Assert.That(runtime.ChoseEasyGate, Is.True);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Landing));
            Assert.That(runtime.ForceCount, Is.Zero);

            Advance(runtime, 9.1f);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Assault));
            Assert.That(runtime.ForceCount, Is.GreaterThan(8));

            AdvanceUntilTerminal(runtime, 30f);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Victory));
            Assert.That(runtime.RewardResult.HasValue, Is.True);
            Assert.That(runtime.RewardResult.Value.Succeeded, Is.True);
            Assert.That(runtime.CanRetry, Is.True);
            Assert.That(runtime.Retry(), Is.True);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Opening));
            Assert.That(runtime.TotalElapsed, Is.Zero);
        }

        [Test]
        public void RightSideCommitsTheAuthoredRiskyGateAndNeverDoubleAppliesIt()
        {
            var runtime = CreateRuntime();
            Assert.That(runtime.Begin(), Is.True);
            Advance(runtime, 3.1f);
            runtime.SetTraversalControl(1f, true);
            Advance(runtime, 4.2f);
            var afterGate = runtime.ForceCount;
            Assert.That(runtime.GateCommitted, Is.True);
            Assert.That(runtime.ChoseEasyGate, Is.False);
            Advance(runtime, 0.5f);
            Assert.That(runtime.ForceCount, Is.GreaterThanOrEqualTo(afterGate));
        }

        [Test]
        public void TraversalWaitsForRealSteeringAndDoesNotCatchUpFromIdleTime()
        {
            var runtime = CreateRuntime();
            Assert.That(runtime.Begin(), Is.True);
            Advance(runtime, 3.1f);
            Advance(runtime, 30f);

            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Traversal));
            Assert.That(runtime.GateCommitted, Is.False);
            Assert.That(runtime.TraversalActiveElapsed, Is.Zero);

            runtime.SetTraversalControl(-1f, true);
            Advance(runtime, 3.8f);
            Assert.That(runtime.GateCommitted, Is.False);
            Advance(runtime, 0.3f);
            Assert.That(runtime.GateCommitted, Is.True);
        }

        [Test]
        public void AssaultWaitsForPlayerFireAndEnforcesReloadCooldown()
        {
            var runtime = CreateRuntime();
            Assert.That(runtime.Begin(), Is.True);
            Advance(runtime, 3.1f);
            runtime.SetTraversalControl(-1f, true);
            Advance(runtime, 10.1f);
            Advance(runtime, 9.1f);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Assault));

            var hostileBefore = runtime.HostileRemaining;
            Advance(runtime, 3f);
            Assert.That(runtime.HostileRemaining, Is.EqualTo(hostileBefore));
            Assert.That(runtime.BossHealth01, Is.EqualTo(1f).Within(0.0001f));

            var first = runtime.TryPrimaryAttack();
            Assert.That(first.Fired, Is.True);
            Assert.That(first.TargetIndex, Is.GreaterThanOrEqualTo(0));
            Assert.That(runtime.HostileRemaining, Is.EqualTo(hostileBefore - 1));
            Assert.That(runtime.TryPrimaryAttack().Fired, Is.False);
            Advance(runtime, 0.6f);
            Assert.That(runtime.CanPrimaryAttack, Is.True);
        }

        private Level01TrialRuntime CreateRuntime()
        {
            var name = "level01-trial-test-" + Guid.NewGuid().ToString("N") + ".json";
            savePath = Path.Combine(Application.persistentDataPath, name);
            var go = new GameObject("Level01TrialRuntimeTests");
            spawned.Add(go);
            var runtime = go.AddComponent<Level01TrialRuntime>();
            runtime.Configure(
                new[] { Load<FlagshipDefinition>("Assets/_Project/Data/Loadouts/VerticalSlice/DefaultFlagship.asset"),
                    Load<FlagshipDefinition>("Assets/_Project/Data/Loadouts/VerticalSlice/LateenRaiderFlagship.asset") },
                new[] { Load<UnitRoleDefinition>("Assets/_Project/Data/Loadouts/VerticalSlice/DefaultSailorCrew.asset"),
                    Load<UnitRoleDefinition>("Assets/_Project/Data/Loadouts/VerticalSlice/SailmakersCrew.asset") },
                new[] { Load<CaptainAbilityDefinition>("Assets/_Project/Data/Loadouts/VerticalSlice/RallyAbility.asset"),
                    Load<CaptainAbilityDefinition>("Assets/_Project/Data/Loadouts/VerticalSlice/PowderBarrageAbility.asset") },
                Load<GateDefinition>("Assets/_Project/Data/Levels/Level01/Level01_Gate_Easy.asset"),
                Load<GateDefinition>("Assets/_Project/Data/Levels/Level01/Level01_Gate_Risky.asset"),
                Load<RescueDefinition>("Assets/_Project/Data/Levels/Level01/Level01_Rescue.asset"),
                Load<BossDefinition>("Assets/_Project/Data/Levels/Level01/Level01_Guardian.asset"),
                Load<RewardDefinition>("Assets/_Project/Data/Rewards/Level01Blueprint.asset"), name);
            return runtime;
        }

        private static T Load<T>(string path) where T : UnityEngine.Object
        {
            var value = AssetDatabase.LoadAssetAtPath<T>(path);
            Assert.That(value, Is.Not.Null, path);
            return value;
        }

        private static void Advance(Level01TrialRuntime runtime, float seconds)
        {
            for (var elapsed = 0f; elapsed < seconds; elapsed += 0.1f) runtime.Step(0.1f);
        }

        private static void AdvanceUntilTerminal(Level01TrialRuntime runtime, float limit)
        {
            for (var elapsed = 0f; elapsed < limit && runtime.IsRunning; elapsed += 0.1f)
            {
                if (runtime.AbilityReady) runtime.TryActivateAbility();
                if (runtime.CanPrimaryAttack) runtime.TryPrimaryAttack();
                runtime.Step(0.1f);
            }
        }

        private static void Delete(string path)
        {
            if (!string.IsNullOrEmpty(path) && File.Exists(path)) File.Delete(path);
        }
    }
}
