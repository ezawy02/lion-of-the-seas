using System;
using SeaLion.Combat;
using UnityEngine;

namespace SeaLion.Gameplay.Levels
{
    public sealed partial class Level01TrialRuntime
    {
        private float PrimaryAttackCooldownSeconds => (levelDefinition != null ? levelDefinition.PrimaryCooldown : .55f) * (loadout != null ? Mathf.Max(.1f, loadout.Crew.CadenceMultiplier) : 1f);
        private float OrdinaryTargetDamage => (levelDefinition != null ? levelDefinition.OrdinaryDamage : 5f) * ArmyDamageScale;
        private float GuardianTargetDamage => (levelDefinition != null ? levelDefinition.GuardianDamage : 18f) * ArmyDamageScale;
        private float AssaultTimeLimit => levelDefinition != null ? levelDefinition.AssaultTimeLimit : 55f;
        public float ArmyDamageScale => Mathf.Max(0f, ForceCount) /
            (levelDefinition != null ? levelDefinition.ReferenceForce : 32f) *
            (loadout != null ? loadout.Crew.DamageMultiplier : 1f);

        private bool traversalPlayerSteered;
        private float traversalActiveElapsed;
        private float primaryAttackCooldown;

        public event Action<Level01PrimaryAttackEvent> PrimaryAttackFired;

        public bool TraversalPlayerSteered => traversalPlayerSteered;
        public float TraversalActiveElapsed => traversalActiveElapsed;
        public bool CanPrimaryAttack => IsRunning && !paused && (Phase == Level01TrialPhase.Assault || BlockadeActive) &&
            primaryAttackCooldown <= 0f;
        public bool CanAssistLanding => IsRunning && !paused && Phase == Level01TrialPhase.Landing &&
            landingIndex < fleet.Count && primaryAttackCooldown <= 0f;
        public bool NeedsSteeringChoice => Phase == Level01TrialPhase.Traversal && !traversalPlayerSteered;
        public bool NeedsGateCommit => Phase == Level01TrialPhase.Traversal &&
            traversalPlayerSteered && !gateCommitted;
        public float DodgeFactor01 => Phase != Level01TrialPhase.Assault ? 0f :
            Mathf.Clamp01(Mathf.Abs(horizontalChoice) * 0.5f);
        public float PrimaryAttackReady01 => Mathf.Clamp01(1f -
            primaryAttackCooldown / PrimaryAttackCooldownSeconds);

        public void SetTraversalControl(float normalizedChoice, bool playerSteered)
        {
            SetHorizontalChoice(normalizedChoice);
            if (Phase == Level01TrialPhase.Traversal && playerSteered)
                traversalPlayerSteered = true;
        }

        public Level01PrimaryAttackResult TryPrimaryAttack()
        {
            if (!CanPrimaryAttack) return Level01PrimaryAttackResult.Rejected;
            if (FireAtBlockade(out var blockadeResult)) return blockadeResult;
            if (ForceCount <= 0) return Level01PrimaryAttackResult.Rejected;
            primaryAttackCooldown = PrimaryAttackCooldownSeconds;
            var hitGuardian = hostileRemaining <= 0 && guardian != null;
            var damage = hitGuardian ? GuardianTargetDamage : OrdinaryTargetDamage;
            if (LevelNumber == 3 && powder > 0) { powder--; damage += PowderDamage; }
            var applied = 0f;
            var target = hitGuardian || combat == null ? -1 :
                combat.ApplyPlayerVolley(combatants, damage, CombatTeam.Hostile, out applied);
            if (hitGuardian)
            {
                var targetGuardian = guardian;
                var before = targetGuardian.Health;
                ApplyBossDamage(damage);
                applied = before - targetGuardian.Health;
            }
            var result = new Level01PrimaryAttackResult(true, target >= 0 || hitGuardian,
                damage, target);
            if (applied > 0f) loadout.ReportDamage(applied);
            PrimaryAttackFired?.Invoke(new Level01PrimaryAttackEvent(result));
            StateChanged?.Invoke();
            return result;
        }

        public bool TryAssistLanding()
        {
            if (!CanAssistLanding) return false;
            primaryAttackCooldown = PrimaryAttackCooldownSeconds;
            TransferNextCraft();
            if (landingIndex >= fleet.Count)
            {
                landing.Complete();
                SetPhase(Level01TrialPhase.Assault);
            }
            StateChanged?.Invoke();
            return true;
        }

        private int ComputeGuardianLoss(int baseLoss)
        {
            if (baseLoss <= 0) return 0;
            var reduced = Mathf.RoundToInt(baseLoss * (1f - DodgeFactor01));
            return Mathf.Max(1, reduced);
        }

        private void StepTraversal(float step) => AdvanceVoyage(step);

        private void TickPlayerInteraction(float step)
        {
            primaryAttackCooldown = Mathf.Max(0f, primaryAttackCooldown - step);
        }

        private void ResetPlayerInteraction()
        {
            traversalPlayerSteered = false;
            traversalActiveElapsed = 0f;
            primaryAttackCooldown = 0f;
        }

        private bool AssaultTimedOut(float elapsed) => elapsed >= AssaultTimeLimit;
    }
}
