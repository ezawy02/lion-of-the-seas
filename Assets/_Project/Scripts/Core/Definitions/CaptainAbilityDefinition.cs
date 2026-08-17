using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Captain Ability", fileName = "CaptainAbilityDefinition")]
    public sealed class CaptainAbilityDefinition : DefinitionAsset
    {
        [SerializeField] private AbilityChargeRule chargeRule;
        [SerializeField] private AbilityActivation activation;
        [SerializeField] private TypedEffect gameplayEffect;
        [SerializeField] [Min(0f)] private float duration;
        [SerializeField] [Min(0f)] private float cooldown;
        [SerializeField] private StableId heroId, vfxId, audioId, cameraProfileId;
        public AbilityChargeRule ChargeRule => chargeRule; public AbilityActivation Activation => activation; public TypedEffect GameplayEffect => gameplayEffect;
        public float Duration => duration; public float Cooldown => cooldown;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); if (!gameplayEffect.IsValid) e.Add("gameplayEffect is invalid."); NonNegative(e, duration, "duration"); NonNegative(e, cooldown, "cooldown");
            Required(e, heroId, "heroId"); Required(e, vfxId, "vfxId"); Required(e, audioId, "audioId"); Required(e, cameraProfileId, "cameraProfileId"); return e;
        }
    }
}
