using System;
using System.Collections.Generic;
using System.IO;
using SeaLion.Core.Definitions;
using SeaLion.Core.Loadout;
using SeaLion.Core.Persistence;
using SeaLion.UI.Localization;
using UnityEngine;
using CoreLoadoutSlot = SeaLion.Core.Loadout.LoadoutSlot;

namespace SeaLion.UI.Loadout
{
    /// <summary>Runtime bridge from authored definitions and save state to the loadout view.</summary>
    public sealed class LoadoutScreenController : MonoBehaviour
    {
        [SerializeField] private FlagshipDefinition[] flagships = new FlagshipDefinition[0];
        [SerializeField] private UnitRoleDefinition[] crewRoles = new UnitRoleDefinition[0];
        [SerializeField] private CaptainAbilityDefinition[] captainAbilities = new CaptainAbilityDefinition[0];
        [SerializeField] private string saveFileName = LocalSaveRepository.DefaultFileName;

        private LoadoutService service;
        private LocalSaveRepository repository;
        public LoadoutScreenView View { get; private set; }
        public LoadoutSnapshot CurrentSnapshot => service == null ? default : service.CurrentSnapshot;
        public string LastFailure { get; private set; }
        public string LanguagePreference
        {
            get
            {
                if (repository == null) return GameLanguagePreference.English;
                var result = repository.Load();
                return result.Data == null || result.Data.settings == null ?
                    GameLanguagePreference.English : result.Data.settings.languagePreference;
            }
        }

        public void Initialize(LocalSaveRepository repository,
            IEnumerable<FlagshipDefinition> flagshipDefinitions,
            IEnumerable<UnitRoleDefinition> crewDefinitions,
            IEnumerable<CaptainAbilityDefinition> abilityDefinitions)
        {
            if (repository == null) throw new ArgumentNullException(nameof(repository));
            this.repository = repository;
            flagships = Copy(flagshipDefinitions);
            crewRoles = Copy(crewDefinitions);
            captainAbilities = Copy(abilityDefinitions);
            service = new LoadoutService(repository, Ids(flagships), Ids(crewRoles), Ids(captainAbilities));
            RefreshView();
        }

        public void InitializeForRuntime()
        {
            if (flagships.Length == 0 || crewRoles.Length == 0 || captainAbilities.Length == 0)
            {
                LastFailure = "error.notConfigured";
                return;
            }
            Initialize(new LocalSaveRepository(Path.Combine(Application.persistentDataPath, saveFileName)),
                flagships, crewRoles, captainAbilities);
        }

        public bool TrySelect(LoadoutSlot slot, StableId optionId)
        {
            if (service == null) { LastFailure = "error.notInitialized"; return false; }
            string failure;
            var coreSlot = slot == LoadoutSlot.Flagship ? CoreLoadoutSlot.Flagship :
                slot == LoadoutSlot.CrewRole ? CoreLoadoutSlot.Crew : CoreLoadoutSlot.CaptainAbility;
            if (!service.TrySelect(coreSlot, optionId, out failure)) { LastFailure = failure; return false; }
            LastFailure = string.Empty;
            RefreshView();
            return true;
        }

        public bool Refresh()
        {
            if (service == null) { LastFailure = "error.notInitialized"; return false; }
            var valid = service.Refresh(); RefreshView(); return valid;
        }

        public bool TrySetLanguagePreference(string preference)
        {
            if (!GameLanguagePreference.IsValid(preference))
            {
                LastFailure = "error.invalidLanguage";
                return false;
            }
            if (repository == null)
            {
                LastFailure = "error.notInitialized";
                return false;
            }
            var result = repository.Load();
            if (!result.Succeeded || result.Data == null)
            {
                LastFailure = result.Failure;
                return false;
            }
            result.Data.settings.languagePreference = preference;
            if (!repository.Save(result.Data, out var failure))
            {
                LastFailure = failure;
                return false;
            }
            LastFailure = string.Empty;
            return true;
        }

        private void RefreshView()
        {
            View = new LoadoutScreenView();
            AddOptions(LoadoutSlot.Flagship, flagships, CurrentSnapshot.FlagshipId);
            AddOptions(LoadoutSlot.CrewRole, crewRoles, CurrentSnapshot.CrewRoleId);
            AddOptions(LoadoutSlot.CaptainAbility, captainAbilities, CurrentSnapshot.CaptainAbilityId);
        }

        private void AddOptions(LoadoutSlot slot, IEnumerable<DefinitionAsset> source, StableId active)
        {
            var loaded = service == null ? null : serviceOwnedIds();
            var options = new List<LoadoutOption>();
            foreach (var definition in source)
            {
                if (definition == null) continue;
                var owned = loaded != null && loaded.Contains(definition.Id.Value);
                Describe(definition, out var name, out var role, out var tradeOff);
                options.Add(new LoadoutOption(definition.Id, slot, name, role, tradeOff,
                    !owned, definition.Id == active));
            }
            View.SetOptions(slot, options);
        }

        private HashSet<string> serviceOwnedIds()
        {
            var result = repository.Load();
            return result.Data == null ? new HashSet<string>() : new HashSet<string>(result.Data.ownedLoadoutIds);
        }

        private static T[] Copy<T>(IEnumerable<T> values) where T : UnityEngine.Object
        { return values == null ? new T[0] : new List<T>(values).ToArray(); }

        private static IEnumerable<StableId> Ids<T>(IEnumerable<T> values) where T : DefinitionAsset
        {
            foreach (var value in values) if (value != null) yield return value.Id;
        }

        private static void Describe(DefinitionAsset definition, out string name,
            out string role, out string tradeOff)
        {
            name = Humanize(definition.Id.Value);
            if (definition is FlagshipDefinition flagship)
            {
                role = flagship.DeployPattern + " deployment";
                tradeOff = flagship.BurstSize + " craft / " + flagship.DeploymentCadence.ToString("0.0") + "s cadence";
                return;
            }
            if (definition is UnitRoleDefinition crew)
            {
                role = crew.Role.ToString();
                tradeOff = "Damage " + crew.Combat.Damage.ToString("0.0") + " / durability " + crew.Durability.ToString("0.0");
                return;
            }
            var ability = (CaptainAbilityDefinition)definition;
            role = ability.GameplayEffect.Outcome + " " + ability.GameplayEffect.Value.ToString("0.#");
            tradeOff = ability.ChargeRule + " charge / " + ability.Cooldown.ToString("0.#") + "s cooldown";
        }

        private static string Humanize(string value)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            var words = value.Replace('-', ' ').Replace('_', ' ');
            return char.ToUpperInvariant(words[0]) + words.Substring(1);
        }
    }
}
