using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Quality Profile", fileName = "QualityProfile")]
    public sealed class QualityProfile : DefinitionAsset
    {
        [SerializeField] private QualityProfileKind profileKind;
        [SerializeField] [Min(1)] private int crowdPresentationCap = 300;
        [SerializeField] private StableId shadowProfileId, waterProfileId;
        [SerializeField] [Range(0f, 1f)] private float vfxDensity = 1f;
        [SerializeField] [Min(0.01f)] private float lodBias = 1f;
        [SerializeField] [Min(0f)] private float fallbackEnterFrameTime = 0.033f;
        [SerializeField] [Min(0f)] private float fallbackExitFrameTime = 0.025f;
        public QualityProfileKind ProfileKind => profileKind; public int CrowdPresentationCap => crowdPresentationCap; public StableId ShadowProfileId => shadowProfileId;
        public StableId WaterProfileId => waterProfileId; public float VfxDensity => vfxDensity; public float LodBias => lodBias;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); if (crowdPresentationCap < 1) e.Add("crowdPresentationCap must be positive.");
            if (profileKind == QualityProfileKind.Primary && crowdPresentationCap < 300) e.Add("Primary profile must support at least 300 agents.");
            if (fallbackExitFrameTime >= fallbackEnterFrameTime) e.Add("fallback triggers require hysteresis (exit below enter).");
            Required(e, shadowProfileId, "shadowProfileId"); Required(e, waterProfileId, "waterProfileId"); Positive(e, lodBias, "lodBias"); return e;
        }
    }
}
