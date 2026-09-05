using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.UI.Loadout;

namespace SeaLion.Tests.EditMode.UI.Loadout
{
    public sealed class LoadoutScreenViewTests
    {
        private static StableId Id(string value) { return new StableId(value); }

        [Test]
        public void SupportsThreeSlotsAndExposesOptionMetadata()
        {
            var view = new LoadoutScreenView();
            view.SetOptions(LoadoutSlot.Flagship, new[] { new LoadoutOption(Id("ship"), LoadoutSlot.Flagship, "Sails", "Speed", "Lower force", false, true) });
            view.SetOptions(LoadoutSlot.CrewRole, new[] { new LoadoutOption(Id("crew"), LoadoutSlot.CrewRole, "Marines", "Range", "Lower durability", true, false) });
            view.SetOptions(LoadoutSlot.CaptainAbility, new[] { new LoadoutOption(Id("ability"), LoadoutSlot.CaptainAbility, "Volley", "Burst", "Long cooldown", false, false) });
            Assert.AreEqual("Speed", view.GetOptions(LoadoutSlot.Flagship)[0].Option.Role);
            Assert.IsFalse(view.GetOptions(LoadoutSlot.CrewRole)[0].CanSelect);
            Assert.IsFalse(view.TrySelect(LoadoutSlot.CrewRole, Id("crew")));
            Assert.IsTrue(view.TrySelect(LoadoutSlot.CaptainAbility, Id("ability")));
        }

        [Test]
        public void SelectingOptionLeavesOnlyOneActiveInItsSlot()
        {
            var view = new LoadoutScreenView();
            view.SetOptions(LoadoutSlot.Flagship, new[] {
                new LoadoutOption(Id("a"), LoadoutSlot.Flagship, "A", "Tank", "Slow", false, true),
                new LoadoutOption(Id("b"), LoadoutSlot.Flagship, "B", "Fast", "Fragile", false, false) });
            Assert.IsTrue(view.TrySelect(LoadoutSlot.Flagship, Id("b")));
            OptionCard active;
            Assert.IsTrue(view.TryGetActive(LoadoutSlot.Flagship, out active));
            Assert.AreEqual(Id("b"), active.Option.Id);
        }
    }
}
