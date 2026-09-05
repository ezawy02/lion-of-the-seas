using NUnit.Framework;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Gameplay.Abilities;

namespace SeaLion.Tests.EditMode.Loadout
{
    public sealed class CaptainAbilitySystemTests
    {
        [Test]
        public void TimeChargeActivatesAndPublishesOutcome()
        {
            var definition = Definition(AbilityChargeRule.Time, 2f, 3f);
            var session = Session(); var system = new CaptainAbilitySystem(definition, session);
            system.Tick(2f); BattleEventType type = default; var value = 0;
            session.Events.Subscribe(e => { type = e.Type; value = e.Payload.Value; });
            Assert.That(system.TryActivate(), Is.EqualTo(AbilityActivationResult.Activated));
            Assert.That(type, Is.EqualTo(BattleEventType.AbilityActivated)); Assert.That(value, Is.EqualTo(4));
            Assert.That(system.CooldownRemaining, Is.EqualTo(3f));
        }

        [Test]
        public void ActivationRejectsWhenSessionIsNotActive()
        {
            var session = new BattleSession(new StableId("level"), new StableId("phase"), default);
            session.TryTransition(BattleState.Ready);
            var system = new CaptainAbilitySystem(Definition(AbilityChargeRule.Time, 1f, 0f), session);
            system.Tick(1f);
            Assert.That(system.TryActivate(), Is.EqualTo(AbilityActivationResult.Rejected));
        }

        [Test]
        public void CooldownBlocksUntilItExpires()
        {
            var session = Session(); var system = new CaptainAbilitySystem(Definition(AbilityChargeRule.Time, 1f, 2f), session);
            session.TryTransition(BattleState.Active); system.Tick(1f);
            Assert.That(system.TryActivate(), Is.EqualTo(AbilityActivationResult.Activated));
            system.Tick(1.9f); Assert.That(system.TryActivate(), Is.EqualTo(AbilityActivationResult.OnCooldown));
            system.Tick(.1f); Assert.That(system.TryActivate(), Is.EqualTo(AbilityActivationResult.NotReady));
            system.Tick(1f); Assert.That(system.TryActivate(), Is.EqualTo(AbilityActivationResult.Activated));
        }

        private static BattleSession Session()
        {
            var session = new BattleSession(new StableId("level"), new StableId("phase"), default(LoadoutSnapshot));
            session.TryTransition(BattleState.Ready); session.TryTransition(BattleState.Active); return session;
        }

        private static CaptainAbilityDefinition Definition(AbilityChargeRule rule, float duration, float cooldown)
        {
            var d = UnityEngine.ScriptableObject.CreateInstance<CaptainAbilityDefinition>();
            var so = new UnityEditor.SerializedObject(d);
            so.FindProperty("id").FindPropertyRelative("value").stringValue = "ability";
            so.FindProperty("chargeRule").enumValueIndex = (int)rule; so.FindProperty("activation").enumValueIndex = (int)AbilityActivation.PlayerTap;
            so.FindProperty("duration").floatValue = duration; so.FindProperty("cooldown").floatValue = cooldown;
            so.FindProperty("gameplayEffect").FindPropertyRelative("outcome").enumValueIndex = (int)GateOutcome.Multiply;
            so.FindProperty("gameplayEffect").FindPropertyRelative("value").floatValue = 4f; so.ApplyModifiedPropertiesWithoutUndo(); return d;
        }
    }
}
