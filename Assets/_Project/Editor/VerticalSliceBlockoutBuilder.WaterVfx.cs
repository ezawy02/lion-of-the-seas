using UnityEditor;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    static void Wake(Transform root, string name, Vector3 position, Vector3 scale,
        string materialOverride = null, float heading = 0f)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/VFX/Wake.prefab");
        if (prefab == null) return;
        var wake = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        wake.name = name;
        wake.transform.SetParent(root);
        wake.transform.position = position;
        wake.transform.rotation = Quaternion.Euler(0f, heading, 0f);
        wake.transform.localScale = scale;
        var material = string.IsNullOrEmpty(materialOverride)
            ? null
            : AssetDatabase.LoadAssetAtPath<Material>(materialOverride);
        if (material != null)
            foreach (var renderer in wake.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
    }

    // Production rule: wakes stay attached to the stern and never become screen-length trails.
    static void CompactCraftWake(Transform root, string name, Vector3 craftPosition,
        float heading, bool flagship = false)
    {
        var rotation = Quaternion.Euler(0f, heading, 0f);
        var offset = rotation * new Vector3(0f, -0.01f, flagship ? -0.65f : -0.42f);
        var scale = flagship ? new Vector3(1.38f, 1f, 1.22f) : new Vector3(0.78f, 1f, 0.90f);
        Wake(root, name, craftPosition + offset, scale, null, heading);
    }

    static void WaterEffect(Transform root, string name, string prefabPath, Vector3 position, Vector3 scale, Material material)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null) return;
        var effect = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        effect.name = name;
        effect.transform.SetParent(root);
        effect.transform.position = position;
        effect.transform.localScale = scale;
        if (material != null)
            foreach (var renderer in effect.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
    }

    static void TargetRing(Transform root, string name, Vector3 position, float radius)
    {
        const int segments = 64;
        var vertices = new Vector3[segments * 2];
        var uv = new Vector2[vertices.Length];
        var colors = new Color[vertices.Length];
        var triangles = new int[segments * 6];
        for (var i = 0; i < segments; i++)
        {
            var t = i / (float)segments;
            var angle = t * Mathf.PI * 2f;
            var direction = new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle));
            vertices[i * 2] = direction * (radius * 0.88f);
            vertices[i * 2 + 1] = direction * radius;
            uv[i * 2] = new Vector2(t, 0f);
            uv[i * 2 + 1] = new Vector2(t, 1f);
            colors[i * 2] = Color.white;
            colors[i * 2 + 1] = Color.white;
            var next = (i + 1) % segments;
            var triangle = i * 6;
            triangles[triangle] = i * 2;
            triangles[triangle + 1] = next * 2;
            triangles[triangle + 2] = i * 2 + 1;
            triangles[triangle + 3] = i * 2 + 1;
            triangles[triangle + 4] = next * 2;
            triangles[triangle + 5] = next * 2 + 1;
        }
        var mesh = new Mesh { name = "L01_IncomingCannonTargetRing_Mesh" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        var value = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        value.transform.SetParent(root);
        value.transform.position = position;
        value.GetComponent<MeshFilter>().sharedMesh = mesh;
        value.GetComponent<MeshRenderer>().sharedMaterial = TargetRingMaterial();
    }

    static void CannonSplash(Transform root, string name, Vector3 position, float width, float height)
    {
        var mesh = new Mesh { name = "L01_IncomingCannonSplash_Mesh" };
        mesh.vertices = new[]
        {
            new Vector3(-width * 0.5f, 0f, 0f), new Vector3(width * 0.5f, 0f, 0f),
            new Vector3(-width * 0.5f, height, 0f), new Vector3(width * 0.5f, height, 0f)
        };
        mesh.uv = new[] { new Vector2(0,0), new Vector2(1,0), new Vector2(0,1), new Vector2(1,1) };
        mesh.triangles = new[] { 0, 2, 1, 1, 2, 3 };
        mesh.RecalculateBounds();
        var value = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        value.transform.SetParent(root);
        value.transform.position = position;
        value.GetComponent<MeshFilter>().sharedMesh = mesh;
        value.GetComponent<MeshRenderer>().sharedMaterial = CannonSplashMaterial();
    }

    static Material CannonSplashMaterial()
    {
        const string texturePath = "Assets/_Project/Art/VFX/L01_CannonSplash.png";
        var importer = AssetImporter.GetAtPath(texturePath) as TextureImporter;
        if (importer != null)
        {
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        var shader = Shader.Find("Sea Lion/VFX/Transparent Sprite");
        var path = MaterialRoot + "L01_CannonSplash.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (shader == null) return material;
        if (material == null)
        {
            material = new Material(shader) { name = "L01_CannonSplash" };
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = shader;
        material.SetTexture("_BaseMap", texture);
        material.SetColor("_BaseColor", Color.white);
        EditorUtility.SetDirty(material);
        return material;
    }

    static Material TargetRingMaterial()
    {
        var path = MaterialRoot + "L01_IncomingCannonTargetRing.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Sea Lion/Water/Styled Mobile");
        if (shader == null) return material;
        if (material == null)
        {
            material = new Material(shader) { name = "L01_IncomingCannonTargetRing" };
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = shader;
        material.SetColor("_ShallowColor", new Color(1f, 0.10f, 0.025f, 0.82f));
        material.SetColor("_DeepColor", new Color(0.42f, 0.015f, 0.005f, 0.68f));
        material.SetColor("_FoamColor", new Color(1f, 0.33f, 0.08f, 1f));
        material.SetFloat("_Opacity", 0.72f);
        material.SetFloat("_FoamStrength", 1.45f);
        material.SetFloat("_EffectMode", 0f);
        material.SetFloat("_EffectIntensity", 1.4f);
        material.SetFloat("_WaveAmplitude", 0f);
        EditorUtility.SetDirty(material);
        return material;
    }
}
