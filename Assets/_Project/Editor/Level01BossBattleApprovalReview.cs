using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string BossBattleReviewScene =
        "Assets/_Project/Scenes/Review/Level01_BossBattle_REVIEW.unity";
    const string BossBattleReviewOutput =
        "Artifacts/Local/Approval/Level01BossBattle";
    const string BossBattleReviewWater =
        "Assets/_Project/Materials/Review/SeaLion_Water_Level01_BossBattle_REVIEW.mat";
    const string BossBattleReviewSky =
        "Assets/_Project/Materials/Review/SeaLion_Sky_Level01_BossBattle_REVIEW.mat";
    const string BossBattleReviewFortressMaterials =
        "Assets/_Project/Materials/Review/BossBattleFortress";

    [MenuItem("Lion of the Seas/Build Level 01 Boss Battle REVIEW")]
    public static void BuildLevel01BossBattleApprovalReview()
    {
        Directory.CreateDirectory("Assets/_Project/Scenes/Review");
        Directory.CreateDirectory("Assets/_Project/Materials/Review");
        Directory.CreateDirectory(BossBattleReviewOutput);
        EnsureBossBattleReviewMaterials();

        var root = Begin("LEVEL01_BOSS_BATTLE_REVIEW__NOT_PRODUCTION");
        BuildLevel01BossBattle(root);
        RestoreBossBattleApprovedFortressScale(root);
        ApplyBossBattleReviewWater();
        ApplyBossBattleFortressPalette();
        BuildBossBattleReviewFeedback(root);
        CameraAndLight(root, new Vector3(-1f, 18f, 35f), new Vector3(0f, 4.8f, 99f), false);
        ApplyBossBattleReviewLighting();
        ValidateBossBattleReview();
        Save(BossBattleReviewScene);
        CaptureBossBattleReviewImages();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Level 01 Boss Battle REVIEW built and captured. Production was not modified.");
    }

    [MenuItem("Lion of the Seas/Capture Level 01 Boss Battle REVIEW")]
    public static void CaptureLevel01BossBattleApprovalReview()
    {
        EditorSceneManager.OpenScene(BossBattleReviewScene, OpenSceneMode.Single);
        Directory.CreateDirectory(BossBattleReviewOutput);
        CaptureBossBattleReviewImages();
        AssetDatabase.Refresh();
    }

    static void EnsureBossBattleReviewMaterials()
    {
        var sourceWater = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Water/SeaLion_Water_Level01.mat");
        var water = AssetDatabase.LoadAssetAtPath<Material>(BossBattleReviewWater);
        if (water == null && sourceWater != null)
        {
            water = new Material(sourceWater) { name = "SeaLion_Water_Level01_BossBattle_REVIEW" };
            AssetDatabase.CreateAsset(water, BossBattleReviewWater);
        }
        if (water != null)
        {
            water.SetColor("_ForegroundColor", new Color(0.025f, 0.27f, 0.31f, 1f));
            water.SetColor("_HorizonColor", new Color(0.02f, 0.36f, 0.48f, 1f));
            water.SetColor("_ShallowColor", new Color(0.02f, 0.64f, 0.73f, 0.82f));
            water.SetFloat("_WaveAmplitude", 0.052f);
            water.SetFloat("_NormalStrength", 1.55f);
            water.SetFloat("_SpecularStrength", 0.95f);
            water.SetFloat("_FoamStrength", 0.58f);
            EditorUtility.SetDirty(water);
        }

        var sourceSky = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
        var sky = AssetDatabase.LoadAssetAtPath<Material>(BossBattleReviewSky);
        if (sky == null && sourceSky != null)
        {
            sky = new Material(sourceSky) { name = "SeaLion_Sky_Level01_BossBattle_REVIEW" };
            AssetDatabase.CreateAsset(sky, BossBattleReviewSky);
        }
        if (sky == null) return;
        sky.SetColor("_ZenithColor", new Color(0.08f, 0.43f, 0.76f, 1f));
        sky.SetColor("_HorizonColor", new Color(0.43f, 0.72f, 0.84f, 1f));
        sky.SetColor("_CloudColor", new Color(0.86f, 0.90f, 0.88f, 1f));
        sky.SetFloat("_CloudStrength", 0.84f);
        sky.SetFloat("_CloudScale", 1.45f);
        EditorUtility.SetDirty(sky);
    }

    static void ApplyBossBattleReviewWater()
    {
        var water = GameObject.Find("ENV__AuthoredWaterSurface");
        var material = AssetDatabase.LoadAssetAtPath<Material>(BossBattleReviewWater);
        var renderer = water == null ? null : water.GetComponent<MeshRenderer>();
        if (renderer == null || material == null)
            throw new MissingReferenceException("Boss review water or material is missing.");
        renderer.sharedMaterial = material;
    }

    static void RestoreBossBattleApprovedFortressScale(Transform root)
    {
        var oldFortress = GameObject.Find("GROUP__BattleFortress_Approved");
        if (oldFortress != null) Object.DestroyImmediate(oldFortress);
        PlaceApprovedLevel01Fortress(root, "GROUP__BattleFortress_Approved",
            new Vector3(-1.5f, 0f, 108f), 38f);
    }

    static void ApplyBossBattleFortressPalette()
    {
        var fortress = GameObject.Find("GROUP__BattleFortress_Approved");
        if (fortress == null)
            throw new MissingReferenceException("Boss review fortress group is missing.");

        Directory.CreateDirectory(BossBattleReviewFortressMaterials);
        var reviewMaterials = new Dictionary<Material, Material>();
        foreach (var renderer in fortress.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (var index = 0; index < materials.Length; index++)
            {
                var source = materials[index];
                if (source == null) continue;
                if (!reviewMaterials.TryGetValue(source, out var review))
                {
                    review = EnsureBossBattleFortressMaterial(source);
                    reviewMaterials.Add(source, review);
                }
                materials[index] = review;
            }
            renderer.sharedMaterials = materials;
        }
    }

    static Material EnsureBossBattleFortressMaterial(Material source)
    {
        var sourcePath = AssetDatabase.GetAssetPath(source);
        var guid = AssetDatabase.AssetPathToGUID(sourcePath);
        var suffix = guid.Length >= 8 ? guid.Substring(0, 8) : "material";
        var safeName = source.name.Replace('/', '_').Replace('\\', '_');
        var path = BossBattleReviewFortressMaterials + "/" + safeName + "_" + suffix + "_REVIEW.mat";
        var review = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (review == null)
        {
            review = new Material(source) { name = safeName + "_BossBattle_REVIEW" };
            AssetDatabase.CreateAsset(review, path);
        }
        else
        {
            review.shader = source.shader;
            review.CopyPropertiesFromMaterial(source);
        }

        if (review.HasProperty("_BaseColor"))
            review.SetColor("_BaseColor", new Color(0.74f, 0.67f, 0.56f, 1f));
        if (review.HasProperty("_ColorBoost")) review.SetFloat("_ColorBoost", 0.82f);
        if (review.HasProperty("_Contrast")) review.SetFloat("_Contrast", 1.00f);
        if (review.HasProperty("_Saturation")) review.SetFloat("_Saturation", 0.92f);
        if (review.HasProperty("_LightResponse")) review.SetFloat("_LightResponse", 0.22f);
        if (review.HasProperty("_Smoothness")) review.SetFloat("_Smoothness", 0.14f);
        EditorUtility.SetDirty(review);
        return review;
    }

    static void BuildBossBattleReviewFeedback(Transform root)
    {
        WaterEffect(root, "VFX__GuardianGroundReaction_REVIEW",
            "Assets/_Project/VFX/BossReaction.prefab", new Vector3(0f, 0.06f, 96f),
            new Vector3(3.6f, 1f, 3.6f), null);
        TargetRing(root, "VFX__GuardianAttackWarning_REVIEW", new Vector3(-3.5f, 0.06f, 82f), 3.4f);
        CannonSplash(root, "VFX__BroadsideImpact_REVIEW", new Vector3(4.8f, 0.07f, 69f), 1.8f, 3.8f);

        var material = AssetDatabase.LoadAssetAtPath<Material>(ApprovedMuzzleFlash);
        if (material == null) return;
        var flash = Primitive(root, "VFX__GuardianHitFlash_REVIEW", PrimitiveType.Sphere,
            new Vector3(0.75f, 6.2f, 93.5f), Vector3.one * 0.18f, material);
        flash.GetComponent<Collider>().enabled = false;
        var light = new GameObject("VFX__GuardianHitLight_REVIEW").AddComponent<Light>();
        light.transform.SetParent(root);
        light.transform.position = new Vector3(0.75f, 6.2f, 93.1f);
        light.type = LightType.Point;
        light.range = 7f;
        light.intensity = 2.4f;
        light.color = new Color(1f, 0.48f, 0.12f);
    }

    static void ApplyBossBattleReviewLighting()
    {
        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(BossBattleReviewSky);
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.30f, 0.35f, 0.37f);
        RenderSettings.ambientIntensity = 0.58f;
        RenderSettings.reflectionIntensity = 0.32f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.40f, 0.61f, 0.70f);
        RenderSettings.fogDensity = 0.0017f;
        var key = GameObject.Find("KEY_LIGHT__Blockout");
        var light = key == null ? null : key.GetComponent<Light>();
        if (light == null) return;
        light.intensity = 0.78f;
        light.color = new Color(1f, 0.86f, 0.70f);
        light.shadows = LightShadows.Soft;
    }

    static void ValidateBossBattleReview()
    {
        foreach (var name in new[]
        {
            "BOSS__HarborGuardian", "GROUP__BattleFortress_Approved",
            "FRIENDLY__LandingForce_Front", "HOSTILE__Defenders_Front",
            "VFX__GuardianHitFlash_REVIEW", "PORTRAIT_CAMERA__Gameplay"
        })
            if (GameObject.Find(name) == null)
                throw new MissingReferenceException("Boss battle review object is missing: " + name);
        var fortress = GameObject.Find("GROUP__BattleFortress_Approved");
        var bounds = CombinedBounds(fortress.transform);
        if (Mathf.Max(bounds.size.x, bounds.size.z) < 37.5f)
            throw new InvalidDataException($"Boss fortress regressed below approved width: {bounds.size}.");
    }

    static void CaptureBossBattleReviewImages()
    {
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Boss battle review camera is missing.");
        camera.fieldOfView = 38f;
        var position = new Vector3(-1f, 18f, 35f);
        var target = new Vector3(0f, 4.8f, 99f);
        Capture(camera, position, target, BossBattleReviewOutput + "/01_BossBattle_Full_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_BossBattle_HUD.png");
        Capture(camera, position, target, BossBattleReviewOutput + "/02_BossBattle_NoHUD_REVIEW.png");
        camera.fieldOfView = 27f;
        Capture(camera, new Vector3(-0.5f, 15f, 60f), new Vector3(0f, 6.4f, 99f),
            BossBattleReviewOutput + "/03_Guardian_Detail_REVIEW.png");
        camera.fieldOfView = 38f;
    }
}
