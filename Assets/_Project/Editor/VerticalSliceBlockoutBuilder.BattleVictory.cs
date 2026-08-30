using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string Level01RiggedHarborGuardian =
        CharacterRoot + "L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized.fbx";
    const string Level01DefeatedHarborGuardian =
        CharacterRoot + "L01-CHR-004_Harbor_Guardian_DefeatedKneel_R1_REVIEW.fbx";

    static void BuildLevel01BossBattle(Transform root)
    {
        Water(root, 125, false, "Assets/_Project/Materials/Water/SeaLion_Water_Level01.mat");
        LandingBeachSurface(root);
        Coast(root, 82, 34, Sand, Limestone);
        Level01Harbor(root);
        PlaceApprovedLevel01Fortress(root, "GROUP__BattleFortress_Approved",
            new Vector3(-1.5f, 0f, 108f), 34f);
        BattleCrowd(root, "FRIENDLY__LandingForce_Front", new Vector3(-3.5f, 0.25f, 81f), 11, 7,
            CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx", 0f);
        BattleCrowd(root, "FRIENDLY__LandingForce_Rear", new Vector3(-5f, 0.25f, 87f), 9, 6,
            CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx", 8f);
        BattleCrowd(root, "HOSTILE__Defenders_Front", new Vector3(4.5f, 0.25f, 88f), 11, 7,
            CharacterRoot + "L01-CHR-003_Hostile_Infantry_Rigged_Optimized.fbx", 180f);
        BattleCrowd(root, "HOSTILE__Defenders_Rear", new Vector3(5.5f, 0.25f, 94f), 9, 5,
            CharacterRoot + "L01-CHR-003_Hostile_Infantry_Rigged_Optimized.fbx", 172f);
        Model(root, "HOSTILE__EnemyCommander_REVIEW",
            CharacterRoot + "L01-CHR-005_Enemy_Commander_UserBatch_R2_REVIEW.fbx",
            new Vector3(5.2f, 0.25f, 87.5f), Vector3.one * 1.35f, new Vector3(-90f, 180f, 0f));
        Model(root, "BOSS__HarborGuardian",
            Level01RiggedHarborGuardian,
            new Vector3(0f, 0.5f, 96f), Vector3.one * 4.25f, new Vector3(0f, 180f, 0f));
        PlaceBattleFlagship(root);
    }

    static void BuildLevel01Victory(Transform root)
    {
        LandingBeachSurface(root);
        Coast(root, 82, 34, Sand, Limestone);
        PlaceApprovedLevel01Fortress(root, "GROUP__VictoryFortress_Approved",
            new Vector3(-1.5f, 0f, 108f), 34f);
        Model(root, "CHARACTER__Hayreddin_Victory", Level01HeroPose,
            new Vector3(-1.4f, 0.3f, 85.8f), Vector3.one * 1.8f, new Vector3(0f, 198f, 0f));
        BattleCrowd(root, "FRIENDLY__VictoryGuard_Rear", new Vector3(-0.8f, 0.25f, 88.5f), 11, 4,
            CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx", 180f, 1.05f);
        BattleCrowd(root, "FRIENDLY__VictoryGuard_Left", new Vector3(-5.5f, 0.25f, 88.8f), 3, 3,
            CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx", 165f, 1.02f);
        BattleCrowd(root, "FRIENDLY__VictoryGuard_Right", new Vector3(5.2f, 0.25f, 89f), 3, 3,
            CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx", 195f, 1.02f);
        Model(root, "FRIENDLY__CaptiveEscort_Left",
            CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx",
            new Vector3(0.05f, 0.3f, 86.8f), Vector3.one * 1.18f, new Vector3(0f, 188f, 0f));
        Model(root, "FRIENDLY__CaptiveEscort_Right",
            CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx",
            new Vector3(2.35f, 0.3f, 86.9f), Vector3.one * 1.18f, new Vector3(0f, 172f, 0f));
        Model(root, "BOSS__HarborGuardian_Defeated",
            Level01DefeatedHarborGuardian,
            new Vector3(1.2f, 0.3f, 86.05f), Vector3.one * 1.55f, new Vector3(-90f, 182f, 0f));
        Model(root, "REWARD__BlueprintChest",
            EnvironmentRoot + "L01-PRP-005_Blueprint_Reward_Chest_Optimized.fbx",
            new Vector3(0f, 0.3f, 84f), Vector3.one * 0.75f, new Vector3(-90f, 180f, 0f));
        Model(root, "PROP__LionWaveVictoryBanner",
            EnvironmentRoot + "L01-PRP-002_Lion_Wave_Banner_Optimized.fbx",
            new Vector3(-7f, 0.25f, 92f), Vector3.one * 1.8f, new Vector3(-90f, 180f, 0f));
    }

    static void PlaceBattleFlagship(Transform root)
    {
        var position = new Vector3(-3.8f, 0.05f, 54f);
        var rotation = new Vector3(-90f, 350f, 0f);
        var ship = Model(root, "PLAYER__BattleFlagship", Level01ReferenceShip,
            position, Vector3.one * 6.2f, rotation);
        ApprovedOpeningModel(root, "PLAYER__BattleSecondLateenAndHelm", ApprovedOpeningAddon,
            position, Vector3.one * 3.35f, rotation);
        Model(root, "CHARACTER__Hayreddin_Battle", Level01HeroPose,
            new Vector3(-3.5f, 3.45f, 48.8f), Vector3.one * 1.55f, new Vector3(0f, -10f, 0f));
        var banner = Model(root, "PROP__Friendly_Landing_Banner",
            EnvironmentRoot + "L01-PRP-002_Lion_Wave_Banner_Optimized.fbx",
            new Vector3(-4.6f, 5.7f, 54f), Vector3.one * 0.72f, rotation);
        if (ship != null && banner != null) banner.transform.SetParent(ship.transform, true);
    }

    static void BattleCrowd(Transform root, string name, Vector3 center, int columns,
        int rows, string assetPath, float facing, float unitScale = 0.74f)
    {
        var group = new GameObject(name).transform;
        group.SetParent(root);
        for (var row = 0; row < rows; row++)
            for (var column = 0; column < columns; column++)
            {
                var seed = row * columns + column;
                var jitter = new Vector3(Mathf.Sin(seed * 2.17f) * 0.42f, 0f,
                    Mathf.Cos(seed * 1.63f) * 0.48f);
                var offset = new Vector3((column - (columns - 1) * 0.5f) * 1.08f,
                    0f, row * 1.04f) + jitter;
                var unit = Model(group, $"Unit_{row}_{column}", assetPath, center + offset,
                    Vector3.one * (unitScale + seed % 3 * 0.025f),
                    new Vector3(0f, facing + Mathf.Sin(seed) * 12f, 0f));
            }
    }
}
