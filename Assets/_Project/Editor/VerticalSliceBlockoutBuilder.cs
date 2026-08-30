using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static partial class VerticalSliceBlockoutBuilder
{
    const string SceneRoot = "Assets/_Project/Scenes/";
    const string MaterialRoot = "Assets/_Project/Materials/Blockout/";
    const string CharacterRoot = "Assets/_Project/Art/Characters/";
    const string ShipRoot = "Assets/_Project/Art/Ships/";
    const string Level01ReferenceShip = ShipRoot + "L01-SHP-004_Hero_Flagship_TripoV31_R2_Optimized_REVIEW.fbx";
    const string Level01HeroPose = CharacterRoot + "L01-CHR-001_Hayreddin_Barbarossa_Rigged_Optimized_R2_LeadershipPose_REVIEW.fbx";
    const string EnvironmentRoot = "Assets/_Project/Art/Environment/";
    const string Level02BossShip = ShipRoot + "L02-SHP-001_Armored_Warship_Boss_UserBatch_R2_REVIEW.fbx";
    const string Level02Mine = EnvironmentRoot + "L02-PRP-001_Floating_Naval_Mine_UserBatch_R2_REVIEW.fbx";
    const string Level02Chain = EnvironmentRoot + "L02-PRP-002_Heavy_Chain_Link_Unit_UserBatch_R2_REVIEW.fbx";
    const string Level03Skiff = ShipRoot + "L03-SHP-001_Gunpowder_Skiff_UserBatch_R2_REVIEW.fbx";
    const string Level03Barrels = EnvironmentRoot + "L03-PRP-001_Gunpowder_Barrel_Cluster_UserBatch_R2_REVIEW.fbx";
    const string Level03Commander = CharacterRoot + "L03-CHR-001_Storm_Fortress_Commander_UserBatch_R2_REVIEW.fbx";
    const string TextureRoot = "Assets/_Project/Art/Textures/Level01/";
    const string ImportedMaterialRoot = "Assets/_Project/Materials/Imported/";
    const string SkyboxMaterialPath = "Assets/_Project/Materials/Level01_MediterraneanSky.mat";

    static readonly Color Sea = new(0.025f, 0.30f, 0.38f);
    static readonly Color Friendly = new(0.04f, 0.68f, 0.65f);
    static readonly Color Ivory = new(0.90f, 0.83f, 0.66f);
    static readonly Color Gold = new(0.73f, 0.48f, 0.12f);
    static readonly Color Choice = new(0.34f, 0.20f, 0.78f);
    static readonly Color Hostile = new(0.55f, 0.055f, 0.045f);
    static readonly Color Charcoal = new(0.10f, 0.12f, 0.15f);
    static readonly Color Copper = new(0.48f, 0.23f, 0.10f);
    static readonly Color Sand = new(0.75f, 0.56f, 0.31f);
    static readonly Color Limestone = new(0.67f, 0.60f, 0.47f);
    static readonly Color Storm = new(0.12f, 0.18f, 0.25f);

    [MenuItem("Lion of the Seas/Build Complete Blockout")]
    public static void BuildAll()
    {
        ArtRenderPipelineSetup.ConfigurePremiumRendering();
        Level01MaterialLibrary.ConfigureTextureImports();
        Directory.CreateDirectory(MaterialRoot);
        Directory.CreateDirectory(ImportedMaterialRoot);
        BuildLevel01();
        BuildLevel02();
        BuildLevel03();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Lion of the Seas/Rebuild Level 01 Art Integration")]
    public static void RebuildLevel01ArtIntegration()
    {
        ArtRenderPipelineSetup.ConfigurePremiumRendering();
        Level01MaterialLibrary.ConfigureTextureImports();
        Directory.CreateDirectory(MaterialRoot);
        Directory.CreateDirectory(ImportedMaterialRoot);
        BuildLevel01();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Lion of the Seas/Capture Level 01 Art Integration")]
    public static void CaptureLevel01Evidence()
    {
        EditorSceneManager.OpenScene(SceneRoot + "Level_01_HundredSails.unity", OpenSceneMode.Single);
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Level 01 MainCamera is missing.");
        Directory.CreateDirectory("Artifacts/Local/Blockout");
        SetOnlyLevel01Phase("PHASE__Opening_ReferenceMatch");
        EnsureOpeningHasNoMultiplierGate();
        camera.fieldOfView = 40;
        Capture(camera, new Vector3(-1f, 14f, -10f), new Vector3(0f, 1.7f, 32f), "Artifacts/Local/Blockout/Level01_Opening.png", "Assets/_Project/Art/UI/Level01_Opening_HUD.png");
        SetOnlyLevel01Phase("PHASE__Traversal_GateRescue_ReferenceMatch");
        camera.fieldOfView = 40;
        Capture(camera, new Vector3(-2f, 13f, -10f), new Vector3(0, 2.2f, 43f), "Artifacts/Local/Blockout/Level01_GateRescue.png", "Assets/_Project/Art/UI/Level01_GateRescue_HUD.png");
        SetOnlyLevel01Phase("PHASE__BeachLanding_ReferenceMatch");
        camera.fieldOfView = 40;
        Capture(camera, new Vector3(-5f, 13.5f, 6f), new Vector3(-1f, 2.5f, 48f), "Artifacts/Local/Blockout/Level01_BeachLanding.png", "Assets/_Project/Art/UI/Level01_BeachLanding_HUD.png");
        SetOnlyLevel01Phase("PHASE__BossBattle_Prototype_NoExecutionReference");
        camera.fieldOfView = 38;
        Capture(camera, new Vector3(-1f, 16f, 28f), new Vector3(-0.5f, 2f, 96f), "Artifacts/Local/Blockout/Level01_BossBattle.png", "Assets/_Project/Art/UI/Level01_BossBattle_HUD.png");
        SetOnlyLevel01Phase("PHASE__VictoryReward_Prototype_NoExecutionReference");
        camera.fieldOfView = 46;
        Capture(camera, new Vector3(-0.7f, 5.5f, 75f), new Vector3(-0.2f, 2f, 88f), "Artifacts/Local/Blockout/Level01_VictoryReward.png", "Assets/_Project/Art/UI/Level01_VictoryReward_HUD.png");
    }

    [MenuItem("Lion of the Seas/Rebuild Level 02-03 Art Integration")]
    public static void RebuildLevel02And03ArtIntegration()
    {
        ArtRenderPipelineSetup.ConfigurePremiumRendering();
        Level01MaterialLibrary.ConfigureTextureImports();
        Directory.CreateDirectory(MaterialRoot);
        Directory.CreateDirectory(ImportedMaterialRoot);
        BuildLevel02();
        BuildLevel03();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    [MenuItem("Lion of the Seas/Capture Level 02-03 Art Integration")]
    public static void CaptureLevel02And03Evidence()
    {
        Directory.CreateDirectory("Artifacts/Local/Blockout");

        EditorSceneManager.OpenScene(SceneRoot + "Level_02_ChainStrait.unity", OpenSceneMode.Single);
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Level 02 MainCamera is missing.");
        camera.fieldOfView = 40;
        Capture(camera, new Vector3(0, 15, -12), new Vector3(0, 2, 48), "Artifacts/Local/Blockout/Level02_Approach_REVIEW.png");
        Capture(camera, new Vector3(-2, 10, 68), new Vector3(0, 1, 89), "Artifacts/Local/Blockout/Level02_Chain_REVIEW.png");
        Capture(camera, new Vector3(-3, 22, 48), new Vector3(0, 3, 108), "Artifacts/Local/Blockout/Level02_Boss_REVIEW.png");

        EditorSceneManager.OpenScene(SceneRoot + "Level_03_StormFortress.unity", OpenSceneMode.Single);
        camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Level 03 MainCamera is missing.");
        camera.fieldOfView = 40;
        Capture(camera, new Vector3(0, 16, -12), new Vector3(0, 2, 55), "Artifacts/Local/Blockout/Level03_Approach_REVIEW.png");
        Capture(camera, new Vector3(0, 10, 42), new Vector3(4, 1, 65), "Artifacts/Local/Blockout/Level03_Powder_REVIEW.png");
        Capture(camera, new Vector3(-4, 24, 58), new Vector3(0, 6, 108), "Artifacts/Local/Blockout/Level03_Fortress_REVIEW.png");
        camera.fieldOfView = 15;
        Capture(camera, new Vector3(-1, 22f, 88), new Vector3(0, 10.8f, 111), "Artifacts/Local/Blockout/Level03_Commander_REVIEW.png");
        camera.fieldOfView = 40;
        AssetDatabase.Refresh();
    }

    public static void BuildLevel01()
    {
        var root = Begin("LEVEL01_ART_INTEGRATION_REVIEW__HundredSails");
        var opening = Phase(root, "PHASE__Opening_ReferenceMatch", true);
        BuildApprovedLevel01Opening(opening);

        var traversal = Phase(root, "PHASE__Traversal_GateRescue_ReferenceMatch", false);
        Water(traversal, 125, false, "Assets/_Project/Materials/Water/SeaLion_Water_Level01.mat");
        PlaceTraversalFlagshipGroup(traversal);
        ModelGate(traversal, "GATE__Multiplier_x4", new Vector3(0.7f, 0.1f, 61), false);
        GateValueBadge(traversal, new Vector3(0.7f, 8.2f, 58.8f));
        GateValueLabel(traversal, new Vector3(0.7f, 8.2f, 58.5f), "X4");
        TraversalCraftFormation(traversal);
        Model(traversal, "RESCUE__CaptiveSailmakers", EnvironmentRoot + "L01-PRP-004_Captive_Sailmakers_Rescue_Raft_Cage_Optimized.fbx", new Vector3(4.2f, 0.05f, 24f), Vector3.one * 2.5f, new Vector3(-90, -16, 0));
        Model(traversal, "ENEMY__Patrol_Left", ShipRoot + "L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx", new Vector3(-7, 0.03f, 58), Vector3.one * 1.8f, new Vector3(-90, 12, 0));
        Model(traversal, "ENEMY__Patrol_Right", ShipRoot + "L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx", new Vector3(7, 0.03f, 64), Vector3.one * 1.8f, new Vector3(-90, -12, 0));
        Model(traversal, "ENEMY__Patrol_FarLeft", ShipRoot + "L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx", new Vector3(-10, 0.03f, 70), Vector3.one * 1.55f, new Vector3(-90, 18, 0));
        Level01OpeningBackdrop(traversal);

        var landing = Phase(root, "PHASE__BeachLanding_ReferenceMatch", false);
        Water(landing, 125, false, "Assets/_Project/Materials/Water/SeaLion_Water_Level01.mat");
        LandingBeachSurface(landing);
        Coast(landing, 94, 100, Sand, Limestone);
        Model(landing, "PLAYER__Flagship", Level01ReferenceShip, new Vector3(-6.5f, 0.05f, 28), Vector3.one * 6.6f, new Vector3(-90, 344, 0));
        Model(landing, "CHARACTER__Hayreddin_OnDeck", Level01HeroPose, new Vector3(-5.7f, 3.65f, 22.5f), Vector3.one * 1.52f, new Vector3(0, -16, 0));
        Model(landing, "PROP__FlagshipLionWaveBanner", EnvironmentRoot + "L01-PRP-002_Lion_Wave_Banner_Optimized.fbx", new Vector3(-5.5f, 8.1f, 26.5f), Vector3.one * 1.2f, new Vector3(-90, 164, 0));
        CompactCraftWake(landing, "VFX__FlagshipWake", new Vector3(-6.5f, 0.048f, 19.65f), 0f, true);
        LandingCraftFan(landing);
        ModelCrowd(landing, "FRIENDLY__LandingForce", new Vector3(2, 0.35f, 94), 10, 5, CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx", 180);
        Level01Harbor(landing);
        PlaceApprovedBeachGangway(landing);
        PlaceBeachCityExtension(landing);
        PlaceApprovedLevel01Fortress(landing, "GROUP__LandingFortress_Right",
            new Vector3(14f, 0f, 108f), 38f);

        var battle = Phase(root, "PHASE__BossBattle_Prototype_NoExecutionReference", false);
        BuildLevel01BossBattle(battle);

        var victory = Phase(root, "PHASE__VictoryReward_Prototype_NoExecutionReference", false);
        BuildLevel01Victory(victory);
        Level01ValidationMarkers(root);
        CameraAndLight(root, new Vector3(0, 10.5f, -16), new Vector3(0, 1.8f, 40), false);
        Save(SceneRoot + "Level_01_HundredSails.unity");
    }

    static void Level01ValidationMarkers(Transform root)
    {
        InvisibleMarker(root, "ANCHOR_01_FlagshipLane_Start", new Vector3(0, 0, 0));
        InvisibleMarker(root, "ANCHOR_02_GateChoice_Easy_x4", new Vector3(-5, 0, 20));
        InvisibleMarker(root, "ANCHOR_03_GateChoice_Risky_Damage1", new Vector3(5, 0, 20));
        InvisibleMarker(root, "ANCHOR_04_PrisonerRescue_Sailmakers", new Vector3(0, 0, 35));
        InvisibleMarker(root, "ANCHOR_05_BeachLanding_Transfer", new Vector3(0, 0, 60));
        InvisibleMarker(root, "GREYBOX_FIELD__DefenderField", new Vector3(0, 0, 75));
        InvisibleMarker(root, "ANCHOR_06_HarborGuardian_Entry", new Vector3(0, 0, 80));
        InvisibleMarker(root, "PORTRAIT_CAMERA__Level01Opening", Vector3.zero);
        InvisibleMarker(root, "KEY_LIGHT__Level01Greybox", Vector3.zero);
    }

    public static void BuildLevel02()
    {
        var root = Begin("LEVEL02_ART_INTEGRATION_REVIEW__ChainStrait");
        Water(root, 150);
        Level02StraitCliffs(root);
        Model(root, "PLAYER__Flagship", Level01ReferenceShip, new Vector3(0, 0.05f, 8), Vector3.one * 5.6f, new Vector3(-90, 0, 0));
        AuthoredGate(root, "LANE_LEFT__Reward_x3", new Vector3(-7, 0.1f, 38), "×3", false);
        AuthoredGate(root, "LANE_CENTER__Safe_Add", new Vector3(0, 0.1f, 38), "+12", false);
        AuthoredGate(root, "LANE_RIGHT__Risk_x5", new Vector3(7, 0.1f, 38), "×5", true);
        AuthoredMineField(root, new Vector3(7, 0.05f, 56));
        AuthoredShoreCannons(root, new Vector3(-10.5f, 2.5f, 70), 4);
        AuthoredChainBarrier(root, new Vector3(0, 0.15f, 88), 22);
        Model(root, "BOSS__ArmoredWarship", Level02BossShip, new Vector3(0, 0.12f, 114), Vector3.one * 14f, new Vector3(-90, 180, 0));
        InvisibleMarker(root, "BOSS_ARMOR__BreakZone", new Vector3(0, 2.2f, 110));
        CameraAndLight(root, new Vector3(0, 48, -45), new Vector3(0, 0, 55), false);
        Save(SceneRoot + "Level_02_ChainStrait.unity");
    }

    public static void BuildLevel03()
    {
        var root = Begin("LEVEL03_ART_INTEGRATION_REVIEW__StormFortress");
        Water(root, 165, true);
        Model(root, "PLAYER__Flagship", Level01ReferenceShip, new Vector3(0, 0.05f, 8), Vector3.one * 5.6f, new Vector3(-90, 0, 0));
        StormColumns(root);
        AuthoredGate(root, "CHOICE__Force_x4", new Vector3(-6, 0.1f, 45), "قوة", true);
        AuthoredGate(root, "CHOICE__Powder", new Vector3(6, 0.1f, 45), "بارود", false);
        AuthoredPowderBoats(root, new Vector3(6, 0.05f, 64));
        Coast(root, 108, 34, Sand * 0.75f, Limestone * 0.75f);
        Level03AuthoredFortress(root);
        Model(root, "OBJECTIVE__OuterGate", EnvironmentRoot + "L01-PRP-012_Fortress_Gate_Door_Optimized.fbx", new Vector3(0, 0.15f, 115), Vector3.one * 3.8f, new Vector3(-90, 180, 0));
        ModelCrowd(root, "FRIENDLY__Assault", new Vector3(0, 0.2f, 111), 8, 5, CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx", 0);
        ModelCrowd(root, "HOSTILE__FortressGuard", new Vector3(0, 0.2f, 122), 9, 5, CharacterRoot + "L01-CHR-003_Hostile_Infantry_Rigged_Optimized.fbx", 180);
        Model(root, "BOSS__StormCommander", Level03Commander, new Vector3(0, 10.0f, 111), Vector3.one * 1.6f, new Vector3(-90, -90, 0));
        CameraAndLight(root, new Vector3(0, 52, -45), new Vector3(0, 0, 62), true);
        Save(SceneRoot + "Level_03_StormFortress.unity");
    }

    static Transform Begin(string name)
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        return new GameObject(name).transform;
    }

    static Transform Phase(Transform root, string name, bool active)
    {
        var phase = new GameObject(name).transform;
        phase.SetParent(root);
        phase.gameObject.SetActive(active);
        return phase;
    }

    static void Level01OpeningBackdrop(Transform root)
    {
        Model(root, "ENV__LeftCoastalCliff", EnvironmentRoot + "L01-ENV-010_Left_Coastal_Cliff_Optimized.fbx", new Vector3(-16, -8f, 80), Vector3.one * 13f, new Vector3(-90, 12, 0));
        Model(root, "ENV__RightArtilleryCliff", EnvironmentRoot + "L01-ENV-011_Right_Artillery_Cliff_Optimized.fbx", new Vector3(22, -6.5f, 86), Vector3.one * 11f, new Vector3(-90, -9, 0));
        Model(root, "CITY__MountainBackdrop", EnvironmentRoot + "L01-ENV-012_Mediterranean_Mountain_City_Backdrop_Optimized.fbx", new Vector3(2, 0.1f, 106), Vector3.one * 32f, new Vector3(-90, 180, 0));
        Model(root, "FORTRESS__RightCliffCannon_Left", EnvironmentRoot + "L01-PRP-001_Shore_Cannon_Optimized.fbx", new Vector3(10.5f, 1.8f, 79), Vector3.one * 1.4f, new Vector3(-90, 180, 0));
        Model(root, "FORTRESS__RightCliffCannon_Right", EnvironmentRoot + "L01-PRP-001_Shore_Cannon_Optimized.fbx", new Vector3(15, 1.8f, 81), Vector3.one * 1.4f, new Vector3(-90, 180, 0));
    }

    static void Save(string path) => EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), path);

    static Material Mat(string name, Color color, float metallic = 0, float smoothness = 0.35f)
    {
        var path = MaterialRoot + name + ".mat";
        var shader = UnityEngine.Rendering.GraphicsSettings.defaultRenderPipeline != null
            ? Shader.Find("Universal Render Pipeline/Lit")
            : Shader.Find("Standard");
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(shader) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = shader;
        material.color = color;
        material.SetFloat("_Metallic", metallic);
        material.SetFloat("_Glossiness", smoothness);
        EditorUtility.SetDirty(material);
        return material;
    }

    static GameObject Primitive(Transform parent, string name, PrimitiveType type, Vector3 position, Vector3 scale, Material material)
    {
        var value = GameObject.CreatePrimitive(type);
        value.name = name;
        value.transform.SetParent(parent);
        value.transform.position = position;
        value.transform.localScale = scale;
        value.GetComponent<Renderer>().sharedMaterial = material;
        return value;
    }

    static GameObject Model(Transform parent, string name, string assetPath, Vector3 position, Vector3 scale, Vector3 rotation)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab == null) return null;
        var value = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        value.name = name;
        value.transform.SetParent(parent);
        value.transform.position = position;
        value.transform.localScale = scale;
        if (assetPath.Contains("_UserBatch_R2_REVIEW")) value.transform.localScale *= 100f;
        value.transform.rotation = Quaternion.Euler(rotation);
        var renderers = value.GetComponentsInChildren<Renderer>();
        var preserveAuthoredMaterials = assetPath == Level01ReferenceShip;
        var importedMaterial = preserveAuthoredMaterials ? null : ImportedMaterial(assetPath);
        if (importedMaterial != null)
            foreach (var renderer in renderers) renderer.sharedMaterial = importedMaterial;
        if (preserveAuthoredMaterials) ApplyLevel01ReferenceShipMaterials(renderers, assetPath);
        var autoGroundFromBounds = !assetPath.Contains("_TripoRig_");
        if (renderers.Length > 0 && autoGroundFromBounds)
        {
            var bounds = renderers[0].bounds;
            for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
            value.transform.position += Vector3.up * (position.y - bounds.min.y);
        }
        return value;
    }

    static void ApplyLevel01ReferenceShipMaterials(Renderer[] renderers, string assetPath)
    {
        var hull = ImportedMaterial(assetPath.Contains("TripoV31_R2")
            ? assetPath
            : ShipRoot + "L01-SHP-004_Hero_Flagship_ReferenceMatch_Optimized.fbx");
        var canvas = Mat("L01_Ship_R7_Aged_Ivory_Canvas", new Color(0.86f, 0.80f, 0.68f), 0.02f, 0.18f);
        var canvasShader = Shader.Find("Universal Render Pipeline/Unlit");
        if (canvasShader != null)
        {
            canvas.shader = canvasShader;
            canvas.SetColor("_BaseColor", new Color(0.88f, 0.83f, 0.72f));
        }
        var wood = Mat("L01_Ship_R7_Aged_Mast_Wood", new Color(0.25f, 0.105f, 0.035f), 0.02f, 0.24f);
        var gold = Mat("L01_Ship_R7_Aged_Gold_Bands", new Color(0.54f, 0.30f, 0.07f), 0.42f, 0.42f);
        var rigging = Mat("L01_Ship_R7_Dark_Rigging", new Color(0.055f, 0.035f, 0.022f), 0f, 0.08f);
        foreach (var renderer in renderers)
        {
            var objectName = renderer.gameObject.name;
            if (objectName.Contains("Edge") || objectName.Contains("Seam") || objectName.Contains("Stay"))
                renderer.sharedMaterial = rigging;
            else if (objectName.Contains("Gold") || objectName.Contains("CrowNest"))
                renderer.sharedMaterial = gold;
            else if (objectName.Contains("Mast") || objectName.Contains("LateenYard"))
                renderer.sharedMaterial = wood;
            else if (objectName.Contains("IvorySail"))
                renderer.sharedMaterial = canvas;
            else if (hull != null)
                renderer.sharedMaterial = hull;
        }
    }

    static Material ImportedMaterial(string assetPath)
        => Level01MaterialLibrary.LoadOrCreate(assetPath);

    static void ModelOrShip(Transform root, string name, string assetPath, Vector3 position, Vector3 scale, Color hull, Color trim, float fallbackSize)
    {
        if (Model(root, name, assetPath, position, scale, Vector3.zero) == null)
            Ship(root, name + "__BLOCKOUT_FALLBACK", position, hull, trim, fallbackSize);
    }

    static void ModelGate(Transform root, string name, Vector3 position, bool hostile)
    {
        var gate = Model(root, name, EnvironmentRoot + "L01-GAT-001_Multiplier_Gate_Arch_Buoy_Optimized.fbx", position, Vector3.one * 6.0f, new Vector3(-90, 0, 0));
        if (gate == null) return;
        if (!hostile) return;
        var material = Mat("L01_Gate_Hostile_Variant", new Color(0.48f, 0.035f, 0.025f), 0.15f, 0.4f);
        foreach (var renderer in gate.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
    }

    static void AuthoredGate(Transform root, string name, Vector3 position, string value, bool hostile)
    {
        var gate = Model(root, name, EnvironmentRoot + "L01-GAT-001_Multiplier_Gate_Arch_Buoy_Optimized.fbx",
            position, Vector3.one * 3.8f, new Vector3(-90, 0, 0));
        if (gate != null && hostile)
        {
            var material = Mat("L01_Gate_Hostile_Variant", new Color(0.48f, 0.035f, 0.025f), 0.15f, 0.4f);
            foreach (var renderer in gate.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = material;
        }
        GateValueLabel(root, position + new Vector3(0, 5.1f, -0.8f), value);
    }

    static void Level02StraitCliffs(Transform root)
    {
        for (var index = 0; index < 3; index++)
        {
            var z = 34f + index * 43f;
            Model(root, "ENV__StraitCliff_Left_" + index,
                EnvironmentRoot + "L01-ENV-010_Left_Coastal_Cliff_Optimized.fbx",
                new Vector3(-15.5f, -1.5f, z), Vector3.one * 14f,
                new Vector3(-90, 8f + index * 13f, 0));
            Model(root, "ENV__StraitCliff_Right_" + index,
                EnvironmentRoot + "L01-ENV-011_Right_Artillery_Cliff_Optimized.fbx",
                new Vector3(15.5f, -1.5f, z + 5f), Vector3.one * 14f,
                new Vector3(-90, -10f - index * 11f, 0));
        }
    }

    static void AuthoredMineField(Transform root, Vector3 start)
    {
        for (var index = 0; index < 10; index++)
        {
            var position = start + new Vector3((index % 3 - 1) * 2.4f, 0, index / 3 * 3.2f);
            Model(root, "HAZARD__Mine_3D_" + index, Level02Mine, position,
                Vector3.one * (index % 2 == 0 ? 0.78f : 0.68f), new Vector3(-90, index * 29f, 0));
        }
    }

    static void AuthoredShoreCannons(Transform root, Vector3 start, int count)
    {
        for (var index = 0; index < count; index++)
            Model(root, "HAZARD__ShoreCannon_3D_" + index,
                EnvironmentRoot + "L01-PRP-001_Shore_Cannon_Optimized.fbx",
                start + new Vector3(index % 2 * 21f, 0, index / 2 * 13f),
                Vector3.one * 1.7f, new Vector3(-90, index % 2 == 0 ? 150f : 210f, 0));
    }

    static void AuthoredChainBarrier(Transform root, Vector3 center, float width)
    {
        const int segmentCount = 7;
        for (var index = 0; index < segmentCount; index++)
        {
            var x = -width * 0.5f + index * width / (segmentCount - 1f);
            Model(root, "OBJECTIVE__ChainUnit_3D_" + index, Level02Chain,
                center + Vector3.right * x, Vector3.one * 1.6f,
                new Vector3(-90, 0, 90));
        }
    }

    static void AuthoredPowderBoats(Transform root, Vector3 start)
    {
        for (var index = 0; index < 5; index++)
        {
            var position = start + new Vector3((index - 2) * 3.3f, 0, index % 2 * 2.2f);
            var heading = (index - 2) * -6f;
            Model(root, "POWDER__Skiff_3D_" + index, Level03Skiff, position,
                Vector3.one * 3.2f, new Vector3(-90, heading, 0));
            Model(root, "POWDER__Barrels_3D_" + index, Level03Barrels,
                position + new Vector3(0, 0.45f, 0.2f), Vector3.one * 0.9f,
                new Vector3(-90, heading + 15f, 0));
        }
    }

    static void Level03AuthoredFortress(Transform root)
    {
        var fortress = new GameObject("GROUP__StormFortress_Authored3D").transform;
        fortress.SetParent(root);
        fortress.position = new Vector3(0, 0, 24);
        fortress.localScale = Vector3.one * 1.22f;
        Level01Fortress(fortress);
    }

    static void InvisibleMarker(Transform root, string name, Vector3 position)
    {
        var marker = new GameObject(name).transform;
        marker.SetParent(root);
        marker.position = position;
    }

    static void GateValueLabel(Transform root, Vector3 position, string value)
    {
        var label = new GameObject("UI3D__GateValue_" + value).AddComponent<TextMesh>();
        label.transform.SetParent(root);
        label.transform.position = position;
        label.transform.rotation = Quaternion.identity;
        label.text = value;
        label.anchor = TextAnchor.MiddleCenter;
        label.alignment = TextAlignment.Center;
        label.characterSize = 0.18f;
        label.fontSize = 96;
        label.fontStyle = FontStyle.Bold;
        label.color = new Color(0.96f, 0.72f, 0.28f);
        label.GetComponent<MeshRenderer>().sortingOrder = 50;
    }

    static void GateValueBadge(Transform root, Vector3 position)
    {
        var rim = Primitive(root, "GATE__ValueBadge_GoldRim", PrimitiveType.Cylinder, position,
            new Vector3(1.35f, 0.08f, 1.35f), Mat("GateBadgeGold", new Color(0.68f, 0.43f, 0.14f)));
        rim.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        var face = Primitive(root, "GATE__ValueBadge_TealFace", PrimitiveType.Cylinder,
            position + new Vector3(0f, 0f, -0.12f), new Vector3(1.14f, 0.08f, 1.14f),
            Mat("GateBadgeTeal", new Color(0.018f, 0.10f, 0.15f)));
        face.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
    }

    static void Level01Harbor(Transform root)
    {
        Model(root, "ENV__HarborDock", EnvironmentRoot + "L01-ENV-004_Mediterranean_Harbor_Dock_Module_Optimized.fbx", new Vector3(-10, 0, 78), Vector3.one * 6.2f, new Vector3(-90, 8, 0));
        Model(root, "ENV__CoastalHouse_Left", EnvironmentRoot + "L01-ENV-005_Mediterranean_Coastal_House_Optimized.fbx", new Vector3(-18, 0.15f, 102), Vector3.one * 4.4f, new Vector3(-90, 18, 0));
        Model(root, "ENV__CoastalHouse_Right", EnvironmentRoot + "L01-ENV-005_Mediterranean_Coastal_House_Optimized.fbx", new Vector3(18, 0.15f, 101), Vector3.one * 3.8f, new Vector3(-90, -22, 0));
        Model(root, "ENV__RockCluster_Left", EnvironmentRoot + "L01-ENV-007_Limestone_Rock_Cluster_Optimized.fbx", new Vector3(-14, 0.05f, 91), Vector3.one * 1.45f, new Vector3(-90, 25, 0));
        Model(root, "ENV__RockCluster_Right", EnvironmentRoot + "L01-ENV-007_Limestone_Rock_Cluster_Optimized.fbx", new Vector3(14, 0.05f, 94), Vector3.one * 1.25f, new Vector3(-90, -35, 0));
        Model(root, "PROP__SupplyCrates", EnvironmentRoot + "L01-PRP-003_Beach_Supply_Crate_Cluster_Optimized.fbx", new Vector3(-8, 0.1f, 88), Vector3.one * 1.4f, new Vector3(-90, 25, 0));
        Model(root, "PROP__RopeNet", EnvironmentRoot + "L01-PRP-006_Rope_Fishing_Net_Unit_Optimized.fbx", new Vector3(-12, 0.2f, 81), Vector3.one, new Vector3(-90, 0, 0));
        Model(root, "PROP__AnchorBollard", EnvironmentRoot + "L01-PRP-008_Anchor_Mooring_Bollard_Optimized.fbx", new Vector3(-7, 0.2f, 82), Vector3.one * 1.2f, new Vector3(-90, 20, 0));
        Model(root, "PROP__ShipwreckDebris", EnvironmentRoot + "L01-PRP-009_Shipwreck_Debris_Cluster_Optimized.fbx", new Vector3(12, 0.1f, 85), Vector3.one * 1.7f, new Vector3(-90, -18, 0));
        Model(root, "PROP__HarborPottery", EnvironmentRoot + "L01-PRP-013_Harbor_Pottery_Supplies_Optimized.fbx", new Vector3(-16, 0.2f, 96), Vector3.one, new Vector3(-90, 15, 0));
        Model(root, "ENV__Vegetation_Left", EnvironmentRoot + "L01-ENV-008_Coastal_Vegetation_Clump_Optimized.fbx", new Vector3(-11, 0.12f, 94), Vector3.one * 0.9f, new Vector3(-90, 0, 0));
        Model(root, "ENV__Vegetation_Right", EnvironmentRoot + "L01-ENV-008_Coastal_Vegetation_Clump_Optimized.fbx", new Vector3(11, 0.12f, 92), Vector3.one * 0.75f, new Vector3(-90, 35, 0));
        Model(root, "ENV__RockSand_Left", EnvironmentRoot + "L01-ENV-009_Shoreline_Rock_Sand_Cluster_Optimized.fbx", new Vector3(-5, 0.08f, 89), Vector3.one * 1.1f, new Vector3(-90, 40, 0));
        Model(root, "ENV__RockSand_Right", EnvironmentRoot + "L01-ENV-009_Shoreline_Rock_Sand_Cluster_Optimized.fbx", new Vector3(8, 0.08f, 90), Vector3.one * 0.85f, new Vector3(-90, -25, 0));
        Model(root, "ENV__PalmCluster_Left", EnvironmentRoot + "L01-ENV-006_Palm_Tree_Cluster_Optimized.fbx", new Vector3(-19, 0.12f, 94), Vector3.one * 3.1f, new Vector3(-90, 0, 0));
        Model(root, "ENV__PalmCluster_MidLeft", EnvironmentRoot + "L01-ENV-006_Palm_Tree_Cluster_Optimized.fbx", new Vector3(-9, 0.12f, 99), Vector3.one * 2.1f, new Vector3(-90, 24, 0));
        Model(root, "ENV__PalmCluster_MidRight", EnvironmentRoot + "L01-ENV-006_Palm_Tree_Cluster_Optimized.fbx", new Vector3(9, 0.12f, 98), Vector3.one * 1.9f, new Vector3(-90, -18, 0));
        Model(root, "ENV__PalmCluster_Right", EnvironmentRoot + "L01-ENV-006_Palm_Tree_Cluster_Optimized.fbx", new Vector3(18, 0.12f, 95), Vector3.one * 2.8f, new Vector3(-90, 40, 0));
    }

    static void Level01Fortress(Transform root)
    {
        Model(root, "FORTRESS__Wall", EnvironmentRoot + "L01-ENV-001_Fortress_Wall_Module_Optimized.fbx", new Vector3(0, 0.2f, 111), Vector3.one * 14f, new Vector3(-90, 0, 0));
        Model(root, "FORTRESS__Tower_Left", EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx", new Vector3(-12, 0.2f, 109), Vector3.one * 5.2f, new Vector3(-90, 0, 0));
        Model(root, "FORTRESS__Tower_Right", EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx", new Vector3(12, 0.2f, 109), Vector3.one * 5.2f, new Vector3(-90, 0, 0));
        Model(root, "FORTRESS__MainGate", EnvironmentRoot + "L01-ENV-003_Fortress_Main_Gate_Module_Optimized.fbx", new Vector3(0, 0.25f, 108), Vector3.one * 5.4f, new Vector3(-90, 0, 0));
        Model(root, "FORTRESS__SideDoor", EnvironmentRoot + "L01-PRP-012_Fortress_Gate_Door_Optimized.fbx", new Vector3(14, 0.25f, 106), Vector3.one * 2.1f, new Vector3(-90, -8, 0));
        Model(root, "FORTRESS__Scaffold", EnvironmentRoot + "L01-PRP-011_Wooden_Siege_Scaffold_Optimized.fbx", new Vector3(-10, 0.25f, 103), Vector3.one * 1.9f, new Vector3(-90, 15, 0));
        Model(root, "FORTRESS__Brazier_Left", EnvironmentRoot + "L01-PRP-010_Fortress_Brazier_Optimized.fbx", new Vector3(-5, 0.3f, 106), Vector3.one * 1.15f, new Vector3(-90, 0, 0));
        Model(root, "FORTRESS__Brazier_Right", EnvironmentRoot + "L01-PRP-010_Fortress_Brazier_Optimized.fbx", new Vector3(5, 0.3f, 106), Vector3.one * 1.15f, new Vector3(-90, 0, 0));
        Model(root, "FORTRESS__AmmoTray", EnvironmentRoot + "L01-PRP-007_Cannonball_Ammo_Tray_Optimized.fbx", new Vector3(8, 0.3f, 104), Vector3.one, new Vector3(-90, 0, 0));
        Model(root, "FORTRESS__ShoreCannon_Left", EnvironmentRoot + "L01-PRP-001_Shore_Cannon_Optimized.fbx", new Vector3(-9, 2.4f, 109), Vector3.one * 1.4f, new Vector3(-90, 180, 0));
        Model(root, "FORTRESS__ShoreCannon_Right", EnvironmentRoot + "L01-PRP-001_Shore_Cannon_Optimized.fbx", new Vector3(9, 2.4f, 109), Vector3.one * 1.4f, new Vector3(-90, 180, 0));
    }

    static void Water(Transform root, float length, bool storm = false, string materialOverride = null)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/VFX/WaterSurface.prefab");
        if (prefab == null) return;
        var water = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        water.name = "ENV__AuthoredWaterSurface";
        water.transform.SetParent(root);
        water.transform.position = new Vector3(0, -0.02f, length * 0.5f);
        water.transform.localScale = new Vector3(60f / 24f, 1, length / 30f);
        var renderer = water.GetComponent<MeshRenderer>();
        var materialPath = materialOverride ?? (storm
            ? "Assets/_Project/Materials/Water/SeaLion_Water_Storm.mat"
            : "Assets/_Project/Materials/Water/SeaLion_Water_Primary.mat");
        var waterMaterial = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if (renderer != null && waterMaterial != null) renderer.sharedMaterial = waterMaterial;
    }

    static void OpeningHud(Transform root)
    {
        const string hudPath = "Assets/_Project/Art/UI/Level01_Opening_HUD.png";
        var importer = AssetImporter.GetAtPath(hudPath) as TextureImporter;
        if (importer != null)
        {
            importer.alphaIsTransparency = true;
            importer.mipmapEnabled = false;
            importer.textureCompression = TextureImporterCompression.CompressedHQ;
            importer.maxTextureSize = 2048;
            importer.SaveAndReimport();
        }
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(hudPath);
        if (texture == null) return;

        var canvasObject = new GameObject("UI__OpeningReferenceHUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        canvasObject.transform.SetParent(root);
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.planeDistance = 1f;
        canvas.sortingOrder = 100;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720, 1280);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var imageObject = new GameObject("HUD__Opening_720x1280", typeof(RectTransform), typeof(RawImage));
        imageObject.transform.SetParent(canvasObject.transform, false);
        var rect = imageObject.GetComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        imageObject.GetComponent<RawImage>().texture = texture;
        imageObject.GetComponent<RawImage>().raycastTarget = false;
    }

    static void Ocean(Transform root, float length, Color color) =>
        Primitive(root, "ENV__Ocean", PrimitiveType.Cube, new Vector3(0, -1, length * 0.5f), new Vector3(34, 1, length), Mat("Ocean", color, 0.05f, 0.75f));

    static void Coast(Transform root, float z, float width, Color sand, Color stone)
    {
        var coast = new GameObject("GROUP__Authored3DCoastline").transform;
        coast.SetParent(root);
        coast.position = new Vector3(0, 0, z);
        coast.rotation = Quaternion.Euler(0, -28f, 0);

        // The visible coastline is assembled only from the user's converted Level 01 models.
        // No primitive beach/shoreline placeholder is allowed in the art-integration scene.
        var segmentCount = Mathf.Max(5, Mathf.CeilToInt(width / 10f));
        var spacing = width / (segmentCount - 1f);
        var shorelineScale = width > 50f ? 2.65f : 1.8f;
        for (var index = 0; index < segmentCount; index++)
        {
            var localX = -width * 0.5f + index * spacing;
            var shoreline = Model(coast, $"ENV__ShorelineRockSand_3D_{index}",
                EnvironmentRoot + "L01-ENV-009_Shoreline_Rock_Sand_Cluster_Optimized.fbx",
                Vector3.zero, Vector3.one * shorelineScale, Vector3.zero);
            if (shoreline == null)
                throw new FileNotFoundException("Missing authored Level 01 shoreline model.");
            shoreline.transform.localPosition = new Vector3(localX, 0.18f, -4.8f + (index % 2) * 1.2f);
            shoreline.transform.localRotation = Quaternion.Euler(-90f, index % 2 == 0 ? 18f : -24f, 0f);

            if (width > 50f)
            {
                var landingShelf = Model(coast, $"ENV__LandingSandShelf_3D_{index}",
                    EnvironmentRoot + "L01-ENV-009_Shoreline_Rock_Sand_Cluster_Optimized.fbx",
                    Vector3.zero, Vector3.one * 2.15f, Vector3.zero);
                if (landingShelf == null)
                    throw new FileNotFoundException("Missing authored Level 01 shoreline shelf model.");
                landingShelf.transform.localPosition = new Vector3(localX + (index % 2 == 0 ? 1.6f : -1.2f),
                    0.14f, -13.0f + (index % 3) * 1.35f);
                landingShelf.transform.localRotation = Quaternion.Euler(-90f, index % 2 == 0 ? -12f : 26f, 0f);
            }

            if (index % 3 != 0) continue;
            var rocks = Model(coast, $"ENV__LimestoneRockCluster_3D_{index}",
                EnvironmentRoot + "L01-ENV-007_Limestone_Rock_Cluster_Optimized.fbx",
                Vector3.zero, Vector3.one * (width > 50f ? 1.75f : shorelineScale * 0.9f), Vector3.zero);
            if (rocks == null)
                throw new FileNotFoundException("Missing authored Level 01 limestone rock model.");
            rocks.transform.localPosition = new Vector3(localX + 1.4f, 0.15f, 1.6f + (index % 2) * 2.0f);
            rocks.transform.localRotation = Quaternion.Euler(-90f, 30f + index * 13f, 0f);
        }

        var vegetationCount = width > 50f ? 5 : 3;
        for (var index = 0; index < vegetationCount; index++)
        {
            var localX = Mathf.Lerp(-width * 0.42f, width * 0.42f, index / (vegetationCount - 1f));
            var vegetation = Model(coast, $"ENV__CoastalVegetation_3D_{index}",
                EnvironmentRoot + "L01-ENV-008_Coastal_Vegetation_Clump_Optimized.fbx",
                Vector3.zero, Vector3.one * (width > 50f ? 1.25f : 0.9f), Vector3.zero);
            if (vegetation == null)
                throw new FileNotFoundException("Missing authored Level 01 coastal vegetation model.");
            vegetation.transform.localPosition = new Vector3(localX, 0.1f, 4.5f + (index % 2) * 2.2f);
            vegetation.transform.localRotation = Quaternion.Euler(-90f, index * 37f, 0f);
        }
    }

    static void LandingBeachSurface(Transform root)
    {
        const int columns = 12;
        const int rows = 5;
        const float width = 70f;
        var vertices = new Vector3[(columns + 1) * (rows + 1)];
        var uv = new Vector2[vertices.Length];
        var triangles = new int[columns * rows * 6];
        for (var row = 0; row <= rows; row++)
        {
            var v = row / (float)rows;
            for (var column = 0; column <= columns; column++)
            {
                var u = column / (float)columns;
                var x = Mathf.Lerp(-width * 0.5f, width * 0.5f, u);
                var waterEdge = 80.0f - x * 0.42f + Mathf.Sin(x * 0.20f) * 2.8f;
                var z = Mathf.Lerp(waterEdge, 116f, v);
                var y = 0.10f + Mathf.Sin(x * 0.31f + z * 0.17f) * 0.035f + v * 0.08f;
                var index = row * (columns + 1) + column;
                vertices[index] = new Vector3(x, y, z);
                uv[index] = new Vector2(u, v);
            }
        }
        for (var row = 0; row < rows; row++)
        for (var column = 0; column < columns; column++)
        {
            var vertex = row * (columns + 1) + column;
            var triangle = (row * columns + column) * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + columns + 1;
            triangles[triangle + 2] = vertex + 1;
            triangles[triangle + 3] = vertex + 1;
            triangles[triangle + 4] = vertex + columns + 1;
            triangles[triangle + 5] = vertex + columns + 2;
        }
        var mesh = new Mesh { name = "L01_LandingBeach_AuthoredSurface" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        var beach = new GameObject("ENV__LandingBeach_AuthoredSurface", typeof(MeshFilter), typeof(MeshRenderer));
        beach.transform.SetParent(root);
        beach.GetComponent<MeshFilter>().sharedMesh = mesh;
        beach.GetComponent<MeshRenderer>().sharedMaterial = NaturalSandMaterial();
        LandingShoreFoam(root);
    }

    static void LandingShoreFoam(Transform root)
    {
        const int segments = 48;
        const float width = 70f;
        var vertices = new Vector3[segments * 2];
        var uv = new Vector2[vertices.Length];
        var colors = new Color[vertices.Length];
        var triangles = new int[(segments - 1) * 6];
        for (var index = 0; index < segments; index++)
        {
            var t = index / (float)(segments - 1);
            var x = Mathf.Lerp(-width * 0.5f, width * 0.5f, t);
            var edge = 80.0f - x * 0.42f + Mathf.Sin(x * 0.20f) * 2.8f;
            var ribbon = 0.55f + Mathf.Sin(x * 0.37f) * 0.16f;
            var vertex = index * 2;
            vertices[vertex] = new Vector3(x, 0.135f, edge - ribbon);
            vertices[vertex + 1] = new Vector3(x, 0.135f, edge + ribbon);
            uv[vertex] = new Vector2(t, 0f);
            uv[vertex + 1] = new Vector2(t, 1f);
            var alpha = 0.72f + Mathf.Sin(index * 1.91f) * 0.18f;
            colors[vertex] = new Color(1f, 1f, 1f, alpha * 0.52f);
            colors[vertex + 1] = new Color(1f, 1f, 1f, alpha);
        }
        for (var index = 0; index < segments - 1; index++)
        {
            var vertex = index * 2;
            var triangle = index * 6;
            triangles[triangle] = vertex;
            triangles[triangle + 1] = vertex + 2;
            triangles[triangle + 2] = vertex + 1;
            triangles[triangle + 3] = vertex + 1;
            triangles[triangle + 4] = vertex + 2;
            triangles[triangle + 5] = vertex + 3;
        }
        var mesh = new Mesh { name = "L01_LandingShoreFoam_Authored" };
        mesh.vertices = vertices;
        mesh.uv = uv;
        mesh.colors = colors;
        mesh.triangles = triangles;
        mesh.RecalculateBounds();
        var foam = new GameObject("VFX__LandingShoreFoam_Authored", typeof(MeshFilter), typeof(MeshRenderer));
        foam.transform.SetParent(root);
        foam.GetComponent<MeshFilter>().sharedMesh = mesh;
        foam.GetComponent<MeshRenderer>().sharedMaterial = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Water/SeaLion_Foam_Primary.mat");
    }

    static Material NaturalSandMaterial()
    {
        var path = MaterialRoot + "L01_NaturalSand.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        var shader = Shader.Find("Sea Lion/Environment/Natural Sand");
        if (shader == null) return material;
        if (material == null)
        {
            material = new Material(shader) { name = "L01_NaturalSand" };
            AssetDatabase.CreateAsset(material, path);
        }
        material.shader = shader;
        material.SetColor("_DryColor", new Color(0.82f, 0.65f, 0.39f, 1f));
        material.SetColor("_WetColor", new Color(0.54f, 0.35f, 0.19f, 1f));
        material.SetFloat("_Variation", 0.14f);
        EditorUtility.SetDirty(material);
        return material;
    }

    static void Cliff(Transform root, Vector3 position, Vector3 scale, Color color) =>
        Primitive(root, "ENV__StraitCliff", PrimitiveType.Cube, position, scale, Mat("StraitStone", color));

    static void Ship(Transform root, string name, Vector3 position, Color hull, Color trim, float size)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        group.position = position;
        Primitive(group, "Hull", PrimitiveType.Capsule, Vector3.zero, new Vector3(size * 2.2f, size, size * 4), Mat(name + "_Hull", hull, 0.1f, 0.4f)).transform.rotation = Quaternion.Euler(90, 0, 0);
        Primitive(group, "Deck", PrimitiveType.Cube, new Vector3(0, size * 0.6f, 0), new Vector3(size * 2.4f, size * 0.3f, size * 4.4f), Mat(name + "_Trim", trim, 0.6f, 0.55f));
        Primitive(group, "Mast", PrimitiveType.Cylinder, new Vector3(0, size * 3.2f, 0), new Vector3(size * 0.15f, size * 3, size * 0.15f), Mat("Wood", new Color(0.25f, 0.11f, 0.04f)));
        var sail = Primitive(group, "Sail", PrimitiveType.Cube, new Vector3(0, size * 4.1f, 0.3f), new Vector3(size * 2.8f, size * 2.6f, size * 0.1f), Mat("IvorySail", Ivory));
        sail.transform.rotation = Quaternion.Euler(0, 0, -8);
    }

    static void Gate(Transform root, string name, Vector3 position, string value, Color color)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        var material = Mat(name + "_Material", color, 0.2f, 0.55f);
        Primitive(group, "Left", PrimitiveType.Cube, position + Vector3.left * 2.4f, new Vector3(0.45f, 4, 0.7f), material);
        Primitive(group, "Right", PrimitiveType.Cube, position + Vector3.right * 2.4f, new Vector3(0.45f, 4, 0.7f), material);
        Primitive(group, "Top", PrimitiveType.Cube, position + Vector3.up * 2, new Vector3(5.2f, 0.45f, 0.7f), material);
        var text = new GameObject("VALUE__" + value).AddComponent<TextMesh>();
        text.transform.SetParent(group);
        text.transform.position = position + new Vector3(0, 0.7f, -0.5f);
        text.transform.rotation = Quaternion.Euler(90, 0, 0);
        text.text = value;
        text.anchor = TextAnchor.MiddleCenter;
        text.alignment = TextAlignment.Center;
        text.characterSize = 1.1f;
        text.fontSize = 64;
        text.color = Color.white;
    }

    static void Marker(Transform root, string name, Vector3 position, Color color, Vector3 scale) =>
        Primitive(root, name, PrimitiveType.Cube, position, scale, Mat(name + "_Material", color));

    static void TraversalCraftFormation(Transform root)
    {
        var positions = new[]
        {
            new Vector3(-1.7f, 0.05f, 42f),
            new Vector3(1.4f, 0.05f, 47f),
            new Vector3(-2.6f, 0.05f, 51f),
            new Vector3(2.1f, 0.05f, 56f),
            new Vector3(-0.4f, 0.05f, 61f)
        };
        var headings = new[] { 4f, -5f, 5f, -4f, 1f };
        for (var i = 0; i < positions.Length; i++)
        {
            Model(root, "FRIENDLY__GateCraft_" + i,
                ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx",
                positions[i], Vector3.one * 2.1f, new Vector3(-90, headings[i], 0));
            CraftCrew(root, "CREW__GateCraft_" + i, positions[i] + Vector3.up * 0.2f, headings[i], 3);
            CompactCraftWake(root, "VFX__GateCraftWake_" + i, positions[i], headings[i]);
        }
    }

    static void CraftCrew(Transform root, string name, Vector3 center, float heading, int count)
    {
        for (var index = 0; index < count; index++)
        {
            var column = index % 2;
            var row = index / 2;
            var offset = new Vector3((column - 0.5f) * 0.42f, 0, (row - 0.5f) * 0.55f);
            Model(root, name + "_" + index,
                CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx",
                center + offset, Vector3.one * 0.36f, new Vector3(0, 180 + heading, 0));
        }
    }

    static void LandingCraftFan(Transform root)
    {
        var positions = new[]
        {
            new Vector3(-6.5f, 0.05f, 48f),
            new Vector3(-1.5f, 0.05f, 51f),
            new Vector3(4.2f, 0.05f, 53f),
            new Vector3(-3.8f, 0.05f, 59f),
            new Vector3(2.2f, 0.05f, 62f),
            new Vector3(7.5f, 0.05f, 66f),
            new Vector3(-0.5f, 0.05f, 70f)
        };
        var headings = new[] { 10f, 5f, -7f, 8f, -4f, -11f, 2f };
        for (var i = 0; i < positions.Length; i++)
        {
            Model(root, "CRAFT__LandingFan_" + i,
                ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx",
                positions[i], Vector3.one * 2.25f, new Vector3(-90, headings[i], 0));
            for (var rider = -1; rider <= 1; rider++)
                Model(root, $"CREW__LandingFan_{i}_{rider}",
                    CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx",
                    positions[i] + new Vector3(rider * 0.36f, 0.23f, rider == 0 ? 0.15f : -0.25f),
                    Vector3.one * 0.45f, new Vector3(0, 180 + headings[i], 0));
            CompactCraftWake(root, "VFX__LandingCraftWake_" + i, positions[i], headings[i]);
        }
    }

    static void LandingCraftLine(Transform root, Vector3 start, int count, Color color)
    {
        for (var i = 0; i < count; i++)
        {
            var position = start + new Vector3((i - count / 2) * 2.2f, 0, i % 2);
            if (Model(root, "CRAFT__" + i, ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx", position, Vector3.one * 1.4f, new Vector3(-90, 0, 0)) == null)
                Primitive(root, "CRAFT__" + i + "__BLOCKOUT_FALLBACK", PrimitiveType.Capsule, position, new Vector3(0.7f, 0.35f, 1.5f), Mat("LandingCraft", color));
        }
    }

    static void Crowd(Transform root, string name, Vector3 center, int columns, int rows, Color color)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        var material = Mat(name + "_Material", color);
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
                Primitive(group, $"Unit_{row}_{column}", PrimitiveType.Capsule, center + new Vector3((column - (columns - 1) * 0.5f) * 1.3f, 0, row * 1.35f), new Vector3(0.42f, 0.7f, 0.42f), material);
    }

    static void ModelCrowd(Transform root, string name, Vector3 center, int columns, int rows, string assetPath, float facing)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                var offset = new Vector3((column - (columns - 1) * 0.5f) * 1.3f, 0, row * 1.35f);
                Model(group, $"Unit_{row}_{column}", assetPath, center + offset, Vector3.one * 0.78f, new Vector3(0, facing, 0));
            }
    }

    static void Fortress(Transform root, Vector3 center, float width, float height, Color stone, Color enemy)
    {
        var material = Mat("FortressStone", stone);
        Primitive(root, "FORTRESS__Wall", PrimitiveType.Cube, center + Vector3.up * height * 0.5f, new Vector3(width, height, 3), material);
        for (var x = -1; x <= 1; x += 2)
            Primitive(root, "FORTRESS__Tower", PrimitiveType.Cube, center + new Vector3(x * width * 0.42f, height * 0.65f, 0), new Vector3(6, height * 1.3f, 6), material);
        Marker(root, "FORTRESS__EnemyGate", center + new Vector3(0, height * 0.35f, -1.7f), enemy, new Vector3(7, height * 0.7f, 0.7f));
    }

    static void Boss(Transform root, string name, Vector3 position, Color color, float size)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        Primitive(group, "Body", PrimitiveType.Capsule, position, new Vector3(size, size * 1.5f, size), Mat(name + "_Armor", color, 0.65f, 0.5f));
        Primitive(group, "Shield", PrimitiveType.Cylinder, position + new Vector3(size, 0, -0.2f), new Vector3(size, 0.25f, size), Mat(name + "_Shield", Charcoal, 0.7f, 0.35f)).transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    static void ShoreCannons(Transform root, Vector3 start, int count)
    {
        for (var i = 0; i < count; i++)
            Primitive(root, "HAZARD__ShoreCannon_" + i, PrimitiveType.Cylinder, start + new Vector3(i % 2 * 21, 0, i / 2 * 13), new Vector3(0.8f, 2.2f, 0.8f), Mat("Cannon", Charcoal, 0.75f, 0.35f)).transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    static void MineField(Transform root, Vector3 start)
    {
        for (var i = 0; i < 10; i++)
            Primitive(root, "HAZARD__Mine_" + i, PrimitiveType.Sphere, start + new Vector3((i % 3 - 1) * 2.2f, 0, i / 3 * 3), Vector3.one * 0.7f, Mat("Mine", Charcoal, 0.8f, 0.3f));
    }

    static void Chain(Transform root, Vector3 center, float width)
    {
        for (var i = 0; i < 18; i++)
        {
            var link = Primitive(root, "OBJECTIVE__ChainLink_" + i, PrimitiveType.Capsule, center + Vector3.right * (-width * 0.5f + i * width / 17), new Vector3(0.35f, 0.7f, 0.35f), Mat("Chain", Copper, 0.8f, 0.35f));
            link.transform.rotation = Quaternion.Euler(0, 0, i % 2 == 0 ? 90 : 0);
        }
    }

    static void StormColumns(Transform root)
    {
        for (var i = 0; i < 9; i++)
            InvisibleMarker(root, "STORM__Gust_VFX_MARKER_" + i, new Vector3((i % 3 - 1) * 8, 2, 22 + i * 7));
    }

    static void PowderBoats(Transform root, Vector3 start)
    {
        for (var i = 0; i < 5; i++)
        {
            Primitive(root, "POWDER__Boat_" + i, PrimitiveType.Capsule, start + new Vector3((i - 2) * 2.1f, 0, i % 2), new Vector3(0.7f, 0.35f, 1.5f), Mat("PowderBoat", Gold));
            Primitive(root, "POWDER__Barrel_" + i, PrimitiveType.Cylinder, start + new Vector3((i - 2) * 2.1f, 0.8f, i % 2), new Vector3(0.5f, 0.7f, 0.5f), Mat("PowderBarrel", Copper));
        }
    }

    static void CameraAndLight(Transform root, Vector3 cameraPosition, Vector3 target, bool storm)
    {
        var cameraObject = new GameObject("PORTRAIT_CAMERA__Gameplay");
        cameraObject.transform.SetParent(root);
        cameraObject.transform.position = cameraPosition;
        cameraObject.transform.rotation = Quaternion.LookRotation(target - cameraPosition);
        var camera = cameraObject.AddComponent<Camera>();
        camera.fieldOfView = 42;
        camera.aspect = 9f / 16f;
        camera.clearFlags = storm ? CameraClearFlags.SolidColor : CameraClearFlags.Skybox;
        camera.backgroundColor = storm ? new Color(0.055f, 0.075f, 0.11f) : new Color(0.22f, 0.54f, 0.74f);
        cameraObject.tag = "MainCamera";
        foreach (var canvas in root.GetComponentsInChildren<Canvas>(true))
            if (canvas.renderMode == RenderMode.ScreenSpaceCamera) canvas.worldCamera = camera;

        var lightObject = new GameObject("KEY_LIGHT__Blockout");
        lightObject.transform.SetParent(root);
        lightObject.transform.rotation = Quaternion.Euler(storm ? 28 : 48, storm ? -55 : -28, 0);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = storm ? 0.78f : 1.15f;
        light.color = storm ? new Color(0.62f, 0.71f, 0.88f) : new Color(1f, 0.93f, 0.82f);
        if (storm)
        {
            RenderSettings.ambientMode = AmbientMode.Flat;
            RenderSettings.ambientLight = new Color(0.10f, 0.14f, 0.21f);
            RenderSettings.ambientIntensity = 1f;
            RenderSettings.reflectionIntensity = 0.28f;
            RenderSettings.fog = false;
            return;
        }

        RenderSettings.skybox = MediterraneanSky();
        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.42f, 0.48f, 0.50f);
        RenderSettings.ambientIntensity = 0.92f;
        RenderSettings.reflectionIntensity = 0.34f;
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.ExponentialSquared;
        RenderSettings.fogColor = new Color(0.48f, 0.69f, 0.78f);
        RenderSettings.fogDensity = 0.0019f;
    }

    static Material MediterraneanSky()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
        var shader = Shader.Find("Sea Lion/Sky/Mediterranean Procedural");
        if (shader == null) return material;
        if (material == null)
        {
            material = new Material(shader) { name = "Level01_MediterraneanSky" };
            AssetDatabase.CreateAsset(material, SkyboxMaterialPath);
        }
        material.shader = shader;
        material.SetColor("_ZenithColor", new Color(0.12f, 0.52f, 0.86f));
        material.SetColor("_HorizonColor", new Color(0.52f, 0.80f, 0.92f));
        material.SetColor("_CloudColor", new Color(0.96f, 0.97f, 0.94f));
        material.SetFloat("_CloudStrength", 0.92f);
        EditorUtility.SetDirty(material);
        return material;
    }
}
