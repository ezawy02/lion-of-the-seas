using System.Collections;
using NUnit.Framework;
using SeaLion.Combat.Bosses;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Crowd.Simulation;
using SeaLion.Gameplay.Deployment;
using SeaLion.Gameplay.Gates;
using SeaLion.Gameplay.Flagship;
using UnityEngine;
using UnityEngine.TestTools;

namespace SeaLion.Tests.PlayMode.Levels
{
    public sealed class Level01EdgeCaseTests
    {
        private GameObject root;
        private FlagshipDefinition definition;

        [UnityTearDown]
        public IEnumerator DestroyRoot()
        {
            if (root != null) Object.Destroy(root);
            if (definition != null) Object.Destroy(definition);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ReleaseBoundsZeroForceOverlappingGatesAndCapCompressionRemainSafe()
        {
            Assert.That(FlagshipController.ClampPosition(999f, 3f, -3f), Is.EqualTo(3f));
            Assert.That(FlagshipController.ClampPosition(-999f, 3f, -3f), Is.EqualTo(-3f));
            Assert.That(LandingCraftDeployer.ComputeContribution(0f), Is.EqualTo(1));
            Assert.That(LandingCraftDeployer.ComputeInterval(0f), Is.EqualTo(float.PositiveInfinity));

            var session = ActiveSession();
            var resolver = new GateResolver(3, session);
            var member = new StableId("member-1");
            var first = resolver.Resolve(new StableId("gate-a"), GateOutcome.Add, 2f, default, 1, member);
            var overlapping = resolver.Resolve(new StableId("gate-b"), GateOutcome.Multiply, 4f, default, first.After, member);
            Assert.That(first.Applied, Is.True);
            Assert.That(overlapping.Applied, Is.True);
            Assert.That(overlapping.After, Is.EqualTo(12));
            Assert.That(overlapping.Displayed, Is.EqualTo(3));
            Assert.That(overlapping.Compressed, Is.True);
            Assert.That(resolver.Resolve(new StableId("gate-a"), GateOutcome.Add, 2f, default, overlapping.After, member).Applied, Is.False);

            var force = new ForceRuntime(12, 3);
            Assert.That(force.LogicalCount, Is.EqualTo(12));
            Assert.That(force.DisplayedAgentCount, Is.EqualTo(3));
            Assert.That(force.DisplayedLogicalIndices[0], Is.EqualTo(0));
            Assert.That(force.DisplayedLogicalIndices[2], Is.EqualTo(11));
            force.SetDisplayCap(0);
            Assert.That(force.LogicalCount, Is.EqualTo(12));
            Assert.That(force.DisplayedAgentCount, Is.Zero);
            var depleted = new HarborGuardianController(new StableId("guardian-depleted"), 10f,
                new[] { new BossPhaseDefinition(new StableId("opening"), new StableId("entry"), 1f) },
                new[] { new StableId("broadside") }, FailurePressure.ForceDepletion, 1f);
            Assert.That(depleted.Enter(), Is.True);
            Assert.That(depleted.NotifyForceRemaining(1), Is.False);
            Assert.That(depleted.NotifyForceRemaining(0), Is.True);
            Assert.That(depleted.State, Is.EqualTo(HarborGuardianState.Failed));
            yield return null;
        }

        [UnityTest]
        public IEnumerator BossDefeatCommitsDuringDelayedVfxAndPauseResumeStopsControlAndDeployment()
        {
            var guardian = new HarborGuardianController(new StableId("guardian-01"), 10f,
                new[] { new BossPhaseDefinition(new StableId("opening"), new StableId("entry"), 1f) },
                new[] { new StableId("broadside") }, FailurePressure.ForceDepletion, 1f);
            var authoritativeVictory = false;
            var vfxStarted = false;
            guardian.Event += e =>
            {
                if (e.Type != HarborGuardianEventType.Victory) return;
                authoritativeVictory = true;
                // Simulates a presentation subscriber that starts a longer VFX sequence.
                vfxStarted = true;
            };
            Assert.That(guardian.Enter(), Is.True);
            Assert.That(guardian.ApplyDamage(10f), Is.True);
            Assert.That(authoritativeVictory, Is.True);
            Assert.That(guardian.State, Is.EqualTo(HarborGuardianState.Defeated));
            Assert.That(vfxStarted, Is.True);
            yield return null;
            Assert.That(guardian.State, Is.EqualTo(HarborGuardianState.Defeated));

            root = new GameObject("Level01PauseHarness");
            var input = root.AddComponent<SeaLion.Gameplay.Input.FlagshipInputAdapter>();
            var flagship = root.AddComponent<FlagshipController>();
            SetPrivateField(flagship, "input", input);
            SetPrivateField(input, "<HorizontalIntent>k__BackingField", 1f);
            yield return null;
            var moved = root.transform.position.x;
            flagship.SetPaused(true);
            yield return null;
            Assert.That(root.transform.position.x, Is.EqualTo(moved).Within(0.0001f));
            flagship.SetPaused(false);

            var session = ActiveSession();
            definition = ScriptableObject.CreateInstance<FlagshipDefinition>();
            SetPrivateField(definition, "controlBounds", new NormalizedBounds(0.1f, 0.9f));
            SetPrivateField(definition, "deployPattern", DeployPattern.Cadence);
            SetPrivateField(definition, "deploymentCadence", 0.1f);
            SetPrivateField(definition, "baseDeployment", 2f);
            SetPrivateField(definition, "presentationShipId", new StableId("ship-presentation"));
            SetPrivateField(definition, "wakeId", new StableId("wake"));
            SetPrivateField(definition, "recoilId", new StableId("recoil"));
            SetPrivateField(definition, "audioId", new StableId("audio"));
            var deployer = root.AddComponent<LandingCraftDeployer>();
            var deployed = 0;
            deployer.Deployed += _ => deployed++;
            deployer.Configure(definition, session, () => new TestCraft(), 4, Vector3.zero);
            deployer.Tick(0.11f);
            Assert.That(deployed, Is.EqualTo(1));
            deployer.SetPaused(true);
            deployer.Tick(1f);
            Assert.That(deployed, Is.EqualTo(1));
            deployer.SetPaused(false);
            deployer.Tick(0.11f);
            Assert.That(deployed, Is.EqualTo(2));
            Assert.That(LandingCraftDeployer.ComputeBurstSize(DeployPattern.Cadence, 99), Is.EqualTo(1));
            Assert.That(LandingCraftDeployer.ComputeSpreadOffset(DeployPattern.Spread, 0, 2, 10f), Is.EqualTo(-5f));
        }

        private static BattleSession ActiveSession()
        {
            var session = new BattleSession(new StableId("level-01"), new StableId("opening"),
                new LoadoutSnapshot(new StableId("ship"), new StableId("crew"), new StableId("ability")));
            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);
            return session;
        }

        private static void SetPrivateField(object target, string field, object value)
        {
            var info = target.GetType().GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, field);
            info.SetValue(target, value);
        }

        private sealed class TestCraft : ILandingCraft
        {
            public void Activate(Vector3 position, int contribution, int sequence) { }
            public void Deactivate() { }
        }
    }
}
