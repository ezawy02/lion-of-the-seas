using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string FortressR8GameScene =
        "Assets/_Project/Scenes/Review/Level01_BeachLanding_FortressR8_REVIEW.unity";
    const string FortressR8GameOutput =
        "Artifacts/Local/Approval/Level01FortressR6/InGame";

    [MenuItem("Lion of the Seas/Build Fortress R8 In-Game REVIEW")]
    public static void BuildFortressR8InGameReview()
    {
        EditorSceneManager.OpenScene(BeachReviewScene, OpenSceneMode.Single);
        var oldFortress = GameObject.Find("GROUP__LandingFortress_Right_REVIEW");
        if (oldFortress != null) Object.DestroyImmediate(oldFortress);

        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FortressR6Model);
        if (prefab == null) throw new FileNotFoundException("Fortress R8 model is missing.", FortressR6Model);
        var fortress = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        fortress.name = "GROUP__LandingFortress_R8_REVIEW";
        fortress.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, -135f, 0f));
        var bounds = CombinedBounds(fortress.transform);
        var scale = 38f / Mathf.Max(bounds.size.x, bounds.size.z);
        fortress.transform.localScale = Vector3.one * scale;
        bounds = CombinedBounds(fortress.transform);
        fortress.transform.position += new Vector3(13f - bounds.center.x, -bounds.min.y, 108f - bounds.center.z);
        ApplyFortressR6Material(fortress, EnsureFortressR6Material());

        Directory.CreateDirectory(Path.GetDirectoryName(FortressR8GameScene));
        Directory.CreateDirectory(FortressR8GameOutput);
        Save(FortressR8GameScene);
        CaptureFortressR8InGameImages();
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = fortress;
        Debug.Log("Fortress R8 in-game REVIEW captured. Production was not modified.");
    }

    [MenuItem("Lion of the Seas/Capture Fortress R8 In-Game REVIEW")]
    public static void CaptureFortressR8InGameReview()
    {
        EditorSceneManager.OpenScene(FortressR8GameScene, OpenSceneMode.Single);
        CaptureFortressR8InGameImages();
        AssetDatabase.Refresh();
    }

    static void CaptureFortressR8InGameImages()
    {
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Beach Landing camera is missing.");
        camera.fieldOfView = 40f;
        Capture(camera, new Vector3(-5f, 13.5f, 6f), new Vector3(-1f, 2.5f, 48f),
            FortressR8GameOutput + "/01_Gameplay_Full_HUD_REVIEW.png",
            "Assets/_Project/Art/UI/Level01_BeachLanding_HUD.png");
        camera.fieldOfView = 34f;
        Capture(camera, new Vector3(-5f, 12f, 38f), new Vector3(10f, 5.2f, 108f),
            FortressR8GameOutput + "/02_Gameplay_Fortress_Approach_REVIEW.png");
    }

    static GameObject PlaceApprovedLevel01Fortress(Transform parent, string name,
        Vector3 center, float targetWidth)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FortressR6Model);
        if (prefab == null) throw new FileNotFoundException("Approved Level 01 fortress is missing.", FortressR6Model);
        var fortress = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        fortress.name = name;
        fortress.transform.SetParent(parent);
        fortress.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(0f, -135f, 0f));
        var bounds = CombinedBounds(fortress.transform);
        fortress.transform.localScale = Vector3.one *
            (targetWidth / Mathf.Max(bounds.size.x, bounds.size.z));
        bounds = CombinedBounds(fortress.transform);
        fortress.transform.position += center -
            new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
        ApplyFortressR6Material(fortress, EnsureFortressR6Material());
        return fortress;
    }

    static GameObject PlaceApprovedBeachGangway(Transform parent)
    {
        var gangwayPath = EnvironmentRoot + "L01-ENV-013_Wooden_Landing_Gangway_REVIEW.fbx";
        var gangway = Model(parent, "ENV__WoodenLandingGangway_APPROVED", gangwayPath,
            new Vector3(2.5f, 0.04f, 84f), Vector3.one * 100f, new Vector3(-90f, 0f, 0f));
        if (gangway == null) throw new MissingReferenceException("Approved wooden gangway is missing.");
        var wood = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Imported/L01-PRP-011_Wooden_Siege_Scaffold.mat");
        if (wood == null) throw new MissingReferenceException("Approved gangway wood material is missing.");
        foreach (var renderer in gangway.GetComponentsInChildren<Renderer>(true))
            renderer.sharedMaterial = wood;
        return gangway;
    }

    static void PlaceBeachCityExtension(Transform parent)
    {
        var group = new GameObject("GROUP__BeachCityExtension_Approved").transform;
        group.SetParent(parent);
        var backdrop = Model(group, "CITY__BeachMountainBackdrop",
            EnvironmentRoot + "L01-ENV-012_Mediterranean_Mountain_City_Backdrop_Optimized.fbx",
            new Vector3(-7f, 0.1f, 122f), Vector3.one * 21f, new Vector3(-90f, 180f, 0f));
        ApplySingleMaterial(backdrop, EnsureBeachCityMaterial(
            "BeachCity_Backdrop_APPROVED", "L01-ENV-012_Mediterranean_Mountain_City_Backdrop",
            new Color(1.00f, 0.97f, 0.90f), 0.84f, 0.66f, 1.52f, 0.02f));
        var positions = new[]
        {
            new Vector3(-24f, 0.3f, 111f), new Vector3(-18.5f, 0.8f, 114f),
            new Vector3(-13.5f, 1.4f, 117f), new Vector3(-9f, 2.0f, 119f)
        };
        var houseMaterial = EnsureBeachCityMaterial(
            "BeachCity_House_APPROVED", "L01-ENV-005_Mediterranean_Coastal_House",
            new Color(1.00f, 0.98f, 0.92f), 0.86f, 0.68f, 1.48f, 0.02f);
        for (var index = 0; index < positions.Length; index++)
        {
            var house = Model(group, $"CITY__BeachTerrace_{index:00}",
                EnvironmentRoot + "L01-ENV-005_Mediterranean_Coastal_House_Optimized.fbx",
                positions[index], Vector3.one * (2.8f + index * 0.22f),
                new Vector3(-90f, index % 2 == 0 ? 12f : -16f, 0f));
            ApplySingleMaterial(house, houseMaterial);
        }
        var watch = Model(group, "CITY__BeachWatchTower",
            EnvironmentRoot + "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
            new Vector3(-11f, 1.5f, 116f), Vector3.one * 2.35f, new Vector3(-90f, 8f, 0f));
        ApplySingleMaterial(watch, houseMaterial);
    }

    static Material EnsureBeachCityMaterial(string name, string sourceName, Color tint,
        float saturation, float contrast, float boost, float lightResponse)
    {
        const string directory = "Assets/_Project/Materials/Approved";
        if (!AssetDatabase.IsValidFolder(directory))
            AssetDatabase.CreateFolder("Assets/_Project/Materials", "Approved");
        var path = directory + "/" + name + ".mat";
        var value = AssetDatabase.LoadAssetAtPath<Material>(path);
        var source = AssetDatabase.LoadAssetAtPath<Material>(
            "Assets/_Project/Materials/Imported/" + sourceName + ".mat");
        if (source == null) throw new MissingReferenceException("Beach city source material is missing: " + sourceName);
        if (value == null)
        {
            value = new Material(source) { name = name };
            AssetDatabase.CreateAsset(value, path);
        }
        value.SetColor("_BaseColor", tint);
        value.SetColor("_Color", tint);
        value.SetFloat("_Saturation", saturation);
        value.SetFloat("_Contrast", contrast);
        value.SetFloat("_ColorBoost", boost);
        value.SetFloat("_LightResponse", lightResponse);
        value.SetFloat("_Smoothness", 0.10f);
        value.SetFloat("_BumpScale", 0.30f);
        EditorUtility.SetDirty(value);
        return value;
    }

    static void ApplySingleMaterial(GameObject target, Material material)
    {
        if (target == null || material == null) return;
        foreach (var renderer in target.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (var index = 0; index < materials.Length; index++) materials[index] = material;
            renderer.sharedMaterials = materials;
        }
    }
}
