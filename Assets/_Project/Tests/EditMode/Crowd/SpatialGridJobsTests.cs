using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using SeaLion.Crowd.Simulation;
using SeaLion.Crowd.Spatial;

namespace SeaLion.Tests.EditMode.Crowd
{
    public sealed class SpatialGridJobsTests
    {
        [Test]
        public void CellMappingUsesFloorForNegativeCoordinates()
        {
            Assert.That(SpatialGrid.CellFor(new float3(-0.01f, 0f, -2.01f), 2f), Is.EqualTo(new int3(-1, 0, -2)));
            Assert.That(SpatialGrid.CellFor(new float3(3.99f, 0f, 4f), 2f), Is.EqualTo(new int3(1, 0, 2)));
        }

        [Test]
        public void RebuildAndQueryReturnOnlyTheBoundedNeighborhood()
        {
            var positions = new NativeArray<float3>(4, Allocator.TempJob);
            var flags = new NativeArray<CrowdAgentFlags>(4, Allocator.TempJob);
            var states = new NativeArray<CrowdAgentState>(4, Allocator.TempJob);
            var active = new NativeArray<byte>(4, Allocator.TempJob);
            var queries = new NativeArray<float3>(1, Allocator.TempJob);
            var results = new NativeArray<int>(1, Allocator.TempJob);
            var counts = new NativeArray<int>(1, Allocator.TempJob);
            var cells = new NativeParallelMultiHashMap<int3, int>(4, Allocator.TempJob);
            try
            {
                positions[0] = new float3(0.1f, 0f, 0.1f);
                positions[1] = new float3(1.9f, 0f, 0.1f);
                positions[2] = new float3(4.1f, 0f, 0.1f);
                positions[3] = new float3(-2.1f, 0f, 0.1f);
                for (var i = 0; i < active.Length; i++) active[i] = 1;
                queries[0] = float3.zero;
                var writer = cells.AsParallelWriter();
                var clear = new SpatialGridClearJob { Cells = cells }.Schedule();
                clear.Complete();
                var rebuild = new SpatialGridRebuildJob { Positions = positions, Flags = flags, States = states,
                    Active = active, Cells = writer, CellSize = 2f }.Schedule(4, 1);
                var query = new SpatialGridQueryJob { QueryPositions = queries, Cells = cells, Results = results, Counts = counts,
                    CellSize = 2f, MaxResultsPerQuery = 1 }.Schedule(1, 1, rebuild);
                query.Complete();
                Assert.That(counts[0], Is.EqualTo(1));
                Assert.That(results[0], Is.EqualTo(0));
            }
            finally { positions.Dispose(); flags.Dispose(); states.Dispose(); active.Dispose(); queries.Dispose(); results.Dispose(); counts.Dispose(); cells.Dispose(); }
        }

        [Test]
        public void RepeatedQueriesHaveStableIndexOrderingAndExcludeDeadInactiveAgents()
        {
            var positions = new NativeArray<float3>(3, Allocator.TempJob);
            var flags = new NativeArray<CrowdAgentFlags>(3, Allocator.TempJob);
            var states = new NativeArray<CrowdAgentState>(3, Allocator.TempJob);
            var active = new NativeArray<byte>(3, Allocator.TempJob);
            var queries = new NativeArray<float3>(2, Allocator.TempJob);
            var results = new NativeArray<int>(6, Allocator.TempJob);
            var counts = new NativeArray<int>(2, Allocator.TempJob);
            var cells = new NativeParallelMultiHashMap<int3, int>(3, Allocator.TempJob);
            try
            {
                positions[0] = new float3(0.1f, 0f, 0.1f); positions[1] = positions[0]; positions[2] = positions[0];
                flags[1] = CrowdAgentFlags.Dead; active[0] = 1; active[1] = 1; active[2] = 0;
                queries[0] = queries[1] = float3.zero;
                var writer = cells.AsParallelWriter();
                var clear = new SpatialGridClearJob { Cells = cells }.Schedule();
                clear.Complete();
                var rebuild = new SpatialGridRebuildJob { Positions = positions, Flags = flags, States = states,
                    Active = active, Cells = writer, CellSize = 1f }.Schedule(3, 1);
                new SpatialGridQueryJob { QueryPositions = queries, Cells = cells, Results = results, Counts = counts,
                    CellSize = 1f, MaxResultsPerQuery = 3 }.Schedule(2, 1, rebuild).Complete();
                Assert.That(counts[0], Is.EqualTo(1)); Assert.That(results[0], Is.EqualTo(0));
                Assert.That(counts[1], Is.EqualTo(1)); Assert.That(results[3], Is.EqualTo(0));
            }
            finally { positions.Dispose(); flags.Dispose(); states.Dispose(); active.Dispose(); queries.Dispose(); results.Dispose(); counts.Dispose(); cells.Dispose(); }
        }

        [Test]
        public void InvalidCellSizeAndPositionProduceNoResults()
        {
            var positions = new NativeArray<float3>(1, Allocator.TempJob); var flags = new NativeArray<CrowdAgentFlags>(1, Allocator.TempJob);
            var states = new NativeArray<CrowdAgentState>(1, Allocator.TempJob); var queries = new NativeArray<float3>(1, Allocator.TempJob);
            var active = new NativeArray<byte>(1, Allocator.TempJob);
            var results = new NativeArray<int>(1, Allocator.TempJob); var counts = new NativeArray<int>(1, Allocator.TempJob);
            var cells = new NativeParallelMultiHashMap<int3, int>(1, Allocator.TempJob);
            try
            {
                positions[0] = float3.zero; active[0] = 1; queries[0] = new float3(float.NaN);
                var writer = cells.AsParallelWriter();
                var clear = new SpatialGridClearJob { Cells = cells }.Schedule();
                clear.Complete();
                var rebuild = new SpatialGridRebuildJob { Positions = positions, Flags = flags, States = states,
                    Active = active, Cells = writer, CellSize = 0f }.Schedule(1, 1);
                new SpatialGridQueryJob { QueryPositions = queries, Cells = cells, Results = results, Counts = counts,
                    CellSize = -1f, MaxResultsPerQuery = 1 }.Schedule(1, 1, rebuild).Complete();
                Assert.That(counts[0], Is.Zero);
            }
            finally { positions.Dispose(); flags.Dispose(); states.Dispose(); active.Dispose(); queries.Dispose(); results.Dispose(); counts.Dispose(); cells.Dispose(); }
        }
    }
}
