using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;

namespace SeaLion.Combat.Bosses
{
    public enum HarborGuardianState : byte { Dormant, Active, Defeated, Failed }
    public enum HarborGuardianEventType : byte { Entered, AttackTelegraphed, AttackFired, HitReaction, PhaseChanged, Victory, Failure }

    public readonly struct HarborGuardianEvent
    {
        public readonly HarborGuardianEventType Type;
        public readonly StableId BossId;
        public readonly StableId AttackId;
        public readonly int Phase;
        public readonly float Before;
        public readonly float After;
        public readonly float Value;
        public readonly long Step;

        public HarborGuardianEvent(HarborGuardianEventType type, StableId bossId, StableId attackId,
            int phase, float before, float after, float value, long step)
        {
            Type = type; BossId = bossId; AttackId = attackId; Phase = phase;
            Before = before; After = after; Value = value; Step = step;
        }
    }

    /// <summary>Fixed-step Harbor Guardian rules. Presentation listens to Events and never mutates state.</summary>
    public sealed class HarborGuardianController
    {
        private readonly List<BossPhaseDefinition> phases;
        private readonly HashSet<StableId> attacks;
        private readonly FailurePressure failurePressure;
        private readonly float failureLimit;
        private readonly float maxHealth;
        private float elapsed;
        private float pressure;
        private bool entered;

        public StableId BossId { get; }
        public HarborGuardianState State { get; private set; }
        public float MaxHealth => maxHealth;
        public float Health { get; private set; }
        public float Health01 => maxHealth <= 0f ? 0f : Health / maxHealth;
        public int PhaseIndex { get; private set; }
        public float FailurePressureValue => pressure;
        public IReadOnlyCollection<StableId> AttackIds => attacks;
        public event Action<HarborGuardianEvent> Event;

        public HarborGuardianController(StableId bossId, float health, IReadOnlyList<BossPhaseDefinition> phases,
            IReadOnlyList<StableId> attacks, FailurePressure failurePressure, float failureLimit)
        {
            if (bossId.IsEmpty) throw new ArgumentException("bossId is required.", nameof(bossId));
            if (!Finite(health) || health <= 0f) throw new ArgumentOutOfRangeException(nameof(health));
            if (phases == null || phases.Count == 0) throw new ArgumentException("at least one phase is required.", nameof(phases));
            if (attacks == null || attacks.Count == 0) throw new ArgumentException("at least one attack is required.", nameof(attacks));
            if (!Finite(failureLimit) || failureLimit <= 0f) throw new ArgumentOutOfRangeException(nameof(failureLimit));
            BossId = bossId; maxHealth = health; Health = health; this.phases = new List<BossPhaseDefinition>(phases);
            this.attacks = new HashSet<StableId>(attacks); this.failurePressure = failurePressure; this.failureLimit = failureLimit;
            State = HarborGuardianState.Dormant;
        }

        public bool Enter(long step = 0)
        {
            if (entered || State != HarborGuardianState.Dormant) return false;
            entered = true; State = HarborGuardianState.Active;
            Emit(HarborGuardianEventType.Entered, default, 0f, Health, Health, step);
            Emit(HarborGuardianEventType.PhaseChanged, default, 0f, Health, Health, step);
            return true;
        }

        public bool TryTelegraphAttack(StableId attackId, long step = 0)
        {
            if (State != HarborGuardianState.Active || !attacks.Contains(attackId)) return false;
            Emit(HarborGuardianEventType.AttackTelegraphed, attackId, 0f, Health, Health, step);
            return true;
        }

        public bool TryFireAttack(StableId attackId, long step = 0)
        {
            if (!TryTelegraphAttack(attackId, step)) return false;
            Emit(HarborGuardianEventType.AttackFired, attackId, 0f, Health, Health, step);
            return true;
        }

        public bool ApplyDamage(float amount, long step = 0)
        {
            if (State != HarborGuardianState.Active || !Finite(amount) || amount <= 0f) return false;
            var before = Health; Health = Math.Max(0f, Health - amount);
            Emit(HarborGuardianEventType.HitReaction, default, amount, before, Health, step);
            AdvancePhases(before, step);
            if (Health <= 0f) { State = HarborGuardianState.Defeated; Emit(HarborGuardianEventType.Victory, default, 0f, before, 0f, step); }
            return true;
        }

        public bool AdvanceTime(float deltaSeconds, long step = 0)
        {
            if (State != HarborGuardianState.Active || !Finite(deltaSeconds) || deltaSeconds < 0f) return false;
            elapsed += deltaSeconds;
            return failurePressure == FailurePressure.Timer && elapsed >= failureLimit && Fail(step);
        }

        public bool NotifyForceRemaining(int remainingForce, long step = 0)
        {
            if (State != HarborGuardianState.Active || remainingForce < 0) return false;
            pressure = remainingForce;
            return failurePressure == FailurePressure.ForceDepletion && remainingForce == 0 && Fail(step);
        }

        public bool NotifyBreakthrough(float amount = 1f, long step = 0)
        {
            if (State != HarborGuardianState.Active || !Finite(amount) || amount <= 0f) return false;
            pressure += amount;
            return failurePressure == FailurePressure.Breakthrough && pressure >= failureLimit && Fail(step);
        }

        private void AdvancePhases(float before, long step)
        {
            while (PhaseIndex + 1 < phases.Count && Health01 <= phases[PhaseIndex + 1].Threshold)
            {
                PhaseIndex++; Emit(HarborGuardianEventType.PhaseChanged, default, 0f, before, Health, step);
            }
        }

        private bool Fail(long step)
        {
            if (State != HarborGuardianState.Active) return false;
            State = HarborGuardianState.Failed; Emit(HarborGuardianEventType.Failure, default, 0f, Health, Health, step); return true;
        }

        private void Emit(HarborGuardianEventType type, StableId attackId, float value, float before, float after, long step)
        { Event?.Invoke(new HarborGuardianEvent(type, BossId, attackId, PhaseIndex, before, after, value, step)); }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
