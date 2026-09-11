using System;
using System.Collections.Generic;
using Unity.Mathematics;

namespace SeaLion.Combat
{
    public enum CombatTeam : byte { Friendly, Hostile }

    public struct CombatUnit
    {
        public CombatTeam Team;
        public float3 Position;
        public float Health;
        public float Damage;
        public float Range;
        public float Cadence;
        public float Cooldown;
        public bool Dead;

        public CombatUnit(CombatTeam team, float3 position, float health, float damage, float range, float cadence)
        { Team = team; Position = position; Health = health; Damage = damage; Range = range; Cadence = cadence; Cooldown = 0f; Dead = false; }
    }

    public readonly struct CombatHit
    {
        public readonly int Source; public readonly int Target; public readonly float Amount; public readonly long Step;
        public CombatHit(int source, int target, float amount, long step) { Source = source; Target = target; Amount = amount; Step = step; }
    }

    public readonly struct CombatDeath
    {
        public readonly int Unit; public readonly long Step;
        public CombatDeath(int unit, long step) { Unit = unit; Step = step; }
    }

    /// <summary>Fixed-step, order-independent ordinary combat domain. Presentation subscribes to Hit/Death.</summary>
    public sealed class OrdinaryCombatSystem
    {
        private readonly List<(int source, int target, float amount)> pending = new List<(int, int, float)>();
        public long StepIndex { get; private set; }
        public event Action<CombatHit> Hit;
        public event Action<CombatDeath> Death;

        /// <summary>Sticky focus for player-ordered volleys. Cleared when the target dies or teams change.</summary>
        public int FocusedTarget { get; private set; } = -1;

        public void ClearFocus() { FocusedTarget = -1; }

        public int ApplyPlayerDamage(CombatUnit[] units, float amount, CombatTeam targetTeam)
        {
            if (units == null || !Finite(amount) || amount <= 0f) return -1;
            var best = SelectTarget(units, targetTeam, TeamCentroid(units, Opposite(targetTeam)));
            if (best < 0) return -1;
            Apply(units, (0, best, amount));
            if (units[best].Dead) FocusedTarget = -1;
            return best;
        }

        /// <summary>Distributes one player volley over live targets. Prefers focused fire, then nearest to the attacking force.</summary>
        public int ApplyPlayerVolley(CombatUnit[] units, float amount, CombatTeam targetTeam, out float applied)
        {
            applied = 0f;
            if (units == null || !Finite(amount) || amount <= 0f) return -1;
            var origin = TeamCentroid(units, Opposite(targetTeam));
            var first = -1;
            while (amount > .0001f)
            {
                var best = SelectTarget(units, targetTeam, origin);
                if (best < 0) break;
                if (first < 0) first = best;
                var hit = Math.Min(amount, units[best].Health);
                Apply(units, (0, best, hit));
                amount -= hit; applied += hit;
                if (units[best].Dead) FocusedTarget = -1;
            }
            return first;
        }

        public void StepHostileAttacks(CombatUnit[] units, float deltaTime)
        {
            StepFiltered(units, deltaTime, CombatTeam.Hostile);
        }

        public void Step(CombatUnit[] units, float deltaTime)
        {
            StepFiltered(units, deltaTime, null);
        }

        private void StepFiltered(CombatUnit[] units, float deltaTime, CombatTeam? attackingTeam)
        {
            if (units == null || !Finite(deltaTime) || deltaTime < 0f) return;
            StepIndex++;
            pending.Clear();
            for (var i = 0; i < units.Length; i++)
            {
                ref var attacker = ref units[i];
                if (attackingTeam.HasValue && attacker.Team != attackingTeam.Value) continue;
                if (attacker.Dead || !Finite(attacker.Health) || attacker.Health <= 0f) continue;
                attacker.Cooldown = Math.Max(0f, Finite(attacker.Cooldown) ? attacker.Cooldown - deltaTime : 0f);
                if (!Finite(attacker.Damage) || attacker.Damage <= 0f || !Finite(attacker.Range) || attacker.Range < 0f ||
                    !Finite(attacker.Cadence) || attacker.Cadence <= 0f || attacker.Cooldown > 0f || !Finite(attacker.Position.x) || !Finite(attacker.Position.y) || !Finite(attacker.Position.z)) continue;
                var target = FindTarget(units, i, attacker);
                if (target < 0) continue;
                pending.Add((i, target, attacker.Damage));
                attacker.Cooldown = attacker.Cadence;
            }
            pending.Sort((a, b) => a.source != b.source ? a.source.CompareTo(b.source) : a.target.CompareTo(b.target));
            for (var i = 0; i < pending.Count; i++) Apply(units, pending[i]);
        }

