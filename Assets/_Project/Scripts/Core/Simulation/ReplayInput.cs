using System;
using System.Collections.Generic;

namespace SeaLion.Core.Simulation
{
    public readonly struct ReplayInputRecord
    {
        public long Tick { get; }
        public float HorizontalIntent { get; }
        public bool AbilityPressed { get; }

        public ReplayInputRecord(long tick, float horizontalIntent, bool abilityPressed)
        {
            if (tick < 0) throw new ArgumentOutOfRangeException(nameof(tick));
            if (float.IsNaN(horizontalIntent) || float.IsInfinity(horizontalIntent))
                throw new ArgumentException("Horizontal intent must be finite.", nameof(horizontalIntent));
            if (horizontalIntent < -1f || horizontalIntent > 1f)
                throw new ArgumentOutOfRangeException(nameof(horizontalIntent));
            Tick = tick;
            HorizontalIntent = horizontalIntent;
            AbilityPressed = abilityPressed;
        }
    }

    /// <summary>Immutable, tick-ordered input sequence used by deterministic replay.</summary>
    public sealed class ReplayInputLog
    {
        private readonly ReplayInputRecord[] records;
        public IReadOnlyList<ReplayInputRecord> Records => records;

        public ReplayInputLog(IEnumerable<ReplayInputRecord> source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var copy = new List<ReplayInputRecord>(source);
            for (var i = 1; i < copy.Count; i++)
                if (copy[i - 1].Tick >= copy[i].Tick)
                    throw new ArgumentException("Replay inputs must have strictly increasing ticks.", nameof(source));
            records = copy.ToArray();
        }

        public bool TryGet(long tick, out ReplayInputRecord record)
        {
            if (tick < 0) throw new ArgumentOutOfRangeException(nameof(tick));
            var low = 0;
            var high = records.Length - 1;
            while (low <= high)
            {
                var middle = low + ((high - low) / 2);
                if (records[middle].Tick == tick) { record = records[middle]; return true; }
                if (records[middle].Tick < tick) low = middle + 1; else high = middle - 1;
            }
            record = default;
            return false;
        }
    }
}
