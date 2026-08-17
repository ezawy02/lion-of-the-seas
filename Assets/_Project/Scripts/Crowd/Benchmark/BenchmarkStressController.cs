using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;
using SeaLion.Crowd.Rendering;
using SeaLion.Crowd.Simulation;

namespace SeaLion.Crowd.Benchmark
{
    /// <summary>Self-contained deterministic 300/500-agent benchmark harness.</summary>
    public sealed class BenchmarkStressController : MonoBehaviour
    {
        [SerializeField] private int agentCount = 300;
        [SerializeField] private uint seed = 2701;
        [SerializeField] private float spacing = 1.35f;
        [SerializeField] private bool animate = true;
        private CrowdBuffers crowd;
        private NativeArray<byte> teams;
        private NativeArray<float> phases;
        private InstancedCrowdRenderer renderer;
        private Material runtimeMaterial;
        private float elapsed;

        public int AgentCount { get { return agentCount; } }
        public uint Seed { get { return seed; } }

        public static float3 PositionFor(int index, int count, uint value, float cellSpacing)
        {
            var width = math.max(1, (int)math.ceil(math.sqrt(count)));
            var hash = value + (uint)index * 747796405u;
            var jitterX = ((hash ^ (hash >> 16)) & 255u) / 255f - .5f;
            var jitterZ = (((hash * 277803737u) >> 8) & 255u) / 255f - .5f;
            return new float3((index % width + jitterX) * cellSpacing, 0f,
                (index / width + jitterZ) * cellSpacing);
        }

        private void Awake()
        {
            agentCount = agentCount == 500 ? 500 : 300;
            Build(agentCount);
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            for (var i = 0; i < crowd.ActiveCount; i++)
            {
                var p = crowd.Positions[i];
                p.y = animate ? math.sin(elapsed * 3f + phases[i] * 6.28318f) * .08f : 0f;
                crowd.Positions[i] = p;
            }
            renderer.Render(crowd, teams, phases);
        }

        public void Build(int count)
        {
            DisposeBuffers();
            agentCount = count == 500 ? 500 : 300;
            crowd = new CrowdBuffers(agentCount, Allocator.Persistent);
            crowd.Initialize(agentCount);
            teams = new NativeArray<byte>(agentCount, Allocator.Persistent);
            phases = new NativeArray<float>(agentCount, Allocator.Persistent);
            for (var i = 0; i < agentCount; i++)
            {
                crowd.Positions[i] = PositionFor(i, agentCount, seed, spacing);
                crowd.Velocities[i] = new float3(0f, 0f, 1f);
                crowd.HealthOrHits[i] = 1f;
                crowd.States[i] = CrowdAgentState.Traversing;
                teams[i] = (byte)(i % 5 == 0 ? 1 : 0);
                phases[i] = math.frac((seed * .0001f) + i * .6180339f);
            }
            EnsureRenderer();
        }

        private void EnsureRenderer()
        {
            if (renderer == null) renderer = gameObject.GetComponent<InstancedCrowdRenderer>() ?? gameObject.AddComponent<InstancedCrowdRenderer>();
            var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            if (runtimeMaterial == null)
            {
                var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                if (shader == null) return;
                runtimeMaterial = new Material(shader) { enableInstancing = true };
                runtimeMaterial.color = new Color(.12f, .55f, .82f);
            }
            renderer.Configure(mesh, runtimeMaterial);
        }

        private void OnDestroy()
        {
            DisposeBuffers();
            if (runtimeMaterial != null) Destroy(runtimeMaterial);
        }
        private void DisposeBuffers()
        {
            if (crowd != null) crowd.Dispose();
            if (teams.IsCreated) teams.Dispose();
            if (phases.IsCreated) phases.Dispose();
            crowd = null;
        }
    }
}
