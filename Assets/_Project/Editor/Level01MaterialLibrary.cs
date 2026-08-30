using System.IO;
using UnityEditor;
using UnityEngine;

public static class Level01MaterialLibrary
{
    static readonly string[] TextureRoots =
    {
        "Assets/_Project/Art/Textures/Level01/",
        "Assets/_Project/Art/Textures/Level02/",
        "Assets/_Project/Art/Textures/Level03/"
    };
    const string MaterialRoot = "Assets/_Project/Materials/Imported/";

    public static Material LoadOrCreate(string assetPath)
    {
        var assetId = Path.GetFileNameWithoutExtension(assetPath)
            .Replace("_DefeatedKneel_R1_REVIEW", "_Boss")
            .Replace("_TripoRig_Optimized_REVIEW", "")
            .Replace("_Optimized_REVIEW", "")
            .Replace("_Rigged_Optimized_R2_LeadershipPose_REVIEW", "")
            .Replace("_Rigged_Optimized", "")
            .Replace("_Optimized", "");
        var baseColor = Load(assetId, "BaseColor");
        if (baseColor == null) return null;
        var normal = Load(assetId, "Normal");
        var metallicSmoothness = Load(assetId, "MetallicSmoothness");
        var shader = Shader.Find("Sea Lion/Art/Reference Lit");
        if (shader == null) throw new MissingReferenceException("Required URP shader is unavailable.");

        Directory.CreateDirectory(MaterialRoot);
        var path = MaterialRoot + assetId + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader) { name = assetId };
            AssetDatabase.CreateAsset(material, path);
        }

        material.shader = shader;
        material.SetColor("_BaseColor", assetId.Contains("Mountain_City_Backdrop")
            ? new Color(1.08f, 1.10f, 1.14f, 1f)
            : Color.white);
        material.SetTexture("_BaseMap", baseColor);
        var isHeroFlagship = assetId.Contains("SHP-004_Hero_Flagship");
        var isLevel01UserCharacter = assetId.Contains("L01-CHR-004_Harbor_Guardian_UserBatch_R2") ||
                                     assetId.Contains("L01-CHR-005_Enemy_Commander_UserBatch_R2");
        var isLevel01RiggedGuardian = assetId.Contains("L01-CHR-004_Harbor_Guardian_Boss") ||
                                      assetId.Contains("L01-CHR-004_Harbor_Guardian_DefeatedKneel");
        material.SetColor("_BaseColor", isLevel01RiggedGuardian
            ? new Color(0.60f, 0.58f, 0.50f, 1f)
            : material.GetColor("_BaseColor"));
        material.SetFloat("_Saturation", isHeroFlagship ? 1.02f : isLevel01UserCharacter ? 1.04f :
            isLevel01RiggedGuardian ? 0.88f : 1.10f);
        material.SetFloat("_Contrast", isHeroFlagship ? 1.04f : isLevel01UserCharacter ? 1.10f :
            isLevel01RiggedGuardian ? 1.12f : 1.08f);
        material.SetFloat("_ColorBoost", isHeroFlagship ? 0.88f : isLevel01UserCharacter ? 0.88f :
            isLevel01RiggedGuardian ? 0.76f : 1.04f);
        material.SetFloat("_LightResponse", isHeroFlagship ? 0.48f : isLevel01UserCharacter ? 0.46f :
            isLevel01RiggedGuardian ? 0.48f : 0.32f);
        material.SetFloat("_Cull", isHeroFlagship ? 0f : 2f);
        // Hyper3D's packed metal channel is intentionally strong. Multiplying it by
        // a restrained scalar preserves gold/iron accents without turning wood,
        // limestone, cloth, and skin into black mirrors under URP.
        material.DisableKeyword("_NORMALMAP");
        // Keep the generated mask imported as source evidence, but do not apply it
        // to the baked-color reference material. Hyper3D's diffuse atlas already
        // contains highlights and applying the strong metal mask a second time
        // produces black/mirror artifacts in Unity.
        material.SetTexture("_MetallicGlossMap", null);
        material.DisableKeyword("_METALLICSPECGLOSSMAP");
        material.enableInstancing = true;
        EditorUtility.SetDirty(material);
        return material;
    }

    [MenuItem("Lion of the Seas/Configure Level 01 Texture Imports")]
    public static void ConfigureTextureImports()
    {
        foreach (var guid in AssetDatabase.FindAssets("t:Texture2D", TextureRoots))
        {
            var path = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(path) as TextureImporter;
            if (importer == null) continue;
            var isNormal = path.EndsWith("_Normal.png");
            var isMask = path.EndsWith("_MetallicSmoothness.png") ||
                path.EndsWith("_MetallicRoughness.png") || path.EndsWith("_ORM.png");
            var desiredType = isNormal ? TextureImporterType.NormalMap : TextureImporterType.Default;
            var desiredSrgb = !isNormal && !isMask;
            var desiredMaxSize = path.Contains("SHP-004") || path.Contains("SHP-001") ||
                path.Contains("CHR-001") || path.Contains("CHR-004") ? 2048 : 1024;
            var changed = importer.textureType != desiredType ||
                importer.sRGBTexture != desiredSrgb ||
                !importer.mipmapEnabled || importer.isReadable ||
                importer.maxTextureSize != desiredMaxSize ||
                importer.textureCompression != TextureImporterCompression.CompressedHQ;
            if (!changed) continue;
            importer.textureType = desiredType;
            importer.sRGBTexture = desiredSrgb;
            importer.mipmapEnabled = true;
            importer.isReadable = false;
            importer.maxTextureSize = desiredMaxSize;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.SaveAndReimport();
        }
        AssetDatabase.SaveAssets();
    }

    static Texture2D Load(string assetId, string role)
    {
        foreach (var root in TextureRoots)
        {
            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{root}{assetId}_{role}.png");
            if (texture != null) return texture;
        }
        return null;
    }
}
