using System;
using UnityEngine;

namespace SeaLion.Core.Definitions
{
    /// <summary>Stable, serialized identifier used to connect authored definitions.</summary>
    [Serializable]
    public struct StableId : IEquatable<StableId>
    {
        [SerializeField] private string value;

        public static StableId Empty => default;
        public string Value => value ?? string.Empty;
        public bool IsEmpty => string.IsNullOrEmpty(value);

        public StableId(string rawValue)
        {
            value = rawValue ?? string.Empty;
        }

        public static bool TryCreate(string rawValue, out StableId id)
        {
            if (!IsValid(rawValue))
            {
                id = Empty;
                return false;
            }

            id = new StableId(rawValue);
            return true;
        }

        public static bool IsValid(string rawValue)
        {
            if (string.IsNullOrEmpty(rawValue) || rawValue.Length > 64)
                return false;

            for (var i = 0; i < rawValue.Length; i++)
            {
                var c = rawValue[i];
                var valid = (c >= 'a' && c <= 'z') || (c >= '0' && c <= '9') || c == '-' || c == '_';
                if (!valid || (i == 0 && (c == '-' || c == '_')) ||
                    (i == rawValue.Length - 1 && (c == '-' || c == '_')))
                    return false;
            }
            return true;
        }

        public bool Equals(StableId other) => string.Equals(Value, other.Value, StringComparison.Ordinal);
        public override bool Equals(object obj) => obj is StableId other && Equals(other);
        public override int GetHashCode()
        {
            unchecked
            {
                const uint offsetBasis = 2166136261;
                const uint prime = 16777619;
                var hash = offsetBasis;
                var current = Value;

                for (var i = 0; i < current.Length; i++)
                {
                    hash ^= current[i];
                    hash *= prime;
                }

                return (int)hash;
            }
        }
        public override string ToString() => Value;
        public static bool operator ==(StableId left, StableId right) => left.Equals(right);
        public static bool operator !=(StableId left, StableId right) => !left.Equals(right);
    }
}
