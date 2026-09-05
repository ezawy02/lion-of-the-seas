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
        [Header("Encounter pacing")]
        [SerializeField] [Range(0f, 3f)] private float openingThreatRevealSeconds = 2f;
        [SerializeField] [Range(45f, 120f)] private float targetDurationSeconds = 68f;
        [SerializeField] [Range(0f, 1f)] private float easyGatePosition = 0.36f;
        [SerializeField] [Range(0f, 1f)] private float riskyGatePosition = 0.64f;
        [SerializeField] [Range(1f, 20f)] private float landingTransferSeconds = 9f;
        [SerializeField] [Range(1f, 20f)] private float guardianPressureIntervalSeconds = 6f;
        [SerializeField, Min(1)] private int initialForce = 8;
        [SerializeField, Min(1)] private int displayCap = 300;
        [SerializeField, Min(1)] private int enemyCount = 8;
        [SerializeField, Min(1f)] private float bossHealth = 140f;
        [SerializeField, Min(.01f)] private float routeSpeed = .1f;
        [SerializeField, Range(0f, 1f)] private float gateProgress = .4f;
        [SerializeField, Range(0f, 1f)] private float rescueProgress = .7f;
        [SerializeField, Range(.05f, 1f)] private float gateHalfWidth = .72f;
        [SerializeField, Min(.1f)] private float primaryCooldown = .55f;
        [SerializeField, Min(.1f)] private float ordinaryDamage = 5f;
        [SerializeField, Min(.1f)] private float guardianDamage = 18f;
        [SerializeField, Min(1f)] private float referenceForce = 32f;
        [SerializeField, Min(1f)] private float assaultTimeLimit = 55f;
        [Header("Campaign encounters")]
        [SerializeField, Min(1f)] private float blockadeHealth = 80f;
        [SerializeField, Range(.1f, .99f)] private float blockadeProgress = .8f;
        [SerializeField, Min(0f)] private float stormStrength = .18f;
        [SerializeField, Min(.1f)] private float hazardWarningSeconds = 2f;
        [SerializeField, Min(.2f)] private float hazardFireSeconds = 3f;
        [SerializeField, Min(0f)] private float powderDamage = 24f;
        public float BlockadeHealth => blockadeHealth;
        public float BlockadeProgress => blockadeProgress;
        public float StormStrength => stormStrength;
        public float HazardWarningSeconds => hazardWarningSeconds;
        public float HazardFireSeconds => hazardFireSeconds;
        public float PowderDamage => powderDamage;
        public int InitialForce => initialForce;
        public int DisplayCap => displayCap;
        public int EnemyCount => enemyCount;
        public float BossHealth => bossHealth;
        public float RouteSpeed => routeSpeed;
        public float GateProgress => gateProgress;
        public float RescueProgress => rescueProgress;
        public float GateHalfWidth => gateHalfWidth;
        public float PrimaryCooldown => primaryCooldown;
        public float OrdinaryDamage => ordinaryDamage;
        public float GuardianDamage => guardianDamage;
        public float ReferenceForce => referenceForce;
        public float AssaultTimeLimit => assaultTimeLimit;
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
            if (order == 1 && (targetDurationSeconds < 60f || targetDurationSeconds > 75f)) e.Add("Level 1 target duration must be 60-75 seconds.");
            if (easyGatePosition >= riskyGatePosition || riskyGatePosition - easyGatePosition < .2f) e.Add("gate positions must provide a readable choice.");
            if (landingTransferSeconds <= 0f || guardianPressureIntervalSeconds <= 0f) e.Add("landing and guardian pacing must be positive.");
            Add(e, DefinitionValidation.ValidateUniqueIds(phases, "phase").Count == 0 ? string.Empty : "phases contain invalid or duplicate IDs.");
            if (initialForce < 1 || displayCap < 1 || enemyCount < 1) e.Add("force and enemy budgets must be positive.");
            if (!(routeSpeed > 0f) || !(gateProgress > 0f && gateProgress < rescueProgress && rescueProgress < 1f)) e.Add("route anchors must be ordered before the shore.");
            if (!(bossHealth > 0f && primaryCooldown > 0f && referenceForce > 0f && assaultTimeLimit > 0f)) e.Add("combat tuning must be positive.");
            if (!(blockadeHealth > 0f && blockadeProgress > gateProgress && blockadeProgress < 1f)) e.Add("blockade must follow gate and precede shore.");
            if (!(hazardFireSeconds > hazardWarningSeconds && hazardWarningSeconds > 0f)) e.Add("hazard requires a warning window.");
            return e;
        }
    }
}
