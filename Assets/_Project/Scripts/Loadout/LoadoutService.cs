using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;
using SeaLion.Core.Persistence;

namespace SeaLion.Core.Loadout
{
    public enum LoadoutSlot
    {
        Flagship,
        Crew,
        CaptainAbility
    }

    /// <summary>Owns the validated, persistent three-slot battle loadout.</summary>
    public sealed class LoadoutService
    {
        private readonly LocalSaveRepository repository;
        private readonly HashSet<string> flagships;
        private readonly HashSet<string> crewRoles;
        private readonly HashSet<string> captainAbilities;
        private PlayerSaveData data;

        public LoadoutSnapshot CurrentSnapshot { get; private set; }

        public LoadoutService(LocalSaveRepository repository,
            IEnumerable<StableId> flagshipIds, IEnumerable<StableId> crewRoleIds,
            IEnumerable<StableId> captainAbilityIds)
        {
            this.repository = repository ?? throw new ArgumentNullException(nameof(repository));
            flagships = CreateOptionSet(flagshipIds, "flagshipIds");
            crewRoles = CreateOptionSet(crewRoleIds, "crewRoleIds");
            captainAbilities = CreateOptionSet(captainAbilityIds, "captainAbilityIds");
            Refresh();
        }

        public bool Refresh()
        {
            var result = repository.Load();
            data = result.Data ?? LocalSaveRepository.CreateDefault();
            if (!result.Succeeded) data = LocalSaveRepository.CreateDefault();
            return ApplySafeSelection();
        }

        public bool TrySelect(LoadoutSlot slot, StableId optionId, out string failure)
        {
            failure = string.Empty;
            if (!IsValidOption(slot, optionId))
            {
                failure = "The selected loadout option is not defined for this slot.";
                return false;
            }
            if (!data.ownedLoadoutIds.Contains(optionId.Value))
            {
                failure = "The selected loadout option is not owned.";
                return false;
            }

            var previous = data.selectedLoadout.ToSnapshot();
            var next = Replace(previous, slot, optionId);
            if (!TrySave(next, out failure)) return false;
            CurrentSnapshot = next;
            return true;
        }

        public bool TrySetLoadout(LoadoutSnapshot snapshot, out string failure)
        {
            failure = string.Empty;
            if (!IsOwnedOption(LoadoutSlot.Flagship, snapshot.FlagshipId) ||
                !IsOwnedOption(LoadoutSlot.Crew, snapshot.CrewRoleId) ||
                !IsOwnedOption(LoadoutSlot.CaptainAbility, snapshot.CaptainAbilityId))
            {
                failure = "The loadout contains an undefined or unowned option.";
                return false;
            }
            if (!TrySave(snapshot, out failure)) return false;
            CurrentSnapshot = snapshot;
            return true;
        }

        private bool ApplySafeSelection()
        {
            var selected = data.selectedLoadout == null ? default : data.selectedLoadout.ToSnapshot();
            if (IsOwnedOption(LoadoutSlot.Flagship, selected.FlagshipId) &&
                IsOwnedOption(LoadoutSlot.Crew, selected.CrewRoleId) &&
                IsOwnedOption(LoadoutSlot.CaptainAbility, selected.CaptainAbilityId))
            {
                CurrentSnapshot = selected;
                return true;
            }

            var fallback = new LoadoutSnapshot(
                FindFallback(LoadoutSlot.Flagship),
                FindFallback(LoadoutSlot.Crew),
                FindFallback(LoadoutSlot.CaptainAbility));
            if (!IsComplete(fallback))
            {
                CurrentSnapshot = selected;
                return false;
            }
            string failure;
            if (!TrySave(fallback, out failure))
            {
                CurrentSnapshot = selected;
                return false;
            }
            CurrentSnapshot = fallback;
            return true;
        }

        private bool TrySave(LoadoutSnapshot snapshot, out string failure)
        {
            var previous = data.selectedLoadout.ToSnapshot();
            data.selectedLoadout = new SaveLoadout
            {
                flagshipId = snapshot.FlagshipId.Value,
                crewRoleId = snapshot.CrewRoleId.Value,
                captainAbilityId = snapshot.CaptainAbilityId.Value
            };
            if (repository.Save(data, out failure)) return true;
            data.selectedLoadout = new SaveLoadout
            {
                flagshipId = previous.FlagshipId.Value,
                crewRoleId = previous.CrewRoleId.Value,
                captainAbilityId = previous.CaptainAbilityId.Value
            };
            return false;
        }

        private bool IsOwnedOption(LoadoutSlot slot, StableId id)
        { return IsValidOption(slot, id) && data.ownedLoadoutIds.Contains(id.Value); }

        private bool IsValidOption(LoadoutSlot slot, StableId id)
        { return !id.IsEmpty && Options(slot).Contains(id.Value); }

        private StableId FindFallback(LoadoutSlot slot)
        {
            var preferred = slot == LoadoutSlot.Flagship ? "default-flagship" :
                slot == LoadoutSlot.Crew ? "default-crew" : "default-ability";
            var options = Options(slot);
            if (options.Contains(preferred) && data.ownedLoadoutIds.Contains(preferred)) return new StableId(preferred);
            foreach (var option in options)
                if (data.ownedLoadoutIds.Contains(option)) return new StableId(option);
            return StableId.Empty;
        }

        private HashSet<string> Options(LoadoutSlot slot)
        {
            return slot == LoadoutSlot.Flagship ? flagships :
                slot == LoadoutSlot.Crew ? crewRoles : captainAbilities;
        }

        private static LoadoutSnapshot Replace(LoadoutSnapshot source, LoadoutSlot slot, StableId id)
        {
            return slot == LoadoutSlot.Flagship ? new LoadoutSnapshot(id, source.CrewRoleId, source.CaptainAbilityId) :
                slot == LoadoutSlot.Crew ? new LoadoutSnapshot(source.FlagshipId, id, source.CaptainAbilityId) :
                new LoadoutSnapshot(source.FlagshipId, source.CrewRoleId, id);
        }

        private static bool IsComplete(LoadoutSnapshot snapshot)
        { return !snapshot.FlagshipId.IsEmpty && !snapshot.CrewRoleId.IsEmpty && !snapshot.CaptainAbilityId.IsEmpty; }

        private static HashSet<string> CreateOptionSet(IEnumerable<StableId> ids, string name)
        {
            if (ids == null) throw new ArgumentNullException(name);
            var result = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids)
            {
                if (!StableId.IsValid(id.Value)) throw new ArgumentException("Loadout option IDs must be valid.", name);
                result.Add(id.Value);
            }
            if (result.Count == 0) throw new ArgumentException("At least one loadout option is required.", name);
            return result;
        }
    }
}
