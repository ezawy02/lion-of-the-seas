using System;
using SeaLion.Core.Definitions;
using UnityEngine;

namespace SeaLion.Gameplay.Levels
{
    public sealed partial class Level01TrialRuntime
    {
        [SerializeField] private GateDefinition centerGate;
        private float blockadeHealth;
        private float hazardElapsed;
        private float telegraphedLane;
        private bool hazardWarning;
        private int assaultStage;
        private int powder;
        public int LevelNumber => levelDefinition != null ? levelDefinition.Order : 1;
        public int AssaultStage => assaultStage + 1;
        public int Powder => powder;
        public bool BlockadeActive => LevelNumber == 2 && routeProgress >= BlockadeProgress && blockadeHealth > 0f;
        public float BlockadeHealth01 => blockadeHealth / BlockadeMaximum;
        public bool HazardWarning => hazardWarning;
        public float HazardLane => telegraphedLane;
        public GateDefinition CenterGate => centerGate;

        private float BlockadeMaximum => levelDefinition != null ? levelDefinition.BlockadeHealth : 80f;
        private float BlockadeProgress => levelDefinition != null ? levelDefinition.BlockadeProgress : .8f;
        private float PowderDamage => levelDefinition != null ? levelDefinition.PowderDamage : 24f;
        public void ConfigureCenterGate(GateDefinition definition) { centerGate = definition; }
        private StableId PhaseId(string phase) => new StableId("level0" + LevelNumber + "-" + phase);

        private void ResetCampaign()
        {
            blockadeHealth = LevelNumber == 2 ? BlockadeMaximum : 0f;
            hazardElapsed = 0f; hazardWarning = false; assaultStage = 0; powder = 0;
        }

        private GateDefinition SelectedGate()
        {
            if (LevelNumber == 2 && Mathf.Abs(horizontalChoice) < .33f && centerGate != null) return centerGate;
            return horizontalChoice < 0f ? easyGate : riskyGate;
        }

        private bool StepCampaignVoyage(float step)
        {
            if (LevelNumber == 3 && traversalPlayerSteered)
                horizontalChoice = Mathf.Clamp(horizontalChoice +
                    Mathf.Sin((float)(clock.Tick * clock.FixedDeltaSeconds) * 1.2f) * (levelDefinition != null ? levelDefinition.StormStrength : .18f) * step, -1f, 1f);
            if (LevelNumber == 2 && gateCommitted && routeProgress < BlockadeProgress)
            {
                hazardElapsed += step;
                if (!hazardWarning && hazardElapsed >= levelDefinition.HazardWarningSeconds)
                {
                    hazardWarning = true;
                    telegraphedLane = Mathf.Sin((float)(clock.Tick * clock.FixedDeltaSeconds)) * .8f;
                }
                if (hazardElapsed >= levelDefinition.HazardFireSeconds)
                {
                    if (Mathf.Abs(horizontalChoice - telegraphedLane) < .28f)
                    {
                        foreach (var craft in fleet)
                            if (craft.Contribution > 0) { DestroyCraft(craft.Sequence); break; }
                    }
                    hazardElapsed = 0f; hazardWarning = false;
                }
            }
            return !BlockadeActive && IsRunning;
        }

        private bool FireAtBlockade(out Level01PrimaryAttackResult result)
        {
            result = Level01PrimaryAttackResult.Rejected;
            if (!BlockadeActive || primaryAttackCooldown > 0f || paused || ForceCount <= 0) return false;
            primaryAttackCooldown = PrimaryAttackCooldownSeconds;
            var damage = OrdinaryTargetDamage;
            blockadeHealth = Mathf.Max(0f, blockadeHealth - damage);
            loadout.ReportDamage(damage);
            result = new Level01PrimaryAttackResult(true, true, damage, -2);
            PrimaryAttackFired?.Invoke(new Level01PrimaryAttackEvent(result));
            return true;
        }

        private bool ApplyBossDamage(float damage)
        {
            if (guardian == null) return false;
            // Armor must break on a separate hit; overflow cannot skip the health stage.
            if (LevelNumber == 2 && guardian.Health > guardian.MaxHealth * .5f)
                damage = Mathf.Min(damage, guardian.Health - guardian.MaxHealth * .5f);
            return guardian.ApplyDamage(damage, clock.Tick);
        }

        private bool AdvanceAssaultStage()
        {
            if (LevelNumber != 3 || assaultStage != 0) return false;
            assaultStage = 1;
            session.TrySetPhase(PhaseId("assault-commander"));
            if (guardian != null) guardian.Event -= HandleGuardianEvent;
            if (combat != null) combat.Death -= HandleCombatDeath;
            phaseElapsed = 0f;
            BeginAssault();
            PhaseChanged?.Invoke(Phase);
            return true;
        }

        private void SaveCampaignProgress()
        {
            var loaded = repository.Load();
            if (!loaded.Succeeded || loaded.Data == null) return;
            loaded.Data.highestUnlockedLevel = Math.Max(loaded.Data.highestUnlockedLevel, Math.Min(3, LevelNumber + 1));
            if (!repository.Save(loaded.Data, out var failure)) FailureReason = failure;
        }
    }
}
