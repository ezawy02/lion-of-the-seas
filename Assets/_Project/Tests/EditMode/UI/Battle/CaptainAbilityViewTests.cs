using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.UI.Battle;

namespace SeaLion.Tests.EditMode.UI.Battle
{
    public sealed class CaptainAbilityViewTests
    {
        [Test]
        public void ActivationRequiresUnlockedReadyInactiveAbility()
        {
            var view = new CaptainAbilityView(new StableId("volley"), "Volley", "Burst", "Cooldown", "Gates 2/2", false, false, true);
            Assert.IsTrue(view.CanActivate);
            Assert.IsFalse(view.WithState(false, true).CanActivate);
            Assert.IsFalse(new CaptainAbilityView(new StableId("locked"), "Locked", "", "", "", true, false, true).CanActivate);
        }

        [Test]
        public void PresenterExposesChargingRejectedAndActiveFeedback()
        {
            var view = new CaptainAbilityView(new StableId("volley"), "Volley", "Burst",
                "Cooldown", "Damage", false, false, false);
            var presenter = new CaptainAbilityPresenter(view);
            Assert.That(presenter.Feedback, Is.EqualTo(CaptainAbilityFeedback.Charging));
            presenter.Handle(SeaLion.Gameplay.Abilities.AbilityActivationResult.Rejected, null);
            Assert.That(presenter.Feedback, Is.EqualTo(CaptainAbilityFeedback.Rejected));
            presenter.Handle(SeaLion.Gameplay.Abilities.AbilityActivationResult.Activated, null);
            Assert.That(presenter.Feedback, Is.EqualTo(CaptainAbilityFeedback.Active));
            Assert.That(presenter.View.IsActive, Is.True);
        }
    }
}
