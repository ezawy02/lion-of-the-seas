using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;

namespace SeaLion.Gameplay.Flagship
{
    public readonly struct FlagshipDeploymentProfile
    {
        public readonly StableId Id;
        public readonly NormalizedBounds ControlBounds;
        public readonly DeployPattern Pattern;
        public readonly float Cadence;
        public readonly int BurstSize;
        public readonly float BaseDeployment;
        public readonly StableId PresentationShipId, WakeId, RecoilId, AudioId;

        public FlagshipDeploymentProfile(FlagshipDefinition value)
        {
            Id = value.Id; ControlBounds = value.ControlBounds; Pattern = value.DeployPattern;
            Cadence = value.DeploymentCadence; BurstSize = value.BurstSize;
            BaseDeployment = value.BaseDeployment; PresentationShipId = value.PresentationShipId;
            WakeId = value.WakeId; RecoilId = value.RecoilId; AudioId = value.AudioId;
        }
    }

    /// <summary>Resolves the selected flagship without coupling loadout state to presentation.</summary>
    public sealed class FlagshipLoadoutAdapter
    {
        private readonly Dictionary<StableId, FlagshipDefinition> definitions;
        private readonly HashSet<StableId> owned;

        public FlagshipLoadoutAdapter(IEnumerable<FlagshipDefinition> definitions, IEnumerable<StableId> ownedIds = null)
        {
            this.definitions = new Dictionary<StableId, FlagshipDefinition>();
            if (definitions != null)
                foreach (var definition in definitions)
                    if (definition != null && !definition.Id.IsEmpty) this.definitions[definition.Id] = definition;
            owned = ownedIds == null ? null : new HashSet<StableId>(ownedIds);
        }

        public IReadOnlyCollection<StableId> AvailableIds => definitions.Keys;

        public bool TryResolve(LoadoutSnapshot snapshot, out FlagshipDefinition definition)
            => TryResolve(snapshot.FlagshipId, out definition);

        public bool TryResolve(StableId id, out FlagshipDefinition definition)
        {
            if (id.IsEmpty || (owned != null && !owned.Contains(id)))
            {
                definition = null;
                return false;
            }
            return definitions.TryGetValue(id, out definition);
        }

        public bool TryResolveDeployment(LoadoutSnapshot snapshot, out FlagshipDeploymentProfile profile)
        {
            if (!TryResolve(snapshot, out var definition))
            {
                profile = default;
                return false;
            }
            profile = new FlagshipDeploymentProfile(definition);
            return true;
        }
    }
}
