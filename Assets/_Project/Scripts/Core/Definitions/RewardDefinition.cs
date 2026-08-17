using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Reward", fileName = "RewardDefinition")]
    public sealed class RewardDefinition : DefinitionAsset
    {
        [SerializeField] private RewardGrantType grantType;
        [SerializeField] private StableId grantTargetId;
        [SerializeField] [Min(1)] private int amount = 1;
        [SerializeField] private bool firstCompletionOnly = true;
        [SerializeField] private StableId iconId, revealId, audioId, descriptionId;
        public RewardGrantType GrantType => grantType; public StableId GrantTargetId => grantTargetId; public int Amount => amount; public bool FirstCompletionOnly => firstCompletionOnly;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); Required(e, grantTargetId, "grantTargetId"); if (amount < 1) e.Add("amount must be positive.");
            Required(e, iconId, "iconId"); Required(e, revealId, "revealId"); Required(e, audioId, "audioId"); Required(e, descriptionId, "descriptionId"); return e;
        }
    }
}
