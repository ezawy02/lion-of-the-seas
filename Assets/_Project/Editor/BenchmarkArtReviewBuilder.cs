using System.IO;
using SeaLion.Core.Definitions;
using SeaLion.Presentation.ArtReview;
using SeaLion.Presentation.Quality;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string BenchmarkArtReviewScene = "Assets/_Project/Scenes/Benchmark_Art.unity";
    const string BenchmarkArtReviewOutput = "Artifacts/Local/Approval/BenchmarkArt";
    const string BenchmarkPrimaryProfile = "Assets/_Project/Settings/Quality/Primary.asset";
    const string BenchmarkReducedProfile = "Assets/_Project/Settings/Quality/Reduced.asset";

    [MenuItem("Lion of the Seas/Build Benchmark Art R3 Gate Energy REVIEW")]
    public static void BuildBenchmarkArtR3Review()
    {
        Directory.CreateDirectory("Assets/_Project/Scenes");
        Directory.CreateDirectory(BenchmarkArtReviewOutput);
        EnsureBossBattleReviewMaterials();
        RequireApprovedProductionAsset<GameObject>(ApprovedOpeningAddon);

        var root = Begin("BENCHMARK_ART_R3_GATE_ENERGY__NOT_USER_APPROVED");
        BuildLevel01BossBattle(root);
        ConfigureBenchmarkTierALods();
        RestoreBossBattleApprovedFortressScale(root);
        ApplyBossBattleReviewWater();
        ApplyBossBattleFortressPalette();
        BuildBossBattleReviewFeedback(root);
        BuildBenchmarkArtGateAndFeedback(root);
        BuildBenchmarkArtReviewControls(root);
        CameraAndLight(root, new Vector3(0f, 22f, 18f), new Vector3(0f, 4.4f, 91f), false);
        BuildBenchmarkArtAudioReview(root);
        var camera = Camera.main;
        if (camera != null) camera.fieldOfView = 38f;
        ApplyBossBattleReviewLighting();
        ValidateBenchmarkArtReview();
        Save(BenchmarkArtReviewScene);
        CaptureBenchmarkArtReviewProfiles();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Benchmark_Art R3 gate-energy REVIEW built and captured. User approval is still required.");
    }

    [MenuItem("Lion of the Seas/Capture Benchmark Art R3 Gate Energy REVIEW")]
    public static void CaptureBenchmarkArtR3Review()
    {
        EditorSceneManager.OpenScene(BenchmarkArtReviewScene, OpenSceneMode.Single);
        Directory.CreateDirectory(BenchmarkArtReviewOutput);
        ValidateBenchmarkArtReview();
        CaptureBenchmarkArtReviewProfiles();
        AssetDatabase.Refresh();
    }

    [MenuItem("Lion of the Seas/Open Benchmark Art R3 Gate Energy REVIEW In Game View")]
    public static void OpenBenchmarkArtR3Review()
    {
        EditorSceneManager.OpenScene(BenchmarkArtReviewScene, OpenSceneMode.Single);
        var camera = Camera.main;
        Selection.activeGameObject = camera == null ? null : camera.gameObject;
        if (camera != null) EditorGUIUtility.PingObject(camera.gameObject);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Opened Benchmark_Art R3 gate-energy REVIEW. This revision is not user-approved.");
    }

    static void BuildBenchmarkArtGateAndFeedback(Transform root)
    {
        var gatePosition = new Vector3(1.2f, 0.1f, 67f);
        var gate = Model(root, "GATE__Benchmark_x4_REVIEW", TraversalGateAsset,
            gatePosition, Vector3.one * 4.7f, new Vector3(-90f, 0f, 0f));
        if (gate == null) throw new FileNotFoundException("Benchmark x4 gate asset is missing.");
        var gateMaterial = ImportedMaterial(TraversalGateMaterialSource);
        if (gateMaterial != null)
            foreach (var renderer in gate.GetComponentsInChildren<Renderer>())
                renderer.sharedMaterial = gateMaterial;
        BuildGateEnergyReview(root, "Benchmark", gatePosition, 9.0f, 7.8f);

        GateValueBadge(root, new Vector3(1.2f, 6.7f, 65.3f));
        GateValueLabel(root, new Vector3(1.2f, 6.7f, 65f), "X4");
        GateValueLabel(root, new Vector3(1.2f, 4.05f, 64.9f), "60 > 240");
        var forceFeedback = GameObject.Find("UI3D__GateValue_60 > 240")?.GetComponent<TextMesh>();
        if (forceFeedback != null)
        {
            forceFeedback.characterSize = 0.075f;
            forceFeedback.fontSize = 72;
            forceFeedback.color = new Color(0.97f, 0.84f, 0.57f);
        }
        WaterEffect(root, "VFX__BenchmarkGateFoam", "Assets/_Project/VFX/FoamPatch.prefab",
            gatePosition + new Vector3(0f, -0.04f, 0f), new Vector3(3.2f, 1f, 2.1f), null);
        Wake(root, "VFX__BenchmarkFlagshipWake", new Vector3(-3.8f, 0.04f, 49.5f),
            new Vector3(2.1f, 1f, 3.5f), TraversalReviewWake);
        WaterEffect(root, "VFX__BenchmarkLandingContact", "Assets/_Project/VFX/LandingSplash.prefab",
            new Vector3(5.5f, 0.06f, 72f), new Vector3(2.4f, 1f, 2.4f), null);
        Model(root, "REWARD__BenchmarkBlueprintChest",
            EnvironmentRoot + "L01-PRP-005_Blueprint_Reward_Chest_Optimized.fbx",
            new Vector3(8.5f, 0.3f, 82f), Vector3.one * 0.72f, new Vector3(-90f, 165f, 0f));
    }

    static void BuildBenchmarkArtReviewControls(Transform root)
    {
        var motion = root.gameObject.AddComponent<BenchmarkArtMotionPreview>();
        var qualityObject = new GameObject("QUALITY__BenchmarkProfileController");
        qualityObject.transform.SetParent(root);
        var quality = qualityObject.AddComponent<QualityProfileController>();
        var serialized = new SerializedObject(quality);
        serialized.FindProperty("primary").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<QualityProfile>(BenchmarkPrimaryProfile);
        serialized.FindProperty("reduced").objectReferenceValue =
            AssetDatabase.LoadAssetAtPath<QualityProfile>(BenchmarkReducedProfile);
        serialized.ApplyModifiedPropertiesWithoutUndo();
        var seedMarker = new GameObject("BENCHMARK_SEED__" + motion.Seed);
        seedMarker.transform.SetParent(root);
    }

    static void ValidateBenchmarkArtReview()
    {
        foreach (var name in new[]
        {
            "PLAYER__BattleFlagship", "VFX__BenchmarkFlagshipWake", "GATE__Benchmark_x4_REVIEW",
            "BOSS__HarborGuardian", "GROUP__BattleFortress_Approved",
            "REWARD__BenchmarkBlueprintChest", "QUALITY__BenchmarkProfileController",
            "PORTRAIT_CAMERA__Gameplay"
        })
            if (GameObject.Find(name) == null)
                throw new MissingReferenceException("Benchmark_Art R3 object is missing: " + name);

        ValidateGateEnergyReview("Benchmark");
        ValidateBenchmarkTierALods();
        ValidateBenchmarkArtAudioReview();

        var friendlyCount = ChildCount("FRIENDLY__LandingForce_Front") +
            ChildCount("FRIENDLY__LandingForce_Rear");
        var hostileCount = ChildCount("HOSTILE__Defenders_Front") +
            ChildCount("HOSTILE__Defenders_Rear");
        if (friendlyCount < 60 || hostileCount < 60)
            throw new InvalidDataException(
                $"Benchmark crowd is below contract: friendly={friendlyCount}, hostile={hostileCount}.");
        if (GameObject.Find("BENCHMARK_SEED__2701") == null)
            throw new InvalidDataException("Benchmark review must use deterministic seed 2701.");
        Debug.Log($"Benchmark_Art R3 structure passed: friendly={friendlyCount}, hostile={hostileCount}.");
    }

    static int ChildCount(string groupName)
    {
        var group = GameObject.Find(groupName);
        return group == null ? 0 : group.transform.childCount;
    }

    static void CaptureBenchmarkArtReviewProfiles()
    {
        var camera = Camera.main;
        var controller = Object.FindFirstObjectByType<QualityProfileController>();
        var primary = AssetDatabase.LoadAssetAtPath<QualityProfile>(BenchmarkPrimaryProfile);
        var reduced = AssetDatabase.LoadAssetAtPath<QualityProfile>(BenchmarkReducedProfile);
        if (camera == null || controller == null || primary == null || reduced == null)
            throw new MissingReferenceException("Benchmark capture camera or quality profiles are missing.");

        var previousLodBias = QualitySettings.lodBias;
        var previousShadowDistance = QualitySettings.shadowDistance;
        var position = new Vector3(0f, 22f, 18f);
        var target = new Vector3(0f, 4.4f, 91f);
        camera.fieldOfView = 38f;
        try
        {
            controller.Apply(primary);
            Capture(camera, position, target, BenchmarkArtReviewOutput + "/01_Primary_REVIEW.png",
                "Assets/_Project/Art/UI/Level01_BossBattle_HUD.png");
            controller.Apply(reduced);
            Capture(camera, position, target, BenchmarkArtReviewOutput + "/02_Reduced_REVIEW.png",
                "Assets/_Project/Art/UI/Level01_BossBattle_HUD.png");
            Capture(camera, position, target, BenchmarkArtReviewOutput + "/03_Reduced_NoHUD_REVIEW.png");
        }
        finally
        {
            QualitySettings.lodBias = previousLodBias;
            QualitySettings.shadowDistance = previousShadowDistance;
        }
    }
}
