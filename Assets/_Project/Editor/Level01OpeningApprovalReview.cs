using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static partial class VerticalSliceBlockoutBuilder
{
    const string OpeningApprovalScene = "Assets/_Project/Scenes/Review/Level01_Opening_Approval_REVIEW.unity";
    const string OpeningApprovalOutput = "Artifacts/Local/Approval/Level01Opening";
    const string OpeningApprovalAddon = ShipRoot + "L01-SHP-004_R9_AftLateen_Helm_Addon_REVIEW.fbx";
    const string OpeningApprovalWater = "Assets/_Project/Materials/Review/SeaLion_Water_Level01_Approval_REVIEW.mat";
    const string OpeningApprovalWake = "Assets/_Project/Materials/Review/SeaLion_Foam_Level01_Wake_Approval_REVIEW.mat";

    [MenuItem("Lion of the Seas/Build Level 01 Opening Approval REVIEW")]
    public static void BuildLevel01OpeningApprovalReview()
    {
        Directory.CreateDirectory("Assets/_Project/Scenes/Review");
        Directory.CreateDirectory("Assets/_Project/Materials/Review");
        Directory.CreateDirectory(OpeningApprovalOutput);

        EnsureApprovalMaterials();
        var root = Begin("LEVEL01_OPENING_APPROVAL_REVIEW__NOT_PRODUCTION");
        BuildApprovalWater(root);
        BuildApprovalFleet(root);
        BuildApprovalCoastAndCity(root);
        CameraAndLight(root, new Vector3(-1f, 14f, -10f), new Vector3(0f, 1.7f, 32f), false);
        Save(OpeningApprovalScene);
        CaptureApprovalImages();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Level 01 opening approval REVIEW scene and captures are ready. Production scene was not modified.");
    }

    [MenuItem("Lion of the Seas/Capture Level 01 Opening Approval REVIEW")]
    public static void CaptureLevel01OpeningApprovalReview()
    {
        EditorSceneManager.OpenScene(OpeningApprovalScene, OpenSceneMode.Single);
        Directory.CreateDirectory(OpeningApprovalOutput);
        CaptureApprovalImages();
        AssetDatabase.Refresh();
    }

    static void EnsureApprovalMaterials()
    {
        var sourceWater = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Water/SeaLion_Water_Level01.mat");
        var water = AssetDatabase.LoadAssetAtPath<Material>(OpeningApprovalWater);
        if (water == null && sourceWater != null)
        {
            water = new Material(sourceWater) { name = "SeaLion_Water_Level01_Approval_REVIEW" };
            AssetDatabase.CreateAsset(water, OpeningApprovalWater);
        }
        if (water != null)
        {
            // Softer near-camera blend removes the hard dark patch while retaining
            // the reference's blue-to-green depth transition.
            water.SetColor("_ForegroundColor", new Color(0.190f, 0.310f, 0.300f, 1f));
            water.SetFloat("_ForegroundStrength", 0.72f);
            water.SetColor("_HorizonColor", new Color(0.020f, 0.350f, 0.460f, 1f));
            water.SetFloat("_ShoreStrength", 0.78f);
            water.SetFloat("_SpecularStrength", 0.92f);
            EditorUtility.SetDirty(water);
        }

        var sourceWake = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Water/SeaLion_Foam_Level01_FlagshipWake.mat");
        var wake = AssetDatabase.LoadAssetAtPath<Material>(OpeningApprovalWake);
        if (wake == null && sourceWake != null)
        {
            wake = new Material(sourceWake) { name = "SeaLion_Foam_Level01_Wake_Approval_REVIEW" };
            AssetDatabase.CreateAsset(wake, OpeningApprovalWake);
        }
        if (wake != null)
        {
            wake.SetFloat("_FoamStrength", 1.15f);
            wake.SetFloat("_EffectAlphaBoost", 1.25f);
            wake.SetFloat("_Opacity", 0.36f);
            EditorUtility.SetDirty(wake);
        }
    }

    static void BuildApprovalWater(Transform root)
    {
        Water(root, 125f, false, OpeningApprovalWater);
        Wake(root, "VFX__FlagshipWake_REVIEW", new Vector3(-1.4f, 0.038f, 14.3f),
            new Vector3(1.85f, 1f, 1.0f), OpeningApprovalWake);
        TargetRing(root, "VFX__IncomingCannonTargetRing_REVIEW", new Vector3(5.1f, 0.045f, 38f), 2.15f);
        CannonSplash(root, "VFX__IncomingCannonSplash_REVIEW", new Vector3(5.1f, 0.06f, 38f), 2.25f, 4.6f);
    }

    static void BuildApprovalFleet(Transform root)
    {
        var ship = Model(root, "PLAYER__Flagship_Preserved_REVIEW", Level01ReferenceShip,
            new Vector3(-1.4f, 0.05f, 15f), Vector3.one * 8.8f, new Vector3(-90f, 350f, 0f));
        if (ship == null) throw new FileNotFoundException("Missing preserved approval flagship.");
        var addon = ApprovalModel(root, "PLAYER__SecondLateenAndHelm_REVIEW", OpeningApprovalAddon,
            new Vector3(-1.4f, 0.05f, 15f), Vector3.one * 5.0f, new Vector3(-90f, 350f, 0f), true);
        if (addon == null) throw new FileNotFoundException("Missing R9 lateen-and-helm addon.");

        Model(root, "CHARACTER__Hayreddin_Leadership_REVIEW", Level01HeroPose,
            new Vector3(-1.4f, 2.82f, 10.75f), Vector3.one * 1.52f, new Vector3(0f, -10f, 0f));
        Model(root, "PROP__LionWaveBanner_REVIEW",
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

    static void BuildApprovalCoastAndCity(Transform root)
    {
        Model(root, "ENV__LeftCoastalCliff_REVIEW",
            EnvironmentRoot + "L01-ENV-010_Left_Coastal_Cliff_Optimized.fbx",
            new Vector3(-16f, -8f, 80f), Vector3.one * 13f, new Vector3(-90f, 12f, 0f));
        Model(root, "ENV__RightArtilleryCliff_REVIEW",
            EnvironmentRoot + "L01-ENV-011_Right_Artillery_Cliff_Optimized.fbx",
            new Vector3(22f, -6.5f, 86f), Vector3.one * 11f, new Vector3(-90f, -9f, 0f));

        // Anchor authored rocks and vegetation at the cliff feet and crowns.
        Model(root, "ENV__LeftShoreFoot_REVIEW",
            EnvironmentRoot + "L01-ENV-009_Shoreline_Rock_Sand_Cluster_Optimized.fbx",
            new Vector3(-12.8f, -0.15f, 73.5f), Vector3.one * 2.5f, new Vector3(-90f, 24f, 0f));
        Model(root, "ENV__RightShoreFoot_REVIEW",
            EnvironmentRoot + "L01-ENV-007_Limestone_Rock_Cluster_Optimized.fbx",
            new Vector3(13.8f, -0.1f, 78f), Vector3.one * 2.2f, new Vector3(-90f, -18f, 0f));
        Model(root, "ENV__LeftCrownVegetation_REVIEW",
            EnvironmentRoot + "L01-ENV-008_Coastal_Vegetation_Clump_Optimized.fbx",
            new Vector3(-13.5f, 4.3f, 79f), Vector3.one * 1.7f, new Vector3(-90f, 18f, 0f));
        Model(root, "ENV__RightCrownVegetation_REVIEW",
            EnvironmentRoot + "L01-ENV-008_Coastal_Vegetation_Clump_Optimized.fbx",
            new Vector3(14.8f, 3.4f, 81f), Vector3.one * 1.5f, new Vector3(-90f, -22f, 0f));

        BuildApprovalCity(root);
        BuildApprovalArtilleryTower(root);
    }

    static void BuildApprovalCity(Transform root)
    {
        Model(root, "CITY__MountainBackdrop_REVIEW",
            EnvironmentRoot + "L01-ENV-012_Mediterranean_Mountain_City_Backdrop_Optimized.fbx",
            new Vector3(1f, 0.1f, 111f), Vector3.one * 27f, new Vector3(-90f, 180f, 0f));
        Model(root, "CITY__FortressWall_REVIEW", EnvironmentRoot + "L01-ENV-001_Fortress_Wall_Module_Optimized.fbx",
            new Vector3(0f, 0.35f, 107f), Vector3.one * 7.2f, new Vector3(-90f, 0f, 0f));
        Model(root, "CITY__Gate_REVIEW", EnvironmentRoot + "L01-ENV-003_Fortress_Main_Gate_Module_Optimized.fbx",
            new Vector3(0f, 0.35f, 106f), Vector3.one * 1.85f, new Vector3(-90f, 0f, 0f));
        Model(root, "CITY__Tower_Left_REVIEW", EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
            new Vector3(-7.2f, 0.35f, 107f), Vector3.one * 1.85f, new Vector3(-90f, 4f, 0f));
        Model(root, "CITY__Tower_Right_REVIEW", EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
            new Vector3(7.4f, 0.35f, 108f), Vector3.one * 1.70f, new Vector3(-90f, -5f, 0f));

        var housePositions = new[]
        {
            new Vector3(-10.5f, 0.55f, 109f), new Vector3(-4.9f, 1.25f, 110f),
            new Vector3(3.8f, 1.55f, 111f), new Vector3(9.8f, 0.75f, 109f)
        };
        for (var index = 0; index < housePositions.Length; index++)
            Model(root, $"CITY__TerraceHouse_{index:00}_REVIEW",
                EnvironmentRoot + "L01-ENV-005_Mediterranean_Coastal_House_Optimized.fbx",
                housePositions[index], Vector3.one * (1.35f + index * 0.08f),
                new Vector3(-90f, index % 2 == 0 ? 16f : -18f, 0f));
    }

    static void BuildApprovalArtilleryTower(Transform root)
    {
        Model(root, "FORTRESS__RightCliffTower_REVIEW",
            EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
            new Vector3(13.6f, 3.3f, 80.5f), Vector3.one * 2.45f, new Vector3(-90f, -8f, 0f));
        Model(root, "FORTRESS__RightCliffCannon_REVIEW",
            EnvironmentRoot + "L01-PRP-001_Shore_Cannon_Optimized.fbx",
            new Vector3(13.2f, 7.9f, 79.3f), Vector3.one * 1.18f, new Vector3(-90f, 180f, 0f));
        MuzzleFlash(root, new Vector3(13.2f, 8.25f, 77.0f));
    }

    static GameObject ApprovalModel(Transform parent, string name, string path, Vector3 position,
        Vector3 scale, Vector3 rotation, bool shipMaterials)
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
        if (shipMaterials) ApplyLevel01ReferenceShipMaterials(renderers, path);
        if (renderers.Length > 0)
        {
            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Length; index++) bounds.Encapsulate(renderers[index].bounds);
            value.transform.position += Vector3.up * (position.y - bounds.min.y);
        }
        return value;
    }

    static void MuzzleFlash(Transform root, Vector3 center)
    {
        var material = ApprovalFlashMaterial();
        var core = Primitive(root, "VFX__CannonMuzzleFlash_Core_REVIEW", PrimitiveType.Sphere,
            center, Vector3.one * 0.52f, material);
        core.GetComponent<Collider>().enabled = false;
        var plume = Primitive(root, "VFX__CannonMuzzleFlash_Plume_REVIEW", PrimitiveType.Sphere,
            center + Vector3.back * 0.42f, new Vector3(0.32f, 0.32f, 0.82f), material);
        plume.GetComponent<Collider>().enabled = false;
        var light = new GameObject("VFX__CannonMuzzleLight_REVIEW").AddComponent<Light>();
        light.transform.SetParent(root);
        light.transform.position = center;
        light.type = LightType.Point;
        light.range = 11f;
        light.intensity = 5.5f;
        light.color = new Color(1f, 0.45f, 0.08f);
    }

    static Material ApprovalFlashMaterial()
    {
        const string path = "Assets/_Project/Materials/Review/L01_CannonMuzzleFlash_REVIEW.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Universal Render Pipeline/Unlit");
        if (material == null && shader != null)
        {
            material = new Material(shader) { name = "L01_CannonMuzzleFlash_REVIEW" };
            AssetDatabase.CreateAsset(material, path);
        }
        if (material != null)
        {
            material.SetColor("_BaseColor", new Color(1f, 0.22f, 0.015f, 1f));
            material.EnableKeyword("_EMISSION");
            material.SetColor("_EmissionColor", new Color(5f, 1.1f, 0.05f, 1f));
            EditorUtility.SetDirty(material);
        }
        return material;
    }

    static void CaptureApprovalImages()
    {
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Approval review camera is missing.");
        camera.fieldOfView = 40f;
        Capture(camera, new Vector3(-1f, 14f, -10f), new Vector3(0f, 1.7f, 32f),
            OpeningApprovalOutput + "/01_Opening_Full_REVIEW.png", "Assets/_Project/Art/UI/Level01_Opening_HUD.png");
        Capture(camera, new Vector3(-1f, 14f, -10f), new Vector3(0f, 1.7f, 32f),
            OpeningApprovalOutput + "/02_Water_And_Wake_REVIEW.png");
        camera.fieldOfView = 25f;
        Capture(camera, new Vector3(-1f, 11f, -2f), new Vector3(-1.2f, 4.5f, 13f),
            OpeningApprovalOutput + "/03_Flagship_Helm_Hayreddin_REVIEW.png");
        camera.fieldOfView = 24f;
        Capture(camera, new Vector3(-1f, 17f, 20f), new Vector3(3f, 5.5f, 91f),
            OpeningApprovalOutput + "/04_City_Cliffs_Artillery_REVIEW.png");
        camera.fieldOfView = 40f;
    }
}