        private int SelectTarget(CombatUnit[] units, CombatTeam targetTeam, float3 origin)
        {
            if (FocusedTarget >= 0 && FocusedTarget < units.Length)
            {
                var focused = units[FocusedTarget];
                if (!focused.Dead && focused.Team == targetTeam && Finite(focused.Health) && focused.Health > 0f)
                    return FocusedTarget;
                FocusedTarget = -1;
            }

            var best = -1; var bestDistance = float.MaxValue;
            var originFinite = Finite(origin.x) && Finite(origin.y) && Finite(origin.z);
            var reference = originFinite ? origin : float3.zero;
            for (var i = 0; i < units.Length; i++)
            {
                var candidate = units[i];
                if (candidate.Dead || candidate.Team != targetTeam || !Finite(candidate.Health) || candidate.Health <= 0f ||
                    !Finite(candidate.Position.x) || !Finite(candidate.Position.y) || !Finite(candidate.Position.z)) continue;
                var distance = math.lengthsq(candidate.Position - reference);
                if (!Finite(distance)) continue;
                if (distance < bestDistance || (distance == bestDistance && (best < 0 || i < best)))
                { best = i; bestDistance = distance; }
            }
            FocusedTarget = best;
            return best;
        }

        private int FindTarget(CombatUnit[] units, int source, CombatUnit attacker)
        {
            var best = -1; var bestDistance = float.MaxValue; var limit = attacker.Range * attacker.Range;
            if (!Finite(limit)) return -1;
            for (var i = 0; i < units.Length; i++)
            {
                var candidate = units[i];
                if (i == source || candidate.Dead || candidate.Team == attacker.Team || !Finite(candidate.Health) || candidate.Health <= 0f || !Finite(candidate.Position.x) || !Finite(candidate.Position.y) || !Finite(candidate.Position.z)) continue;
                var d = candidate.Position - attacker.Position; var distance = math.lengthsq(d);
                if (!Finite(distance) || distance > limit) continue;
                if (distance < bestDistance || (distance == bestDistance && i < best)) { best = i; bestDistance = distance; }
            }
            return best;
        }

        private static CombatTeam Opposite(CombatTeam team)
        {
            return team == CombatTeam.Friendly ? CombatTeam.Hostile : CombatTeam.Friendly;
        }

        private static float3 TeamCentroid(CombatUnit[] units, CombatTeam team)
        {
            var sum = float3.zero; var count = 0;
            for (var i = 0; i < units.Length; i++)
            {
                var unit = units[i];
                if (unit.Dead || unit.Team != team || !Finite(unit.Health) || unit.Health <= 0f) continue;
                if (!Finite(unit.Position.x) || !Finite(unit.Position.y) || !Finite(unit.Position.z)) continue;
                sum += unit.Position; count++;
            }
            return count == 0 ? float3.zero : sum / count;
        }

        private void Apply(CombatUnit[] units, (int source, int target, float amount) hit)
        {
            if (hit.target < 0 || hit.target >= units.Length || !Finite(hit.amount)) return;
            ref var target = ref units[hit.target];
            if (target.Dead || !Finite(target.Health)) return;
            var before = target.Health; target.Health = Math.Max(0f, before - Math.Max(0f, hit.amount));
            Hit?.Invoke(new CombatHit(hit.source, hit.target, Math.Max(0f, hit.amount), StepIndex));
            if (before > 0f && target.Health <= 0f) { target.Dead = true; Death?.Invoke(new CombatDeath(hit.target, StepIndex)); }
        }

        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
    }
}
