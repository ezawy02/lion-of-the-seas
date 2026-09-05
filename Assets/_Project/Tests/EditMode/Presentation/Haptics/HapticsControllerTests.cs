using NUnit.Framework;
using UnityEngine;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Presentation.Haptics;

namespace SeaLion.Tests.EditMode.Presentation.Haptics
{
    public sealed class HapticsControllerTests
    {
        private sealed class FakePlatform : IHapticsPlatform
        {
            public bool IsSupported { get; set; }
            public int Pulses { get; private set; }
            public void Vibrate() { Pulses++; }
        }

        [Test]
        public void SettingDisablesPulseAtPlatformSeam()
        {
            var host = new GameObject("HapticsTest");
            var controller = host.AddComponent<HapticsController>();
            var platform = new FakePlatform { IsSupported = true };
            controller.Initialize(platform);
            controller.EnabledBySetting = false;
            Assert.IsFalse(controller.TryPulse());
            Assert.AreEqual(0, platform.Pulses);
            Object.DestroyImmediate(host);
        }

        [Test]
        public void SupportedPlatformReceivesPulseWhenEnabled()
        {
            var host = new GameObject("HapticsTest");
            var controller = host.AddComponent<HapticsController>();
            var platform = new FakePlatform { IsSupported = true };
            controller.Initialize(platform);
            Assert.IsTrue(controller.TryPulse());
            Assert.AreEqual(1, platform.Pulses);
            Object.DestroyImmediate(host);
        }

        [Test]
        public void UnsupportedPlatformDoesNotReceivePulse()
        {
            var host = new GameObject("HapticsTest");
            var controller = host.AddComponent<HapticsController>();
            var platform = new FakePlatform { IsSupported = false };
            controller.Initialize(platform);
            Assert.IsFalse(controller.TryPulse());
            Assert.AreEqual(0, platform.Pulses);
            Object.DestroyImmediate(host);
        }

        [Test]
        public void NamedBattleCuesRemainOptionalAndDistinct()
        {
            var host = new GameObject("HapticsTest");
            var controller = host.AddComponent<HapticsController>();
            var platform = new FakePlatform { IsSupported = true };
            controller.Initialize(platform);
            foreach (var cue in new[] { HapticCue.Gate, HapticCue.Broadside, HapticCue.ArmorBreak, HapticCue.Victory })
            {
                Assert.IsTrue(controller.TryPulse(cue));
                Assert.AreEqual(cue, controller.LastCue);
            }
            Assert.AreEqual(4, platform.Pulses);
            Object.DestroyImmediate(host);
        }

        [Test]
        public void BoundBattleStreamPulsesGateArmorBreakAndVictory()
        {
            var host = new GameObject("HapticsTest");
            var controller = host.AddComponent<HapticsController>();
            var platform = new FakePlatform { IsSupported = true };
            controller.Initialize(platform);
            var session = new BattleSession(new StableId("level-01"), new StableId("opening"), default);
            controller.Bind(session.Events);
            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);
            var payload = new BattleEventPayload(session.SessionId, new StableId("gate"), default,
                Allegiance.Friendly, 8, 32, 4, GateOutcome.Multiply, default);
            Assert.That(session.TryPublishGameplayEvent(BattleEventType.GateResolved, payload), Is.True);
            Assert.That(session.TryPublishGameplayEvent(BattleEventType.BossPhaseChanged, payload), Is.True);
            Assert.That(session.End(true, "guardian-defeated"), Is.True);
            Assert.That(platform.Pulses, Is.EqualTo(3));
            Assert.That(controller.LastCue, Is.EqualTo(HapticCue.Victory));
            Object.DestroyImmediate(host);
        }
    }
}
