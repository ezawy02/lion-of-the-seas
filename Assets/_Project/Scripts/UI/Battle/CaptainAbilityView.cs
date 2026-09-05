using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Abilities;

namespace SeaLion.UI.Battle
{
    public enum CaptainAbilityFeedback { Locked, Charging, Ready, Rejected, Active, Cooldown }

    public readonly struct CaptainAbilityView
    {
        public StableId Id { get; }
        public string Name { get; }
        public string Role { get; }
        public string TradeOff { get; }
        public string ChargeText { get; }
        public bool IsLocked { get; }
        public bool IsActive { get; }
        public bool IsReady { get; }
        public bool CanActivate { get { return !IsLocked && IsReady && !IsActive; } }

        public CaptainAbilityView(StableId id, string name, string role, string tradeOff,
            string chargeText, bool isLocked, bool isActive, bool isReady)
        {
            Id = id;
            Name = name ?? string.Empty;
            Role = role ?? string.Empty;
            TradeOff = tradeOff ?? string.Empty;
            ChargeText = chargeText ?? string.Empty;
            IsLocked = isLocked;
            IsActive = isActive;
            IsReady = isReady;
        }

        public CaptainAbilityView WithState(bool ready, bool active)
        {
            return new CaptainAbilityView(Id, Name, Role, TradeOff, ChargeText,
                IsLocked, active, ready);
        }
    }

    /// <summary>Maps deterministic ability state and activation results to readable UI feedback.</summary>
    public sealed class CaptainAbilityPresenter
    {
        public CaptainAbilityView View { get; private set; }
        public CaptainAbilityFeedback Feedback { get; private set; }
        public string Message { get; private set; }

        public CaptainAbilityPresenter(CaptainAbilityView initial)
        {
            View = initial;
            Sync(null);
        }

        public void Sync(CaptainAbilitySystem system)
        {
            if (View.IsLocked)
            {
                Set(CaptainAbilityFeedback.Locked, "Ability is locked.", false, false);
                return;
            }
            if (system == null || system.CooldownRemaining > 0f)
            {
                var seconds = system == null ? 0f : system.CooldownRemaining;
                Set(system == null ? CaptainAbilityFeedback.Charging : CaptainAbilityFeedback.Cooldown,
                    system == null ? "Ability is charging." : "Cooldown " + seconds.ToString("0.0") + "s",
                    false, false);
                return;
            }
            Set(system.IsReady ? CaptainAbilityFeedback.Ready : CaptainAbilityFeedback.Charging,
                system.IsReady ? "Ability ready." : "Ability is charging.", system.IsReady, false);
        }

        public void Handle(AbilityActivationResult result, CaptainAbilitySystem system)
        {
            if (result == AbilityActivationResult.Activated)
                Set(CaptainAbilityFeedback.Active, "Ability activated.", false, true);
            else if (result == AbilityActivationResult.Rejected)
                Set(CaptainAbilityFeedback.Rejected, "Activation rejected by battle state.", false, false);
            else Sync(system);
        }

        private void Set(CaptainAbilityFeedback feedback, string message, bool ready, bool active)
        {
            Feedback = feedback;
            Message = message;
            View = View.WithState(ready, active);
        }
    }
}
