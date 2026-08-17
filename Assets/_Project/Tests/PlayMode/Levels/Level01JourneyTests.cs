using System;
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using SeaLion.Combat.Bosses;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Persistence;
using SeaLion.Crowd.Simulation;
using SeaLion.Gameplay.Deployment;
using SeaLion.Gameplay.Gates;
using SeaLion.Gameplay.Landing;
using SeaLion.Gameplay.Results;
using UnityEngine;
using UnityEngine.TestTools;

namespace SeaLion.Tests.PlayMode.Levels
{
    public sealed class Level01JourneyTests
    {
        private readonly List<UnityEngine.Object> spawned = new List<UnityEngine.Object>();

        [UnityTearDown]
        public IEnumerator CleanScene()
        {
            for (var i = spawned.Count - 1; i >= 0; i--)
                if (spawned[i] != null) UnityEngine.Object.Destroy(spawned[i]);
            yield return null;
        }

        [UnityTest]
        public IEnumerator CleanSaveLevel01CompletesLandingGuardianVictoryAndRetry()
        {
            var saveFiles = new MemorySaveFileSystem();
            var repository = new LocalSaveRepository("level01-save", saveFiles);
            var clean = repository.Load();
            Assert.That(clean.UsedDefault, Is.True);
            Assert.That(clean.Data.highestUnlockedLevel, Is.EqualTo(1));

            var session = CreateSession();
            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);

            var input = Spawn<SeaLion.Gameplay.Input.FlagshipInputAdapter>();
            var flagship = Spawn<SeaLion.Gameplay.Flagship.FlagshipController>();
            SetPrivateField(flagship, "input", input);
            SetPrivateField(input, "<HorizontalIntent>k__BackingField", 1f);
            flagship.transform.position = new Vector3(0f, 0f, 0f);
            typeof(SeaLion.Gameplay.Flagship.FlagshipController)
                .GetMethod("Update", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                .Invoke(flagship, null);
            Assert.That(flagship.transform.position.x, Is.GreaterThan(0f));
            Assert.That(flagship.transform.position.x, Is.LessThanOrEqualTo(flagship.RightBound));
            yield return null;

            var force = new ForceRuntime(8, 4);
            var gate = new GateResolver(4, session);
            var gateResult = gate.Resolve(new StableId("level01-gate"), GateOutcome.Multiply, 4f,
                default, force.LogicalCount, new StableId("flagship"));
            force.SetLogicalCount(gateResult.After);
            Assert.That(gateResult.After, Is.EqualTo(32));
            Assert.That(force.LogicalCount, Is.EqualTo(32));

            var landForce = new ForceRuntime(0, 4);
            var landing = Spawn<LandingZoneController>();
            landing.Configure(session, landForce, new StableId("level01-beach"), 2);
            var craftA = new TestCraft();
            var craftB = new TestCraft();
            Assert.That(landing.TryAccept(craftA, 0, 12, true), Is.True);
            Assert.That(landing.TryAccept(craftB, 1, 20, true), Is.True);
            Assert.That(landing.IsCompleted, Is.True);
            Assert.That(landing.TransferredContribution, Is.EqualTo(32));
            Assert.That(landForce.LogicalCount, Is.EqualTo(32));
            Assert.That(session.TryTransition(BattleState.Landing), Is.True);
            Assert.That(session.TryTransition(BattleState.Assault), Is.True);

            var guardian = CreateGuardian();
            var bossVictory = false;
            guardian.Event += e => { if (e.Type == HarborGuardianEventType.Victory) bossVictory = true; };
            Assert.That(guardian.Enter(10), Is.True);
            Assert.That(guardian.ApplyDamage(100f, 11), Is.True);
            Assert.That(bossVictory, Is.True);

            var results = new BattleResultController(session, CreateSession);
            Assert.That(session.End(true, "guardian-defeated"), Is.True);
            Assert.That(results.HasTerminalResult, Is.True);
            Assert.That(results.TerminalResult.Value.IsVictory, Is.True);
            Assert.That(repository.TryGrantRewardWithOwnership("level01-tx", "level01-reward", "captain-2", out var applied, out var failure), Is.True, failure);
            Assert.That(applied, Is.True);
            Assert.That(results.TryRetry(out var retried), Is.True);
            Assert.That(retried.State, Is.EqualTo(BattleState.Loading));
            Assert.That(retried.TryTransition(BattleState.Ready), Is.True);
            Assert.That(retried.TryTransition(BattleState.Active), Is.True);
            Assert.That(retried.TryTransition(BattleState.Landing), Is.True);
            Assert.That(retried.TryTransition(BattleState.Assault), Is.True);
            Assert.That(retried.End(false, "force-depleted"), Is.True);
            Assert.That(results.TerminalResult.Value.IsVictory, Is.False);
            results.Dispose();
        }

        private T Spawn<T>() where T : Component
        {
            var go = new GameObject(typeof(T).Name);
            spawned.Add(go);
            return go.AddComponent<T>();
        }

        private static BattleSession CreateSession()
        {
            var loadout = new LoadoutSnapshot(new StableId("ship-1"), new StableId("crew-1"), new StableId("ability-1"));
            return new BattleSession(new StableId("level-01"), new StableId("opening"), loadout);
        }

        private static HarborGuardianController CreateGuardian()
        {
            return new HarborGuardianController(new StableId("guardian-01"), 100f,
                new[] { new BossPhaseDefinition(new StableId("opening"), new StableId("entry"), 1f) },
                new[] { new StableId("broadside") }, FailurePressure.ForceDepletion, 1f);
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

        private sealed class MemorySaveFileSystem : ILocalSaveFileSystem
        {
            private readonly Dictionary<string, string> files = new Dictionary<string, string>();
            public bool Exists(string path) => files.ContainsKey(path);
            public string ReadAllText(string path) => files[path];
            public void WriteAllText(string path, string contents) => files[path] = contents;
            public void Replace(string temporaryPath, string destinationPath, string backupPath)
            { if (files.ContainsKey(destinationPath)) files[backupPath] = files[destinationPath]; files[destinationPath] = files[temporaryPath]; files.Remove(temporaryPath); }
            public void Delete(string path) => files.Remove(path);
        }
    }
}
