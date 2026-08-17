using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using SeaLion.Crowd.Simulation;

namespace SeaLion.Crowd.Spatial
{
    /// <summary>Deterministic, exact cell mapping used by both rebuild and query jobs.</summary>
    public static class SpatialGrid
    {
        public static int3 CellFor(float3 position, float cellSize)
        {
            if (!math.all(math.isfinite(position)) || !math.isfinite(cellSize) || cellSize <= 0f)
                return int3.zero;
            var cell = math.floor(position / cellSize);
            return math.all(math.isfinite(cell)) ? (int3)cell : int3.zero;
        }

        public static bool IsValidCellSize(float cellSize)
        {
            return math.isfinite(cellSize) && cellSize > 0f;
        }
    }

    /// <summary>Clears a grid as an explicit dependency before any parallel writes.</summary>
    [BurstCompile]
    public struct SpatialGridClearJob : IJob
    {
        public NativeParallelMultiHashMap<int3, int> Cells;

        public void Execute()
        {
            Cells.Clear();
        }
    }

    /// <summary>Writes each live simulation slot to exactly one integer cell.</summary>
    [BurstCompile]
    public struct SpatialGridRebuildJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public NativeArray<CrowdAgentFlags> Flags;
        [ReadOnly] public NativeArray<CrowdAgentState> States;
        [ReadOnly] public NativeArray<byte> Active;
        public NativeParallelMultiHashMap<int3, int>.ParallelWriter Cells;
        public float CellSize;

        public void Execute(int index)
        {
            if (!SpatialGrid.IsValidCellSize(CellSize) || !math.all(math.isfinite(Positions[index]))) return;
            if (Active.IsCreated && (index >= Active.Length || Active[index] == 0)) return;
            if ((Flags[index] & CrowdAgentFlags.Dead) != 0 ||
                States[index] == CrowdAgentState.Routed || States[index] == CrowdAgentState.Complete) return;
            Cells.Add(SpatialGrid.CellFor(Positions[index], CellSize), index);
        }
    }

    /// <summary>Queries a bounded 3x3x3 neighborhood into fixed caller-owned output slices.</summary>
    [BurstCompile]
    public struct SpatialGridQueryJob : IJobParallelFor
    {
        [ReadOnly] public NativeArray<float3> QueryPositions;
        [ReadOnly] public NativeParallelMultiHashMap<int3, int> Cells;
        // Each query owns a disjoint fixed-size slice of this shared array.
        [NativeDisableParallelForRestriction] public NativeArray<int> Results;
        public NativeArray<int> Counts;
        public float CellSize;
        public int MaxResultsPerQuery;

        public void Execute(int queryIndex)
        {
            var outputStart = queryIndex * math.max(0, MaxResultsPerQuery);
            if (MaxResultsPerQuery <= 0 || !SpatialGrid.IsValidCellSize(CellSize) ||
                !math.all(math.isfinite(QueryPositions[queryIndex])))
            {
                Counts[queryIndex] = 0;
                return;
            }

            var count = 0;
            var center = SpatialGrid.CellFor(QueryPositions[queryIndex], CellSize);
            for (var z = -1; z <= 1; z++)
            for (var y = -1; y <= 1; y++)
            for (var x = -1; x <= 1; x++)
            {
                int value;
                NativeParallelMultiHashMapIterator<int3> iterator;
                var key = center + new int3(x, y, z);
                if (!Cells.TryGetFirstValue(key, out value, out iterator)) continue;
                do
                {
                    InsertSorted(Results, outputStart, ref count, MaxResultsPerQuery, value);
                } while (Cells.TryGetNextValue(out value, ref iterator));
            }

            Counts[queryIndex] = count;
        }

        private static void InsertSorted(NativeArray<int> output, int start, ref int count, int capacity, int value)
        {
            var insertAt = count;
            if (count == capacity)
            {
                if (value >= output[start + count - 1]) return;
                insertAt = count - 1;
            }
            else count++;
            while (insertAt > 0 && output[start + insertAt - 1] > value)
            {
                output[start + insertAt] = output[start + insertAt - 1];
                insertAt--;
            }
            output[start + insertAt] = value;
        }
    }
}
