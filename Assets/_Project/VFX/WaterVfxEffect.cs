using System;
using UnityEngine;

namespace SeaLion.Presentation.Vfx
{
    public enum WaterVfxKind : byte
    {
        Wake,
        FoamPatch,
        LandingSplash,
        HitSplash,
        BossReaction
    }

    /// <summary>
    /// Pooled-ready, allocation-free-at-play water effect. The mesh is authored deterministically
    /// once per prefab instance; Play only updates transforms and a MaterialPropertyBlock.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class WaterVfxEffect : MonoBehaviour
    {
        [SerializeField] private WaterVfxKind kind = WaterVfxKind.FoamPatch;
        [SerializeField, Min(0.05f)] private float lifetime = 1.1f;
        [SerializeField, Min(0.01f)] private float maxScale = 1f;
        [SerializeField, Range(0f, 1f)] private float density = 1f;
        [SerializeField] private bool reduced;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock properties;
        private Mesh generatedMesh;
        private WaterVfxKind builtKind;
        private bool meshBuilt;
        private Action<WaterVfxEffect> releaseToPool;
        private float elapsed;
        private float intensity = 1f;
        private bool playing;

        public WaterVfxKind Kind => kind;
        public bool IsPlaying => playing;
        public float Lifetime => lifetime;

        private void OnEnable()
        {
            CacheComponents();
            EnsureMesh();
            if (!Application.isPlaying) ApplyVisualProperties(0f, 1f);
        }

        private void OnValidate()
        {
            lifetime = Mathf.Max(0.05f, lifetime);
            maxScale = Mathf.Max(0.01f, maxScale);
            density = Mathf.Clamp01(density);
            CacheComponents();
            EnsureMesh();
            ApplyVisualProperties(0f, 1f);
        }

        private void Update()
        {
            if (!playing || !Application.isPlaying) return;
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / lifetime);
            var eased = 1f - Mathf.Pow(1f - progress, 2f);
            transform.localScale = Vector3.one * Mathf.Lerp(StartScale(), maxScale, eased);
            ApplyVisualProperties(progress, 1f - progress);
            if (progress >= 1f) ReturnToPool();
        }

        /// <summary>Sets the callback used by the presentation pool before the effect is activated.</summary>
        public void SetPoolRelease(Action<WaterVfxEffect> callback) => releaseToPool = callback;

        /// <summary>Applies Primary/Reduced presentation density without affecting simulation.</summary>
        public void SetQuality(float vfxDensity, bool useReduced)
        {
            density = Mathf.Clamp01(vfxDensity);
            reduced = useReduced;
            if (!playing) ApplyVisualProperties(0f, 1f);
        }

        public void Play(Vector3 worldPosition, Quaternion worldRotation, float effectIntensity = 1f)
        {
            CacheComponents();
            EnsureMesh();
            transform.SetPositionAndRotation(worldPosition, worldRotation);
            transform.localScale = Vector3.one * StartScale();
            elapsed = 0f;
            intensity = Mathf.Clamp(effectIntensity, 0.2f, 2f);
            playing = true;
            gameObject.SetActive(true);
            ApplyVisualProperties(0f, 1f);
        }

        public void Stop()
        {
            if (!playing) return;
            playing = false;
            ApplyVisualProperties(1f, 0f);
            gameObject.SetActive(false);
        }

        public void ReturnToPool()
        {
            playing = false;
            ApplyVisualProperties(1f, 0f);
            if (releaseToPool != null) releaseToPool(this);
            else gameObject.SetActive(false);
        }

