using System;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Persistence;

namespace SeaLion.Gameplay.Rewards
{
    public enum FutureAttemptEffect
    {
        None,
        BlueprintUnlocked,
        BlueprintAlreadyUnlocked
    }

    public readonly struct RewardGrantResult
    {
        public readonly bool Succeeded;
        public readonly bool Applied;
        public readonly bool AlreadyGranted;
        public readonly StableId RewardId;
        public readonly StableId TargetId;
        public readonly string TransactionId;
        public readonly int Amount;
        public readonly FutureAttemptEffect FutureAttemptEffect;
        public readonly string Failure;

        public RewardGrantResult(bool succeeded, bool applied, bool alreadyGranted, StableId rewardId,
            StableId targetId, string transactionId, int amount, FutureAttemptEffect futureAttemptEffect, string failure)
        {
            Succeeded = succeeded; Applied = applied; AlreadyGranted = alreadyGranted; RewardId = rewardId;
            TargetId = targetId; TransactionId = transactionId ?? string.Empty; Amount = amount;
            FutureAttemptEffect = futureAttemptEffect; Failure = failure ?? string.Empty;
        }
    }

    /// <summary>Grants a victory reward once and records its loadout effect for future attempts.</summary>
    public sealed class RewardGrantService
    {
        private readonly LocalSaveRepository saves;

        public RewardGrantService(LocalSaveRepository saves)
        { this.saves = saves ?? throw new ArgumentNullException(nameof(saves)); }

        public bool TryGrant(BattleSession session, RewardDefinition reward, out RewardGrantResult result)
        {
            if (session == null) throw new ArgumentNullException(nameof(session));
            var transactionId = session.SessionId.ToString("N");
            var victory = session.Result.HasValue && session.Result.Value.IsVictory;
            return TryGrant(victory, reward, transactionId, out result);
        }

        public bool TryGrant(bool victory, RewardDefinition reward, string transactionId, out RewardGrantResult result)
        {
            if (reward == null) throw new ArgumentNullException(nameof(reward));
            var rewardId = reward.Id;
            var targetId = reward.GrantTargetId;
            if (!StableId.IsValid(rewardId.Value) || !StableId.IsValid(targetId.Value) || !StableId.IsValid(transactionId))
                return Reject(rewardId, targetId, transactionId, reward.Amount, "Reward, blueprint, and transaction IDs must be valid stable IDs.", out result);
            if (!victory)
                return Reject(rewardId, targetId, transactionId, reward.Amount, "Rewards require a victorious completion.", out result);
            if (reward.FirstCompletionOnly == false)
                return Reject(rewardId, targetId, transactionId, reward.Amount, "Repeatable rewards are not supported by the first-completion grant flow.", out result);

            bool applied;
            string failure;
            if (!saves.TryGrantRewardWithOwnership(transactionId, rewardId.Value, targetId.Value, out applied, out failure))
                return Reject(rewardId, targetId, transactionId, reward.Amount, failure, out result);

            result = new RewardGrantResult(true, applied, !applied, rewardId, targetId, transactionId, reward.Amount,
                applied ? FutureAttemptEffect.BlueprintUnlocked : FutureAttemptEffect.BlueprintAlreadyUnlocked, string.Empty);
            return true;
        }

        private static bool Reject(StableId rewardId, StableId targetId, string transactionId, int amount,
            string failure, out RewardGrantResult result)
        {
            result = new RewardGrantResult(false, false, false, rewardId, targetId, transactionId, amount,
                FutureAttemptEffect.None, failure);
            return false;
        }
    }
}
