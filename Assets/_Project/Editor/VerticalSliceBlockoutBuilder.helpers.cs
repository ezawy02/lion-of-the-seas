using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;

public static partial class VerticalSliceBlockoutBuilder
{
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

}
