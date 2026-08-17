using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Rescue", fileName = "RescueDefinition")]
    public sealed class RescueDefinition : DefinitionAsset
    {
        [SerializeField] private LocalizedTextKey objective;
        [SerializeField] private StableId phaseId;
        [SerializeField] private StableId rescuedUnitRoleId;
        [SerializeField] [Min(1)] private int survivorCount = 12;
        [SerializeField] [Min(0f)] private float timeBudget = 18f;
        [SerializeField] private LocalizedTextKey completionRule;
        public LocalizedTextKey Objective => objective; public StableId PhaseId => phaseId; public StableId RescuedUnitRoleId => rescuedUnitRoleId;
        public int SurvivorCount => survivorCount; public float TimeBudget => timeBudget; public LocalizedTextKey CompletionRule => completionRule;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); if (objective.IsEmpty) e.Add("objective is required.");
            Required(e, phaseId, "phaseId"); Required(e, rescuedUnitRoleId, "rescuedUnitRoleId");
            if (survivorCount < 1) e.Add("survivorCount must be positive."); NonNegative(e, timeBudget, "timeBudget");
            if (completionRule.IsEmpty) e.Add("completionRule is required."); return e;
        }
    }
}
