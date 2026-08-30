using System.IO;
using SeaLion.Presentation.Vfx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string TraversalReviewScene = "Assets/_Project/Scenes/Review/Level01_Traversal_GateRescue_REVIEW.unity";
    const string TraversalReviewOutput = "Artifacts/Local/Approval/Level01Traversal";
    const string TraversalReviewWater = "Assets/_Project/Materials/Review/SeaLion_Water_Level01_Traversal_REVIEW.mat";
    const string TraversalReviewWake = "Assets/_Project/Materials/Review/SeaLion_Foam_Level01_Traversal_REVIEW.mat";
    const string TraversalReviewSky = "Assets/_Project/Materials/Review/SeaLion_Sky_Level01_Traversal_REVIEW.mat";
    const string TraversalGateAsset =
        EnvironmentRoot + "L01-GAT-001_Multiplier_Gate_OpenArch_R1_REVIEW.fbx";
    const string TraversalGateMaterialSource =
        EnvironmentRoot + "L01-GAT-001_Multiplier_Gate_Arch_Buoy_Optimized.fbx";

    [MenuItem("Lion of the Seas/Build Level 01 Traversal Gate Rescue REVIEW %#g")]
    public static void BuildLevel01TraversalApprovalReview()
    {
        Directory.CreateDirectory("Assets/_Project/Scenes/Review");
        Directory.CreateDirectory("Assets/_Project/Materials/Review");
        Directory.CreateDirectory(TraversalReviewOutput);
        RequireApprovedProductionAsset<GameObject>(ApprovedOpeningAddon);
        RequireApprovedProductionAsset<Material>(ApprovedOpeningWater);
        RequireApprovedProductionAsset<Material>(ApprovedOpeningWake);
        EnsureTraversalReviewMaterials();

        var root = Begin("LEVEL01_TRAVERSAL_GATE_RESCUE_REVIEW__NOT_PRODUCTION");
        BuildTraversalReviewWater(root);
        BuildTraversalReviewFleet(root);
        BuildTraversalReviewObjectives(root);
        BuildApprovedOpeningCoastAndCity(root);
        TuneTraversalReviewBackground();
        ValidateTraversalReviewHierarchy();
        CameraAndLight(root, new Vector3(-0.4f, 11.4f, -10f), new Vector3(1.6f, 0.6f, 43f), false);
        ApplyTraversalReviewLighting();
        Save(TraversalReviewScene);
        CaptureTraversalReviewImages();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Level 01 Traversal Gate/Rescue REVIEW built and captured. Production was not modified.");
    }

    [MenuItem("Lion of the Seas/Capture Level 01 Traversal Gate Rescue REVIEW")]
    public static void CaptureLevel01TraversalApprovalReview()
    {
        EditorSceneManager.OpenScene(TraversalReviewScene, OpenSceneMode.Single);
        Directory.CreateDirectory(TraversalReviewOutput);
        CaptureTraversalReviewImages();
        AssetDatabase.Refresh();
    }

    static void BuildTraversalReviewWater(Transform root)
    {
        Water(root, 125f, false, TraversalReviewWater);
        Wake(root, "VFX__FlagshipWake_REVIEW", new Vector3(-3.2f, 0.038f, 7.2f),
            new Vector3(2.4f, 1f, 4.6f), TraversalReviewWake);
    }

    static void EnsureTraversalReviewMaterials()
    {
        var sourceWater = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Water/SeaLion_Water_Level01.mat");
        var water = AssetDatabase.LoadAssetAtPath<Material>(TraversalReviewWater);
        if (water == null && sourceWater != null)
        {
            water = new Material(sourceWater) { name = "SeaLion_Water_Level01_Traversal_REVIEW" };
            AssetDatabase.CreateAsset(water, TraversalReviewWater);
        }
        if (water != null)
        {
            water.SetColor("_ForegroundColor", new Color(0.03f, 0.235f, 0.245f, 1f));
            water.SetColor("_HorizonColor", new Color(0.01f, 0.29f, 0.36f, 1f));
            water.SetColor("_ShallowColor", new Color(0f, 0.58f, 0.68f, 0.82f));
            water.SetFloat("_SpecularStrength", 0.65f);
            water.SetFloat("_NormalStrength", 1.25f);
            water.SetFloat("_WaveFrequency", 1.8f);
            water.SetFloat("_FoamStrength", 0.42f);
            EditorUtility.SetDirty(water);
        }

        var sourceWake = AssetDatabase.LoadAssetAtPath<Material>(ApprovedOpeningWake);
        var wake = AssetDatabase.LoadAssetAtPath<Material>(TraversalReviewWake);
        if (wake == null && sourceWake != null)
        {
            wake = new Material(sourceWake) { name = "SeaLion_Foam_Level01_Traversal_REVIEW" };
            AssetDatabase.CreateAsset(wake, TraversalReviewWake);
        }
        if (wake != null)
        {
            wake.SetFloat("_FoamStrength", 0.82f);
            wake.SetFloat("_EffectAlphaBoost", 0.78f);
            wake.SetFloat("_Opacity", 0.22f);
            EditorUtility.SetDirty(wake);
        }

        var sourceSky = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
        var sky = AssetDatabase.LoadAssetAtPath<Material>(TraversalReviewSky);
        if (sky == null && sourceSky != null)
        {
            sky = new Material(sourceSky) { name = "SeaLion_Sky_Level01_Traversal_REVIEW" };
            AssetDatabase.CreateAsset(sky, TraversalReviewSky);
        }
        if (sky == null) return;
        sky.SetFloat("_Exposure", 0.78f);
        sky.SetColor("_ZenithColor", new Color(0.09f, 0.39f, 0.65f, 1f));
        sky.SetColor("_HorizonColor", new Color(0.39f, 0.60f, 0.69f, 1f));
        sky.SetColor("_CloudColor", new Color(0.72f, 0.73f, 0.71f, 1f));
        sky.SetFloat("_CloudStrength", 0.92f);
        EditorUtility.SetDirty(sky);
    }

    static void ApplyTraversalReviewLighting()
    {
        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(TraversalReviewSky);
        RenderSettings.ambientLight = new Color(0.30f, 0.34f, 0.36f);
        RenderSettings.ambientIntensity = 0.76f;
        RenderSettings.fogColor = new Color(0.37f, 0.53f, 0.59f);
        var key = GameObject.Find("KEY_LIGHT__Blockout");
        var light = key == null ? null : key.GetComponent<Light>();
        if (light != null) light.intensity = 0.82f;
    }

    static void BuildTraversalReviewFleet(Transform root)
    {
        var ship = Model(root, "PLAYER__Flagship_REVIEW", Level01ReferenceShip,
            new Vector3(-1.2f, 0.05f, 14.5f), Vector3.one * 9.5f, new Vector3(-90f, 350f, 0f));
        ApprovedOpeningModel(root, "PLAYER__SecondLateenAndHelm_REVIEW", ApprovedOpeningAddon,
            new Vector3(-1.2f, 0.05f, 14.5f), Vector3.one * 5.4f, new Vector3(-90f, 350f, 0f));
        Model(root, "CHARACTER__Hayreddin_OnDeck_REVIEW", Level01HeroPose,
            new Vector3(-0.4f, 2.4f, 9.5f), Vector3.one * 1.3f, new Vector3(0f, -10f, 0f));
        var banner = Model(root, "PROP__FlagshipLionWaveBanner_REVIEW",
            EnvironmentRoot + "L01-PRP-002_Lion_Wave_Banner_Optimized.fbx",
            new Vector3(-1f, 7.75f, 14.43f), Vector3.one * 0.97f, new Vector3(-90f, 350f, 0f));
        if (ship != null && banner != null) banner.transform.SetParent(ship.transform, true);
        TraversalReviewCraftFormation(root);
    }

    static void TraversalReviewCraftFormation(Transform root)
    {
        var positions = new[]
        {
            new Vector3(-1.7f, 0.05f, 42f), new Vector3(1.4f, 0.05f, 47f),
            new Vector3(-2.6f, 0.05f, 51f), new Vector3(2.1f, 0.05f, 56f),
            new Vector3(-0.4f, 0.05f, 61f)
        };
        var headings = new[] { 4f, -5f, 5f, -4f, 1f };
        for (var index = 0; index < positions.Length; index++)
        {
            Model(root, "FRIENDLY__GateCraft_" + index,
                ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx", positions[index],
                Vector3.one * 2.1f, new Vector3(-90f, headings[index], 0f));
            CraftCrew(root, "CREW__GateCraft_" + index,
                positions[index] + Vector3.up * 0.2f, headings[index], 3);
            Wake(root, "VFX__GateCraftWake_" + index,
                positions[index] + new Vector3(0f, -0.01f, -0.7f),
                new Vector3(0.9f, 1f, 1.7f), TraversalReviewWake);
        }
    }

    static void BuildTraversalReviewObjectives(Transform root)
    {
        var gate = Model(root, "GATE__Multiplier_x4_REVIEW", TraversalGateAsset,
            new Vector3(0.7f, 0.1f, 61f), Vector3.one * 6f, new Vector3(-90f, 0f, 0f));
        if (gate == null) throw new FileNotFoundException("Registered L01-GAT-001 gate is missing.");
        var gateMaterial = ImportedMaterial(TraversalGateMaterialSource);
        if (gateMaterial != null)
            foreach (var renderer in gate.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = gateMaterial;
        GateValueBadge(root, new Vector3(0.7f, 8.2f, 58.8f));
        GateValueLabel(root, new Vector3(0.7f, 8.2f, 58.5f), "X4");
        Model(root, "RESCUE__CaptiveSailmakers_REVIEW",
            EnvironmentRoot + "L01-PRP-004_Captive_Sailmakers_Rescue_Raft_Cage_Optimized.fbx",
            new Vector3(3.6f, 0.05f, 22f), Vector3.one * 2.3f, new Vector3(-90f, -16f, 0f));
        Model(root, "ENEMY__Patrol_Left_REVIEW", ShipRoot + "L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx",
            new Vector3(-7f, 0.03f, 58f), Vector3.one * 3.1f, new Vector3(-90f, 12f, 0f));
        Model(root, "ENEMY__Patrol_Right_REVIEW", ShipRoot + "L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx",
            new Vector3(7f, 0.03f, 64f), Vector3.one * 3.1f, new Vector3(-90f, -12f, 0f));
        Model(root, "ENEMY__Patrol_FarLeft_REVIEW", ShipRoot + "L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx",
            new Vector3(-10f, 0.03f, 70f), Vector3.one * 2.7f, new Vector3(-90f, 18f, 0f));
    }

    static void TuneTraversalReviewBackground()
    {
        foreach (var name in new[]
        {
            "ENV__LeftCoastalCliff", "ENV__RightArtilleryCliff", "ENV__LeftShoreFoot",
            "ENV__RightShoreFoot", "ENV__LeftCrownVegetation", "ENV__RightCrownVegetation",
            "CITY__FortressWall", "CITY__Gate", "CITY__Tower_Left", "CITY__Tower_Right",
            "CITY__TerraceHouse_00", "CITY__TerraceHouse_01", "CITY__TerraceHouse_02",
            "CITY__TerraceHouse_03"
        })
        {
            var value = GameObject.Find(name);
            if (value != null) value.transform.position += Vector3.up * 8f;
        }
        foreach (var name in new[] { "FORTRESS__RightCliffTower", "FORTRESS__RightCliffCannon" })
        {
            var value = GameObject.Find(name);
            if (value != null) value.transform.position += Vector3.up * 4.5f;
        }
        var backdrop = GameObject.Find("CITY__MountainBackdrop");
        if (backdrop != null) backdrop.transform.position += Vector3.up * 5.5f;
    }

    static void ValidateTraversalReviewHierarchy()
    {
        var gate = GameObject.Find("GATE__Multiplier_x4_REVIEW");
        if (gate == null || PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gate) != TraversalGateAsset)
            throw new InvalidDataException("Traversal review must use the registered L01-GAT-001 gate.");
        var ship = GameObject.Find("PLAYER__Flagship_REVIEW");
        var banner = GameObject.Find("PROP__FlagshipLionWaveBanner_REVIEW");
        if (ship == null || banner == null || !banner.transform.IsChildOf(ship.transform))
            throw new InvalidDataException("The Lion-Wave banner must remain attached to the flagship.");
        foreach (var effect in Object.FindObjectsByType<WaterVfxEffect>(FindObjectsSortMode.None))
        {
            if (!effect.name.Contains("Wake")) continue;
            if (effect.transform.localScale.z > 4.61f)
                throw new InvalidDataException($"Traversal wake is too long: {effect.name}.");
            var renderer = effect.GetComponent<MeshRenderer>();
            if (renderer != null && AssetDatabase.GetAssetPath(renderer.sharedMaterial) != TraversalReviewWake)
                throw new InvalidDataException($"Traversal wake uses an unexpected material: {effect.name}.");
        }
    }

    static void CaptureTraversalReviewImages()
    {
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Traversal review camera is missing.");
        camera.fieldOfView = 40f;
        Capture(camera, new Vector3(-0.4f, 11.4f, -10f), new Vector3(1.6f, 0.6f, 43f),
            TraversalReviewOutput + "/01_Traversal_Full_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_GateRescue_HUD.png");
        Capture(camera, new Vector3(-0.4f, 11.4f, -10f), new Vector3(1.6f, 0.6f, 43f),
            TraversalReviewOutput + "/02_Traversal_NoHUD_REVIEW.png");
        camera.fieldOfView = 27f;
        Capture(camera, new Vector3(-1f, 11f, 3f), new Vector3(0.7f, 3.5f, 55f),
            TraversalReviewOutput + "/03_Gate_Rescue_Detail_REVIEW.png");
        camera.fieldOfView = 40f;
    }
}
