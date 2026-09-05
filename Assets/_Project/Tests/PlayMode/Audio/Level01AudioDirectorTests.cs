using System.Collections;
using NUnit.Framework;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Presentation.Audio;
using UnityEngine;
using UnityEngine.TestTools;

namespace SeaLion.Tests.PlayMode.Audio
{
    public sealed class Level01AudioDirectorTests
    {
        [UnityTest]
        public IEnumerator BoundBattleEventsDriveAssaultAndResultMixes()
        {
            var owner = new GameObject("Level01AudioDirectorTest");
            var director = owner.AddComponent<Level01AudioDirector>();
            var stream = new BattleEventStream();
            var session = new BattleSession(
                new StableId("level-01"), new StableId("phase-traversal"),
                default(LoadoutSnapshot), stream: stream);
            director.Configure(null, false);
            director.Bind(stream);
            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);

            var payload = new BattleEventPayload(
                session.SessionId, default(StableId), default(StableId), Allegiance.Hostile,
                100, 50, 0, default(GateOutcome), default(BattleResult));
            Assert.That(session.TryPublishGameplayEvent(BattleEventType.BossPhaseChanged, payload), Is.True);
            Assert.That(director.CurrentSnapshot, Is.EqualTo(Level01AudioMixSnapshot.Assault));
            Assert.That(session.End(true, "guardian defeated"), Is.True);
            Assert.That(director.CurrentSnapshot, Is.EqualTo(Level01AudioMixSnapshot.Victory));

            Object.Destroy(owner);
            yield return null;
        }

        [UnityTest]
        public IEnumerator ImpactConcurrencyRejectsSpamButKeepsLossDistinct()
        {
            var owner = new GameObject("Level01AudioConcurrencyTest");
            var director = owner.AddComponent<Level01AudioDirector>();
            var library = ScriptableObject.CreateInstance<Level01AudioLibrary>();
            var clip = AudioClip.Create("impact", 4800, 1, 48000, false);
            SetPrivate(library, "guardianArmorHit", clip);
            SetPrivate(library, "crewLoss", clip);
            director.Configure(library, false);
            Assert.That(director.PlayGuardianHit(), Is.True);
            Assert.That(director.PlayGuardianHit(), Is.False);
            Assert.That(director.PlayCrewLoss(), Is.True);
            Assert.That(director.ActiveOneShotCount, Is.EqualTo(2));
            Assert.That(director.LastPlayedCue, Is.EqualTo(Level01AudioCue.CrewLoss));
            Object.Destroy(owner);
            Object.Destroy(library);
            Object.Destroy(clip);
            yield return null;
        }

        private static void SetPrivate(Object target, string field, object value)
        {
            var info = target.GetType().GetField(field,
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            Assert.That(info, Is.Not.Null, field);
            info.SetValue(target, value);
        }
    }
}
