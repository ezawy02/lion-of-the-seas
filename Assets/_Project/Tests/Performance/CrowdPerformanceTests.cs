using System;
using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.PerformanceTesting;
using SeaLion.Crowd.Benchmark;
using SeaLion.Crowd.Simulation;
using SeaLion.Crowd.Simulation.Jobs;

namespace SeaLion.Tests.Performance
{
    /// <summary>Performance evidence for the deterministic crowd stress contract.</summary>
    public sealed class CrowdPerformanceTests
    {
        [Test, Performance]
        public void Primary_300Agents_AllocationFreeUpdatePath()
        {
            using (var crowd = CreateCrowd(300))
            using (var targets = CreateTargets(crowd))
            {
                Assert.That(crowd.LogicalCount, Is.EqualTo(300));
                Assert.That(crowd.ActiveCount, Is.EqualTo(300));
                Measure.Method(() => StepAgents(crowd, targets))
                    .WarmupCount(5).MeasurementCount(20).GC().Run();
            }
        }

        [Test, Performance]
        public void Floor_500Agents_AllocationFreeUpdatePath()
        {
            using (var crowd = CreateCrowd(500))
            using (var targets = CreateTargets(crowd))
            {
                Assert.That(crowd.LogicalCount, Is.EqualTo(500));
                Assert.That(crowd.ActiveCount, Is.EqualTo(500));
                Measure.Method(() => StepAgents(crowd, targets))
                    .WarmupCount(5).MeasurementCount(20).GC().Run();
            }
        }

        [Test, Performance]
        public void ReusedCrowdBuffers_DoNotCreateManagedAllocationSpikes()
        {
            using (var crowd = CreateCrowd(500))
            using (var targets = CreateTargets(crowd))
            {
                StepAgents(crowd, targets);
                var before = GC.GetAllocatedBytesForCurrentThread();
                StepAgents(crowd, targets);
                var allocated = GC.GetAllocatedBytesForCurrentThread() - before;
                Assert.That(allocated, Is.EqualTo(0), "The warmed update path must not allocate managed memory.");
            }
        }

        [Test]
        public void ReducedPresentationCap_PreservesLogicalOutcomeInputs()
        {
            using (var primary = CreateCrowd(300))
            using (var reduced = CreateCrowd(300))
            using (var primaryTargets = CreateTargets(primary))
            using (var reducedTargets = CreateTargets(reduced))
            {
                reduced.SetDisplayedAgentCount(120);
                for (var step = 0; step < 10; step++)
                {
                    StepAgents(primary, primaryTargets);
                    StepAgents(reduced, reducedTargets);
                }
                Assert.That(reduced.LogicalCount, Is.EqualTo(primary.LogicalCount));
                Assert.That(reduced.ActiveCount, Is.EqualTo(primary.ActiveCount));
                Assert.That(reduced.DisplayedAgentCount, Is.LessThan(primary.DisplayedAgentCount));
                for (var i = 0; i < primary.ActiveCount; i++)
                {
                    Assert.That(reduced.States[i], Is.EqualTo(primary.States[i]));
                    Assert.That(math.distance(reduced.Positions[i], primary.Positions[i]), Is.LessThan(0.00001f));
                }
            }
        }

        private static CrowdBuffers CreateCrowd(int count)
        {
            var crowd = new CrowdBuffers(count, Allocator.TempJob);
            crowd.Initialize(count);
            for (var i = 0; i < count; i++)
            {
                crowd.Positions[i] = BenchmarkStressController.PositionFor(i, count, 2701, 1.35f);
                crowd.States[i] = CrowdAgentState.Traversing;
            }
            return crowd;
        }

        private static NativeArray<float3> CreateTargets(CrowdBuffers crowd)
        {
            var targets = new NativeArray<float3>(crowd.ActiveCount, Allocator.TempJob);
            for (var i = 0; i < crowd.ActiveCount; i++)
                targets[i] = crowd.Positions[i] + new float3(0f, 0f, 10f);
            return targets;
        }

        private static void StepAgents(CrowdBuffers crowd, NativeArray<float3> targets)
        {
            new CrowdMovementIntegrationJob
            {
                Positions = crowd.Positions,
                Velocities = crowd.Velocities,
                Targets = targets,
                States = crowd.States,
                Flags = crowd.Flags,
                DeltaTime = 1f / 60f,
                MaxSpeed = 4f,
                Acceleration = 8f
            }.Schedule(crowd.ActiveCount, 64).Complete();
        }
    }
}
