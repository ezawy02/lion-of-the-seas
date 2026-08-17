using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Level", fileName = "LevelDefinition")]
    public sealed class LevelDefinition : DefinitionAsset
    {
        [SerializeField] private LocalizedTextKey displayName;
        [SerializeField] private StableId sceneId;
        [SerializeField] [Range(1, 3)] private int order = 1;
        [SerializeField] private List<StableId> phases = new List<StableId>();
        [SerializeField] private List<StableId> gateSets = new List<StableId>();
        [SerializeField] private List<StableId> encounters = new List<StableId>();
        [SerializeField] private StableId rewardId;
        [SerializeField] private StableId qualityProfileId;
        [SerializeField] private List<StableId> storeMoments = new List<StableId>();
        [Header("Level 1 pacing")]
        [SerializeField] [Range(0f, 3f)] private float openingThreatRevealSeconds = 2f;
        [SerializeField] [Range(45f, 120f)] private float targetDurationSeconds = 68f;
        [SerializeField] [Range(0f, 1f)] private float easyGatePosition = 0.36f;
        [SerializeField] [Range(0f, 1f)] private float riskyGatePosition = 0.64f;
        [SerializeField] [Range(1f, 20f)] private float landingTransferSeconds = 9f;
        [SerializeField] [Range(1f, 20f)] private float guardianPressureIntervalSeconds = 6f;
        public LocalizedTextKey DisplayName => displayName;
        public StableId SceneId => sceneId;
        public int Order => order;
        public IReadOnlyList<StableId> Phases => phases;
        public IReadOnlyList<StableId> GateSets => gateSets;
        public IReadOnlyList<StableId> Encounters => encounters;
        public StableId RewardId => rewardId;
        public StableId QualityProfileId => qualityProfileId;
        public IReadOnlyList<StableId> StoreMoments => storeMoments;
        public float OpeningThreatRevealSeconds => openingThreatRevealSeconds;
        public float TargetDurationSeconds => targetDurationSeconds;
        public float EasyGatePosition => easyGatePosition;
        public float RiskyGatePosition => riskyGatePosition;
        public float LandingTransferSeconds => landingTransferSeconds;
        public float GuardianPressureIntervalSeconds => guardianPressureIntervalSeconds;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate());
            if (displayName.IsEmpty) e.Add("displayName is required.");
            Required(e, sceneId, "sceneId"); Required(e, rewardId, "rewardId"); Required(e, qualityProfileId, "qualityProfileId");
            if (order < 1 || order > 3) e.Add("order must be 1-3.");
            if (phases == null || phases.Count < 4) e.Add("phases must contain opening, traversal, assault and result.");
            if (openingThreatRevealSeconds < 0f || openingThreatRevealSeconds > 3f) e.Add("opening threat must appear within three seconds.");
            if (targetDurationSeconds < 60f || targetDurationSeconds > 75f) e.Add("Level 1 target duration must be 60-75 seconds.");
            if (easyGatePosition >= riskyGatePosition || riskyGatePosition - easyGatePosition < .2f) e.Add("gate positions must provide a readable choice.");
            if (landingTransferSeconds <= 0f || guardianPressureIntervalSeconds <= 0f) e.Add("landing and guardian pacing must be positive.");
            Add(e, DefinitionValidation.ValidateUniqueIds(phases, "phase").Count == 0 ? string.Empty : "phases contain invalid or duplicate IDs.");
            return e;
        }
    }
}
