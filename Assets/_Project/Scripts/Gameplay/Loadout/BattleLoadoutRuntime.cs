using System;
using System.Collections.Generic;
using SeaLion.Combat;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Abilities;
using SeaLion.Gameplay.Deployment;
using SeaLion.Gameplay.Flagship;
using UnityEngine;

namespace SeaLion.Gameplay.Loadout
{
    /// <summary>Resolves one immutable session loadout into the systems used by battle runtime.</summary>
    public sealed class BattleLoadoutRuntime
    {
        private readonly BattleSession session;
        private readonly FlagshipDefinition flagship;

        private BattleLoadoutRuntime(BattleSession session, FlagshipDefinition flagship,
            FlagshipDeploymentProfile deployment, CrewRoleProfile crew,
            CaptainAbilitySystem ability)
        {
            this.session = session;
            this.flagship = flagship;
            Deployment = deployment;
            Crew = crew;
            Ability = ability;
        }

        public BattleSession Session => session;
        public FlagshipDeploymentProfile Deployment { get; }
        public CrewRoleProfile Crew { get; }
        public CaptainAbilitySystem Ability { get; }

        public static bool TryCreate(BattleSession session,
            IEnumerable<FlagshipDefinition> flagships,
            IEnumerable<UnitRoleDefinition> crewRoles,
            IEnumerable<CaptainAbilityDefinition> abilities,
            out BattleLoadoutRuntime runtime, out string failure)
        {
            runtime = null;
            failure = string.Empty;
            if (session == null)
            {
                failure = "A battle session is required.";
                return false;
            }

            var flagshipAdapter = new FlagshipLoadoutAdapter(flagships);
            if (!flagshipAdapter.TryResolve(session.SelectedLoadout, out var flagship) ||
                !flagshipAdapter.TryResolveDeployment(session.SelectedLoadout, out var deployment))
            {
                failure = "The selected flagship is unavailable.";
                return false;
            }

            var crewAdapter = new CrewRoleLoadoutAdapter(BuildCrewProfiles(crewRoles));
            if (!crewAdapter.TryResolve(session.SelectedLoadout, out var crew))
            {
                failure = "The selected crew role is unavailable.";
                return false;
            }

            var ability = FindAbility(abilities, session.SelectedLoadout.CaptainAbilityId);
            if (ability == null)
            {
                failure = "The selected captain ability is unavailable.";
                return false;
            }

            runtime = new BattleLoadoutRuntime(session, flagship, deployment, crew,
                new CaptainAbilitySystem(ability, session));
            return true;
        }

        public void Configure(LandingCraftDeployer deployer, Func<ILandingCraft> factory,
            int capacity, Vector3 origin, float spreadWidth = 1f, int warmCount = 0)
        {
            if (deployer == null) throw new ArgumentNullException(nameof(deployer));
            deployer.Configure(flagship, session, factory, capacity, origin, spreadWidth, warmCount);
        }

        public CombatUnit ApplyCrewTo(CombatUnit source) => Crew.ApplyTo(source);
        public void TickAbility(float seconds) => Ability.Tick(seconds);
        public void ReportDamage(float amount) => Ability.ReportDamage(amount);
        public void ReportGateResolved() => Ability.ReportGateResolved();
        public AbilityActivationResult TryActivateAbility() => Ability.TryActivate();

        private static CaptainAbilityDefinition FindAbility(
            IEnumerable<CaptainAbilityDefinition> abilities, StableId id)
        {
            if (abilities == null || id.IsEmpty) return null;
            foreach (var ability in abilities)
                if (ability != null && ability.Id == id) return ability;
            return null;
        }

        private static IEnumerable<CrewRoleProfile> BuildCrewProfiles(
            IEnumerable<UnitRoleDefinition> definitions)
        {
            var valid = new List<UnitRoleDefinition>();
            if (definitions != null)
                foreach (var definition in definitions)
                    if (definition != null && !definition.Id.IsEmpty) valid.Add(definition);
            if (valid.Count == 0) return Array.Empty<CrewRoleProfile>();

            var baseline = valid.Find(value => value.Id == new StableId("default-crew")) ?? valid[0];
            var profiles = new List<CrewRoleProfile>(valid.Count);
            foreach (var definition in valid)
            {
                var damage = Ratio(definition.Combat.Damage, baseline.Combat.Damage);
                var durability = Ratio(definition.Durability, baseline.Durability);
                var cadence = Ratio(definition.Combat.Cadence, baseline.Combat.Cadence);
                var contribution = Math.Max(1, (int)Math.Round(durability,
                    MidpointRounding.AwayFromZero));
                profiles.Add(new CrewRoleProfile(definition.Id, definition.Role, damage,
                    durability, cadence, contribution));
            }
            return profiles;
        }

        private static float Ratio(float value, float baseline)
            => Finite(value) && Finite(baseline) && baseline > 0f
                ? Math.Max(0f, value / baseline)
                : 1f;

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
