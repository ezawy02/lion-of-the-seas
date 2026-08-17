using NUnit.Framework;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Gameplay.Gates;

namespace SeaLion.Tests.EditMode.Gates
{
    public sealed class GateResolverTests
    {
        [Test]
        public void AddAndMultiplyUseExplicitWholeNumberRules()
        {
            var resolver = new GateResolver(100);
            Assert.That(Resolve(resolver, "add", GateOutcome.Add, 7f, 12, "one").After, Is.EqualTo(19));
            Assert.That(Resolve(resolver, "multiply", GateOutcome.Multiply, 1.5f, 3, "two").After, Is.EqualTo(5));
            Assert.That(Resolve(resolver, "multiply-odd", GateOutcome.Multiply, 1.5f, 5, "three").After, Is.EqualTo(8));
        }

        [Test]
        public void ConvertReportsWholeGroupsAndRemainder()
        {
            var result = Resolve(new GateResolver(100), "convert", GateOutcome.Convert, 3f, 7, "one", "crew");
            Assert.That(result.After, Is.EqualTo(2));
            Assert.That(result.Converted, Is.EqualTo(2));
            Assert.That(result.Remainder, Is.EqualTo(1));
            Assert.That(result.ConversionId, Is.EqualTo(new StableId("crew")));
        }

        [Test]
        public void DamageCannotIncreaseOrDropBelowZero()
        {
            var resolver = new GateResolver(100);
            Assert.That(Resolve(resolver, "damage", GateOutcome.Damage, 4f, 12, "one").After, Is.EqualTo(8));
            Assert.That(Resolve(resolver, "lethal", GateOutcome.Damage, 99f, 3, "two").After, Is.Zero);
        }

        [Test]
        public void PresentationCapNeverChangesLogicalResult()
        {
            var result = Resolve(new GateResolver(20), "multiply", GateOutcome.Multiply, 4f, 6, "one");
            Assert.That(result.After, Is.EqualTo(24));
            Assert.That(result.Displayed, Is.EqualTo(20));
            Assert.That(result.Compressed, Is.True);
        }

        [Test]
        public void MemberAndGatePairResolvesExactlyOnceAndPublishesOneEvent()
        {
            var session = Session();
            var resolver = new GateResolver(100, session);
            var first = Resolve(resolver, "gate-a", GateOutcome.Add, 5f, 10, "craft-1");
            var second = Resolve(resolver, "gate-a", GateOutcome.Add, 5f, first.After, "craft-1");
            Assert.That(first.Applied, Is.True);
            Assert.That(second.Applied, Is.False);
            Assert.That(second.After, Is.EqualTo(15));
            Assert.That(session.Events.Events, Has.Count.EqualTo(3));
            var gateEvent = session.Events.Events[2];
            Assert.That(gateEvent.Type, Is.EqualTo(BattleEventType.GateResolved));
            Assert.That(gateEvent.Payload.Before, Is.EqualTo(10));
            Assert.That(gateEvent.Payload.After, Is.EqualTo(15));
        }

        [Test]
        public void DifferentMembersResolveIndependently()
        {
            var resolver = new GateResolver(100);
            Assert.That(Resolve(resolver, "gate-a", GateOutcome.Add, 2f, 10, "one").Applied, Is.True);
            Assert.That(Resolve(resolver, "gate-a", GateOutcome.Add, 2f, 12, "two").Applied, Is.True);
        }

        private static GateResolution Resolve(GateResolver resolver, string gate, GateOutcome outcome,
            float value, int before, string member, string conversion = null)
        {
            return resolver.Resolve(new StableId(gate), outcome, value,
                string.IsNullOrEmpty(conversion) ? default : new StableId(conversion), before, new StableId(member));
        }

        private static BattleSession Session()
        {
            var session = new BattleSession(new StableId("level-01"), new StableId("opening"), default);
            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);
            return session;
        }
    }
}
