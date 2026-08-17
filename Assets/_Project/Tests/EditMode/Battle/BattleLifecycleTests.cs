using System;
using NUnit.Framework;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;

namespace SeaLion.Tests.EditMode.Battle
{
    public sealed class BattleLifecycleTests
    {
        [Test]
        public void LegalLifecyclePublishesOrderedEventsAndEndsOnce()
        {
            var session = CreateSession();

            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);
            Assert.That(session.TryPublishGameplayEvent(BattleEventType.ForceChanged, default), Is.True);
            Assert.That(session.TryTransition(BattleState.Landing), Is.True);
            Assert.That(session.TryTransition(BattleState.Assault), Is.True);
            Assert.That(session.End(true, "guardian-defeated"), Is.True);
            Assert.That(session.End(false), Is.False);

            Assert.That(session.Events.Events.Count, Is.EqualTo(4));
            Assert.That(session.Events.Events[0].Sequence, Is.EqualTo(1));
            Assert.That(session.Events.Events[3].Type, Is.EqualTo(BattleEventType.BattleEnded));
            Assert.That(session.State, Is.EqualTo(BattleState.Victory));
        }

        [Test]
        public void TerminalStatesCannotBeBypassedByTransition()
        {
            var session = CreateActiveSession();

            Assert.That(session.TryTransition(BattleState.Victory), Is.False);
            Assert.That(session.TryTransition(BattleState.Failure), Is.False);
            Assert.That(session.Result, Is.Null);
        }

        [Test]
        public void NoGameplayMutationIsAcceptedAfterEnd()
        {
            var session = CreateActiveSession();
            Assert.That(session.End(false, "force-depleted"), Is.True);

            Assert.That(session.TrySetPhase(Id("phase-2")), Is.False);
            Assert.That(session.TryAdvance(0.02f), Is.False);
            Assert.That(session.TryPublishGameplayEvent(BattleEventType.GateResolved, default), Is.False);
            Assert.That(session.Events.Events.Count, Is.EqualTo(3));
        }

        [TestCase(float.NaN)]
        [TestCase(float.PositiveInfinity)]
        [TestCase(-0.01f)]
        public void InvalidTimeDeltaCannotCorruptElapsedTime(float delta)
        {
            var session = CreateActiveSession();

            Assert.That(session.TryAdvance(delta), Is.False);
            Assert.That(session.ElapsedTime, Is.Zero);
        }

        [Test]
        public void SubscriberFailureIsReportedAndDoesNotStopOtherSubscribers()
        {
            Exception reported = null;
            var received = 0;
            var stream = new BattleEventStream(exception => reported = exception);
            stream.Subscribe(_ => throw new InvalidOperationException("presentation failed"));
            stream.Subscribe(_ => received++);
            var session = CreateSession(stream);

            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(reported, Is.TypeOf<InvalidOperationException>());
            Assert.That(received, Is.EqualTo(1));
        }

        private static BattleSession CreateActiveSession()
        {
            var session = CreateSession();
            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);
            return session;
        }

        private static BattleSession CreateSession(BattleEventStream stream = null)
        {
            var loadout = new LoadoutSnapshot(Id("ship-1"), Id("crew-1"), Id("ability-1"));
            return new BattleSession(Id("level-1"), Id("opening"), loadout, Guid.Empty, stream);
        }

        private static StableId Id(string value)
        {
            Assert.That(StableId.TryCreate(value, out var id), Is.True);
            return id;
        }
    }
}
