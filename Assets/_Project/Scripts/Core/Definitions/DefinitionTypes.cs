using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    public enum PhaseKind { Opening, Traversal, Landing, Assault, Boss, Result }
    public enum GateOutcome { Multiply, Add, Convert, Damage, Reward }
    public enum ForceEligibility { LandingCraft, Crew, Both }
    public enum MovementProfile { Static, Horizontal, Vertical, Timed }
    public enum GateVisualStyle { Friendly, Risky, Hostile, Special }
    public enum Allegiance { Friendly, Hostile }
    public enum UnitRole { Sailor, Musketeer, Bomber, Defender, BossSupport }
    public enum SimulationCostTier { Ordinary, Hero, Boss }
    public enum DeployPattern { Cadence, Burst, Spread }
    public enum AbilityChargeRule { Time, Damage, Gates, Hybrid }
    public enum AbilityActivation { PlayerTap, Automatic }
    public enum BossHealthModel { Health, ArmorAndHealth, StagedObjectives }
    public enum BossTargetRule { Force, Flagship, Objective }
    public enum FailurePressure { Timer, Breakthrough, ForceDepletion }
    public enum RewardGrantType { Ownership, Upgrade, CurrencyPlaceholder }
    public enum QualityProfileKind { Primary, Reduced }

    [Serializable]
    public struct PhaseLink
    {
        [SerializeField] private StableId id;
        [SerializeField] private StableId nextPhaseId;
        [SerializeField] private bool terminal;

        public StableId Id => id;
        public StableId NextPhaseId => nextPhaseId;
        public bool IsTerminal => terminal;

        public PhaseLink(StableId id, StableId nextPhaseId, bool terminal)
        {
            this.id = id;
            this.nextPhaseId = nextPhaseId;
            this.terminal = terminal;
        }
    }

    [Serializable]
    public struct LoadoutSnapshot
    {
        [SerializeField] private StableId flagshipId;
        [SerializeField] private StableId crewRoleId;
        [SerializeField] private StableId captainAbilityId;

        public StableId FlagshipId => flagshipId;
        public StableId CrewRoleId => crewRoleId;
        public StableId CaptainAbilityId => captainAbilityId;

        public LoadoutSnapshot(StableId flagshipId, StableId crewRoleId, StableId captainAbilityId)
        {
            this.flagshipId = flagshipId;
            this.crewRoleId = crewRoleId;
            this.captainAbilityId = captainAbilityId;
        }
    }

    [Serializable]
    public struct LocalizedTextKey
    {
        [SerializeField] private string key;
        public string Key => key ?? string.Empty;
        public bool IsEmpty => string.IsNullOrEmpty(Key);
        public LocalizedTextKey(string value) { key = value ?? string.Empty; }
    }

    [Serializable]
    public struct NormalizedBounds
    {
        [Range(0f, 1f)] [SerializeField] private float left;
        [Range(0f, 1f)] [SerializeField] private float right;
        public float Left => left;
        public float Right => right;
        public bool IsValid => left < right && left >= 0f && right <= 1f;
        public NormalizedBounds(float left, float right) { this.left = left; this.right = right; }
    }

    [Serializable]
    public struct MovementStats
    {
        [Min(0f)] [SerializeField] private float speed;
        [Min(0f)] [SerializeField] private float steering;
        public float Speed => speed;
        public float Steering => steering;
        public bool IsValid => speed > 0f && steering >= 0f;
        public MovementStats(float speed, float steering) { this.speed = speed; this.steering = steering; }
    }

    [Serializable]
    public struct CombatStats
    {
        [Min(0f)] [SerializeField] private float damage;
        [Min(0f)] [SerializeField] private float cadence;
        [Min(0f)] [SerializeField] private float range;
        public float Damage => damage;
        public float Cadence => cadence;
        public float Range => range;
        public bool IsValid => damage >= 0f && cadence >= 0f && range >= 0f;
        public CombatStats(float damage, float cadence, float range) { this.damage = damage; this.cadence = cadence; this.range = range; }
    }

    [Serializable]
    public struct TypedEffect
    {
        [SerializeField] private GateOutcome outcome;
        [SerializeField] private float value;
        [SerializeField] private StableId conversionId;
        public GateOutcome Outcome => outcome;
        public float Value => value;
        public StableId ConversionId => conversionId;
        public bool IsValid => value >= 0f && (outcome != GateOutcome.Convert || !conversionId.IsEmpty);
        public TypedEffect(GateOutcome outcome, float value, StableId conversionId)
        { this.outcome = outcome; this.value = value; this.conversionId = conversionId; }
    }

    public abstract class DefinitionAsset : ScriptableObject
    {
        [SerializeField] private StableId id;
        public StableId Id => id;
        public virtual IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            Required(errors, id, "id");
            return errors;
        }
        protected static void Add(List<string> errors, string error) { if (!string.IsNullOrEmpty(error)) errors.Add(error); }
        protected static void Required(List<string> errors, StableId value, string name) { Add(errors, DefinitionValidation.ValidateId(value, name)); }
        protected static void Required(List<string> errors, string value, string name) { if (string.IsNullOrWhiteSpace(value)) errors.Add(name + " is required."); }
        protected static void Positive(List<string> errors, float value, string name) { if (value <= 0f) errors.Add(name + " must be positive."); }
        protected static void NonNegative(List<string> errors, float value, string name) { if (value < 0f) errors.Add(name + " cannot be negative."); }
        protected virtual void OnValidate()
        {
            foreach (var error in Validate()) Debug.LogWarning(name + ": " + error, this);
        }
    }
}
