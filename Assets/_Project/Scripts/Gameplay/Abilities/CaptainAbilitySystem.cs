using System;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;

namespace SeaLion.Gameplay.Abilities
{
    public enum AbilityActivationResult { Activated, NotReady, OnCooldown, Rejected }

    /// <summary>Deterministic charge/cooldown authority for a captain ability.</summary>
    public sealed class CaptainAbilitySystem
    {
        private readonly CaptainAbilityDefinition definition;
        private readonly BattleSession session;
        private float charge;
        private float cooldownRemaining;

        public CaptainAbilitySystem(CaptainAbilityDefinition definition, BattleSession session)
        {
            this.definition = definition ?? throw new ArgumentNullException("definition");
            this.session = session ?? throw new ArgumentNullException("session");
        }

        public float Charge => charge;
        public float CooldownRemaining => cooldownRemaining;
        public bool IsReady => charge >= 1f && cooldownRemaining <= 0f;
        public CaptainAbilityDefinition Definition => definition;

        public void Tick(float seconds)
        {
            if (!Finite(seconds) || seconds < 0f) return;
            var chargeSeconds = seconds;
            if (cooldownRemaining > 0f)
                chargeSeconds = Math.Max(0f, seconds - cooldownRemaining);
            var remaining = cooldownRemaining - seconds;
            cooldownRemaining = remaining <= 0.0001f ? 0f : remaining;
            if (definition.ChargeRule == AbilityChargeRule.Time || definition.ChargeRule == AbilityChargeRule.Hybrid)
                AddCharge(chargeSeconds / ChargeDuration());
        }

        public void ReportDamage(float amount)
        {
            if ((definition.ChargeRule == AbilityChargeRule.Damage || definition.ChargeRule == AbilityChargeRule.Hybrid) && Finite(amount) && amount > 0f)
                AddCharge(amount / ChargeDuration());
        }

        public void ReportGateResolved()
        {
            if ((definition.ChargeRule == AbilityChargeRule.Gates || definition.ChargeRule == AbilityChargeRule.Hybrid)) AddCharge(1f / ChargeDuration());
        }

        public AbilityActivationResult TryActivate()
        {
            if (definition.Activation != AbilityActivation.PlayerTap || !IsReady) return cooldownRemaining > 0f ? AbilityActivationResult.OnCooldown : AbilityActivationResult.NotReady;
            var payload = new BattleEventPayload(session.SessionId, definition.Id, StableId.Empty, default, 0, 0, (int)definition.GameplayEffect.Value, definition.GameplayEffect.Outcome, default);
            if (!session.TryPublishGameplayEvent(BattleEventType.AbilityActivated, payload)) return AbilityActivationResult.Rejected;
            charge = 0f;
            cooldownRemaining = Math.Max(0f, definition.Cooldown);
            return AbilityActivationResult.Activated;
        }

        private void AddCharge(float amount) { if (Finite(amount) && amount > 0f) charge = Math.Min(1f, charge + amount); }
        private float ChargeDuration() => Math.Max(1f, definition.Duration > 0f ? definition.Duration : 10f);
        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
