using System;

namespace SeaLion.Core.Simulation
{
    /// <summary>Validated seed carried by a battle and its replay.</summary>
    public readonly struct DeterministicSeed : IEquatable<DeterministicSeed>
    {
        public uint Value { get; }

        public DeterministicSeed(uint value)
        {
            Value = value;
        }

        public static bool TryCreate(long value, out DeterministicSeed seed)
        {
            if (value < 0 || value > uint.MaxValue)
            {
                seed = default;
                return false;
            }
            seed = new DeterministicSeed((uint)value);
            return true;
        }

        public bool Equals(DeterministicSeed other) => Value == other.Value;
        public override bool Equals(object obj) => obj is DeterministicSeed other && Equals(other);
        public override int GetHashCode() => (int)Value;
        public override string ToString() => Value.ToString();
        public static bool operator ==(DeterministicSeed left, DeterministicSeed right) => left.Equals(right);
        public static bool operator !=(DeterministicSeed left, DeterministicSeed right) => !left.Equals(right);
    }
}
