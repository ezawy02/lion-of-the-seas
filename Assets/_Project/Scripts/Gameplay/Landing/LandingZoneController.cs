using System;
using System.Collections.Generic;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Crowd.Simulation;
using SeaLion.Gameplay.Deployment;
using UnityEngine;

namespace SeaLion.Gameplay.Landing
{
    /// <summary>Deterministic, idempotent sea-to-land transfer boundary.</summary>
    public sealed class LandingZoneController : MonoBehaviour
    {
        private readonly HashSet<ILandingCraft> registered = new HashSet<ILandingCraft>();
        private readonly SortedDictionary<int, Arrival> arrivals = new SortedDictionary<int, Arrival>();
        private BattleSession session;
        private ForceRuntime landForce;
        private StableId zoneId;
        private int expected;
        private int nextSequence;
        private int resolved;
        private int transferred;
        private bool fixedExpected;
        private bool started;
        private bool completed;
        private bool paused;
        private bool terminal;

        public bool IsCompleted => completed;
        public bool IsStarted => started;
        public int TransferredContribution => transferred;
        public int PendingCount => fixedExpected ? Math.Max(0, expected - resolved) : arrivals.Count;

        public void Configure(BattleSession battle, ForceRuntime destination, StableId id,
            int expectedCrafts = 0, int firstSequence = 0)
        {
            if (battle == null) throw new ArgumentNullException(nameof(battle));
            if (destination == null) throw new ArgumentNullException(nameof(destination));
            if (id.IsEmpty) throw new ArgumentException("A landing-zone ID is required.", nameof(id));
            if (expectedCrafts < 0) throw new ArgumentOutOfRangeException(nameof(expectedCrafts));
            ResetRuntime();
            session = battle;
            landForce = destination;
            zoneId = id;
            expected = expectedCrafts;
            fixedExpected = expectedCrafts > 0;
            nextSequence = firstSequence;
        }

        public void SetPaused(bool value) => paused = value;

        public void SetTerminal(bool value)
        {
            terminal = value;
            if (value) Complete();
        }

        public bool TryAccept(ILandingCraft craft, int sequence, int contribution,
            bool eligible = true, bool destroyed = false, UnitRole role = UnitRole.Sailor)
        {
            if (craft == null || completed || paused || terminal || sequence < nextSequence ||
                contribution < 0 || registered.Contains(craft) || arrivals.ContainsKey(sequence)) return false;
            if (fixedExpected && resolved + arrivals.Count >= expected) return false;
            registered.Add(craft);
            arrivals.Add(sequence, new Arrival(craft, contribution, eligible, destroyed, role));
            ProcessContiguous();
            return true;
        }

        public bool NotifyCraftDestroyed(ILandingCraft craft, int sequence) =>
            TryAccept(craft, sequence, 0, false, true);

        /// <summary>Closes a dynamic batch or resolves remaining known arrivals at terminal state.</summary>
        public void Complete()
        {
            if (completed || session == null || landForce == null) return;
            var pending = new List<int>(arrivals.Keys);
            for (var i = 0; i < pending.Count; i++) Resolve(pending[i]);
            expected = resolved;
            fixedExpected = true;
            CompleteIfReady();
        }

        private void ProcessContiguous()
        {
            while (arrivals.ContainsKey(nextSequence))
            {
                Resolve(nextSequence);
                nextSequence++;
            }
            CompleteIfReady();
        }

        private void Resolve(int sequence)
        {
            var arrival = arrivals[sequence];
            arrivals.Remove(sequence);
            resolved++;
            if (arrival.Destroyed || !arrival.Eligible || arrival.Contribution == 0) return;
            if (!started)
            {
                started = true;
                session.TryPublishGameplayEvent(BattleEventType.LandingStarted,
                    new BattleEventPayload(session.SessionId, zoneId, default, Allegiance.Friendly,
                        0, 0, arrival.Contribution, default, default));
            }
            Transfer(arrival.Contribution, arrival.Role);
        }

        private void Transfer(int contribution, UnitRole role)
        {
            var counts = new Dictionary<UnitRole, int>();
            foreach (var pair in landForce.RoleCounts) counts[pair.Key] = pair.Value;
            counts.TryGetValue(role, out var current);
            counts[role] = checked(current + contribution);
            landForce.SetLogicalCount(checked(landForce.LogicalCount + contribution));
            landForce.SetRoleCounts(counts);
            transferred = checked(transferred + contribution);
        }

        private void CompleteIfReady()
        {
            if (completed || !fixedExpected || resolved < expected || arrivals.Count != 0) return;
            completed = true;
            session.TryPublishGameplayEvent(BattleEventType.LandingCompleted,
                new BattleEventPayload(session.SessionId, zoneId, default, Allegiance.Friendly,
                    0, landForce.LogicalCount, transferred, default, default));
        }

        private void ResetRuntime()
        {
            registered.Clear();
            arrivals.Clear();
            expected = nextSequence = resolved = transferred = 0;
            fixedExpected = started = completed = paused = terminal = false;
        }

        private readonly struct Arrival
        {
            public readonly ILandingCraft Craft;
            public readonly int Contribution;
            public readonly bool Eligible, Destroyed;
            public readonly UnitRole Role;
            public Arrival(ILandingCraft craft, int contribution, bool eligible, bool destroyed, UnitRole role)
            { Craft = craft; Contribution = contribution; Eligible = eligible; Destroyed = destroyed; Role = role; }
        }
    }
}