        private void CacheComponents()
        {
            if (meshFilter == null) meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer == null) meshRenderer = GetComponent<MeshRenderer>();
            if (properties == null) properties = new MaterialPropertyBlock();
        }

        private void EnsureMesh()
        {
            if (meshFilter == null) return;
            if (generatedMesh != null && meshBuilt && builtKind == kind && meshFilter.sharedMesh == generatedMesh) return;
            if (generatedMesh != null)
            {
                if (Application.isPlaying) Destroy(generatedMesh);
                else DestroyImmediate(generatedMesh);
            }

            generatedMesh = BuildMesh(kind);
            generatedMesh.MarkDynamic();
            meshFilter.sharedMesh = generatedMesh;
            builtKind = kind;
            meshBuilt = true;
        }

        private Mesh BuildMesh(WaterVfxKind effectKind)
        {
            switch (effectKind)
            {
                case WaterVfxKind.Wake: return BuildWakeMesh();
                case WaterVfxKind.HitSplash: return BuildRingMesh(0.08f, 0.95f, 32, 1, true);
                case WaterVfxKind.LandingSplash: return BuildRingMesh(0.34f, 1.65f, 32, 1, false);
                case WaterVfxKind.BossReaction: return BuildBossReactionMesh();
                default: return BuildRingMesh(0.18f, 0.9f, 32, 1, false);
            }
        }

        private static Mesh BuildWakeMesh()
        {
            const int segments = 18;
            const float length = 3.8f;
            var vertices = new Vector3[segments * 2];
            var uv = new Vector2[vertices.Length];
            var colors = new Color[vertices.Length];
            var triangles = new int[(segments - 1) * 6];
            for (var i = 0; i < segments; i++)
            {
                var t = i / (float)(segments - 1);
                var z = -t * length;
                var width = Mathf.Lerp(0.48f, 0.08f, t) * (0.92f + 0.08f * Mathf.Cos(t * Mathf.PI * 3f));
                var alpha = Mathf.Lerp(1f, 0.25f, t);
                vertices[i * 2] = new Vector3(-width, 0.018f, z);
                vertices[i * 2 + 1] = new Vector3(width, 0.018f, z);
                uv[i * 2] = new Vector2(t, 0f); uv[i * 2 + 1] = new Vector2(t, 1f);
                colors[i * 2] = new Color(1f, 1f, 1f, alpha);
                colors[i * 2 + 1] = new Color(1f, 1f, 1f, alpha);
            }
            for (var i = 0; i < segments - 1; i++)
            {
                var v = i * 2; var t = i * 6;
                triangles[t] = v; triangles[t + 1] = v + 2; triangles[t + 2] = v + 1;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 2; triangles[t + 5] = v + 3;
            }
            return CreateMesh("SeaLion_Wake_Runtime", vertices, uv, colors, triangles);
        }

        private static Mesh BuildRingMesh(float inner, float outer, int segments, int ringCount, bool spikes)
        {
            var vertices = new Vector3[segments * 2 * ringCount];
            var uv = new Vector2[vertices.Length];
            var colors = new Color[vertices.Length];
            var triangles = new int[(segments * 6) * ringCount];
            for (var ring = 0; ring < ringCount; ring++)
            {
                var radiusOffset = ring * 0.02f;
                for (var i = 0; i < segments; i++)
                {
                    var t = i / (float)segments;
                    var angle = t * Mathf.PI * 2f;
                    var spike = spikes ? 1f + 0.24f * Mathf.Pow(Mathf.Abs(Mathf.Sin(angle * 4f)), 8f) : 1f;
                    var innerRadius = inner + radiusOffset;
                    var outerRadius = outer * spike + radiusOffset;
                    var baseVertex = ring * segments * 2 + i * 2;
                    vertices[baseVertex] = new Vector3(Mathf.Cos(angle) * innerRadius, 0.022f + ring * 0.01f, Mathf.Sin(angle) * innerRadius);
                    vertices[baseVertex + 1] = new Vector3(Mathf.Cos(angle) * outerRadius, 0.024f + ring * 0.01f, Mathf.Sin(angle) * outerRadius);
                    uv[baseVertex] = new Vector2(t, 0f); uv[baseVertex + 1] = new Vector2(t, 1f);
                    colors[baseVertex] = new Color(1f, 1f, 1f, 0.56f);
                    colors[baseVertex + 1] = new Color(1f, 1f, 1f, 1f);
                    var triangle = ring * segments * 6 + i * 6;
                    var next = ring * segments * 2 + ((i + 1) % segments) * 2;
                    triangles[triangle] = baseVertex; triangles[triangle + 1] = next; triangles[triangle + 2] = baseVertex + 1;
                    triangles[triangle + 3] = baseVertex + 1; triangles[triangle + 4] = next; triangles[triangle + 5] = next + 1;
                }
            }
            return CreateMesh("SeaLion_WaterRing_Runtime", vertices, uv, colors, triangles);
        }

        private static Mesh BuildBossReactionMesh()
        {
            var first = BuildRingMesh(0.18f, 1.24f, 32, 1, true);
            var second = BuildRingMesh(0.08f, 2.2f, 32, 1, false);
            var vertices = new Vector3[first.vertexCount + second.vertexCount];
            var uv = new Vector2[vertices.Length];
            var colors = new Color[vertices.Length];
            var triangles = new int[first.triangles.Length + second.triangles.Length];
            CopyMesh(first, vertices, uv, colors, triangles, 0, 0, 0f);
            CopyMesh(second, vertices, uv, colors, triangles, first.vertexCount, first.triangles.Length, 0.16f);
            DestroyRuntimeMesh(first); DestroyRuntimeMesh(second);
            return CreateMesh("SeaLion_BossReaction_Runtime", vertices, uv, colors, triangles);
        }

        private static void CopyMesh(Mesh source, Vector3[] vertices, Vector2[] uv, Color[] colors, int[] triangles, int vertexOffset, int triangleOffset, float uvOffset)
        {
            var sourceVertices = source.vertices; var sourceUv = source.uv; var sourceColors = source.colors; var sourceTriangles = source.triangles;
            Array.Copy(sourceVertices, 0, vertices, vertexOffset, sourceVertices.Length);
            Array.Copy(sourceUv, 0, uv, vertexOffset, sourceUv.Length);
            Array.Copy(sourceColors, 0, colors, vertexOffset, sourceColors.Length);
            for (var i = 0; i < sourceTriangles.Length; i++) triangles[triangleOffset + i] = sourceTriangles[i] + vertexOffset;
            if (uvOffset <= 0f) return;
            for (var i = vertexOffset; i < vertexOffset + sourceUv.Length; i++) uv[i].y = uv[i].y * 0.65f + uvOffset;
        }

        private static Mesh CreateMesh(string meshName, Vector3[] vertices, Vector2[] uv, Color[] colors, int[] triangles)
        {
            var mesh = new Mesh { name = meshName };
            mesh.vertices = vertices; mesh.uv = uv; mesh.colors = colors; mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        private static void DestroyRuntimeMesh(Mesh mesh)
        {
            if (mesh == null) return;
            if (Application.isPlaying) Destroy(mesh);
            else DestroyImmediate(mesh);
        }

        private float StartScale() => kind == WaterVfxKind.Wake ? 1f : 0.22f;

        private void ApplyVisualProperties(float progress, float fade)
        {
            if (meshRenderer == null || properties == null) return;
            meshRenderer.GetPropertyBlock(properties);
            properties.SetFloat("_EffectMode", 1f);
            properties.SetFloat("_EffectProgress", Mathf.Clamp01(progress));
            properties.SetFloat("_EffectIntensity", intensity * density * fade);
            properties.SetFloat("_ReducedMode", reduced ? 1f : 0f);
            meshRenderer.SetPropertyBlock(properties);
        }

        private void OnDestroy()
        {
            if (generatedMesh == null) return;
            if (Application.isPlaying) Destroy(generatedMesh);
            else DestroyImmediate(generatedMesh);
            generatedMesh = null;
            meshBuilt = false;
        }
    }
}
