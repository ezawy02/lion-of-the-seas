using System;
using SeaLion.Combat;
using UnityEngine;

namespace SeaLion.Gameplay.Levels
{
    public sealed partial class Level01TrialRuntime
    {
        private const float GateChoiceThreshold = 0.18f;
        private const float GateCommitTime = 4f;
        private const float RescueTime = 7f;
        private const float LandingTime = 10f;
        private const float PrimaryAttackCooldownSeconds = 0.55f;
        private const float OrdinaryTargetDamage = 5f;
        private const float GuardianTargetDamage = 18f;
        private const float AssaultTimeLimit = 55f;

        private bool traversalPlayerSteered;
        private float traversalActiveElapsed;
        private float primaryAttackCooldown;

        public event Action<Level01PrimaryAttackEvent> PrimaryAttackFired;

        public bool TraversalPlayerSteered => traversalPlayerSteered;
        public float TraversalActiveElapsed => traversalActiveElapsed;
        public bool CanPrimaryAttack => IsRunning && Phase == Level01TrialPhase.Assault &&
            primaryAttackCooldown <= 0f;
        public bool CanAssistLanding => IsRunning && Phase == Level01TrialPhase.Landing &&
            landingIndex < LandingCraftCount && primaryAttackCooldown <= 0f;
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
            primaryAttackCooldown = PrimaryAttackCooldownSeconds;
            var target = hostileRemaining > 0 && combat != null
                ? combat.ApplyPlayerDamage(combatants, OrdinaryTargetDamage, CombatTeam.Hostile)
                : -1;
            var hitGuardian = target < 0 && hostileRemaining <= 0 && guardian != null;
            var damage = hitGuardian ? GuardianTargetDamage : OrdinaryTargetDamage;
            if (hitGuardian)
                guardian.ApplyDamage(damage, Mathf.RoundToInt(totalElapsed * 10f));
            var result = new Level01PrimaryAttackResult(true, target >= 0 || hitGuardian,
                damage, target);
            PrimaryAttackFired?.Invoke(new Level01PrimaryAttackEvent(result));
            StateChanged?.Invoke();
            return result;
        }

        public bool TryAssistLanding()
        {
            if (!CanAssistLanding) return false;
            primaryAttackCooldown = PrimaryAttackCooldownSeconds;
            var contribution = landingContribution + (landingIndex < landingRemainder ? 1 : 0);
            if (landingIndex < landingTokens.Count)
                landing.TryAccept(landingTokens[landingIndex], landingIndex, contribution, contribution > 0);
            landingIndex++;
            if (landingIndex >= LandingCraftCount)
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

        private void StepTraversal(float step)
        {
            if (!traversalPlayerSteered) return;
            traversalActiveElapsed += step;
            deployer.Tick(step);
            if (!gateCommitted && traversalActiveElapsed >= GateCommitTime &&
                Mathf.Abs(horizontalChoice) >= GateChoiceThreshold)
                CommitGate(horizontalChoice < 0f ? easyGate : riskyGate);
            if (gateCommitted && !rescueApplied && traversalActiveElapsed >= RescueTime)
                ApplyRescue();
            if (gateCommitted && rescueApplied && traversalActiveElapsed >= LandingTime)
                SetPhase(Level01TrialPhase.Landing);
        }

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

        private static bool AssaultTimedOut(float elapsed) => elapsed >= AssaultTimeLimit;
    }
}
