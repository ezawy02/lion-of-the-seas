using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;

namespace SeaLion.UI.Loadout
{
    public enum LoadoutSlot
    {
        Flagship,
        CrewRole,
        CaptainAbility
    }

    public readonly struct LoadoutOption
    {
        public StableId Id { get; }
        public LoadoutSlot Slot { get; }
        public string Name { get; }
        public string Role { get; }
        public string TradeOff { get; }
        public bool IsLocked { get; }
        public bool IsActive { get; }

        public LoadoutOption(StableId id, LoadoutSlot slot, string name, string role,
            string tradeOff, bool isLocked, bool isActive)
        {
            Id = id;
            Slot = slot;
            Name = name ?? string.Empty;
            Role = role ?? string.Empty;
            TradeOff = tradeOff ?? string.Empty;
            IsLocked = isLocked;
            IsActive = isActive;
        }
    }

    public sealed class OptionCard
    {
        public LoadoutOption Option { get; private set; }
        public bool CanSelect { get { return !Option.IsLocked; } }

        public OptionCard(LoadoutOption option) { Option = option; }

        public void SetActive(bool active)
        {
            Option = new LoadoutOption(Option.Id, Option.Slot, Option.Name, Option.Role,
                Option.TradeOff, Option.IsLocked, active);
        }
    }

    public sealed class LoadoutScreenView
    {
        private readonly Dictionary<LoadoutSlot, List<OptionCard>> cards =
            new Dictionary<LoadoutSlot, List<OptionCard>>();

        public IReadOnlyList<OptionCard> GetOptions(LoadoutSlot slot)
        {
            List<OptionCard> result;
            return cards.TryGetValue(slot, out result) ? result : Array.Empty<OptionCard>();
        }

        public void SetOptions(LoadoutSlot slot, IEnumerable<LoadoutOption> options)
        {
            if (options == null) throw new ArgumentNullException(nameof(options));
            var list = new List<OptionCard>();
            foreach (var option in options)
            {
                if (option.Slot != slot) throw new ArgumentException("Option slot mismatch.", nameof(options));
                list.Add(new OptionCard(option));
            }
            cards[slot] = list;
        }

        public bool TrySelect(LoadoutSlot slot, StableId id)
        {
            OptionCard selected;
            if (!TryFind(slot, id, out selected) || !selected.CanSelect) return false;
            foreach (var card in GetOptions(slot)) card.SetActive(card == selected);
            return true;
        }

        public bool TryGetActive(LoadoutSlot slot, out OptionCard active)
        {
            foreach (var card in GetOptions(slot))
            {
                if (card.Option.IsActive) { active = card; return true; }
            }
            active = null;
            return false;
        }

        private bool TryFind(LoadoutSlot slot, StableId id, out OptionCard found)
        {
            foreach (var card in GetOptions(slot))
            {
                if (card.Option.Id.Equals(id)) { found = card; return true; }
            }
            found = null;
            return false;
        }
    }
}
