using System;
using Unity.Collections;
using Unity.Mathematics;
using SeaLion.Core.Definitions;

namespace SeaLion.Crowd.Simulation
{
    [Flags]
    public enum CrowdAgentFlags : byte
    {
        None = 0,
        GateProcessed = 1 << 0,
        HitQueued = 1 << 1,
        Dead = 1 << 2,
        LandingEligible = 1 << 3
    }

    public enum CrowdAgentState : byte
    {
        Deploying,
        Traversing,
        Landing,
        Fighting,
        Routed,
        Complete
    }

    /// <summary>Owns the simulation-only, structure-of-arrays state for ordinary agents.</summary>
    public sealed class CrowdBuffers : IDisposable
    {
        private int capacity;
        private bool initialized;
        private int logicalCount;
        private int activeCount;
        private int displayedAgentCount;

        public NativeArray<float3> Positions;
        public NativeArray<float3> Velocities;
        public NativeArray<float> HealthOrHits;
        public NativeArray<UnitRole> Roles;
        public NativeArray<CrowdAgentState> States;
        public NativeArray<CrowdAgentFlags> Flags;

        public int Capacity { get { return capacity; } }
        public int LogicalCount { get { EnsureInitialized(); return logicalCount; } }
        public int ActiveCount { get { EnsureInitialized(); return activeCount; } }
        public int DisplayedAgentCount { get { EnsureInitialized(); return displayedAgentCount; } }
        public bool IsCreated { get { return initialized; } }
        public bool IsDisposed { get { return !initialized; } }

        // Compatibility aliases for callers that use the data-model terminology.
        public NativeArray<float> Health { get { return HealthOrHits; } }
        public NativeArray<float> Hits { get { return HealthOrHits; } }

        public CrowdBuffers(int capacity, Allocator allocator)
        {
            if (capacity < 0) throw new ArgumentOutOfRangeException(nameof(capacity));
            if (allocator == Allocator.Invalid || allocator == Allocator.None)
                throw new ArgumentException("A concrete allocator is required.", nameof(allocator));

            this.capacity = capacity;
            initialized = true;
            logicalCount = 0;
            activeCount = 0;
            displayedAgentCount = 0;
            Positions = default;
            Velocities = default;
            HealthOrHits = default;
            Roles = default;
            States = default;
            Flags = default;

            try
            {
                if (capacity == 0) return;
                Positions = new NativeArray<float3>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
                Velocities = new NativeArray<float3>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
                HealthOrHits = new NativeArray<float>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
                Roles = new NativeArray<UnitRole>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
                States = new NativeArray<CrowdAgentState>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
                Flags = new NativeArray<CrowdAgentFlags>(capacity, allocator, NativeArrayOptions.UninitializedMemory);
                ClearStorage();
            }
            catch
            {
                Dispose();
                throw;
            }
        }

        public void Initialize(int count)
        {
            EnsureInitialized();
            ValidateLogicalCount(count, nameof(count));
            ClearStorage();
            logicalCount = count;
            activeCount = Math.Min(count, capacity);
            displayedAgentCount = activeCount;
        }

        public void Reset(int count = 0)
        {
            Initialize(count);
        }

        public void SetLogicalCount(int count)
        {
            EnsureInitialized();
            ValidateLogicalCount(count, nameof(count));
            logicalCount = count;
            if (activeCount > count) activeCount = count;
            if (displayedAgentCount > count) displayedAgentCount = count;
        }

        public void SetActiveCount(int count)
        {
            EnsureInitialized();
            ValidateCount(count, nameof(count));
            if (count > logicalCount) throw new ArgumentOutOfRangeException(nameof(count), "Active count cannot exceed logical count.");
            activeCount = count;
            if (displayedAgentCount > count) displayedAgentCount = count;
        }

        public void SetDisplayedAgentCount(int count)
        {
            EnsureInitialized();
            ValidateCount(count, nameof(count));
            if (count > activeCount) throw new ArgumentOutOfRangeException(nameof(count), "Displayed count cannot exceed active count.");
            displayedAgentCount = count;
        }

        public void ValidateIndex(int index)
        {
            EnsureInitialized();
            if ((uint)index >= (uint)capacity) throw new ArgumentOutOfRangeException(nameof(index));
        }

        public void Dispose()
        {
            DisposeArray(ref Positions);
            DisposeArray(ref Velocities);
            DisposeArray(ref HealthOrHits);
            DisposeArray(ref Roles);
            DisposeArray(ref States);
            DisposeArray(ref Flags);
            capacity = 0;
            logicalCount = 0;
            activeCount = 0;
            displayedAgentCount = 0;
            initialized = false;
        }

        private void EnsureInitialized()
        {
            if (!initialized) throw new ObjectDisposedException(nameof(CrowdBuffers));
        }

        private void ValidateCount(int count, string parameterName)
        {
            if (count < 0 || count > capacity) throw new ArgumentOutOfRangeException(parameterName);
        }

        private static void ValidateLogicalCount(int count, string parameterName)
        {
            if (count < 0) throw new ArgumentOutOfRangeException(parameterName);
        }

        private void ClearStorage()
        {
            for (var i = 0; i < capacity; i++)
            {
                Positions[i] = default;
                Velocities[i] = default;
                HealthOrHits[i] = default;
                Roles[i] = default;
                States[i] = default;
                Flags[i] = default;
            }
        }

        private static void DisposeArray<T>(ref NativeArray<T> array) where T : struct
        {
            if (array.IsCreated) array.Dispose();
            array = default;
        }
    }
}
