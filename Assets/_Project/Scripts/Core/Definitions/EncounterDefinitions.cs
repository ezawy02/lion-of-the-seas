using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Gate Set", fileName = "GateSetDefinition")]
    public sealed class GateSetDefinition : DefinitionAsset
    {
        [SerializeField] private List<StableId> gates = new List<StableId>();
        [SerializeField] private LocalizedTextKey decisionPrompt;
        public IReadOnlyList<StableId> Gates => gates;
        public LocalizedTextKey DecisionPrompt => decisionPrompt;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate());
            if (gates == null || gates.Count == 0) e.Add("at least one gate is required.");
            else if (DefinitionValidation.ValidateUniqueIds(gates, "gate").Count > 0) e.Add("gates contain invalid or duplicate IDs.");
            if (decisionPrompt.IsEmpty) e.Add("decisionPrompt is required.");
            return e;
        }
    }

}
