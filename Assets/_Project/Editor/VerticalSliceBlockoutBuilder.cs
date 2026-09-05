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
}
