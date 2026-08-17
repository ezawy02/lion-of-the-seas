using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace SeaLion.Crowd.Simulation.Jobs
{
    /// <summary>Immutable inputs for a deterministic row-major formation.</summary>
    public struct FormationLayout
    {
        public float3 Origin;
        public float3 Forward;
        public float Spacing;
        public float RowSpacing;
        public int Columns;
        public float Jitter;
        public uint Seed;

        public float3 PositionFor(int index)
        {
            var columns = math.max(1, Columns);
            var row = index / columns;
            var column = index - row * columns;
            var origin = math.all(math.isfinite(Origin)) ? Origin : float3.zero;
            var safeForward = math.all(math.isfinite(Forward)) ? Forward : new float3(0f, 0f, 1f);
            var forward = math.normalizesafe(safeForward, new float3(0f, 0f, 1f));
            var side = math.normalizesafe(math.cross(new float3(0f, 1f, 0f), forward), new float3(1f, 0f, 0f));
            var centered = column - (columns - 1) * 0.5f;
            var jitter = Hash01((uint)index + Seed) * 2f - 1f;
            var spacing = math.max(0f, math.isfinite(Spacing) ? Spacing : 0f);
            var rowSpacing = math.max(0f, math.isfinite(RowSpacing) ? RowSpacing : 0f);
            var jitterAmount = math.max(0f, math.isfinite(Jitter) ? Jitter : 0f);
            return origin + forward * (row * rowSpacing) + side * (centered * spacing + jitter * jitterAmount);
        }

        private static float Hash01(uint value)
        {
            value ^= value >> 16;
            value *= 0x7feb352du;
            value ^= value >> 15;
            value *= 0x846ca68bu;
            value ^= value >> 16;
            return (value & 0x00ffffffu) / 16777215f;
        }
    }

    [BurstCompile]
    public struct FormationTargetGenerationJob : IJobParallelFor
    {
        [WriteOnly] public NativeArray<float3> Targets;
        public FormationLayout Layout;

        public void Execute(int index)
        {
            var target = Layout.PositionFor(index);
            Targets[index] = math.all(math.isfinite(target)) ? target : float3.zero;
        }
    }

    /// <summary>Integrates one authoritative fixed step. Each index owns its output slot.</summary>
    [BurstCompile]
    public struct CrowdMovementIntegrationJob : IJobParallelFor
    {
        public NativeArray<float3> Positions;
        public NativeArray<float3> Velocities;
        [ReadOnly] public NativeArray<float3> Targets;
        [ReadOnly] public NativeArray<CrowdAgentState> States;
        [ReadOnly] public NativeArray<CrowdAgentFlags> Flags;
        public float DeltaTime;
        public float MaxSpeed;
        public float Acceleration;

        public void Execute(int index)
        {
            if ((Flags[index] & CrowdAgentFlags.Dead) != 0 ||
                States[index] == CrowdAgentState.Routed || States[index] == CrowdAgentState.Complete)
            {
                Positions[index] = FiniteOr(Positions[index], float3.zero);
                Velocities[index] = float3.zero;
                return;
            }

            var position = FiniteOr(Positions[index], float3.zero);
            var velocity = FiniteOr(Velocities[index], float3.zero);
            var target = FiniteOr(Targets[index], position);
            var dt = math.max(0f, math.isfinite(DeltaTime) ? DeltaTime : 0f);
            var speed = math.max(0f, math.isfinite(MaxSpeed) ? MaxSpeed : 0f);
            var acceleration = math.max(0f, math.isfinite(Acceleration) ? Acceleration : 0f);
            var desired = math.normalizesafe(target - position) * speed;
            velocity = math.lerp(velocity, desired, math.saturate(acceleration * dt));
            velocity = math.clamp(velocity, new float3(-speed), new float3(speed));
            Positions[index] = position + velocity * dt;
            Velocities[index] = velocity;
        }

        private static float3 FiniteOr(float3 value, float3 fallback)
        {
            return math.all(math.isfinite(value)) ? value : fallback;
        }
    }

    public struct CrowdStateTransitionRules
    {
        public float ArrivalDistance;
        public float LandingDistance;
        public float CompleteDistance;
    }

    [BurstCompile]
    public struct CrowdStateTransitionJob : IJobParallelFor
    {
        public NativeArray<CrowdAgentState> States;
        [ReadOnly] public NativeArray<float3> Positions;
        [ReadOnly] public NativeArray<float3> Targets;
        [ReadOnly] public NativeArray<float> HealthOrHits;
        [ReadOnly] public NativeArray<CrowdAgentFlags> Flags;
        public CrowdStateTransitionRules Rules;

        public void Execute(int index)
        {
            var state = States[index];
            var health = HealthOrHits[index];
            if ((Flags[index] & CrowdAgentFlags.Dead) != 0 || !math.isfinite(health) || health <= 0f)
            {
                States[index] = CrowdAgentState.Routed;
                return;
            }

            var distance = math.distance(Positions[index], Targets[index]);
            if (!math.isfinite(distance)) return;
            var arrival = math.max(0f, math.isfinite(Rules.ArrivalDistance) ? Rules.ArrivalDistance : 0f);
            var landingValue = math.max(0f, math.isfinite(Rules.LandingDistance) ? Rules.LandingDistance : 0f);
            var completeValue = math.max(0f, math.isfinite(Rules.CompleteDistance) ? Rules.CompleteDistance : 0f);
            var landing = math.max(arrival, landingValue);
            var complete = math.max(landing, completeValue);
            switch (state)
            {
                case CrowdAgentState.Deploying:
                    if (distance <= complete) States[index] = CrowdAgentState.Traversing;
                    break;
                case CrowdAgentState.Traversing:
                    if (distance <= landing) States[index] = CrowdAgentState.Landing;
                    break;
                case CrowdAgentState.Landing:
                    if (distance <= arrival) States[index] = CrowdAgentState.Fighting;
                    break;
                case CrowdAgentState.Fighting:
                    if (distance <= arrival) States[index] = CrowdAgentState.Complete;
                    break;
            }
        }
    }
}
