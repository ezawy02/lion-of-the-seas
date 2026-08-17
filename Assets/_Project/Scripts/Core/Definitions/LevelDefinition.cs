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
        public LocalizedTextKey DisplayName => displayName;
        public StableId SceneId => sceneId;
        public int Order => order;
        public IReadOnlyList<StableId> Phases => phases;
        public IReadOnlyList<StableId> GateSets => gateSets;
        public IReadOnlyList<StableId> Encounters => encounters;
        public StableId RewardId => rewardId;
        public StableId QualityProfileId => qualityProfileId;
        public IReadOnlyList<StableId> StoreMoments => storeMoments;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate());
            if (displayName.IsEmpty) e.Add("displayName is required.");
            Required(e, sceneId, "sceneId"); Required(e, rewardId, "rewardId"); Required(e, qualityProfileId, "qualityProfileId");
            if (order < 1 || order > 3) e.Add("order must be 1-3.");
            if (phases == null || phases.Count < 4) e.Add("phases must contain opening, traversal, assault and result.");
            Add(e, DefinitionValidation.ValidateUniqueIds(phases, "phase").Count == 0 ? string.Empty : "phases contain invalid or duplicate IDs.");
            return e;
        }
    }
}
