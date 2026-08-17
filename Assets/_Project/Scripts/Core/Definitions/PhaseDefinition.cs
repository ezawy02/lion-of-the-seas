using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Phase", fileName = "PhaseDefinition")]
    public sealed class PhaseDefinition : DefinitionAsset
    {
        [SerializeField] private PhaseKind kind;
        [SerializeField] [Min(0f)] private float durationBudget;
        [SerializeField] private StableId cameraProfileId;
        [SerializeField] private List<StableId> spawnGroups = new List<StableId>();
        [SerializeField] private LocalizedTextKey completionRule;
        [SerializeField] private StableId nextPhaseId;
        public PhaseKind Kind => kind; public float DurationBudget => durationBudget; public StableId CameraProfileId => cameraProfileId;
        public IReadOnlyList<StableId> SpawnGroups => spawnGroups; public LocalizedTextKey CompletionRule => completionRule; public StableId NextPhaseId => nextPhaseId;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); Required(e, cameraProfileId, "cameraProfileId");
            if (completionRule.IsEmpty) e.Add("completionRule is required."); NonNegative(e, durationBudget, "durationBudget");
            if (kind != PhaseKind.Result) Required(e, nextPhaseId, "nextPhaseId");
            return e;
        }
    }
}
