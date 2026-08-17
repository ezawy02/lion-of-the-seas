using System;
using System.Collections.Generic;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using UnityEngine;

namespace SeaLion.Core.Events
{
    public enum BattleEventType { BattleReady, BattleStarted, ForceChanged, GateResolved, LandingStarted, LandingCompleted, BossPhaseChanged, AbilityActivated, BattleEnded, RewardGranted }

    public readonly struct BattleEventPayload
    {
        public readonly Guid SessionId;
        public readonly StableId PrimaryId;
        public readonly StableId SecondaryId;
        public readonly Allegiance Allegiance;
        public readonly int Before;
        public readonly int After;
        public readonly int Value;
        public readonly GateOutcome Outcome;
        public readonly BattleResult Result;
        public BattleEventPayload(Guid sessionId, StableId primaryId, StableId secondaryId, Allegiance allegiance, int before, int after, int value, GateOutcome outcome, BattleResult result)
        { SessionId = sessionId; PrimaryId = primaryId; SecondaryId = secondaryId; Allegiance = allegiance; Before = before; After = after; Value = value; Outcome = outcome; Result = result; }
    }

    public readonly struct BattleEvent
    {
        public readonly long Sequence;
        public readonly BattleEventType Type;
        public readonly BattleEventPayload Payload;
        public BattleEvent(long sequence, BattleEventType type, BattleEventPayload payload) { Sequence = sequence; Type = type; Payload = payload; }
    }

    public sealed class BattleEventStream
    {
        private readonly List<BattleEvent> events = new List<BattleEvent>();
        private readonly List<Action<BattleEvent>> subscribers = new List<Action<BattleEvent>>();
        private readonly Action<Exception> subscriberErrorHandler;
        public IReadOnlyList<BattleEvent> Events => events;

        public BattleEventStream(Action<Exception> subscriberErrorHandler = null)
        {
            this.subscriberErrorHandler = subscriberErrorHandler ?? Debug.LogException;
        }

        public IDisposable Subscribe(Action<BattleEvent> subscriber)
        {
            if (subscriber == null) throw new ArgumentNullException(nameof(subscriber));
            subscribers.Add(subscriber);
            return new Subscription(subscribers, subscriber);
        }
        internal BattleEvent Append(long sequence, BattleEventType type, BattleEventPayload payload)
        {
            if (events.Count > 0 && sequence <= events[events.Count - 1].Sequence) throw new InvalidOperationException("Event sequence must increase.");
            var item = new BattleEvent(sequence, type, payload);
            events.Add(item);
            for (var i = 0; i < subscribers.Count; i++)
            {
                try { subscribers[i](item); }
                catch (Exception exception) { subscriberErrorHandler(exception); }
            }
            return item;
        }
        private sealed class Subscription : IDisposable
        {
            private readonly List<Action<BattleEvent>> owner; private readonly Action<BattleEvent> callback;
            public Subscription(List<Action<BattleEvent>> owner, Action<BattleEvent> callback) { this.owner = owner; this.callback = callback; }
            public void Dispose() { owner.Remove(callback); }
        }
    }
}
