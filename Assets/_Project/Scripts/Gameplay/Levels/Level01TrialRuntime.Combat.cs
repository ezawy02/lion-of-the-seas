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
            assaultStance = AssaultStance.Hold;
            assaultAdvance = 0f;
            ClearGuardianTelegraph();
            if (combat != null) combat.ClearFocus();
        }

        private void StepAssault(float step)
        {
            if (assaultStance == AssaultStance.Engage)
            {
                assaultAdvance = math.min(1f, assaultAdvance + step / 2.4f);
                combatAccumulator += step;
                while (combatAccumulator >= 0.25f && hostileRemaining > 0)
                {
                    combatAccumulator -= 0.25f;
                    combat.StepHostileAttacks(combatants, 0.25f);
                }
            }

            guardianAttackAccumulator += step;
            var interval = GuardianInterval;
            if (GuardianShouldTelegraph(interval)) TryStartGuardianTelegraph();
            if (guardianAttackAccumulator >= interval && guardian.State == HarborGuardianState.Active)
            {
                guardianAttackAccumulator -= interval;
                guardian.TryFireAttack(FirstAttack(), clock.Tick);
                var phaseScale = 1f + GuardianPhase * 0.35f;
                var baseLoss = Mathf.Max(3, Mathf.CeilToInt(landForce.LogicalCount * 0.12f * phaseScale));
                var loss = AbsorbGuardianSlam(ComputeGuardianLoss(baseLoss));
                ChangeForce(landForce, Mathf.Max(0, landForce.LogicalCount - loss));
                guardian.NotifyForceRemaining(landForce.LogicalCount, clock.Tick);
                ClearGuardianTelegraph();
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
            {
                var elite = index == HostileCombatants - 1;
                combatants[FriendlyCombatants + index] = new CombatUnit(CombatTeam.Hostile,
                    elite ? new float3(0.4f, 0f, 3.2f) : new float3(index % 4, 0f, 1f + index / 4),
                    elite ? 16f : 5f, elite ? 2.1f : 1.1f, 12f, elite ? 1.35f : 1.15f);
            }
        }

        private void HandleCombatDeath(CombatDeath death)
        {
            if (!IsRunning) return;
            if (death.Unit < FriendlyCombatants)
            {
                ChangeForce(landForce, Mathf.Max(0, landForce.LogicalCount - lossPerFriendly));
                OnFriendlyHit();
                if (landForce.LogicalCount == 0) Finish(false, "force-depleted");
            }
            else
            {
                hostileRemaining = Mathf.Max(0, hostileRemaining - 1);
                if (hostileRemaining == 0) OnEliteBroken();
            }
        }

    }
}
