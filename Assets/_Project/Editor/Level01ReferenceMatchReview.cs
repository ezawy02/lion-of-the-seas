using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string ReferenceMatchOutput = "Artifacts/Local/Approval/Level01ReferenceMatch/R5";
    const string ReferenceMatchSceneRoot = "Assets/_Project/Scenes/Review/ReferenceMatch";

    [MenuItem("Lion of the Seas/Build Level 01 Reference Match R5 REVIEW")]
    public static void BuildLevel01ReferenceMatchR5Review()
    {
        Directory.CreateDirectory(ReferenceMatchOutput);
        Directory.CreateDirectory(ReferenceMatchSceneRoot);
        BuildReferenceMatchOpening();
        BuildReferenceMatchTraversal();
        BuildReferenceMatchBeachLanding();
        BuildReferenceMatchBossBattle();
        BuildReferenceMatchBenchmark();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Level 01 Reference Match R5 REVIEW built. User approval is required before production transfer.");
    }

    [MenuItem("Lion of the Seas/Open Level 01 Reference Match R5 In Game View")]
    public static void OpenLevel01ReferenceMatchR5Review()
    {
        EditorSceneManager.OpenScene(ReferenceMatchSceneRoot + "/Level01_Benchmark_ReferenceMatch_R5_REVIEW.unity",
            OpenSceneMode.Single);
        var camera = Camera.main;
        Selection.activeGameObject = camera == null ? null : camera.gameObject;
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Opened Level 01 Reference Match R5 REVIEW. This revision is not user-approved.");
    }

    static void BuildReferenceMatchOpening()
    {
        EnsureApprovalMaterials();
        var root = Begin("LEVEL01_OPENING_REFERENCE_MATCH_R5__NOT_USER_APPROVED");
        BuildApprovalWater(root);
        BuildApprovalFleet(root);
        BuildApprovalCoastAndCity(root);
        ScaleGroup(root, "GROUP__OpeningFlagship_ReferenceMatch_R5", new Vector3(-1.9f, 0f, 15.5f),
            1.06f, "PLAYER__Flagship_Preserved_REVIEW", "PLAYER__SecondLateenAndHelm_REVIEW",
            "CHARACTER__Hayreddin_Leadership_REVIEW", "PROP__LionWaveBanner_REVIEW");
        RotateObjectY("CHARACTER__Hayreddin_Leadership_REVIEW", 142f);
        ApplyHayreddinReviewMaterial("CHARACTER__Hayreddin_Leadership_REVIEW");
        TuneWake("VFX__FlagshipWake_REVIEW", new Vector3(2f, 1f, 3.7f));
        CameraAndLight(root, new Vector3(-1.35f, 13.4f, -10.5f), new Vector3(-0.45f, 2.25f, 35.5f), false);
        ApplyWarmReferenceLighting(0.86f, new Color(1f, 0.91f, 0.78f),
            new Color(0.48f, 0.68f, 0.75f), 0.0011f);
        var camera = Camera.main;
        if (camera != null) camera.fieldOfView = 39.5f;
        Save(ReferenceMatchSceneRoot + "/Level01_Opening_ReferenceMatch_R5_REVIEW.unity");
        Capture(camera, new Vector3(-1.35f, 13.4f, -10.5f), new Vector3(-0.45f, 2.25f, 35.5f),
            ReferenceMatchOutput + "/01_Opening_ReferenceMatch_R5_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_Opening_HUD.png");
    }

    static void BuildReferenceMatchTraversal()
    {
        RequireApprovedProductionAsset<GameObject>(ApprovedOpeningAddon);
        RequireApprovedProductionAsset<Material>(ApprovedOpeningWater);
        RequireApprovedProductionAsset<Material>(ApprovedOpeningWake);
        EnsureTraversalReviewMaterials();
        var root = Begin("LEVEL01_TRAVERSAL_REFERENCE_MATCH_R5__NOT_USER_APPROVED");
        BuildTraversalReviewWater(root);
        BuildTraversalReviewFleet(root);
        BuildTraversalReviewObjectives(root);
        BuildApprovedOpeningCoastAndCity(root);
        TuneTraversalReviewBackground();
        ScaleGroup(root, "GROUP__TraversalFlagship_ReferenceMatch_R5", new Vector3(-1.3f, 0f, 14.5f),
            1.03f, "PLAYER__Flagship_REVIEW", "PLAYER__SecondLateenAndHelm_REVIEW",
            "CHARACTER__Hayreddin_OnDeck_REVIEW");
        RotateObjectY("CHARACTER__Hayreddin_OnDeck_REVIEW", 138f);
        ApplyHayreddinReviewMaterial("CHARACTER__Hayreddin_OnDeck_REVIEW");
        ValidateTraversalReviewHierarchy();
        CameraAndLight(root, new Vector3(-0.4f, 11.5f, -9.5f), new Vector3(1.4f, 0.8f, 43f), false);
        ApplyTraversalReviewLighting();
        var camera = Camera.main;
        if (camera != null) camera.fieldOfView = 39.5f;
        Save(ReferenceMatchSceneRoot + "/Level01_Traversal_ReferenceMatch_R5_REVIEW.unity");
        Capture(camera, new Vector3(-0.4f, 11.5f, -9.5f), new Vector3(1.4f, 0.8f, 43f),
            ReferenceMatchOutput + "/02_Traversal_ReferenceMatch_R5_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_GateRescue_HUD.png");
    }

    static void BuildReferenceMatchBeachLanding()
    {
        RequireApprovedProductionAsset<GameObject>(ApprovedOpeningAddon);
        EnsureBeachReviewMaterials();
        var root = Begin("LEVEL01_BEACH_REFERENCE_MATCH_R5__NOT_USER_APPROVED");
        BuildBeachReviewWaterAndShore(root);
        BuildBeachReviewFleet(root);
        BuildBeachReviewLandingForce(root);
        BuildBeachReviewHarbor(root);
        ReplaceBeachFortressForReferenceMatch(root);
        ScaleGroup(root, "GROUP__BeachFlagship_ReferenceMatch_R5", new Vector3(-6.5f, 0f, 28f),
            1.30f, "PLAYER__Flagship_REVIEW", "PLAYER__SecondLateenAndHelm_REVIEW",
            "CHARACTER__Hayreddin_OnDeck_REVIEW");
        MoveObject("GROUP__BeachFlagship_ReferenceMatch_R5", new Vector3(1f, 0f, -2f));
        RotateObjectY("CHARACTER__Hayreddin_OnDeck_REVIEW", 138f);
        ApplyHayreddinReviewMaterial("CHARACTER__Hayreddin_OnDeck_REVIEW");
        ScaleLandingCraft(1.22f);
        ValidateReferenceMatchBeach();
        CameraAndLight(root, new Vector3(-5.4f, 14f, 5f), new Vector3(-1.8f, 3.55f, 53.5f), false);
        ApplyBeachReviewLighting();
        ApplyWarmReferenceLighting(0.92f, new Color(1f, 0.89f, 0.72f),
            new Color(0.44f, 0.62f, 0.66f), 0.0014f);
        ApplyReferenceMatchBeachPalette();
        var camera = Camera.main;
        if (camera != null) camera.fieldOfView = 40.5f;
        Save(ReferenceMatchSceneRoot + "/Level01_Beach_ReferenceMatch_R5_REVIEW.unity");
        Capture(camera, new Vector3(-5.4f, 14f, 5f), new Vector3(-1.8f, 3.55f, 53.5f),
            ReferenceMatchOutput + "/03_BeachLanding_ReferenceMatch_R5_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_BeachLanding_HUD.png");
    }

    static void BuildReferenceMatchBossBattle()
    {
        EnsureBossBattleReviewMaterials();
        var root = Begin("LEVEL01_BOSS_REFERENCE_MATCH_R5__NOT_USER_APPROVED");
        BuildLevel01BossBattle(root);
        RestoreBossBattleApprovedFortressScale(root);
        ApplyBossBattleReviewWater();
        ApplyBossBattleFortressPalette();
        BuildBossBattleReviewFeedback(root);
        CameraAndLight(root, new Vector3(-1f, 15.8f, 43f), new Vector3(0f, 5.2f, 100f), false);
        ApplyBossBattleReviewLighting();
        var camera = Camera.main;
        if (camera != null) camera.fieldOfView = 34f;
        ValidateBossBattleReview();
        Save(ReferenceMatchSceneRoot + "/Level01_Boss_ReferenceMatch_R5_REVIEW.unity");
        Capture(camera, new Vector3(-1f, 15.8f, 43f), new Vector3(0f, 5.2f, 100f),
            ReferenceMatchOutput + "/04_BossBattle_ReferenceMatch_R5_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_BossBattle_HUD.png");
    }

    static void BuildReferenceMatchBenchmark()
    {
        EnsureBossBattleReviewMaterials();
        RequireApprovedProductionAsset<GameObject>(ApprovedOpeningAddon);
        var root = Begin("LEVEL01_BENCHMARK_REFERENCE_MATCH_R5__NOT_USER_APPROVED");
        BuildLevel01BossBattle(root);
        ConfigureBenchmarkTierALods();
        RestoreBossBattleApprovedFortressScale(root);
        ApplyBossBattleReviewWater();
        ApplyBossBattleFortressPalette();
        BuildBossBattleReviewFeedback(root);
        BuildBenchmarkArtGateAndFeedback(root);
        BuildBenchmarkArtReviewControls(root);
        ScaleObject("PLAYER__BattleFlagship__LOD_GROUP", 1.55f, new Vector3(0f, 0f, -2.5f));
        ScaleObject("GATE__Benchmark_x4_REVIEW", 0.82f, new Vector3(0f, 0f, 0.5f));
        RotateObjectY("CHARACTER__Hayreddin_Battle", 168f);
        ApplyHayreddinReviewMaterial("CHARACTER__Hayreddin_Battle");
        CameraAndLight(root, new Vector3(0f, 17.5f, 12f), new Vector3(0f, 4.5f, 91f), false);
        BuildBenchmarkArtAudioReview(root);
        ApplyBossBattleReviewLighting();
        var camera = Camera.main;
        if (camera != null) camera.fieldOfView = 34.5f;
        ValidateBenchmarkArtReview();
        Save(ReferenceMatchSceneRoot + "/Level01_Benchmark_ReferenceMatch_R5_REVIEW.unity");
        Capture(camera, new Vector3(0f, 17.5f, 12f), new Vector3(0f, 4.5f, 91f),
            ReferenceMatchOutput + "/05_Benchmark_ReferenceMatch_R5_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_BossBattle_HUD.png");
    }

    static void ScaleGroup(Transform root, string name, Vector3 pivot, float scale,
        params string[] objectNames)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        group.position = pivot;
        for (var index = 0; index < objectNames.Length; index++)
        {
            var value = GameObject.Find(objectNames[index]);
            if (value != null) value.transform.SetParent(group, true);
        }
        group.localScale = Vector3.one * scale;
    }

    static void ScaleLandingCraft(float scale)
    {
        foreach (var value in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (value.name.StartsWith("CRAFT__LandingFan_")) value.transform.localScale *= scale;
    }

    static void ScaleObject(string name, float scale, Vector3 positionOffset)
    {
        var value = GameObject.Find(name);
        if (value == null) throw new MissingReferenceException("Reference-match object is missing: " + name);
        value.transform.localScale *= scale;
        value.transform.position += positionOffset;
    }

    static void MoveObject(string name, Vector3 positionOffset)
    {
        var value = GameObject.Find(name);
        if (value == null) throw new MissingReferenceException("Reference-match object is missing: " + name);
        value.transform.position += positionOffset;
    }

    static void RotateObjectY(string name, float degrees)
    {
        var value = GameObject.Find(name);
        if (value != null) value.transform.Rotate(Vector3.up, degrees, Space.World);
    }

    static void ApplyHayreddinReviewMaterial(string name)
    {
        var value = GameObject.Find(name);
        if (value == null) return;
        var material = EnsureHayreddinReviewMaterial();
        foreach (var renderer in value.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (var index = 0; index < materials.Length; index++)
                if (materials.Length == 1 ||
                    (materials[index] != null && materials[index].name.Contains("Hayreddin")))
                    materials[index] = material;
            renderer.sharedMaterials = materials;
        }
    }

    static Material EnsureHayreddinReviewMaterial()
    {
        const string sourcePath =
            "Assets/_Project/Materials/Imported/L01-CHR-001_Hayreddin_Barbarossa.mat";
        const string reviewPath =
            "Assets/_Project/Materials/Review/SeaLion_Hayreddin_R5_REVIEW.mat";
        var source = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
        if (source == null) throw new MissingReferenceException("Hayreddin source material is missing.");
        var review = AssetDatabase.LoadAssetAtPath<Material>(reviewPath);
        if (review == null)
        {
            review = new Material(source) { name = "SeaLion_Hayreddin_R5_REVIEW" };
            AssetDatabase.CreateAsset(review, reviewPath);
        }
        else review.CopyPropertiesFromMaterial(source);
        if (review.HasProperty("_BaseColor"))
            review.SetColor("_BaseColor", new Color(0.92f, 0.90f, 0.86f, 1f));
        if (review.HasProperty("_BumpScale")) review.SetFloat("_BumpScale", 0.72f);
        if (review.HasProperty("_ColorBoost")) review.SetFloat("_ColorBoost", 0.92f);
        if (review.HasProperty("_Contrast")) review.SetFloat("_Contrast", 1.02f);
        if (review.HasProperty("_Saturation")) review.SetFloat("_Saturation", 1.04f);
        if (review.HasProperty("_LightResponse")) review.SetFloat("_LightResponse", 0.48f);
        EditorUtility.SetDirty(review);
        return review;
    }

    static void ApplyReferenceMatchBeachPalette()
    {
        var sky = EnsureReferenceMatchMaterial(BeachReviewSky,
            "Assets/_Project/Materials/Review/SeaLion_Sky_Level01_Beach_R5_REVIEW.mat");
        if (sky.HasProperty("_ZenithColor"))
            sky.SetColor("_ZenithColor", new Color(0.08f, 0.42f, 0.72f, 1f));
        if (sky.HasProperty("_HorizonColor"))
            sky.SetColor("_HorizonColor", new Color(0.48f, 0.73f, 0.82f, 1f));
        if (sky.HasProperty("_CloudColor"))
            sky.SetColor("_CloudColor", new Color(0.88f, 0.90f, 0.88f, 1f));
        if (sky.HasProperty("_CloudStrength")) sky.SetFloat("_CloudStrength", 0.68f);
        if (sky.HasProperty("_Exposure")) sky.SetFloat("_Exposure", 0.88f);
        EditorUtility.SetDirty(sky);
        RenderSettings.skybox = sky;

        var water = EnsureReferenceMatchMaterial(BeachReviewWater,
            "Assets/_Project/Materials/Review/SeaLion_Water_Level01_Beach_R5_REVIEW.mat");
        if (water.HasProperty("_DeepColor"))
            water.SetColor("_DeepColor", new Color(0.005f, 0.06f, 0.09f, 0.96f));
        if (water.HasProperty("_ForegroundColor"))
            water.SetColor("_ForegroundColor", new Color(0.06f, 0.30f, 0.34f, 1f));
        if (water.HasProperty("_HorizonColor"))
            water.SetColor("_HorizonColor", new Color(0.04f, 0.38f, 0.48f, 1f));
        if (water.HasProperty("_ShallowColor"))
            water.SetColor("_ShallowColor", new Color(0.05f, 0.62f, 0.68f, 0.84f));
        if (water.HasProperty("_SpecularStrength")) water.SetFloat("_SpecularStrength", 0.76f);
        EditorUtility.SetDirty(water);
        var surface = GameObject.Find("ENV__AuthoredWaterSurface")?.GetComponent<MeshRenderer>();
        if (surface != null) surface.sharedMaterial = water;

        RenderSettings.ambientLight = new Color(0.36f, 0.39f, 0.39f);
        RenderSettings.ambientIntensity = 0.74f;
        RenderSettings.fogColor = new Color(0.50f, 0.70f, 0.76f);
        RenderSettings.fogDensity = 0.0010f;
    }

    static void ApplyReferenceMatchFortressPalette(string name)
    {
        var fortress = GameObject.Find(name);
        if (fortress == null) throw new MissingReferenceException("Reference-match fortress is missing.");
        Directory.CreateDirectory(BossBattleReviewFortressMaterials);
        foreach (var renderer in fortress.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (var index = 0; index < materials.Length; index++)
                if (materials[index] != null)
                    materials[index] = EnsureBossBattleFortressMaterial(materials[index]);
            renderer.sharedMaterials = materials;
        }
    }

    static Material EnsureReferenceMatchMaterial(string sourcePath, string reviewPath)
    {
        var source = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
        if (source == null) throw new MissingReferenceException("Reference-match source material is missing: " + sourcePath);
        var review = AssetDatabase.LoadAssetAtPath<Material>(reviewPath);
        if (review == null)
        {
            review = new Material(source) { name = Path.GetFileNameWithoutExtension(reviewPath) };
            AssetDatabase.CreateAsset(review, reviewPath);
        }
        else review.CopyPropertiesFromMaterial(source);
        return review;
    }

    static void ReplaceBeachFortressForReferenceMatch(Transform root)
    {
        var oldFortress = GameObject.Find("GROUP__LandingFortress_Right_REVIEW");
        if (oldFortress != null) Object.DestroyImmediate(oldFortress);
        PlaceApprovedLevel01Fortress(root, "GROUP__LandingFortress_ReferenceMatch_R5",
            new Vector3(11f, 0f, 108f), 42f);
        ApplyReferenceMatchFortressPalette("GROUP__LandingFortress_ReferenceMatch_R5");
    }

    static void ValidateReferenceMatchBeach()
    {
        var flagship = GameObject.Find("GROUP__BeachFlagship_ReferenceMatch_R5");
        var fortress = GameObject.Find("GROUP__LandingFortress_ReferenceMatch_R5");
        if (flagship == null || fortress == null)
            throw new MissingReferenceException("Reference-match beach flagship or fortress is missing.");
        var craftCount = 0;
        foreach (var value in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (value.name.StartsWith("CRAFT__LandingFan_")) craftCount++;
        if (craftCount != 7)
            throw new MissingReferenceException("Reference-match beach requires exactly seven landing craft.");
    }

    static void TuneWake(string name, Vector3 scale)
    {
        var wake = GameObject.Find(name);
        if (wake != null) wake.transform.localScale = scale;
    }

    static void ApplyWarmReferenceLighting(float intensity, Color lightColor, Color fogColor,
        float fogDensity)
    {
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.33f, 0.36f, 0.37f);
        RenderSettings.ambientIntensity = 0.72f;
        RenderSettings.reflectionIntensity = 0.42f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = fogColor;
        RenderSettings.fogDensity = fogDensity;
        var key = GameObject.Find("KEY_LIGHT__Blockout")?.GetComponent<Light>();
        if (key == null) return;
        key.intensity = intensity;
        key.color = lightColor;
        key.shadows = LightShadows.Soft;
    }
}
