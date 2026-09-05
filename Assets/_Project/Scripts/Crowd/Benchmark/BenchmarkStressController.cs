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
        [SerializeField] private Material benchmarkMaterial;
        [SerializeField] private bool cycleDevelopmentProfiles;
        [SerializeField, Min(5f)] private float developmentProfileDuration = 20f;
        private CrowdBuffers crowd;
        private NativeArray<byte> teams;
        private NativeArray<float> phases;
        private InstancedCrowdRenderer crowdRenderer;
        private Material runtimeMaterial;
        private float elapsed;
        private float profileElapsed;
        private bool profileCycled;
        private bool loggedFirstRender;

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
            Debug.Log($"[BenchmarkStress] profile={agentCount} seed={seed}");
        }

        private void Update()
        {
            elapsed += Time.deltaTime;
            profileElapsed += Time.unscaledDeltaTime;
            if (cycleDevelopmentProfiles && !profileCycled && profileElapsed >= developmentProfileDuration)
            {
                profileCycled = true;
                Build(agentCount == 300 ? 500 : 300);
                Debug.Log($"[BenchmarkStress] profile={agentCount} seed={seed}");
            }
            for (var i = 0; i < crowd.ActiveCount; i++)
            {
                var p = crowd.Positions[i];
                p.y = animate ? math.sin(elapsed * 3f + phases[i] * 6.28318f) * .08f : 0f;
                crowd.Positions[i] = p;
            }
            var rendered = crowdRenderer.Render(crowd, teams, phases);
            if (!loggedFirstRender)
            {
                loggedFirstRender = true;
                if (rendered == 0)
                    Debug.LogError("[BenchmarkStress] rendered=0; benchmark evidence is invalid.", this);
                Debug.Log($"[BenchmarkStress] rendered={rendered} " +
                    $"supportsInstancing={SystemInfo.supportsInstancing}");
            }
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
            ConfigureCamera();
        }

        private void EnsureRenderer()
        {
            if (crowdRenderer == null)
                crowdRenderer = gameObject.GetComponent<InstancedCrowdRenderer>() ??
                    gameObject.AddComponent<InstancedCrowdRenderer>();
            var mesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
            if (runtimeMaterial == null)
            {
                if (benchmarkMaterial != null)
                    runtimeMaterial = new Material(benchmarkMaterial);
                else
                {
                    var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
                    if (shader == null) return;
                    runtimeMaterial = new Material(shader);
                }
                runtimeMaterial.enableInstancing = true;
                runtimeMaterial.color = new Color(.12f, .55f, .82f);
            }
            crowdRenderer.Configure(mesh, runtimeMaterial);
            Debug.Log($"[BenchmarkStress] mesh={mesh?.name ?? "null"} " +
                $"shader={runtimeMaterial?.shader?.name ?? "null"} " +
                $"supported={runtimeMaterial != null && runtimeMaterial.shader != null && runtimeMaterial.shader.isSupported} " +
                $"instancing={runtimeMaterial != null && runtimeMaterial.enableInstancing} " +
                $"supportsInstancing={SystemInfo.supportsInstancing}");
        }

        private void ConfigureCamera()
        {
            var benchmarkCamera = FindFirstObjectByType<Camera>();
            if (benchmarkCamera == null) return;

            var columns = Mathf.Max(1, Mathf.CeilToInt(Mathf.Sqrt(agentCount)));
            var rows = Mathf.CeilToInt(agentCount / (float)columns);
            var width = Mathf.Max(spacing, (columns - 1) * spacing);
            var depth = Mathf.Max(spacing, (rows - 1) * spacing);
            var aspect = Mathf.Max(.25f, benchmarkCamera.aspect);
            var halfHeightForWidth = (width * .5f + 2f) / aspect;
            benchmarkCamera.transform.SetPositionAndRotation(
                new Vector3(width * .5f, 50f, depth * .5f),
                Quaternion.Euler(90f, 0f, 0f));
            benchmarkCamera.orthographic = true;
            benchmarkCamera.orthographicSize = Mathf.Max(depth * .5f + 2f, halfHeightForWidth);
            benchmarkCamera.nearClipPlane = .1f;
            benchmarkCamera.farClipPlane = 100f;
            benchmarkCamera.cullingMask = ~0;
            benchmarkCamera.clearFlags = CameraClearFlags.SolidColor;
            benchmarkCamera.backgroundColor = new Color(.035f, .07f, .12f, 1f);
            RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(.55f, .62f, .7f, 1f);
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
