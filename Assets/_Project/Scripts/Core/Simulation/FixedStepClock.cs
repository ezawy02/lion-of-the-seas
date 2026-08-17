using System;

namespace SeaLion.Core.Simulation
{
    /// <summary>Explicit fixed-step clock. Callers own elapsed time; Unity frame time is never read.</summary>
    public sealed class FixedStepClock
    {
        public const int DefaultTicksPerSecond = 60;
        private readonly int ticksPerSecond;
        private long tick;

        public long Tick => tick;
        public int TicksPerSecond => ticksPerSecond;
        public double FixedDeltaSeconds => 1d / ticksPerSecond;

        public FixedStepClock(int ticksPerSecond = DefaultTicksPerSecond)
        {
            if (ticksPerSecond <= 0) throw new ArgumentOutOfRangeException(nameof(ticksPerSecond));
            this.ticksPerSecond = ticksPerSecond;
        }

        public void Reset(long tick = 0)
        {
            if (tick < 0) throw new ArgumentOutOfRangeException(nameof(tick));
            this.tick = tick;
        }

        public int AdvanceTicks(int count)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(nameof(count));
            if (count > long.MaxValue - tick) throw new InvalidOperationException("Simulation tick overflow.");
            tick += count;
            return count;
        }
    }
}
