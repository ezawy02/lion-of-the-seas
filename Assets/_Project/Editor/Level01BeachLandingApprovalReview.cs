using System.IO;
using SeaLion.Presentation.Vfx;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string BeachReviewScene = "Assets/_Project/Scenes/Review/Level01_BeachLanding_REVIEW.unity";
    const string BeachReviewOutput = "Artifacts/Local/Approval/Level01BeachLanding";
    const string BeachReviewWater = "Assets/_Project/Materials/Review/SeaLion_Water_Level01_Beach_REVIEW.mat";
    const string BeachReviewWake = "Assets/_Project/Materials/Review/SeaLion_Foam_Level01_Beach_REVIEW.mat";
    const string BeachReviewSky = "Assets/_Project/Materials/Review/SeaLion_Sky_Level01_Beach_REVIEW.mat";

    [MenuItem("Lion of the Seas/Build Level 01 Beach Landing REVIEW")]
    public static void BuildLevel01BeachLandingApprovalReview()
    {
        Directory.CreateDirectory("Assets/_Project/Scenes/Review");
        Directory.CreateDirectory("Assets/_Project/Materials/Review");
        Directory.CreateDirectory(BeachReviewOutput);
        RequireApprovedProductionAsset<GameObject>(ApprovedOpeningAddon);
        EnsureBeachReviewMaterials();

        var root = Begin("LEVEL01_BEACH_LANDING_REVIEW__NOT_PRODUCTION");
        BuildBeachReviewWaterAndShore(root);
        BuildBeachReviewFleet(root);
        BuildBeachReviewLandingForce(root);
        BuildBeachReviewHarbor(root);
        ValidateBeachReviewHierarchy();
        CameraAndLight(root, new Vector3(-5f, 13.5f, 6f), new Vector3(-1f, 2.5f, 48f), false);
        ApplyBeachReviewLighting();
        Camera.main.fieldOfView = 40f;
        Save(BeachReviewScene);
        CaptureBeachReviewImages();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Level 01 Beach Landing REVIEW built and captured. Production was not modified.");
    }

    [MenuItem("Lion of the Seas/Capture Level 01 Beach Landing REVIEW")]
    public static void CaptureLevel01BeachLandingApprovalReview()
    {
        EditorSceneManager.OpenScene(BeachReviewScene, OpenSceneMode.Single);
        Directory.CreateDirectory(BeachReviewOutput);
        CaptureBeachReviewImages();
        AssetDatabase.Refresh();
    }

    static void EnsureBeachReviewMaterials()
    {
        var sourceWater = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Water/SeaLion_Water_Level01.mat");
        var water = AssetDatabase.LoadAssetAtPath<Material>(BeachReviewWater);
        if (water == null && sourceWater != null)
        {
            water = new Material(sourceWater) { name = "SeaLion_Water_Level01_Beach_REVIEW" };
            AssetDatabase.CreateAsset(water, BeachReviewWater);
        }
        if (water != null)
        {
            water.SetColor("_ForegroundColor", new Color(0.18f, 0.265f, 0.27f, 1f));
            water.SetColor("_HorizonColor", new Color(0.07f, 0.31f, 0.34f, 1f));
            water.SetColor("_ShallowColor", new Color(0.15f, 0.58f, 0.60f, 0.82f));
            water.SetFloat("_SpecularStrength", 0.62f);
            EditorUtility.SetDirty(water);
        }

        var sourceWake = AssetDatabase.LoadAssetAtPath<Material>(ApprovedOpeningWake);
        var wake = AssetDatabase.LoadAssetAtPath<Material>(BeachReviewWake);
        if (wake == null && sourceWake != null)
        {
            wake = new Material(sourceWake) { name = "SeaLion_Foam_Level01_Beach_REVIEW" };
            AssetDatabase.CreateAsset(wake, BeachReviewWake);
        }
        if (wake != null)
        {
            wake.SetFloat("_FoamStrength", 0.88f);
            wake.SetFloat("_EffectAlphaBoost", 0.82f);
            wake.SetFloat("_Opacity", 0.24f);
            EditorUtility.SetDirty(wake);
        }

        var sourceSky = AssetDatabase.LoadAssetAtPath<Material>(SkyboxMaterialPath);
        var sky = AssetDatabase.LoadAssetAtPath<Material>(BeachReviewSky);
        if (sky == null && sourceSky != null)
        {
            sky = new Material(sourceSky) { name = "SeaLion_Sky_Level01_Beach_REVIEW" };
            AssetDatabase.CreateAsset(sky, BeachReviewSky);
        }
        if (sky == null) return;
        sky.SetColor("_ZenithColor", new Color(0.139f, 0.331f, 0.47f, 1f));
        sky.SetColor("_HorizonColor", new Color(0.418f, 0.524f, 0.568f, 1f));
        sky.SetColor("_CloudColor", new Color(0.679f, 0.662f, 0.627f, 1f));
        sky.SetFloat("_CloudStrength", 0.92f);
        EditorUtility.SetDirty(sky);
    }

    static void BuildBeachReviewWaterAndShore(Transform root)
    {
        Water(root, 125f, false, BeachReviewWater);
        LandingBeachSurface(root);
        Coast(root, 94f, 100f, Sand, Limestone);
        DisableBeachStoneCorridor(root);
        Wake(root, "VFX__FlagshipWake_REVIEW", new Vector3(-6.5f, 0.038f, 20.5f),
            new Vector3(2.5f, 1f, 5f), BeachReviewWake);
    }

    static void BuildBeachReviewFleet(Transform root)
    {
        var ship = Model(root, "PLAYER__Flagship_REVIEW", Level01ReferenceShip,
            new Vector3(-6.5f, 0.05f, 28f), Vector3.one * 6.6f, new Vector3(-90f, 344f, 0f));
        ApprovedOpeningModel(root, "PLAYER__SecondLateenAndHelm_REVIEW", ApprovedOpeningAddon,
            new Vector3(-6.5f, 0.05f, 28f), Vector3.one * 3.75f, new Vector3(-90f, 344f, 0f));
        Model(root, "CHARACTER__Hayreddin_OnDeck_REVIEW", Level01HeroPose,
            new Vector3(-5.7f, 3.65f, 22.5f), Vector3.one * 1.52f, new Vector3(0f, -16f, 0f));
        var banner = Model(root, "PROP__FlagshipLionWaveBanner_REVIEW",
            EnvironmentRoot + "L01-PRP-002_Lion_Wave_Banner_Optimized.fbx",
            new Vector3(-6.24f, 5.41f, 27.93f), Vector3.one * 0.68f, new Vector3(-90f, 344f, 0f));
        if (ship != null && banner != null) banner.transform.SetParent(ship.transform, true);
        BeachReviewCraftFan(root);
    }

    static void BeachReviewCraftFan(Transform root)
    {
        var positions = new[]
        {
            new Vector3(-6.5f, 0.05f, 48f), new Vector3(-1.5f, 0.05f, 51f),
            new Vector3(4.2f, 0.05f, 53f), new Vector3(-3.8f, 0.05f, 59f),
            new Vector3(2.2f, 0.05f, 62f), new Vector3(7.5f, 0.05f, 66f),
            new Vector3(-0.5f, 0.05f, 70f)
        };
        var headings = new[] { 10f, 5f, -7f, 8f, -4f, -11f, 2f };
        for (var index = 0; index < positions.Length; index++)
        {
            Model(root, "CRAFT__LandingFan_" + index,
                ShipRoot + "L01-SHP-002_Landing_Craft_Optimized.fbx", positions[index],
                Vector3.one * 2.25f, new Vector3(-90f, headings[index], 0f));
            for (var rider = -1; rider <= 1; rider++)
                Model(root, $"CREW__LandingFan_{index}_{rider}",
                    CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx",
                    positions[index] + new Vector3(rider * 0.36f, 0.23f, rider == 0 ? 0.15f : -0.25f),
                    Vector3.one * 0.45f, new Vector3(0f, 180f + headings[index], 0f));
            Wake(root, "VFX__LandingCraftWake_" + index,
                positions[index] + new Vector3(0f, -0.01f, -0.75f),
                new Vector3(0.95f, 1f, 1.9f), BeachReviewWake);
        }
    }

    static void BuildBeachReviewLandingForce(Transform root)
    {
        ModelCrowd(root, "FRIENDLY__LandingForce_REVIEW", new Vector3(2f, 0.35f, 94f),
            10, 5, CharacterRoot + "L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx", 180f);
    }

    static void BuildBeachReviewHarbor(Transform root)
    {
        Level01Harbor(root);
        BuildBeachWoodenFerry(root);
        BuildBeachPalmGrove(root);
        var fortress = new GameObject("GROUP__LandingFortress_Right_REVIEW").transform;
        fortress.SetParent(root);
        fortress.position = new Vector3(0f, 0f, 108f);
        Level01Fortress(fortress);
        ReplaceBeachTower(fortress, "FORTRESS__Tower_Left");
        ReplaceBeachTower(fortress, "FORTRESS__Tower_Right");
        fortress.localScale = Vector3.one * 1.48f;
        fortress.position = new Vector3(14f, 0f, 108f);
        PlaceBeachCannonOnTower(fortress, "FORTRESS__ShoreCannon_Left", "FORTRESS__Tower_Left");
        PlaceBeachCannonOnTower(fortress, "FORTRESS__ShoreCannon_Right", "FORTRESS__Tower_Right");
    }

    static void ReplaceBeachTower(Transform fortress, string towerName)
    {
        var original = fortress.Find(towerName);
        if (original == null) throw new MissingReferenceException($"Beach Landing original tower is missing: {towerName}.");
        var originalBounds = CombinedBounds(original);
        Object.DestroyImmediate(original.gameObject);

        var towerPath = EnvironmentRoot +
            "L01-ENV-014_Fortress_Tower_TripoV31_R2_Optimized_REVIEW.fbx";
        var replacement = Model(fortress, towerName, towerPath, originalBounds.center,
            Vector3.one * 100f, new Vector3(-90f, 0f, 0f));
        if (replacement == null) throw new MissingReferenceException($"Beach Landing generated tower is missing: {towerName}.");
        var replacementBounds = CombinedBounds(replacement.transform);
        var heightScale = originalBounds.size.y / replacementBounds.size.y;
        replacement.transform.localScale *= heightScale;
        replacement.transform.localScale = Vector3.Scale(replacement.transform.localScale,
            new Vector3(0.80f, 0.80f, 1f));
        replacementBounds = CombinedBounds(replacement.transform);
        replacement.transform.position += new Vector3(
            originalBounds.center.x - replacementBounds.center.x,
            originalBounds.min.y - replacementBounds.min.y,
            originalBounds.center.z - replacementBounds.center.z);

        var material = ImportedMaterial(towerPath);
        if (material == null) throw new MissingReferenceException("Beach Landing generated tower material is missing.");
        material.SetColor("_BaseColor", new Color(1.49f, 1.49f, 1.49f, 1f));
        material.SetFloat("_Saturation", 1.10f);
        material.SetFloat("_Contrast", 1.08f);
        material.SetFloat("_ColorBoost", 1.04f);
        material.SetFloat("_LightResponse", 0.32f);
        EditorUtility.SetDirty(material);
    }

    static void DisableBeachStoneCorridor(Transform root)
    {
        var corridorNames = new[]
        {
            "ENV__LandingSandShelf_3D_4", "ENV__LandingSandShelf_3D_5",
            "ENV__ShorelineRockSand_3D_4", "ENV__ShorelineRockSand_3D_5"
        };
        foreach (var value in root.GetComponentsInChildren<Transform>(true))
            foreach (var corridorName in corridorNames)
                if (value.name == corridorName) value.gameObject.SetActive(false);
    }

    static void BuildBeachWoodenFerry(Transform root)
    {
        var gangwayPath = EnvironmentRoot + "L01-ENV-013_Wooden_Landing_Gangway_REVIEW.fbx";
        var woodPath = EnvironmentRoot + "L01-PRP-011_Wooden_Siege_Scaffold_Optimized.fbx";
        var gangway = Model(root, "ENV__WoodenLandingGangway_REVIEW", gangwayPath,
            new Vector3(2.5f, 0.04f, 84f), Vector3.one * 100f, new Vector3(-90f, 0f, 0f));
        if (gangway == null) throw new MissingReferenceException("Beach Landing wooden gangway asset is missing.");
        var wood = ImportedMaterial(woodPath);
        if (wood == null) throw new MissingReferenceException("Beach Landing wooden gangway material is missing.");
        foreach (var renderer in gangway.GetComponentsInChildren<Renderer>()) renderer.sharedMaterial = wood;
    }

    static void BuildBeachPalmGrove(Transform root)
    {
        var positions = new[]
        {
            new Vector3(-15f, 0.12f, 104f), new Vector3(-4f, 0.12f, 106f),
            new Vector3(5f, 0.12f, 105f), new Vector3(15f, 0.12f, 103f)
        };
        for (var index = 0; index < positions.Length; index++)
            Model(root, $"ENV__PalmCluster_Extra_{index}",
                EnvironmentRoot + "L01-ENV-006_Palm_Tree_Cluster_Optimized.fbx", positions[index],
                Vector3.one * (2.15f + index % 2 * 0.35f), new Vector3(-90f, index * 31f - 18f, 0f));
    }

    static void PlaceBeachCannonOnTower(Transform fortress, string cannonName, string towerName)
    {
        var cannon = fortress.Find(cannonName);
        var tower = fortress.Find(towerName);
        if (cannon == null || tower == null)
            throw new MissingReferenceException($"Beach Landing tower artillery is incomplete: {towerName}/{cannonName}.");
        var cannonBounds = CombinedBounds(cannon);
        var towerBounds = CombinedBounds(tower);
        cannon.position += new Vector3(towerBounds.center.x - cannonBounds.center.x,
            towerBounds.max.y + 0.08f - cannonBounds.min.y,
            towerBounds.center.z - 0.8f - cannonBounds.center.z);
    }

    static Bounds CombinedBounds(Transform value)
    {
        var renderers = value.GetComponentsInChildren<Renderer>();
        if (renderers.Length == 0) throw new MissingReferenceException($"No renderers found below {value.name}.");
        var bounds = renderers[0].bounds;
        for (var index = 1; index < renderers.Length; index++) bounds.Encapsulate(renderers[index].bounds);
        return bounds;
    }

    static void ApplyBeachReviewLighting()
    {
        RenderSettings.skybox = AssetDatabase.LoadAssetAtPath<Material>(BeachReviewSky);
        RenderSettings.ambientLight = new Color(0.34f, 0.34f, 0.32f);
        RenderSettings.ambientIntensity = 0.76f;
        RenderSettings.fogColor = new Color(0.43f, 0.50f, 0.50f);
        var key = GameObject.Find("KEY_LIGHT__Blockout");
        var light = key == null ? null : key.GetComponent<Light>();
        if (light != null) light.intensity = 0.84f;
    }

    static void ValidateBeachReviewHierarchy()
    {
        foreach (var value in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (value.name.Contains("Multiplier"))
                throw new InvalidDataException("Beach Landing review must not contain a multiplier gate.");
        var ship = GameObject.Find("PLAYER__Flagship_REVIEW");
        var banner = GameObject.Find("PROP__FlagshipLionWaveBanner_REVIEW");
        if (ship == null || banner == null || !banner.transform.IsChildOf(ship.transform))
            throw new InvalidDataException("The Beach Landing banner must remain attached to the flagship.");
        var fortress = GameObject.Find("GROUP__LandingFortress_Right_REVIEW");
        var fortressRenderers = fortress == null ? null : fortress.GetComponentsInChildren<Renderer>();
        if (fortressRenderers == null || fortressRenderers.Length == 0)
            throw new InvalidDataException("Beach Landing fortress renderers are missing.");
        var fortressBounds = fortressRenderers[0].bounds;
        for (var index = 1; index < fortressRenderers.Length; index++)
            fortressBounds.Encapsulate(fortressRenderers[index].bounds);
        if (fortressBounds.center.z < 95f || fortressBounds.center.z > 130f)
            throw new InvalidDataException($"Beach Landing fortress depth is invalid: {fortressBounds.center.z:F2}.");
        if (fortressBounds.min.y > 1f)
            throw new InvalidDataException($"Beach Landing fortress is floating: minY={fortressBounds.min.y:F2}.");
        var beach = GameObject.Find("ENV__LandingBeach_AuthoredSurface");
        var beachRenderer = beach == null ? null : beach.GetComponent<MeshRenderer>();
        if (beachRenderer == null || fortressBounds.min.y > beachRenderer.bounds.max.y + 0.35f)
            throw new InvalidDataException("Beach Landing fortress is not grounded on the authored shore.");
        if (fortress.transform.localScale.x < 1.47f)
            throw new InvalidDataException("Beach Landing fortress is smaller than the reviewed composition target.");
        ValidateBeachTowerCannon(fortress.transform, "FORTRESS__ShoreCannon_Left", "FORTRESS__Tower_Left");
        ValidateBeachTowerCannon(fortress.transform, "FORTRESS__ShoreCannon_Right", "FORTRESS__Tower_Right");
        ValidateBeachGeneratedTowers(fortress.transform);
        ValidateBeachWoodenFerryAndPalms(root: fortress.transform.root);
        var craftCount = 0;
        foreach (var value in Object.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
            if (value.name.StartsWith("CRAFT__LandingFan_")) craftCount++;
        if (craftCount != 7 || GameObject.Find("FRIENDLY__LandingForce_REVIEW") == null)
            throw new InvalidDataException($"Beach Landing formation is incomplete: craftCount={craftCount}.");
        Debug.Log($"Beach Landing fortress bounds validated: center={fortressBounds.center}, minY={fortressBounds.min.y:F2}.");
        foreach (var effect in Object.FindObjectsByType<WaterVfxEffect>(FindObjectsSortMode.None))
        {
            if (!effect.name.Contains("Wake")) continue;
            if (effect.transform.localScale.z > 5.01f)
                throw new InvalidDataException($"Beach Landing wake is too long: {effect.name}.");
            var renderer = effect.GetComponent<MeshRenderer>();
            if (renderer != null && AssetDatabase.GetAssetPath(renderer.sharedMaterial) != BeachReviewWake)
                throw new InvalidDataException($"Beach Landing wake uses an unexpected material: {effect.name}.");
        }
    }

    static void ValidateBeachWoodenFerryAndPalms(Transform root)
    {
        var gangwayPath = EnvironmentRoot + "L01-ENV-013_Wooden_Landing_Gangway_REVIEW.fbx";
        var woodPath = EnvironmentRoot + "L01-PRP-011_Wooden_Siege_Scaffold_Optimized.fbx";
        var gangway = GameObject.Find("ENV__WoodenLandingGangway_REVIEW");
        if (gangway == null || PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(gangway) != gangwayPath)
            throw new InvalidDataException("Beach Landing wooden gangway does not use its dedicated review model.");
        if (GameObject.Find("ENV__WoodenFerry_0_REVIEW") != null ||
            GameObject.Find("ENV__WoodenFerry_1_REVIEW") != null)
            throw new InvalidDataException("Rejected stone dock modules remain in the wooden gangway corridor.");
        var bounds = CombinedBounds(gangway.transform);
        if (bounds.min.y > 0.5f || bounds.size.z < 18f || bounds.size.x > 7f)
            throw new InvalidDataException($"Beach Landing wooden gangway bounds are invalid: {bounds}.");
        var wood = ImportedMaterial(woodPath);
        foreach (var renderer in gangway.GetComponentsInChildren<Renderer>())
            if (renderer.sharedMaterial != wood)
                throw new InvalidDataException("Beach Landing wooden gangway does not use the verified wood material.");
        var palms = 0;
        foreach (var value in root.GetComponentsInChildren<Transform>(true))
            if (value.name.StartsWith("ENV__PalmCluster_")) palms++;
        if (palms < 8) throw new InvalidDataException($"Beach Landing palm count is too low: {palms}.");
    }

    static void ValidateBeachTowerCannon(Transform fortress, string cannonName, string towerName)
    {
        var cannon = fortress.Find(cannonName);
        var tower = fortress.Find(towerName);
        if (cannon == null || tower == null)
            throw new MissingReferenceException($"Beach Landing tower artillery is missing: {towerName}/{cannonName}.");
        var cannonBounds = CombinedBounds(cannon);
        var towerBounds = CombinedBounds(tower);
        if (cannonBounds.min.y < towerBounds.max.y - 0.15f)
            throw new InvalidDataException($"{cannonName} is not mounted on {towerName}.");
    }

    static void ValidateBeachGeneratedTowers(Transform fortress)
    {
        var left = fortress.Find("FORTRESS__Tower_Left");
        var right = fortress.Find("FORTRESS__Tower_Right");
        var towerPath = EnvironmentRoot +
            "L01-ENV-014_Fortress_Tower_TripoV31_R2_Optimized_REVIEW.fbx";
        if (left == null || right == null ||
            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(left.gameObject) != towerPath ||
            PrefabUtility.GetPrefabAssetPathOfNearestInstanceRoot(right.gameObject) != towerPath)
            throw new InvalidDataException("Beach Landing generated tower pair is not installed correctly.");
        var leftBounds = CombinedBounds(left);
        var rightBounds = CombinedBounds(right);
        if (Vector3.Distance(leftBounds.size, rightBounds.size) > 0.1f ||
            leftBounds.size.x < 8.2f || leftBounds.size.x > 8.8f ||
            leftBounds.min.y > 0.75f || rightBounds.min.y > 0.75f)
            throw new InvalidDataException(
                $"Beach Landing generated tower pair bounds are invalid: left={leftBounds}, right={rightBounds}.");
        var renderer = right.GetComponentInChildren<Renderer>();
        var material = renderer == null ? null : renderer.sharedMaterial;
        if (material == null || material.GetColor("_BaseColor").r < 1.48f)
            throw new InvalidDataException("Beach Landing generated tower color match is missing.");
    }

    static void CaptureBeachReviewImages()
    {
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Beach Landing review camera is missing.");
        camera.fieldOfView = 40f;
        Capture(camera, new Vector3(-5f, 13.5f, 6f), new Vector3(-1f, 2.5f, 48f),
            BeachReviewOutput + "/01_BeachLanding_Full_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_BeachLanding_HUD.png");
        Capture(camera, new Vector3(-5f, 13.5f, 6f), new Vector3(-1f, 2.5f, 48f),
            BeachReviewOutput + "/02_BeachLanding_NoHUD_REVIEW.png");
        camera.fieldOfView = 27f;
        Capture(camera, new Vector3(-4f, 11f, 30f), new Vector3(1f, 1.8f, 82f),
            BeachReviewOutput + "/03_Shore_Force_Detail_REVIEW.png");
        camera.fieldOfView = 40f;
    }
}
