using System.Collections.Generic;
using NUnit.Framework;
using SeaLion.Combat.Bosses;
using SeaLion.Core.Definitions;

namespace SeaLion.Tests.EditMode.Combat
{
    public sealed class HarborGuardianControllerTests
    {
        private static readonly StableId Boss = new StableId("guardian");
        private static readonly StableId Broadside = new StableId("broadside");

        private static HarborGuardianController Create(FailurePressure pressure = FailurePressure.ForceDepletion, float limit = 0f)
        {
            var phases = new[]
            {
                new BossPhaseDefinition(new StableId("opening"), new StableId("entry"), 1f),
                new BossPhaseDefinition(new StableId("broken"), new StableId("half-health"), .5f)
            };
            return new HarborGuardianController(Boss, 100f, phases, new[] { Broadside }, pressure, limit <= 0f ? 1f : limit);
        }

        [Test]
        public void EntryTelegraphsAttackAndRecordsReadableHitReaction()
        {
            var guardian = Create(); var events = new List<HarborGuardianEvent>(); guardian.Event += events.Add;
            Assert.IsTrue(guardian.Enter(3));
            Assert.IsTrue(guardian.TryFireAttack(Broadside, 4));
            Assert.IsTrue(guardian.ApplyDamage(60f, 5));
            Assert.AreEqual(HarborGuardianState.Active, guardian.State);
            Assert.AreEqual(40f, guardian.Health);
            Assert.IsTrue(events.Exists(e => e.Type == HarborGuardianEventType.Entered));
            Assert.IsTrue(events.Exists(e => e.Type == HarborGuardianEventType.AttackTelegraphed));
            Assert.IsTrue(events.Exists(e => e.Type == HarborGuardianEventType.HitReaction && e.Value == 60f));
            Assert.AreEqual(1, guardian.PhaseIndex);
        }

        [Test]
        public void DefeatEmitsVictoryOnceAndIgnoresLaterDamage()
        {
            var guardian = Create(); var victories = 0; guardian.Event += e => { if (e.Type == HarborGuardianEventType.Victory) victories++; };
            guardian.Enter(); Assert.IsTrue(guardian.ApplyDamage(100f, 7));
            Assert.AreEqual(HarborGuardianState.Defeated, guardian.State);
            Assert.IsFalse(guardian.ApplyDamage(1f, 8)); Assert.AreEqual(1, victories);
        }

        [Test]
        public void FailurePressureEndsEncounterAndIsTerminal()
        {
            var guardian = Create(FailurePressure.Timer, 2f); var failures = 0; guardian.Event += e => { if (e.Type == HarborGuardianEventType.Failure) failures++; };
            guardian.Enter(); Assert.IsFalse(guardian.AdvanceTime(1f)); Assert.IsTrue(guardian.AdvanceTime(1f));
            Assert.AreEqual(HarborGuardianState.Failed, guardian.State); Assert.AreEqual(1, failures);
            Assert.IsFalse(guardian.TryFireAttack(Broadside));
        }

        [Test]
        public void ForceDepletionTriggersFailureOnlyAtZero()
        {
            var guardian = Create(); guardian.Enter();
            Assert.IsFalse(guardian.NotifyForceRemaining(1));
            Assert.IsTrue(guardian.NotifyForceRemaining(0));
            Assert.AreEqual(HarborGuardianState.Failed, guardian.State);
        }
    }
}
