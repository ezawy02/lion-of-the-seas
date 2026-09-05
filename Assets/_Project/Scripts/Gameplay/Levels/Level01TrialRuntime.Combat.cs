using SeaLion.Combat;
using SeaLion.Combat.Bosses;
using Unity.Mathematics;
using UnityEngine;

namespace SeaLion.Gameplay.Levels
{
    public sealed partial class Level01TrialRuntime
    {
        private float GuardianInterval => levelDefinition != null ? levelDefinition.GuardianPressureIntervalSeconds : 6f;
        private void BeginAssault()
        {
            guardian = new HarborGuardianController(guardianDefinition.Id, guardianHealth,
                guardianDefinition.Phases, guardianDefinition.Attacks,
                guardianDefinition.FailurePressure, 1f);
            guardian.Event += HandleGuardianEvent;
            guardian.Enter();
            combat = new OrdinaryCombatSystem();
            combat.Death += HandleCombatDeath;
            FriendlyCombatants = Mathf.Clamp(landForce.LogicalCount, 1, displayCap);
            BuildCombatants();
            hostileRemaining = HostileCombatants;
            lossPerFriendly = Mathf.Max(1, Mathf.CeilToInt(landForce.LogicalCount / (float)FriendlyCombatants));
            combatAccumulator = guardianAttackAccumulator = 0f;
        }

        private void StepAssault(float step)
        {
            combatAccumulator += step;
            while (combatAccumulator >= 0.25f && hostileRemaining > 0)
            {
                combatAccumulator -= 0.25f;
                combat.StepHostileAttacks(combatants, 0.25f);
            }

            guardianAttackAccumulator += step;
            if (guardianAttackAccumulator >= GuardianInterval && guardian.State == HarborGuardianState.Active)
            {
                guardianAttackAccumulator -= GuardianInterval;
                var attack = FirstAttack();
                guardian.TryFireAttack(attack, clock.Tick);
                var baseLoss = Mathf.Max(3, Mathf.CeilToInt(landForce.LogicalCount * 0.12f));
                var loss = ComputeGuardianLoss(baseLoss);
                ChangeForce(landForce, Mathf.Max(0, landForce.LogicalCount - loss));
                guardian.NotifyForceRemaining(landForce.LogicalCount, clock.Tick);
            }
            if (AssaultTimedOut(phaseElapsed) && Phase == Level01TrialPhase.Assault)
                Finish(false, "guardian-timeout");
        }

        private void BuildCombatants()
        {
            combatants = new CombatUnit[FriendlyCombatants + HostileCombatants];
            for (var index = 0; index < FriendlyCombatants; index++)
            {
                var unit = new CombatUnit(CombatTeam.Friendly, new float3((index % 12) * .3f, 0f, (index / 12) * .3f),
                    6f, 2f, 12f, 0.75f);
                combatants[index] = loadout.ApplyCrewTo(unit);
            }
            for (var index = 0; index < HostileCombatants; index++)
                combatants[FriendlyCombatants + index] = new CombatUnit(CombatTeam.Hostile,
                    new float3(index % 4, 0f, 1f + index / 4), 5f, 1.1f, 12f, 1.15f);
        }

        private void HandleCombatDeath(CombatDeath death)
        {
            if (!IsRunning) return;
            if (death.Unit < FriendlyCombatants)
            {
                ChangeForce(landForce, Mathf.Max(0, landForce.LogicalCount - lossPerFriendly));
                if (landForce.LogicalCount == 0) Finish(false, "force-depleted");
            }
            else
                hostileRemaining = Mathf.Max(0, hostileRemaining - 1);
        }

    }
}
