using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Gate", fileName = "GateDefinition")]
    public sealed class GateDefinition : DefinitionAsset
    {
        [SerializeField] private GateOutcome outcome;
        [SerializeField] private float value;
        [SerializeField] private StableId conversionId;
        [SerializeField] private ForceEligibility eligibleForce;
        [SerializeField] private MovementProfile movementProfile;
        [SerializeField] private float commitmentLine;
        [SerializeField] private GateVisualStyle visualStyle;
        [SerializeField] private StableId feedbackProfileId;
        public GateOutcome Outcome => outcome; public float Value => value; public StableId ConversionId => conversionId;
        public ForceEligibility EligibleForce => eligibleForce; public MovementProfile MovementProfile => movementProfile; public float CommitmentLine => commitmentLine;
        public GateVisualStyle VisualStyle => visualStyle; public StableId FeedbackProfileId => feedbackProfileId;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); Required(e, feedbackProfileId, "feedbackProfileId");
            e.AddRange(DefinitionValidation.ValidateGate(outcome, value, conversionId, visualStyle));
            return e;
        }
    }
}
