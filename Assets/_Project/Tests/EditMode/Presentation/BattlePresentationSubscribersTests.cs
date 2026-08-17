using System;
using NUnit.Framework;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Presentation.Battle;

namespace SeaLion.Tests.EditMode
{
    public sealed class BattlePresentationSubscribersTests
    {
        [Test]
        public void EventsMapToDeterministicKindsAndReleaseImmediately()
        {
            var stream = new BattleEventStream();
            var session = new BattleSession(new StableId("level"), new StableId("phase"), default(LoadoutSnapshot), Guid.NewGuid(), stream);
            var seen = new System.Collections.Generic.List<BattlePresentationKind>();
            using (var subscriber = new BattlePresentationSubscribers(stream, e => seen.Add(e.Kind), 1))
            {
                session.TryTransition(BattleState.Ready); session.TryTransition(BattleState.Active);
                session.TryPublishGameplayEvent(BattleEventType.GateResolved, Payload(4, 16));
                session.TryPublishGameplayEvent(BattleEventType.ForceChanged, Payload(8, 7));
                session.TryPublishGameplayEvent(BattleEventType.ForceChanged, Payload(1, 0));
                session.TryTransition(BattleState.Landing); session.TryTransition(BattleState.Assault);
                session.End(true, "done");
            }
            CollectionAssert.AreEqual(new[] { BattlePresentationKind.Gate, BattlePresentationKind.Loss,
                BattlePresentationKind.Destruction, BattlePresentationKind.Victory }, seen);
        }

        [Test]
        public void GrowthLandingBossAndFailureMapToTheirDedicatedEffects()
        {
            var stream = new BattleEventStream();
            var session = new BattleSession(new StableId("level"), new StableId("phase"), default(LoadoutSnapshot), Guid.NewGuid(), stream);
            var seen = new System.Collections.Generic.List<BattlePresentationKind>();
            using (var subscriber = new BattlePresentationSubscribers(stream, e => seen.Add(e.Kind), 1))
            {
                session.TryTransition(BattleState.Ready); session.TryTransition(BattleState.Active);
                session.TryPublishGameplayEvent(BattleEventType.ForceChanged, Payload(4, 8));
                session.TryPublishGameplayEvent(BattleEventType.LandingStarted, Payload(8, 8));
                session.TryPublishGameplayEvent(BattleEventType.BossPhaseChanged, Payload(100, 50));
                session.End(false, "force depleted");
            }
            CollectionAssert.AreEqual(new[] { BattlePresentationKind.Hit, BattlePresentationKind.Landing,
                BattlePresentationKind.Boss, BattlePresentationKind.Failure }, seen);
        }

        private static BattleEventPayload Payload(int before, int after)
        { return new BattleEventPayload(Guid.Empty, default(StableId), default(StableId), default(Allegiance), before, after, 0, default(GateOutcome), default(BattleResult)); }
    }
}
