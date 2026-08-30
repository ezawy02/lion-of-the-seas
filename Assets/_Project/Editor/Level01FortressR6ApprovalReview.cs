using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static partial class VerticalSliceBlockoutBuilder
{
    const string FortressR6Model =
        "Assets/_Project/Art/Environment/L01-ENV-015_Fortress_R6_Modular_R5_VISIBLE_REVIEW.fbx";
    const string FortressR6Scene =
        "Assets/_Project/Scenes/Review/Level01_Fortress_R6_Modular_R5_REVIEW.unity";
    const string FortressR6Output =
        "Artifacts/Local/Approval/Level01FortressR6/Unity_R8_Existing_Modular_R5_REVIEW.png";
    const string FortressR6BaseColor =
        "Assets/_Project/Art/Models/Environment/Level01/FortressR6/Fortress_R6_BaseColor.png";
    const string FortressR6Normal =
        "Assets/_Project/Art/Models/Environment/Level01/FortressR6/Fortress_R6_Normal.png";
    const string FortressR6Material =
        "Assets/_Project/Materials/Review/Fortress_R6_Tripo_R2_REVIEW.mat";

    [MenuItem("Lion of the Seas/Build Fortress R6 Unity REVIEW")]
    public static void BuildFortressR6UnityReview()
    {
        Directory.CreateDirectory("Assets/_Project/Scenes/Review");
        Directory.CreateDirectory("Assets/_Project/Materials/Review");
        Directory.CreateDirectory(Path.GetDirectoryName(FortressR6Output));
        var stoneMaterial = EnsureFortressR6Material();
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(FortressR6Model);
        if (prefab == null)
            throw new FileNotFoundException("Fortress R6 REVIEW model is not imported.", FortressR6Model);

        var root = Begin("LEVEL01_FORTRESS_R6_UNITY_REVIEW__NOT_PRODUCTION");
        var fortress = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        fortress.name = "ENV__Fortress_R6_Modular_R5_REVIEW";
        fortress.transform.SetParent(root);
        fortress.transform.SetPositionAndRotation(Vector3.zero, Quaternion.identity);
        ApplyFortressR6Material(fortress, stoneMaterial);

        var bounds = CombinedBounds(fortress.transform);
        fortress.transform.position += Vector3.up * -bounds.min.y;
        bounds = CombinedBounds(fortress.transform);
        bounds = CombinedBounds(root);
        BuildFortressR6ReviewGround(root, bounds);
        var camera = BuildFortressR6ReviewCamera(root, bounds);
        BuildFortressR6ReviewLighting(root, bounds);

        Save(FortressR6Scene);
        Capture(camera, camera.transform.position, bounds.center + Vector3.up * bounds.extents.y * 0.1f,
            FortressR6Output);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = fortress;
        Debug.Log("Fortress R6 Unity REVIEW scene and local capture are ready. Production was not modified.");
    }

    static void CaptureFortressR6Cardinals(Camera camera, Bounds bounds)
    {
        var radius = Mathf.Max(bounds.size.x, bounds.size.z);
        var target = bounds.center + Vector3.up * bounds.extents.y * 0.1f;
        var directions = new[]
        {
            new Vector2(1, 1), new Vector2(1, -1),
            new Vector2(-1, 1), new Vector2(-1, -1)
        };
        var labels = new[] { "PP", "PN", "NP", "NN" };
        for (var index = 0; index < directions.Length; index++)
        {
            var direction = directions[index];
            var position = bounds.center + new Vector3(
                radius * direction.x * 1.7f, radius * 0.8f, radius * direction.y * 1.7f);
            camera.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target - position));
            var output = Path.Combine(Path.GetDirectoryName(FortressR6Output),
                $"Unity_R8_Cardinal_{labels[index]}.png");
            Capture(camera, position, target, output);
        }
    }

    [MenuItem("Lion of the Seas/Capture Fortress R6 Unity REVIEW")]
    public static void CaptureFortressR6UnityReview()
    {
        EditorSceneManager.OpenScene(FortressR6Scene, OpenSceneMode.Single);
        var fortress = GameObject.Find("ENV__Fortress_R6_Modular_R5_REVIEW");
        var camera = Camera.main;
        if (fortress == null || camera == null)
            throw new MissingReferenceException("Fortress R6 REVIEW scene is incomplete.");
        var bounds = CombinedBounds(fortress.transform);
        Directory.CreateDirectory(Path.GetDirectoryName(FortressR6Output));
        Capture(camera, camera.transform.position, bounds.center + Vector3.up * bounds.extents.y * 0.1f,
            FortressR6Output);
        AssetDatabase.Refresh();
    }

    static void BuildFortressR6ReviewGround(Transform root, Bounds bounds)
    {
        var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "REVIEW__NeutralGround";
        ground.transform.SetParent(root);
        ground.transform.position = new Vector3(bounds.center.x, -0.04f, bounds.center.z);
        ground.transform.localScale = Vector3.one * Mathf.Max(bounds.size.x, bounds.size.z) * 0.16f;
        var renderer = ground.GetComponent<Renderer>();
        var shader = Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
        var material = new Material(shader) { name = "Fortress R6 Review Ground (Temporary)" };
        material.color = new Color(0.30f, 0.245f, 0.18f, 1f);
        renderer.sharedMaterial = material;
    }

    static Material EnsureFortressR6Material()
    {
        var baseColor = AssetDatabase.LoadAssetAtPath<Texture2D>(FortressR6BaseColor);
        var normal = AssetDatabase.LoadAssetAtPath<Texture2D>(FortressR6Normal);
        if (baseColor == null || normal == null)
            throw new FileNotFoundException("Fortress R6 generated textures are missing.");
        var normalImporter = AssetImporter.GetAtPath(FortressR6Normal) as TextureImporter;
        if (normalImporter != null && normalImporter.textureType != TextureImporterType.NormalMap)
        {
            normalImporter.textureType = TextureImporterType.NormalMap;
            normalImporter.SaveAndReimport();
            normal = AssetDatabase.LoadAssetAtPath<Texture2D>(FortressR6Normal);
        }
        var material = AssetDatabase.LoadAssetAtPath<Material>(FortressR6Material);
        if (material == null)
        {
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            material = new Material(shader) { name = "Fortress_R6_Tripo_R2_REVIEW" };
            AssetDatabase.CreateAsset(material, FortressR6Material);
        }
        material.SetTexture("_BaseMap", baseColor);
        material.SetTexture("_BumpMap", normal);
        var limestoneTint = new Color(0.82f, 0.75f, 0.64f, 1f);
        material.SetColor("_BaseColor", limestoneTint);
        material.SetColor("_Color", limestoneTint);
        material.SetFloat("_BumpScale", 0.95f);
        material.SetFloat("_Smoothness", 0.12f);
        material.EnableKeyword("_NORMALMAP");
        EditorUtility.SetDirty(material);
        return material;
    }

    static void ApplyFortressR6Material(GameObject fortress, Material stoneMaterial)
    {
        var keys = new[]
        {
            "L01-ENV-001_Fortress_Wall_Module", "L01-ENV-002_Fortress_Tower_Module",
            "L01-ENV-003_Fortress_Main_Gate_Module", "L01-ENV-005_Mediterranean_Coastal_House",
            "L01-ENV-006_Palm_Tree_Cluster", "L01-ENV-007_Limestone_Rock_Cluster",
            "L01-PRP-001_Shore_Cannon", "L01-PRP-002_Lion_Wave_Banner",
            "L01-PRP-011_Wooden_Siege_Scaffold"
        };
        foreach (var renderer in fortress.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            var changed = false;
            for (var index = 0; index < materials.Length; index++)
            {
                var sourceName = materials[index] == null ? renderer.name : materials[index].name;
                foreach (var key in keys)
                {
                    if (sourceName.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) < 0 &&
                        renderer.name.IndexOf(key, System.StringComparison.OrdinalIgnoreCase) < 0) continue;
                    var mapped = AssetDatabase.LoadAssetAtPath<Material>(
                        "Assets/_Project/Materials/Imported/" + key + ".mat");
                    if (mapped == null) continue;
                    materials[index] = mapped;
                    changed = true;
                    break;
                }
            }
            if (changed) renderer.sharedMaterials = materials;
        }
    }

    static void AlignGeneratedCannon(GameObject fortress)
    {
        var renderers = fortress.GetComponentsInChildren<Renderer>(true);
        var bodyReady = false;
        var cannonReady = false;
        var body = new Bounds();
        var cannon = new Bounds();
        foreach (var renderer in renderers)
        {
            var isCannon = renderer.name.IndexOf("cannon", System.StringComparison.OrdinalIgnoreCase) >= 0;
            if (isCannon)
            {
                if (!cannonReady) { cannon = renderer.bounds; cannonReady = true; }
                else cannon.Encapsulate(renderer.bounds);
            }
            else if (renderer.name.IndexOf("banner", System.StringComparison.OrdinalIgnoreCase) < 0)
            {
                if (!bodyReady) { body = renderer.bounds; bodyReady = true; }
                else body.Encapsulate(renderer.bounds);
            }
        }
        if (!bodyReady || !cannonReady) return;
        var target = body.center + new Vector3(
            body.extents.x * 0.43f,
            body.extents.y * 0.43f,
            body.extents.z * 0.18f);
        var delta = target - cannon.center;
        foreach (var renderer in renderers)
            if (renderer.name.IndexOf("cannon", System.StringComparison.OrdinalIgnoreCase) >= 0)
                renderer.transform.position += delta;
    }

    static void AdjustGeneratedFloatingProps(GameObject fortress)
    {
        foreach (var value in fortress.GetComponentsInChildren<Transform>(true))
        {
            if (value == fortress.transform) continue;
            if (value.name.IndexOf("banner", System.StringComparison.OrdinalIgnoreCase) >= 0)
                value.position += Vector3.down * 0.28f;
            if (value.name.IndexOf("cannon", System.StringComparison.OrdinalIgnoreCase) >= 0)
                value.position += Vector3.down * 0.72f;
        }
    }

    static void BuildFortressR6AuthoredModules(Transform root, Bounds fortress)
    {
        const string environment = "Assets/_Project/Art/Environment/";
        var rock = EnsureFortressR6SolidMaterial("Fortress_R6_Rock_R3_REVIEW",
            new Color(0.31f, 0.255f, 0.19f), 0.08f);
        var wood = EnsureFortressR6SolidMaterial("Fortress_R6_Wood_R3_REVIEW", new Color(0.24f, 0.11f, 0.045f));
        var frontZ = fortress.min.z - 0.08f;
        var baseY = fortress.min.y;
        var center = fortress.center;
        var height = fortress.size.y;
        var width = fortress.size.x;

        PlaceFortressR6Module(root, "AUTHORED__GateDoor_REVIEW",
            environment + "L01-PRP-012_Fortress_Gate_Door_Optimized.fbx",
            new Vector3(center.x + width * 0.18f, baseY + height * 0.015f, frontZ - 0.12f),
            height * 0.20f, 0f, wood);
        PlaceFortressR6Module(root, "AUTHORED__SiegeScaffold_Right_REVIEW",
            environment + "L01-PRP-011_Wooden_Siege_Scaffold_Optimized.fbx",
            new Vector3(fortress.max.x + width * 0.015f, baseY, center.z - fortress.size.z * 0.12f),
            height * 0.31f, -90f, wood);

        var rockPath = environment + "L01-ENV-007_Limestone_Rock_Cluster_Optimized.fbx";
        for (var index = 0; index < 5; index++)
        {
            var t = index / 4f;
            PlaceFortressR6Module(root, $"AUTHORED__BaseRocks_{index:00}_REVIEW", rockPath,
                new Vector3(Mathf.Lerp(fortress.min.x, fortress.max.x, t), baseY - height * 0.015f,
                    frontZ - fortress.size.z * (0.03f + 0.025f * (index % 2))),
                height * (0.10f + 0.015f * (index % 3)), index * 23f, rock);
        }
    }

    static GameObject PlaceFortressR6Module(Transform root, string name, string path,
        Vector3 bottomCenter, float targetHeight, float yaw, Material material)
    {
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (prefab == null) throw new FileNotFoundException("Missing fortress module.", path);
        var value = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        value.name = name;
        value.transform.SetParent(root);
        value.transform.SetPositionAndRotation(Vector3.zero, Quaternion.Euler(-90f, yaw, 0f));
        value.transform.localScale = Vector3.one;
        var sourceBounds = CombinedBounds(value.transform);
        var scale = targetHeight / Mathf.Max(0.01f, sourceBounds.size.y);
        value.transform.localScale = Vector3.one * scale;
        var fittedBounds = CombinedBounds(value.transform);
        value.transform.position += bottomCenter -
            new Vector3(fittedBounds.center.x, fittedBounds.min.y, fittedBounds.center.z);
        foreach (var renderer in value.GetComponentsInChildren<Renderer>(true))
        {
            var materials = renderer.sharedMaterials;
            for (var index = 0; index < materials.Length; index++) materials[index] = material;
            renderer.sharedMaterials = materials;
        }
        return value;
    }

    static Material EnsureFortressR6SolidMaterial(string name, Color color,
        float smoothness = 0.13f, float metallic = 0f)
    {
        var path = "Assets/_Project/Materials/Review/" + name + ".mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit")) { name = name };
            AssetDatabase.CreateAsset(material, path);
        }
        material.SetColor("_BaseColor", color);
        material.SetColor("_Color", color);
        material.SetFloat("_Smoothness", smoothness);
        material.SetFloat("_Metallic", metallic);
        EditorUtility.SetDirty(material);
        return material;
    }

    static Camera BuildFortressR6ReviewCamera(Transform root, Bounds bounds)
    {
        var value = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        value.tag = "MainCamera";
        value.transform.SetParent(root);
        var camera = value.GetComponent<Camera>();
        camera.fieldOfView = 38f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 500f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.66f, 0.71f, 0.77f, 1f);
        var radius = Mathf.Max(bounds.size.x, bounds.size.z);
        // Blender's -Y review face imports as Unity's +Z. Keep this camera on that
        // side so the approved front wall, banners, and gate remain visible.
        camera.transform.position = bounds.center + new Vector3(-radius * 1.4f, radius * 0.8f, radius * 2.0f);
        camera.transform.rotation = Quaternion.LookRotation(bounds.center + Vector3.up * bounds.extents.y * 0.1f -
            camera.transform.position);
        return camera;
    }

    static void BuildFortressR6ReviewLighting(Transform root, Bounds bounds)
    {
        var sun = new GameObject("REVIEW__Sun", typeof(Light));
        sun.transform.SetParent(root);
        sun.transform.rotation = Quaternion.Euler(42f, -32f, 0f);
        var light = sun.GetComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.90f, 0.78f);
        light.intensity = 1.55f;
        light.shadows = LightShadows.Soft;
        RenderSettings.sun = light;
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.skybox = null;
        RenderSettings.ambientIntensity = 1.25f;
        RenderSettings.ambientSkyColor = new Color(0.43f, 0.49f, 0.56f);
        RenderSettings.ambientEquatorColor = new Color(0.38f, 0.34f, 0.28f);
        RenderSettings.ambientGroundColor = new Color(0.20f, 0.17f, 0.13f);
    }
}
