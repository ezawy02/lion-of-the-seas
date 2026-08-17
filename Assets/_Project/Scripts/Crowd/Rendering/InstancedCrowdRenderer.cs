using System;
using Unity.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using SeaLion.Crowd.Simulation;

namespace SeaLion.Crowd.Rendering
{
    /// <summary>Draws ordinary agents in allocation-free GPU-instanced batches.</summary>
    public sealed class InstancedCrowdRenderer : MonoBehaviour
    {
        public const int MaxInstancesPerBatch = 1023;

        private static readonly int TeamColorId = Shader.PropertyToID("_TeamColor");
        private static readonly int AgentStateId = Shader.PropertyToID("_AgentState");
        private static readonly int AnimationPhaseId = Shader.PropertyToID("_AnimationPhase");

        [SerializeField] private Mesh mesh;
        [SerializeField] private Material material;
        [SerializeField] private Color friendlyColor = new Color(0.12f, 0.65f, 1f, 1f);
        [SerializeField] private Color hostileColor = new Color(0.95f, 0.2f, 0.12f, 1f);
        [SerializeField] private Vector3 instanceScale = Vector3.one;
        [SerializeField] private bool castShadows;

        private readonly Matrix4x4[] matrices = new Matrix4x4[MaxInstancesPerBatch];
        private readonly Vector4[] teamColors = new Vector4[MaxInstancesPerBatch];
        private readonly float[] states = new float[MaxInstancesPerBatch];
        private readonly float[] phases = new float[MaxInstancesPerBatch];
        private MaterialPropertyBlock properties;

        public Mesh Mesh { get { return mesh; } }
        public Material Material { get { return material; } }

        public void Configure(Mesh sharedMesh, Material sharedMaterial)
        {
            mesh = sharedMesh;
            material = sharedMaterial;
        }

        public int Render(
            CrowdBuffers crowd,
            NativeArray<byte> teams,
            NativeArray<float> animationPhases)
        {
            if (crowd == null) throw new ArgumentNullException(nameof(crowd));
            if (!crowd.IsCreated || crowd.DisplayedAgentCount == 0) return 0;
            if (!teams.IsCreated || teams.Length < crowd.DisplayedAgentCount)
                throw new ArgumentException("Team data must cover every displayed agent.", nameof(teams));
            if (!animationPhases.IsCreated || animationPhases.Length < crowd.DisplayedAgentCount)
                throw new ArgumentException("Animation data must cover every displayed agent.", nameof(animationPhases));
            if (mesh == null || material == null || !material.enableInstancing) return 0;

            if (properties == null) properties = new MaterialPropertyBlock();
            var rendered = 0;
            while (rendered < crowd.DisplayedAgentCount)
            {
                var batchCount = Math.Min(MaxInstancesPerBatch, crowd.DisplayedAgentCount - rendered);
                FillBatch(crowd, teams, animationPhases, rendered, batchCount);
                properties.Clear();
                properties.SetVectorArray(TeamColorId, teamColors);
                properties.SetFloatArray(AgentStateId, states);
                properties.SetFloatArray(AnimationPhaseId, phases);
                Graphics.DrawMeshInstanced(
                    mesh, 0, material, matrices, batchCount, properties,
                    castShadows ? ShadowCastingMode.On : ShadowCastingMode.Off,
                    false, gameObject.layer, null, LightProbeUsage.Off);
                rendered += batchCount;
            }
            return rendered;
        }

        public static float NormalizePhase(float phase)
        {
            if (float.IsNaN(phase) || float.IsInfinity(phase)) return 0f;
            return phase - Mathf.Floor(phase);
        }

        private void FillBatch(
            CrowdBuffers crowd,
            NativeArray<byte> teams,
            NativeArray<float> animationPhases,
            int start,
            int count)
        {
            for (var batchIndex = 0; batchIndex < count; batchIndex++)
            {
                var index = start + batchIndex;
                var position = crowd.Positions[index];
                var velocity = crowd.Velocities[index];
                var direction = new Vector3(velocity.x, 0f, velocity.z);
                var rotation = direction.sqrMagnitude > 0.0001f
                    ? Quaternion.LookRotation(direction.normalized, Vector3.up)
                    : Quaternion.identity;
                matrices[batchIndex] = Matrix4x4.TRS(
                    new Vector3(position.x, position.y, position.z), rotation, instanceScale);
                teamColors[batchIndex] = teams[index] == 0 ? friendlyColor : hostileColor;
                states[batchIndex] = (float)crowd.States[index];
                phases[batchIndex] = NormalizePhase(animationPhases[index]);
            }
        }
    }
}
