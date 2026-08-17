using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    [CreateAssetMenu(menuName = "Sea Lion/Definitions/Unit Role", fileName = "UnitRoleDefinition")]
    public sealed class UnitRoleDefinition : DefinitionAsset
    {
        [SerializeField] private Allegiance allegiance;
        [SerializeField] private UnitRole role;
        [SerializeField] private MovementStats movement;
        [SerializeField] private CombatStats combat;
        [SerializeField] [Min(0.01f)] private float durability = 1f;
        [SerializeField] private StableId meshId, materialId, poseId, vfxId, audioId;
        [SerializeField] private SimulationCostTier simulationCostTier;
        public Allegiance Allegiance => allegiance; public UnitRole Role => role; public MovementStats Movement => movement; public CombatStats Combat => combat;
        public float Durability => durability; public SimulationCostTier SimulationCostTier => simulationCostTier;
        public override IReadOnlyList<string> Validate()
        {
            var e = new List<string>(base.Validate()); if (!movement.IsValid) e.Add("movement values must be positive.");
            if (!combat.IsValid) e.Add("combat values cannot be negative."); Positive(e, durability, "durability");
            Required(e, meshId, "meshId"); Required(e, materialId, "materialId"); Required(e, poseId, "poseId"); Required(e, vfxId, "vfxId"); Required(e, audioId, "audioId"); return e;
        }
    }
}
