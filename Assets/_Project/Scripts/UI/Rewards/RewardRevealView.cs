using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Rewards;

namespace SeaLion.UI.Rewards
{
    public readonly struct RewardRevealState
    {
        public readonly bool Visible;
        public readonly bool NewlyEarned;
        public readonly StableId RewardId;
        public readonly StableId BlueprintId;
        public readonly int Amount;
        public readonly FutureAttemptEffect FutureAttemptEffect;
        public readonly string Message;

        public RewardRevealState(bool visible, bool newlyEarned, StableId rewardId, StableId blueprintId,
            int amount, FutureAttemptEffect futureAttemptEffect, string message)
        {
            Visible = visible; NewlyEarned = newlyEarned; RewardId = rewardId; BlueprintId = blueprintId;
            Amount = amount; FutureAttemptEffect = futureAttemptEffect; Message = message ?? string.Empty;
        }
    }

    /// <summary>Small presentation model for the victory blueprint reveal; it never changes save state.</summary>
    public sealed class RewardRevealView
    {
        public RewardRevealState State { get; private set; }

        public RewardRevealView()
        { Clear(); }

        public void Present(RewardGrantResult grant)
        {
            if (!grant.Succeeded) { Clear(); return; }
            var message = grant.Applied ? "Blueprint earned: changes your next attempt." :
                "Blueprint already earned: available on your next attempt.";
            State = new RewardRevealState(true, grant.Applied, grant.RewardId, grant.TargetId, grant.Amount,
                grant.FutureAttemptEffect, message);
        }

        public void Clear()
        { State = new RewardRevealState(false, false, StableId.Empty, StableId.Empty, 0, FutureAttemptEffect.None, string.Empty); }
    }
}
