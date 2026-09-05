using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;

namespace SeaLion.Combat
{
    public readonly struct CrewRoleProfile
    {
        public readonly StableId Id;
        public readonly UnitRole Role;
        public readonly float DamageMultiplier;
        public readonly float DurabilityMultiplier;
        public readonly float CadenceMultiplier;
        public readonly int LandingContribution;

        public CrewRoleProfile(StableId id, UnitRole role, float damageMultiplier,
            float durabilityMultiplier, int landingContribution)
            : this(id, role, damageMultiplier, durabilityMultiplier, 1f, landingContribution)
        {
        }

        public CrewRoleProfile(StableId id, UnitRole role, float damageMultiplier,
            float durabilityMultiplier, float cadenceMultiplier, int landingContribution)
        {
            Id = id;
            Role = role;
            DamageMultiplier = damageMultiplier;
            DurabilityMultiplier = durabilityMultiplier;
            CadenceMultiplier = cadenceMultiplier;
            LandingContribution = landingContribution;
        }

        public CombatUnit ApplyTo(CombatUnit source)
        {
            source.Damage = Scale(source.Damage, DamageMultiplier);
            source.Health = Scale(source.Health, DurabilityMultiplier);
            source.Cadence = Scale(source.Cadence, CadenceMultiplier);
            return source;
        }

        private static float Scale(float value, float multiplier)
            => Finite(value) && Finite(multiplier) ? Math.Max(0f, value * Math.Max(0f, multiplier)) : 0f;

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    /// <summary>Resolves a selected crew role while keeping combat independent of save/presentation code.</summary>
    public sealed class CrewRoleLoadoutAdapter
    {
        private readonly Dictionary<StableId, CrewRoleProfile> profiles;
        private readonly HashSet<StableId> owned;

        public CrewRoleLoadoutAdapter(IEnumerable<CrewRoleProfile> profiles, IEnumerable<StableId> ownedIds = null)
        {
            this.profiles = new Dictionary<StableId, CrewRoleProfile>();
            if (profiles != null)
                foreach (var profile in profiles)
                    if (!profile.Id.IsEmpty) this.profiles[profile.Id] = profile;
            owned = ownedIds == null ? null : new HashSet<StableId>(ownedIds);
        }

        public IReadOnlyCollection<StableId> AvailableIds => profiles.Keys;

        public bool TryResolve(LoadoutSnapshot snapshot, out CrewRoleProfile profile)
            => TryResolve(snapshot.CrewRoleId, out profile);

        public bool TryResolve(StableId id, out CrewRoleProfile profile)
        {
            if (id.IsEmpty || (owned != null && !owned.Contains(id)))
            {
                profile = default;
                return false;
            }
            return profiles.TryGetValue(id, out profile);
        }

    }
}
