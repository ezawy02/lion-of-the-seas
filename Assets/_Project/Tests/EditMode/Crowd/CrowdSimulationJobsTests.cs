using NUnit.Framework;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using SeaLion.Crowd.Simulation;
using SeaLion.Crowd.Simulation.Jobs;

namespace SeaLion.Tests.EditMode.Crowd
{
    public sealed class CrowdSimulationJobsTests
    {
        [Test]
        public void FormationTargetsRepeatExactlyForSameSeed()
        {
            var first = new NativeArray<float3>(12, Allocator.TempJob);
            var second = new NativeArray<float3>(12, Allocator.TempJob);
            try
            {
                var layout = new FormationLayout { Origin = new float3(2, 0, 3), Forward = new float3(0, 0, 1),
                    Columns = 4, Spacing = 1.5f, RowSpacing = 2f, Jitter = .1f, Seed = 42 };
                new FormationTargetGenerationJob { Targets = first, Layout = layout }.Schedule(first.Length, 32).Complete();
                new FormationTargetGenerationJob { Targets = second, Layout = layout }.Schedule(second.Length, 32).Complete();
                for (var i = 0; i < first.Length; i++) Assert.That(second[i], Is.EqualTo(first[i]));
            }
            finally { first.Dispose(); second.Dispose(); }
        }

        [Test]
        public void FormationSanitizesInvalidLayoutValues()
        {
            var targets = new NativeArray<float3>(2, Allocator.TempJob);
            try
            {
                var layout = new FormationLayout
                {
                    Origin = new float3(float.NaN),
                    Forward = new float3(float.PositiveInfinity),
                    Columns = 0,
                    Spacing = -1f,
                    RowSpacing = float.NaN,
                    Jitter = -1f
                };
                new FormationTargetGenerationJob { Targets = targets, Layout = layout }
                    .Schedule(targets.Length, 1).Complete();
                Assert.That(math.all(math.isfinite(targets[0])), Is.True);
                Assert.That(math.all(math.isfinite(targets[1])), Is.True);
            }
            finally { targets.Dispose(); }
        }

        [Test]
        public void MovementIsFixedStepAndTerminalAgentsDoNotMove()
        {
            var positions = new NativeArray<float3>(2, Allocator.TempJob);
            var velocities = new NativeArray<float3>(2, Allocator.TempJob);
            var targets = new NativeArray<float3>(2, Allocator.TempJob);
            var states = new NativeArray<CrowdAgentState>(2, Allocator.TempJob);
            var flags = new NativeArray<CrowdAgentFlags>(2, Allocator.TempJob);
            try
            {
                positions[0] = positions[1] = float3.zero; targets[0] = targets[1] = new float3(10, 0, 0);
                states[0] = CrowdAgentState.Traversing; states[1] = CrowdAgentState.Complete;
                new CrowdMovementIntegrationJob { Positions = positions, Velocities = velocities, Targets = targets,
                    States = states, Flags = flags, DeltaTime = 1f / 60f, MaxSpeed = 6f, Acceleration = 60f }
                    .Schedule(2, 1).Complete();
                Assert.That(positions[0].x, Is.GreaterThan(0f));
                Assert.That(positions[1], Is.EqualTo(float3.zero));
                Assert.That(velocities[1], Is.EqualTo(float3.zero));
                var firstStep = positions[0];
                positions[0] = positions[1] = float3.zero;
                velocities[0] = velocities[1] = float3.zero;
                new CrowdMovementIntegrationJob { Positions = positions, Velocities = velocities, Targets = targets,
                    States = states, Flags = flags, DeltaTime = 1f / 60f, MaxSpeed = 6f, Acceleration = 60f }
                    .Schedule(2, 1).Complete();
                Assert.That(positions[0], Is.EqualTo(firstStep));
            }
            finally { positions.Dispose(); velocities.Dispose(); targets.Dispose(); states.Dispose(); flags.Dispose(); }
        }

        [Test]
        public void StateTransitionsAreExplicitAndDeadStateWins()
        {
            var states = new NativeArray<CrowdAgentState>(2, Allocator.TempJob);
            var positions = new NativeArray<float3>(2, Allocator.TempJob);
            var targets = new NativeArray<float3>(2, Allocator.TempJob);
            var health = new NativeArray<float>(2, Allocator.TempJob);
            var flags = new NativeArray<CrowdAgentFlags>(2, Allocator.TempJob);
            try
            {
                states[0] = CrowdAgentState.Traversing; states[1] = CrowdAgentState.Fighting;
                health[0] = health[1] = 1f; positions[0] = positions[1] = float3.zero; targets[0] = targets[1] = float3.zero;
                flags[1] = CrowdAgentFlags.Dead;
                new CrowdStateTransitionJob { States = states, Positions = positions, Targets = targets, HealthOrHits = health,
                    Flags = flags, Rules = new CrowdStateTransitionRules { ArrivalDistance = .5f, LandingDistance = 1f, CompleteDistance = 2f } }
                    .Schedule(2, 1).Complete();
                Assert.That(states[0], Is.EqualTo(CrowdAgentState.Landing));
                Assert.That(states[1], Is.EqualTo(CrowdAgentState.Routed));
            }
            finally { states.Dispose(); positions.Dispose(); targets.Dispose(); health.Dispose(); flags.Dispose(); }
        }
    }
}
