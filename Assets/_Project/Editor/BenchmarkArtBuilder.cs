#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>Builds the portrait art review scene from the authored FBX benchmark assets.</summary>
public static class BenchmarkArtBuilder
{
    public const string ScenePath = "Assets/_Project/Scenes/Benchmark_Art.unity";

    private static readonly string ArtRoot = "Assets/_Project/Art/";

    [MenuItem("Sea Lion/Art Review/Build Benchmark Art Scene")]
    public static void Build()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var root = new GameObject("BENCHMARK_ART__PortraitReview").transform;
        var environment = new GameObject("ENVIRONMENT__MediterraneanHarbor").transform;
        environment.SetParent(root);
        PlaceModel(environment, "MediterraneanHarbor", ArtRoot + "Environment/MediterraneanHarbor.fbx", new Vector3(0f, 0f, 26f), Quaternion.identity, Vector3.one);

        var water = PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/VFX/WaterSurface.prefab")) as GameObject;
        water.name = "WATER__PrimarySurface";
        water.transform.SetParent(root);
        water.transform.position = new Vector3(0f, -0.04f, 5f);
        water.transform.localScale = new Vector3(1.75f, 1f, 2.2f);

        var ships = new GameObject("FORMATION__FlagshipAndWakes").transform;
        ships.SetParent(root);
        PlaceModel(ships, "FLAGSHIP__Hero", ArtRoot + "Ships/Flagship.fbx", new Vector3(0f, 0.2f, -4f), Quaternion.Euler(0f, 180f, 0f), Vector3.one);
        PlaceVfx(ships, "VFX__FlagshipWake", "Assets/_Project/VFX/Wake.prefab", new Vector3(0f, 0.03f, -7.3f), Quaternion.identity, 1.4f);

        var friendly = new GameObject("FORMATION__FriendlyCrew").transform;
        friendly.SetParent(root);
        for (var i = 0; i < 7; i++)
        {
            var x = (i - 3) * 2.15f;
            var z = 5.5f + (i % 2) * 1.7f;
            PlaceModel(friendly, "FRIENDLY__Crew_" + i.ToString("00"), ArtRoot + "Characters/FriendlyCrew.fbx", new Vector3(x, 0f, z), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 0.82f);
        }

        var hostile = new GameObject("FORMATION__HostileDefenders").transform;
        hostile.SetParent(root);
        for (var i = 0; i < 7; i++)
        {
            var x = (i - 3) * 2.2f;
            var z = 15f + (i % 2) * 1.4f;
            PlaceModel(hostile, "HOSTILE__Defender_" + i.ToString("00"), ArtRoot + "Characters/HostileEnemy.fbx", new Vector3(x, 0f, z), Quaternion.identity, Vector3.one * 0.86f);
        }

        var setPieces = new GameObject("SETPIECES__GateAndGuardian").transform;
        setPieces.SetParent(root);
        PlaceModel(setPieces, "GATE__MultiplierChoice", ArtRoot + "Environment/GateMultiplier.fbx", new Vector3(0f, 0f, 20f), Quaternion.identity, Vector3.one);
        PlaceModel(setPieces, "GUARDIAN__HarborBoss", ArtRoot + "Characters/HarborGuardian.fbx", new Vector3(0f, 0f, 29f), Quaternion.identity, Vector3.one * 0.9f);
        PlaceVfx(setPieces, "VFX__GateFoam", "Assets/_Project/VFX/FoamPatch.prefab", new Vector3(0f, 0.035f, 20f), Quaternion.identity, 2.5f);
        PlaceVfx(setPieces, "VFX__GuardianReaction", "Assets/_Project/VFX/BossReaction.prefab", new Vector3(0f, 0.04f, 29f), Quaternion.identity, 2.2f);

        var cameraObject = new GameObject("CAMERA__PortraitArtReview");
        cameraObject.transform.SetParent(root);
        cameraObject.transform.position = new Vector3(0f, 25f, -29f);
        cameraObject.transform.rotation = Quaternion.Euler(52f, 0f, 0f);
        var camera = cameraObject.AddComponent<Camera>();
        camera.tag = "MainCamera";
        camera.fieldOfView = 43f;
        camera.aspect = 9f / 16f;
        camera.nearClipPlane = 0.1f;
        camera.farClipPlane = 180f;

        var lightObject = new GameObject("LIGHTING__WarmKey");
        lightObject.transform.SetParent(root);
        lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.35f;
        light.color = new Color(1f, 0.82f, 0.64f);
        light.shadows = LightShadows.Soft;

        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = new Color(0.16f, 0.27f, 0.4f);
        RenderSettings.ambientEquatorColor = new Color(0.35f, 0.48f, 0.5f);
        RenderSettings.ambientGroundColor = new Color(0.1f, 0.12f, 0.15f);
        RenderSettings.fog = true;
        RenderSettings.fogColor = new Color(0.18f, 0.3f, 0.36f);
        RenderSettings.fogMode = FogMode.Linear;
        RenderSettings.fogStartDistance = 45f;
        RenderSettings.fogEndDistance = 120f;

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Benchmark art scene built: " + ScenePath);
    }

    public static void ValidateScene()
    {
        var scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var names = new[] { "WATER__PrimarySurface", "FLAGSHIP__Hero", "FORMATION__FriendlyCrew", "FORMATION__HostileDefenders", "GATE__MultiplierChoice", "GUARDIAN__HarborBoss", "CAMERA__PortraitArtReview" };
        var missing = new List<string>();
        for (var i = 0; i < names.Length; i++) if (GameObject.Find(names[i]) == null) missing.Add(names[i]);
        var renderers = Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None);
        if (missing.Count > 0) throw new System.InvalidOperationException("Benchmark art scene missing: " + string.Join(", ", missing));
        if (renderers.Length < 15) throw new System.InvalidOperationException("Benchmark art scene has too few renderers: " + renderers.Length);
        Debug.Log("Benchmark art scene validation passed: " + renderers.Length + " renderers.");
    }

    [MenuItem("Sea Lion/Art Review/Open Rejected Prototype In Game View")]
    public static void OpenReview()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        var camera = GameObject.Find("CAMERA__PortraitArtReview");
        Selection.activeGameObject = camera;
        if (camera != null) EditorGUIUtility.PingObject(camera);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
        Debug.Log("Opened rejected T047 prototype for in-engine review. This is not approved art.");
    }

    private static GameObject PlaceModel(Transform parent, string name, string path, Vector3 position, Quaternion rotation, Vector3 scale)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) { Debug.LogError("Missing benchmark FBX: " + path); return null; }
        var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        instance.name = name;
        instance.transform.SetParent(parent);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.transform.localScale = scale;
        return instance;
    }

    private static GameObject PlaceVfx(Transform parent, string name, string path, Vector3 position, Quaternion rotation, float scale)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) { Debug.LogError("Missing benchmark VFX prefab: " + path); return null; }
        var instance = PrefabUtility.InstantiatePrefab(asset) as GameObject;
        instance.name = name;
        instance.transform.SetParent(parent);
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.transform.localScale = Vector3.one * scale;
        return instance;
    }
}
#endif
