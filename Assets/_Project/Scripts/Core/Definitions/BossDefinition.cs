using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [Serializable] public struct BossPhaseDefinition
    {
        [SerializeField] private StableId id, entryRule;
        [SerializeField] [Min(0f)] private float threshold;
        public StableId Id => id; public StableId EntryRule => entryRule; public float Threshold => threshold;
    }

    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Boss", fileName = "BossDefinition")]
    public sealed class BossDefinition : DefinitionAsset
    {
        [SerializeField] private List<BossPhaseDefinition> phases = new List<BossPhaseDefinition>();
        [SerializeField] private BossHealthModel healthModel;
        [SerializeField] private List<StableId> attacks = new List<StableId>();
        [SerializeField] private BossTargetRule targetRule;
        [SerializeField] private LocalizedTextKey victoryRule;
        [SerializeField] private FailurePressure failurePressure;
        public IReadOnlyList<BossPhaseDefinition> Phases => phases; public BossHealthModel HealthModel => healthModel; public IReadOnlyList<StableId> Attacks => attacks;
        public BossTargetRule TargetRule => targetRule; public LocalizedTextKey VictoryRule => victoryRule; public FailurePressure FailurePressure => failurePressure;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); if (phases == null || phases.Count == 0) e.Add("at least one boss phase is required.");
            if (attacks == null || attacks.Count == 0) e.Add("at least one readable attack is required."); if (victoryRule.IsEmpty) e.Add("victoryRule is required.");
            if (DefinitionValidation.ValidateUniqueIds(attacks, "attack").Count > 0) e.Add("attacks contain invalid or duplicate IDs."); return e;
        }
    }
}
