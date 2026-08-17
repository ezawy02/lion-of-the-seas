using System;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;

namespace SeaLion.Core.Battle
{
    public enum BattleState { Loading, Ready, Active, Landing, Assault, Victory, Failure, Exiting }
    public readonly struct BattleResult
    {
        public readonly bool IsVictory; public readonly string Reason;
        public BattleResult(bool isVictory, string reason) { IsVictory = isVictory; Reason = reason ?? string.Empty; }
    }

    public sealed class BattleSession
    {
        private readonly BattleEventStream eventStream;
        private long sequence;
        private bool ended;
        public Guid SessionId { get; }
        public StableId LevelId { get; }
        public StableId PhaseId { get; private set; }
        public LoadoutSnapshot SelectedLoadout { get; }
        public BattleState State { get; private set; }
        public float ElapsedTime { get; private set; }
        public BattleResult? Result { get; private set; }
        public BattleEventStream Events => eventStream;
        public BattleSession(StableId levelId, StableId phaseId, LoadoutSnapshot loadout, Guid? sessionId = null, BattleEventStream stream = null)
        {
            if (levelId.IsEmpty || phaseId.IsEmpty) throw new ArgumentException("Level and phase IDs are required.");
            SessionId = sessionId ?? Guid.NewGuid(); LevelId = levelId; PhaseId = phaseId; SelectedLoadout = loadout;
            eventStream = stream ?? new BattleEventStream(); State = BattleState.Loading;
        }
        public bool TryTransition(BattleState next)
        {
            if (!IsLegal(State, next) || ended) return false;
            State = next;
            if (next == BattleState.Ready) Publish(BattleEventType.BattleReady, default);
            if (next == BattleState.Active) Publish(BattleEventType.BattleStarted, default);
            return true;
        }
        public bool TrySetPhase(StableId phaseId) { if (ended || phaseId.IsEmpty) return false; PhaseId = phaseId; return true; }
        public bool TryAdvance(float deltaSeconds) { if (float.IsNaN(deltaSeconds) || float.IsInfinity(deltaSeconds) || deltaSeconds < 0f || ended || State != BattleState.Active && State != BattleState.Landing && State != BattleState.Assault) return false; ElapsedTime += deltaSeconds; return true; }
        public bool TryPublishGameplayEvent(BattleEventType type, BattleEventPayload payload)
        {
            if (ended || State == BattleState.Loading || State == BattleState.Ready ||
                type == BattleEventType.BattleReady || type == BattleEventType.BattleStarted ||
                type == BattleEventType.BattleEnded || type == BattleEventType.RewardGranted)
                return false;

            Publish(type, payload);
            return true;
        }
        public bool End(bool victory, string reason = null)
        {
            if (ended || (State != BattleState.Assault && State != BattleState.Active && State != BattleState.Landing)) return false;
            var result = new BattleResult(victory, reason); ended = true; Result = result; State = victory ? BattleState.Victory : BattleState.Failure;
            Publish(BattleEventType.BattleEnded, new BattleEventPayload(SessionId, default, default, default, 0, 0, 0, default, result)); return true;
        }
        public bool TryExit()
        {
            if (State != BattleState.Victory && State != BattleState.Failure) return false;
            State = BattleState.Exiting;
            return true;
        }
        private void Publish(BattleEventType type, BattleEventPayload payload) { eventStream.Append(++sequence, type, new BattleEventPayload(SessionId, payload.PrimaryId, payload.SecondaryId, payload.Allegiance, payload.Before, payload.After, payload.Value, payload.Outcome, payload.Result)); }
        private static bool IsLegal(BattleState from, BattleState to)
        { return (from == BattleState.Loading && to == BattleState.Ready) || (from == BattleState.Ready && to == BattleState.Active) || (from == BattleState.Active && to == BattleState.Landing) || (from == BattleState.Landing && to == BattleState.Assault); }
    }
}
