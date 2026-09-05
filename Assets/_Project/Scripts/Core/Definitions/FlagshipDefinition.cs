using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Flagship", fileName = "FlagshipDefinition")]
    public sealed class FlagshipDefinition : DefinitionAsset
    {
        [SerializeField] private NormalizedBounds controlBounds;
        [SerializeField] private DeployPattern deployPattern;
        [SerializeField] [Min(0.01f)] private float deploymentCadence = 1f;
        [SerializeField] [Min(1)] private int burstSize = 1;
        [SerializeField] [Min(0.01f)] private float baseDeployment = 1f;
        [SerializeField] private StableId presentationShipId, wakeId, recoilId, audioId;
        [SerializeField] private bool defaultUnlock;
        [SerializeField] private StableId unlockRewardId;
        public NormalizedBounds ControlBounds => controlBounds; public DeployPattern DeployPattern => deployPattern; public float DeploymentCadence => deploymentCadence;
        public int BurstSize => burstSize; public float BaseDeployment => baseDeployment; public bool IsDefault => defaultUnlock; public StableId UnlockRewardId => unlockRewardId;
        public StableId PresentationShipId => presentationShipId; public StableId WakeId => wakeId;
        public StableId RecoilId => recoilId; public StableId AudioId => audioId;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); if (!controlBounds.IsValid) e.Add("controlBounds must be normalized and left < right.");
            Positive(e, deploymentCadence, "deploymentCadence"); Positive(e, baseDeployment, "baseDeployment"); if (burstSize < 1) e.Add("burstSize must be at least one.");
            Required(e, presentationShipId, "presentationShipId"); Required(e, wakeId, "wakeId"); Required(e, recoilId, "recoilId"); Required(e, audioId, "audioId");
            if (!defaultUnlock) Required(e, unlockRewardId, "unlockRewardId"); return e;
        }
    }
}
