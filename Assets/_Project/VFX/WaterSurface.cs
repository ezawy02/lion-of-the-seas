using UnityEngine;

namespace SeaLion.Presentation.Vfx
{
    /// <summary>
    /// Small authored grid for the benchmark sea. The shader supplies the wave motion so the
    /// mesh remains cheap on mobile and can be reduced without changing gameplay geometry.
    /// </summary>
    [ExecuteAlways]
    [DisallowMultipleComponent]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public sealed class WaterSurface : MonoBehaviour
    {
        [SerializeField, Min(2)] private int resolution = 18;
        [SerializeField, Min(1f)] private float width = 24f;
        [SerializeField, Min(1f)] private float length = 30f;
        [SerializeField, Range(0f, 1f)] private float vfxDensity = 1f;
        [SerializeField] private bool reduced;

        private MeshFilter meshFilter;
        private MeshRenderer meshRenderer;
        private MaterialPropertyBlock properties;
        private Mesh generatedMesh;
        private int lastResolution;

        public float VfxDensity => vfxDensity;
        public bool Reduced => reduced;

        private void OnEnable()
        {
            CacheComponents();
            EnsureMesh();
            ApplyQualityProperties();
        }

        private void OnValidate()
        {
            resolution = Mathf.Clamp(resolution, 2, 32);
            width = Mathf.Max(1f, width);
            length = Mathf.Max(1f, length);
            vfxDensity = Mathf.Clamp01(vfxDensity);
            CacheComponents();
            EnsureMesh();
            ApplyQualityProperties();
        }

        private void LateUpdate()
        {
            if (meshRenderer == null) CacheComponents();
            ApplyQualityProperties();
        }

        /// <summary>Called by a quality controller; it only changes presentation density.</summary>
        public void SetQuality(float density, bool useReduced)
        {
            vfxDensity = Mathf.Clamp01(density);
            reduced = useReduced;
            ApplyQualityProperties();
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
            if (generatedMesh != null && lastResolution == resolution && meshFilter.sharedMesh == generatedMesh) return;
            if (generatedMesh != null)
            {
                if (Application.isPlaying) Destroy(generatedMesh);
                else DestroyImmediate(generatedMesh);
            }

            var vertexCount = resolution * resolution;
            var vertices = new Vector3[vertexCount];
            var uv = new Vector2[vertexCount];
            var colors = new Color[vertexCount];
            var cells = resolution - 1;
            var triangles = new int[cells * cells * 6];
            var index = 0;

            for (var z = 0; z < resolution; z++)
            {
                var z01 = z / (float)cells;
                for (var x = 0; x < resolution; x++)
                {
                    var x01 = x / (float)cells;
                    var vertex = z * resolution + x;
                    vertices[vertex] = new Vector3((x01 - 0.5f) * width, 0f, (z01 - 0.5f) * length);
                    uv[vertex] = new Vector2(x01, z01);
                    var edge = Mathf.Min(Mathf.Min(x01, 1f - x01), Mathf.Min(z01, 1f - z01));
                    colors[vertex] = new Color(1f, 1f, 1f, Mathf.Clamp01(1f - edge * 7f));
                }
            }

            for (var z = 0; z < cells; z++)
            {
                for (var x = 0; x < cells; x++)
                {
                    var a = z * resolution + x;
                    var b = a + 1;
                    var c = a + resolution;
                    var d = c + 1;
                    triangles[index++] = a; triangles[index++] = c; triangles[index++] = b;
                    triangles[index++] = b; triangles[index++] = c; triangles[index++] = d;
                }
            }

            generatedMesh = new Mesh { name = "SeaLion_WaterSurface_Runtime" };
            generatedMesh.MarkDynamic();
            generatedMesh.vertices = vertices;
            generatedMesh.uv = uv;
            generatedMesh.colors = colors;
            generatedMesh.triangles = triangles;
            generatedMesh.RecalculateBounds();
            meshFilter.sharedMesh = generatedMesh;
            lastResolution = resolution;
        }

        private void ApplyQualityProperties()
        {
            if (meshRenderer == null || properties == null) return;
            meshRenderer.GetPropertyBlock(properties);
            properties.SetFloat("_ReducedMode", reduced ? 1f : 0f);
            properties.SetFloat("_EffectIntensity", Mathf.Max(0.1f, vfxDensity));
            meshRenderer.SetPropertyBlock(properties);
        }

        private void OnDestroy()
        {
            if (generatedMesh == null) return;
            if (Application.isPlaying) Destroy(generatedMesh);
            else DestroyImmediate(generatedMesh);
            generatedMesh = null;
        }
    }
}
