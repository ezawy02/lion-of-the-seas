using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string ApprovedOpeningAddon = ShipRoot + "L01-SHP-004_AftLateen_Helm_Addon_Optimized.fbx";
    const string ApprovedOpeningWater = "Assets/_Project/Materials/Water/SeaLion_Water_Level01_Opening_Approved.mat";
    const string ApprovedOpeningWake = "Assets/_Project/Materials/Water/SeaLion_Foam_Level01_OpeningWake_Approved.mat";
    const string ApprovedMuzzleFlash = "Assets/_Project/Materials/VFX/L01_CannonMuzzleFlash_Approved.mat";
    const string ApprovedCapture = "Artifacts/Local/Approval/Level01Opening/01_Opening_Full_REVIEW.png";
    const string ApprovedReviewScene = "Assets/_Project/Scenes/Review/Level01_Opening_Approval_REVIEW.unity";
    const string ApprovalReport = "Artifacts/Local/Approval/Level01Opening/APPROVAL_REPORT.md";

    [MenuItem("Lion of the Seas/Transfer Approved Level 01 Opening to Production %#t")]
    public static void TransferApprovedLevel01OpeningToProduction()
    {
        VerifyApprovedEvidence();
        PromoteApprovedAssets();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        BuildLevel01();
        ValidateApprovedOpeningHierarchy();
        AssetDatabase.SaveAssets();
        CaptureLevel01Evidence();

        EditorSceneManager.OpenScene(SceneRoot + "Level_01_HundredSails.unity", OpenSceneMode.Single);
        SetOnlyLevel01Phase("PHASE__Opening_ReferenceMatch");
        Debug.Log("Approved Level 01 opening transferred to production and captured successfully.");
    }

    static void VerifyApprovedEvidence()
    {
        VerifySha256(ApprovedCapture, "a5eac5eb4080988592354ab626af8105580da0d50cf41e0de3974593c72a20ea");
        VerifySha256(ApprovedReviewScene, "7a0e41d8d31b85a1b77befe1cafbe5fc84dc0878178600404fba812f2a1344f1");
        if (!File.Exists(ApprovalReport) ||
            !File.ReadAllText(ApprovalReport).Contains("USER APPROVED FOR PRODUCTION TRANSFER"))
            throw new InvalidDataException("The exact opening revision does not have recorded user approval.");
    }

    static void VerifySha256(string path, string expected)
    {
        if (!File.Exists(path)) throw new FileNotFoundException("Approved evidence is missing.", path);
        using var stream = File.OpenRead(path);
        using var sha = SHA256.Create();
        var actual = BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        if (!string.Equals(actual, expected, StringComparison.Ordinal))
            throw new InvalidDataException($"Approved evidence hash changed: {path}\nExpected {expected}\nActual   {actual}");
    }

    static void PromoteApprovedAssets()
    {
        Directory.CreateDirectory("Assets/_Project/Materials/VFX");
        PromoteApprovedAsset(OpeningApprovalAddon, ApprovedOpeningAddon);
        PromoteApprovedAsset(OpeningApprovalWater, ApprovedOpeningWater);
        PromoteApprovedAsset(OpeningApprovalWake, ApprovedOpeningWake);
        PromoteApprovedAsset("Assets/_Project/Materials/Review/L01_CannonMuzzleFlash_REVIEW.mat", ApprovedMuzzleFlash);
    }

    static void PromoteApprovedAsset(string source, string destination)
    {
        if (AssetDatabase.LoadMainAssetAtPath(source) == null)
            throw new FileNotFoundException("Approved source asset is missing.", source);
        if (AssetDatabase.LoadMainAssetAtPath(destination) != null) return;
        if (!AssetDatabase.CopyAsset(source, destination))
            throw new IOException($"Unable to promote approved asset from {source} to {destination}.");
    }

    static void BuildApprovedLevel01Opening(Transform root)
    {
        RequireApprovedProductionAsset<GameObject>(ApprovedOpeningAddon);
        RequireApprovedProductionAsset<Material>(ApprovedOpeningWater);
        RequireApprovedProductionAsset<Material>(ApprovedOpeningWake);
        RequireApprovedProductionAsset<Material>(ApprovedMuzzleFlash);

        BuildApprovedOpeningWater(root);
        BuildApprovedOpeningFleet(root);
        BuildApprovedOpeningCoastAndCity(root);
    }

    static void RequireApprovedProductionAsset<T>(string path) where T : UnityEngine.Object
    {
        if (AssetDatabase.LoadAssetAtPath<T>(path) == null)
            throw new FileNotFoundException("Approved production asset is missing. Run the transfer menu first.", path);
    }

    static void BuildApprovedOpeningWater(Transform root)
    {
        Water(root, 125f, false, ApprovedOpeningWater);
        Wake(root, "VFX__FlagshipWake", new Vector3(-1.4f, 0.038f, 14.3f),
            new Vector3(1.85f, 1f, 1f), ApprovedOpeningWake);
        TargetRing(root, "VFX__IncomingCannonTargetRing", new Vector3(5.1f, 0.045f, 38f), 2.15f);
        CannonSplash(root, "VFX__IncomingCannonSplash", new Vector3(5.1f, 0.06f, 38f), 2.25f, 4.6f);
    }

    static void BuildApprovedOpeningFleet(Transform root)
    {
        Model(root, "PLAYER__Flagship", Level01ReferenceShip,
            new Vector3(-1.4f, 0.05f, 15f), Vector3.one * 8.8f, new Vector3(-90f, 350f, 0f));
        ApprovedOpeningModel(root, "PLAYER__SecondLateenAndHelm", ApprovedOpeningAddon,
            new Vector3(-1.4f, 0.05f, 15f), Vector3.one * 5f, new Vector3(-90f, 350f, 0f));
        Model(root, "CHARACTER__Hayreddin_OnDeck", Level01HeroPose,
            new Vector3(-1.4f, 2.82f, 10.75f), Vector3.one * 1.52f, new Vector3(0f, -10f, 0f));
        Model(root, "PROP__FlagshipLionWaveBanner",
            EnvironmentRoot + "L01-PRP-002_Lion_Wave_Banner_Optimized.fbx",
            new Vector3(-1.05f, 7.2f, 14.9f), Vector3.one * 0.9f, new Vector3(-90f, 350f, 0f));

        Model(root, "ESCORT__Port", ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx",
            new Vector3(-5.7f, 0.03f, 16.5f), Vector3.one * 2.6f, new Vector3(-90f, 4f, 0f));
        CraftCrew(root, "CREW__OpeningPort", new Vector3(-5.7f, 0.25f, 16.5f), 4f, 4);
        Model(root, "ESCORT__Starboard", ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx",
            new Vector3(4.8f, 0.03f, 17f), Vector3.one * 2.6f, new Vector3(-90f, -5f, 0f));
        CraftCrew(root, "CREW__OpeningStarboard", new Vector3(4.8f, 0.25f, 17f), -5f, 4);
        Model(root, "ENEMY__Patrol_Left", ShipRoot + "L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx",
            new Vector3(-1.5f, 0.03f, 43f), Vector3.one * 4f, new Vector3(-90f, 8f, 0f));
        Model(root, "ENEMY__Patrol_Right", ShipRoot + "L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx",
            new Vector3(6f, 0.03f, 47f), Vector3.one * 4.15f, new Vector3(-90f, -8f, 0f));
        Wake(root, "VFX__EnemyWake_Left", new Vector3(-3.8f, 0.034f, 42.4f), new Vector3(0.9f, 1f, 1.7f));
        Wake(root, "VFX__EnemyWake_Right", new Vector3(6f, 0.036f, 46.4f), new Vector3(0.9f, 1f, 1.7f));
    }

    static void BuildApprovedOpeningCoastAndCity(Transform root)
    {
        Model(root, "ENV__LeftCoastalCliff", EnvironmentRoot + "L01-ENV-010_Left_Coastal_Cliff_Optimized.fbx",
            new Vector3(-16f, -8f, 80f), Vector3.one * 13f, new Vector3(-90f, 12f, 0f));
        Model(root, "ENV__RightArtilleryCliff", EnvironmentRoot + "L01-ENV-011_Right_Artillery_Cliff_Optimized.fbx",
            new Vector3(22f, -6.5f, 86f), Vector3.one * 11f, new Vector3(-90f, -9f, 0f));
        Model(root, "ENV__LeftShoreFoot", EnvironmentRoot + "L01-ENV-009_Shoreline_Rock_Sand_Cluster_Optimized.fbx",
            new Vector3(-12.8f, -0.15f, 73.5f), Vector3.one * 2.5f, new Vector3(-90f, 24f, 0f));
        Model(root, "ENV__RightShoreFoot", EnvironmentRoot + "L01-ENV-007_Limestone_Rock_Cluster_Optimized.fbx",
            new Vector3(13.8f, -0.1f, 78f), Vector3.one * 2.2f, new Vector3(-90f, -18f, 0f));
        Model(root, "ENV__LeftCrownVegetation", EnvironmentRoot + "L01-ENV-008_Coastal_Vegetation_Clump_Optimized.fbx",
            new Vector3(-13.5f, 4.3f, 79f), Vector3.one * 1.7f, new Vector3(-90f, 18f, 0f));
        Model(root, "ENV__RightCrownVegetation", EnvironmentRoot + "L01-ENV-008_Coastal_Vegetation_Clump_Optimized.fbx",
            new Vector3(14.8f, 3.4f, 81f), Vector3.one * 1.5f, new Vector3(-90f, -22f, 0f));
        BuildApprovedOpeningCity(root);
        BuildApprovedOpeningArtillery(root);
    }

    static void BuildApprovedOpeningCity(Transform root)
    {
        Model(root, "CITY__MountainBackdrop", EnvironmentRoot + "L01-ENV-012_Mediterranean_Mountain_City_Backdrop_Optimized.fbx",
            new Vector3(1f, 0.1f, 111f), Vector3.one * 27f, new Vector3(-90f, 180f, 0f));
        Model(root, "CITY__FortressWall", EnvironmentRoot + "L01-ENV-001_Fortress_Wall_Module_Optimized.fbx",
            new Vector3(0f, 0.35f, 107f), Vector3.one * 7.2f, new Vector3(-90f, 0f, 0f));
        Model(root, "CITY__Gate", EnvironmentRoot + "L01-ENV-003_Fortress_Main_Gate_Module_Optimized.fbx",
            new Vector3(0f, 0.35f, 106f), Vector3.one * 1.85f, new Vector3(-90f, 0f, 0f));
        Model(root, "CITY__Tower_Left", EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
            new Vector3(-7.2f, 0.35f, 107f), Vector3.one * 1.85f, new Vector3(-90f, 4f, 0f));
        Model(root, "CITY__Tower_Right", EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
            new Vector3(7.4f, 0.35f, 108f), Vector3.one * 1.7f, new Vector3(-90f, -5f, 0f));
        var positions = new[]
        {
            new Vector3(-10.5f, 0.55f, 109f), new Vector3(-4.9f, 1.25f, 110f),
            new Vector3(3.8f, 1.55f, 111f), new Vector3(9.8f, 0.75f, 109f)
        };
        for (var index = 0; index < positions.Length; index++)
            Model(root, $"CITY__TerraceHouse_{index:00}", EnvironmentRoot + "L01-ENV-005_Mediterranean_Coastal_House_Optimized.fbx",
                positions[index], Vector3.one * (1.35f + index * 0.08f),
                new Vector3(-90f, index % 2 == 0 ? 16f : -18f, 0f));
    }

    static void BuildApprovedOpeningArtillery(Transform root)
    {
        Model(root, "FORTRESS__RightCliffTower", EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
            new Vector3(13.6f, 3.3f, 80.5f), Vector3.one * 2.45f, new Vector3(-90f, -8f, 0f));
        Model(root, "FORTRESS__RightCliffCannon", EnvironmentRoot + "L01-PRP-001_Shore_Cannon_Optimized.fbx",
            new Vector3(13.2f, 7.9f, 79.3f), Vector3.one * 1.18f, new Vector3(-90f, 180f, 0f));
        ApprovedMuzzleFlashObject(root, new Vector3(13.2f, 8.25f, 77f));
    }

    static GameObject ApprovedOpeningModel(Transform parent, string name, string path, Vector3 position,
        Vector3 scale, Vector3 rotation)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) return null;
        var value = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        value.name = name;
        value.transform.SetParent(parent);
        value.transform.position = position;
        value.transform.localScale = scale;
        value.transform.rotation = Quaternion.Euler(rotation);
        var renderers = value.GetComponentsInChildren<Renderer>();
        ApplyLevel01ReferenceShipMaterials(renderers, path);
        if (renderers.Length == 0) return value;
        var bounds = renderers[0].bounds;
        for (var index = 1; index < renderers.Length; index++) bounds.Encapsulate(renderers[index].bounds);
        value.transform.position += Vector3.up * (position.y - bounds.min.y);
        return value;
    }

    static void ApprovedMuzzleFlashObject(Transform root, Vector3 center)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(ApprovedMuzzleFlash);
        var core = Primitive(root, "VFX__CannonMuzzleFlash_Core", PrimitiveType.Sphere,
            center, Vector3.one * 0.52f, material);
        core.GetComponent<Collider>().enabled = false;
        var plume = Primitive(root, "VFX__CannonMuzzleFlash_Plume", PrimitiveType.Sphere,
            center + Vector3.back * 0.42f, new Vector3(0.32f, 0.32f, 0.82f), material);
        plume.GetComponent<Collider>().enabled = false;
        var light = new GameObject("VFX__CannonMuzzleLight").AddComponent<Light>();
        light.transform.SetParent(root);
        light.transform.position = center;
        light.type = LightType.Point;
        light.range = 11f;
        light.intensity = 5.5f;
        light.color = new Color(1f, 0.45f, 0.08f);
    }

    static void ValidateApprovedOpeningHierarchy()
    {
        var required = new[]
        {
            "PLAYER__Flagship", "PLAYER__SecondLateenAndHelm", "CHARACTER__Hayreddin_OnDeck",
            "CITY__MountainBackdrop", "CITY__FortressWall", "FORTRESS__RightCliffTower",
            "FORTRESS__RightCliffCannon", "VFX__CannonMuzzleFlash_Core", "VFX__FlagshipWake"
        };
        foreach (var name in required)
            if (GameObject.Find(name) == null) throw new MissingReferenceException($"Approved opening object is missing: {name}");

        var opening = GameObject.Find("PHASE__Opening_ReferenceMatch");
        if (opening == null) throw new MissingReferenceException("Approved opening phase is missing.");
        var water = opening.transform.Find("ENV__AuthoredWaterSurface");
        if (water == null) throw new MissingReferenceException("Opening-specific approved water is missing.");
        var renderer = water.GetComponent<MeshRenderer>();
        if (renderer == null || AssetDatabase.GetAssetPath(renderer.sharedMaterial) != ApprovedOpeningWater)
            throw new MissingReferenceException("Opening water does not use the approved production material.");
    }
}
