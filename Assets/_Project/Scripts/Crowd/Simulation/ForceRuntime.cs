using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;

namespace SeaLion.Crowd.Simulation
{
    /// <summary>Authoritative force arithmetic plus a deterministic presentation projection.</summary>
    public sealed class ForceRuntime
    {
        private readonly Dictionary<UnitRole, int> roleCounts = new Dictionary<UnitRole, int>();
        private int logicalCount;
        private int displayCap;
        private readonly List<int> displayedLogicalIndices = new List<int>();

        public int LogicalCount { get { return logicalCount; } }
        public int DisplayedAgentCount { get { return displayedLogicalIndices.Count; } }
        public int DisplayCap { get { return displayCap; } }
        public IReadOnlyDictionary<UnitRole, int> RoleCounts { get { return roleCounts; } }
        public IReadOnlyList<int> DisplayedLogicalIndices { get { return displayedLogicalIndices; } }

        public ForceRuntime(int logicalCount = 0, int displayCap = 1)
        {
            ValidateCount(logicalCount, nameof(logicalCount));
            SetDisplayCap(displayCap);
            SetLogicalCount(logicalCount);
        }

        public void SetLogicalCount(int count)
        {
            ValidateCount(count, nameof(count));
            logicalCount = count;
            roleCounts.Clear();
            RebuildDisplayMap();
        }

        public int ApplyMultiplier(int multiplier)
        {
            if (multiplier < 0) throw new ArgumentOutOfRangeException(nameof(multiplier));
            try
            {
                var nextLogicalCount = checked(logicalCount * multiplier);
                var nextRoles = new Dictionary<UnitRole, int>();
                foreach (var pair in roleCounts)
                    nextRoles.Add(pair.Key, checked(pair.Value * multiplier));
                logicalCount = nextLogicalCount;
                roleCounts.Clear();
                foreach (var pair in nextRoles) roleCounts.Add(pair.Key, pair.Value);
                RebuildDisplayMap();
            }
            catch (OverflowException) { throw new ArgumentOutOfRangeException(nameof(multiplier)); }
            return logicalCount;
        }

        /// <summary>Adds a contribution while preserving the logical/role-count invariant atomically.</summary>
        public int AddToRole(UnitRole role, int contribution)
        {
            ValidateCount(contribution, nameof(contribution));
            if (contribution == 0) return logicalCount;
            var nextLogicalCount = checked(logicalCount + contribution);
            roleCounts.TryGetValue(role, out var current);
            if (roleCounts.Count == 0 && logicalCount > 0) current = logicalCount;
            roleCounts[role] = checked(current + contribution);
            logicalCount = nextLogicalCount;
            RebuildDisplayMap();
            return logicalCount;
        }

        public void SetDisplayCap(int cap)
        {
            if (cap < 0) throw new ArgumentOutOfRangeException(nameof(cap));
            displayCap = cap;
            RebuildDisplayMap();
        }

        public void SetRoleCounts(IEnumerable<KeyValuePair<UnitRole, int>> counts)
        {
            if (counts == null) throw new ArgumentNullException(nameof(counts));
            var next = new Dictionary<UnitRole, int>();
            var total = 0;
            foreach (var pair in counts)
            {
                ValidateCount(pair.Value, nameof(counts));
                next[pair.Key] = pair.Value;
                total = checked(total + pair.Value);
            }
            if (total != logicalCount) throw new ArgumentException("Role counts must sum to logical count.", nameof(counts));
            roleCounts.Clear();
            foreach (var pair in next) roleCounts.Add(pair.Key, pair.Value);
        }

        private void RebuildDisplayMap()
        {
            var shown = Math.Min(logicalCount, displayCap);
            displayedLogicalIndices.Clear();
            if (shown == 0 || logicalCount == 0) return;
            if (shown == 1)
            {
                displayedLogicalIndices.Add((logicalCount - 1) / 2);
                return;
            }
            for (var i = 0; i < shown; i++)
                displayedLogicalIndices.Add((int)((long)i * (logicalCount - 1) / (shown - 1)));
        }

        private static void ValidateCount(int count, string name)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(name);
        }
    }
}
