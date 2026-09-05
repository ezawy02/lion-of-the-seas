using System.IO;
using UnityEditor;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string GateEnergyShader = "Assets/_Project/Materials/VFX/SeaLionGateEnergy.shader";
    const string GateEnergyPortalMaterial =
        "Assets/_Project/Materials/Review/SeaLion_GateEnergyPortal_REVIEW.mat";
    const string GateEnergyBeamMaterial =
        "Assets/_Project/Materials/Review/SeaLion_GateEnergyBeam_REVIEW.mat";
    const string GateEnergyWaterMaterial =
        "Assets/_Project/Materials/Review/SeaLion_GateEnergyWater_REVIEW.mat";

    static void BuildGateEnergyReview(Transform root, string prefix, Vector3 gatePosition,
        float openingWidth, float openingHeight)
    {
        EnsureGateEnergyReviewMaterials();
        var center = gatePosition + new Vector3(0f, openingHeight * 0.5f, 0.12f);
        GateEnergyQuad(root, "VFX__" + prefix + "GateEnergyPortal_REVIEW", center,
            new Vector2(openingWidth, openingHeight), Quaternion.identity, GateEnergyPortalMaterial);
        GateEnergyQuad(root, "VFX__" + prefix + "GateEnergyBeam_REVIEW",
            center + new Vector3(0f, 0f, -0.025f), new Vector2(openingWidth * 0.38f, openingHeight),
            Quaternion.identity, GateEnergyBeamMaterial);
        GateEnergyQuad(root, "VFX__" + prefix + "GateEnergyWaterHalo_REVIEW",
            gatePosition + new Vector3(0f, 0.045f, -0.18f),
            new Vector2(openingWidth * 1.32f, openingWidth * 0.72f),
            Quaternion.Euler(90f, 0f, 0f), GateEnergyWaterMaterial);
    }

    static void EnsureGateEnergyReviewMaterials()
    {
        Directory.CreateDirectory("Assets/_Project/Materials/Review");
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(GateEnergyShader);
        if (shader == null) throw new FileNotFoundException("Gate energy shader is missing.", GateEnergyShader);
        ConfigureGateEnergyMaterial(GateEnergyPortalMaterial, shader,
            new Color(0.03f, 0.24f, 0.78f, 1f), new Color(0.05f, 0.75f, 1.70f, 1f),
            0.88f, 0.82f, 0.14f, 1.25f, 2.1f, false);
        ConfigureGateEnergyMaterial(GateEnergyBeamMaterial, shader,
            new Color(0.08f, 0.62f, 1.35f, 1f), new Color(0.03f, 0.30f, 1.05f, 1f),
            0.46f, 0.14f, 0.78f, 0.48f, 2.8f, true);
        ConfigureGateEnergyMaterial(GateEnergyWaterMaterial, shader,
            new Color(0.06f, 0.66f, 1.48f, 1f), new Color(0.03f, 0.58f, 1.52f, 1f),
            0.78f, 0.10f, 0.03f, 0.62f, 1.5f, true);
    }

    static void ConfigureGateEnergyMaterial(string path, Shader shader, Color core, Color edge,
        float opacity, float fieldStrength, float beamStrength, float intensity, float pulseSpeed,
        bool additive)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader) { name = Path.GetFileNameWithoutExtension(path) };
            AssetDatabase.CreateAsset(material, path);
        }
        else if (material.shader != shader)
        {
            material.shader = shader;
        }
        material.SetColor("_CoreColor", core);
        material.SetColor("_EdgeColor", edge);
        material.SetFloat("_Opacity", opacity);
        material.SetFloat("_FieldStrength", fieldStrength);
        material.SetFloat("_BeamStrength", beamStrength);
        material.SetFloat("_Intensity", intensity);
        material.SetFloat("_EdgeSoftness", fieldStrength > 0.5f ? 0.18f : 0.32f);
        material.SetFloat("_PulseSpeed", pulseSpeed);
        material.SetFloat("_SrcBlend", (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
        material.SetFloat("_DstBlend", additive
            ? (float)UnityEngine.Rendering.BlendMode.One
            : (float)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
        EditorUtility.SetDirty(material);
    }

    static void GateEnergyQuad(Transform root, string name, Vector3 position, Vector2 size,
        Quaternion rotation, string materialPath)
    {
        var mesh = new Mesh { name = name + "_Mesh" };
        mesh.vertices = new[]
        {
            new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f),
            new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f)
        };
        mesh.uv = new[] { Vector2.zero, Vector2.right, Vector2.up, Vector2.one };
        mesh.triangles = new[] { 0, 2, 1, 2, 3, 1 };
        mesh.RecalculateBounds();

        var value = new GameObject(name, typeof(MeshFilter), typeof(MeshRenderer));
        value.transform.SetParent(root);
        value.transform.SetPositionAndRotation(position, rotation);
        value.transform.localScale = new Vector3(size.x, size.y, 1f);
        value.GetComponent<MeshFilter>().sharedMesh = mesh;
        value.GetComponent<MeshRenderer>().sharedMaterial =
            AssetDatabase.LoadAssetAtPath<Material>(materialPath);
    }

    static void ValidateGateEnergyReview(string prefix)
    {
        foreach (var suffix in new[] { "Portal", "Beam", "WaterHalo" })
        {
            var value = GameObject.Find("VFX__" + prefix + "GateEnergy" + suffix + "_REVIEW");
            var renderer = value == null ? null : value.GetComponent<MeshRenderer>();
            if (renderer == null || renderer.sharedMaterial == null)
                throw new MissingReferenceException(prefix + " gate energy layer is missing: " + suffix);
        }
    }
}
