using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

internal static class Level01FortressConceptPreview
{
    const string SourceScene = "Assets/_Project/Scenes/Review/Level01_BeachLanding_REVIEW.unity";
    const string PreviewScene = "Assets/_Project/Scenes/Review/Level01_Fortress_R5_CONCEPT_REVIEW.unity";
    const string CapturePath = "Artifacts/Local/Approval/Level01FortressModules/Fortress_R5_ModularBody_PREVIEW_ONLY.png";
    const string SourceStoneMaterial = "Assets/_Project/Materials/Imported/L01-ENV-001_Fortress_Wall_Module.mat";
    const string SourceAccentMaterial = "Assets/_Project/Materials/Blockout/Limestone.mat";
    const string StoneMaterial = "Assets/_Project/Materials/Review/Fortress_R5_Stone_PREVIEW.mat";
    const string AccentMaterial = "Assets/_Project/Materials/Review/Fortress_R5_Accent_PREVIEW.mat";
    const string RecessMaterial = "Assets/_Project/Materials/Review/Fortress_R5_Recess_PREVIEW.mat";

    static readonly string[] ReplacedObjects =
    {
        "FORTRESS__Wall",
        "FORTRESS__Tower_Left",
        "FORTRESS__Tower_Right",
        "FORTRESS__MainGate",
        "FORTRESS__SideDoor",
        "FORTRESS__ShoreCannon_Left",
        "FORTRESS__ShoreCannon_Right",
        "FORTRESS__Brazier_Left",
        "FORTRESS__Brazier_Right",
        "FORTRESS__AmmoTray"
    };

    [MenuItem("Lion of the Seas/Build Fortress R5 Modular Concept PREVIEW")]
    public static void BuildAndCapture()
    {
        OpenPreviewCopy();
        BuildConcept();
        EditorSceneManager.SaveOpenScenes();
        CaptureGameplayCamera();
        Debug.Log($"Fortress R5 modular concept built in {PreviewScene}; production was not modified.");
    }

    static void OpenPreviewCopy()
    {
        var current = SceneManager.GetActiveScene();
        if (current.path != SourceScene)
            EditorSceneManager.OpenScene(SourceScene, OpenSceneMode.Single);

        AssetDatabase.DeleteAsset(PreviewScene);
        if (!AssetDatabase.CopyAsset(SourceScene, PreviewScene))
            throw new InvalidOperationException($"Could not copy {SourceScene} to {PreviewScene}.");

        AssetDatabase.ImportAsset(PreviewScene, ImportAssetOptions.ForceSynchronousImport);
        EditorSceneManager.OpenScene(PreviewScene, OpenSceneMode.Single);
    }

    static void BuildConcept()
    {
        var existingGate = FindSceneObject("FORTRESS__MainGate");
        var existingRightCannon = FindSceneObject("FORTRESS__ShoreCannon_Right");
        var gateClone = existingGate == null ? null : UnityEngine.Object.Instantiate(existingGate);
        var cannonClone = existingRightCannon == null ? null : UnityEngine.Object.Instantiate(existingRightCannon);
        if (gateClone != null)
            gateClone.transform.SetPositionAndRotation(existingGate.transform.position, existingGate.transform.rotation);

        foreach (var name in ReplacedObjects)
        {
            var existing = FindSceneObject(name);
            if (existing != null) existing.SetActive(false);
        }

        var previous = FindSceneObject("FORTRESS_R5_CONCEPT_PREVIEW_ONLY");
        if (previous != null) UnityEngine.Object.DestroyImmediate(previous);

        var root = new GameObject("FORTRESS_R5_CONCEPT_PREVIEW_ONLY");
        var stone = EnsureReviewMaterial(StoneMaterial, SourceStoneMaterial, new Color(0.72f, 0.82f, 0.86f, 1f));
        var accent = EnsureReviewMaterial(AccentMaterial, SourceAccentMaterial, new Color(0.82f, 0.78f, 0.68f, 1f));
        var recess = EnsureReviewMaterial(RecessMaterial, SourceStoneMaterial, new Color(0.18f, 0.19f, 0.2f, 1f));

        BuildRockPlinth(root.transform, accent);
        BuildLeftBastion(root.transform, stone, accent);
        BuildRightArtilleryBastion(root.transform, stone, accent);
        BuildGatehouse(root.transform, stone, accent, gateClone);
        BuildCurtainWalls(root.transform, stone, accent);
        BuildRearKeep(root.transform, stone, accent);
        BuildFacadeDetails(root.transform, accent, recess);
        PlaceRightCannon(root.transform, cannonClone);

        Selection.activeGameObject = root;
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
    }

    static void BuildRockPlinth(Transform root, Material rock)
    {
        MakeBlock(root, "Plinth_Center", new Vector3(14f, 1.05f, 111.5f), new Vector3(43f, 2.1f, 14f), rock);
        MakeBlock(root, "Plinth_Left_Slope", new Vector3(-6.2f, 1.3f, 109.8f), new Vector3(8f, 2.6f, 13f), rock, new Vector3(0f, 8f, -4f));
        MakeBlock(root, "Plinth_Right_Slope", new Vector3(34.2f, 1.4f, 109.4f), new Vector3(9f, 2.8f, 14f), rock, new Vector3(0f, -7f, 4f));
    }

    static void BuildLeftBastion(Transform root, Material stone, Material accent)
    {
        const float x = -3.8f;
        MakeBlock(root, "LeftBastion_Lower", new Vector3(x, 6.1f, 110.3f), new Vector3(13f, 10.6f, 12f), stone);
        MakeBlock(root, "LeftBastion_Upper", new Vector3(x, 12.1f, 110.6f), new Vector3(11.2f, 3.8f, 10.4f), stone);
        MakeBlock(root, "LeftBastion_Cap", new Vector3(x, 14.2f, 110.6f), new Vector3(12.1f, 0.65f, 11.2f), accent);
        BuildBattlements(root, "Left", new Vector3(x, 14.95f, 105.3f), 11.2f, true, stone);
        BuildBattlements(root, "LeftSide", new Vector3(-9.35f, 14.95f, 110.6f), 10.4f, false, stone);
    }

    static void BuildRightArtilleryBastion(Transform root, Material stone, Material accent)
    {
        const float x = 31.7f;
        MakeBlock(root, "RightBastion_Lower", new Vector3(x, 6.3f, 110f), new Vector3(14.5f, 11f, 13.2f), stone);
        MakeBlock(root, "RightBastion_Upper", new Vector3(x, 12.5f, 109.7f), new Vector3(12.6f, 4.2f, 11.4f), stone);
        MakeBlock(root, "RightBastion_Cap", new Vector3(x, 14.8f, 109.7f), new Vector3(13.5f, 0.7f, 12.3f), accent);
        BuildBattlements(root, "Right", new Vector3(x, 15.65f, 103.95f), 12.6f, true, stone);
        BuildBattlements(root, "RightSide", new Vector3(37.95f, 15.65f, 109.7f), 11.4f, false, stone);
    }

    static void BuildGatehouse(Transform root, Material stone, Material accent, GameObject gate)
    {
        var gateCenter = new Vector3(14f, 0.37f, 108f);
        var gateWidth = 8f;
        var gateHeight = 8f;
        if (gate != null)
        {
            gate.name = "Concept_ExistingMainGateAsset";
            gate.transform.SetParent(root, true);
            gate.SetActive(true);
            var bounds = CombinedBounds(gate);
            gateCenter = bounds.center;
            gateWidth = Mathf.Clamp(bounds.size.x, 6f, 12f);
            gateHeight = Mathf.Clamp(bounds.size.y, 6f, 10f);
        }

        var pierWidth = 4.2f;
        var pierHeight = 11.5f;
        var leftX = gateCenter.x - gateWidth * 0.5f - pierWidth * 0.5f + 0.4f;
        var rightX = gateCenter.x + gateWidth * 0.5f + pierWidth * 0.5f - 0.4f;
        MakeBlock(root, "Gatehouse_LeftPier", new Vector3(leftX, 6f, 110f), new Vector3(pierWidth, pierHeight, 8f), stone);
        MakeBlock(root, "Gatehouse_RightPier", new Vector3(rightX, 6f, 110f), new Vector3(pierWidth, pierHeight, 8f), stone);
        MakeBlock(root, "Gatehouse_Lintel", new Vector3(gateCenter.x, gateHeight + 2.8f, 110f), new Vector3(gateWidth + 2f, 4.3f, 8f), stone);
        MakeBlock(root, "Gatehouse_Cap", new Vector3(gateCenter.x, 13.2f, 110f), new Vector3(gateWidth + 10f, 0.7f, 9f), accent);
        BuildBattlements(root, "Gatehouse", new Vector3(gateCenter.x, 14f, 105.6f), gateWidth + 9f, true, stone);
    }

    static void BuildCurtainWalls(Transform root, Material stone, Material accent)
    {
        MakeBlock(root, "Curtain_Left", new Vector3(3.5f, 6f, 112f), new Vector3(8f, 9.3f, 6f), stone);
        MakeBlock(root, "Curtain_Right", new Vector3(24.5f, 6f, 112f), new Vector3(8f, 9.3f, 6f), stone);
        MakeBlock(root, "Curtain_Left_Cap", new Vector3(3.5f, 10.9f, 112f), new Vector3(8.7f, 0.55f, 6.7f), accent);
        MakeBlock(root, "Curtain_Right_Cap", new Vector3(24.5f, 10.9f, 112f), new Vector3(8.7f, 0.55f, 6.7f), accent);
        BuildBattlements(root, "CurtainLeft", new Vector3(3.5f, 11.65f, 108.9f), 8f, true, stone);
        BuildBattlements(root, "CurtainRight", new Vector3(24.5f, 11.65f, 108.9f), 8f, true, stone);
    }

    static void BuildRearKeep(Transform root, Material stone, Material accent)
    {
        MakeBlock(root, "RearKeep_Main", new Vector3(14f, 9.8f, 117.2f), new Vector3(20f, 9.5f, 7f), stone);
        MakeBlock(root, "RearKeep_Upper", new Vector3(14f, 15.8f, 117.2f), new Vector3(14f, 2.5f, 6f), stone);
        MakeBlock(root, "RearKeep_Cap", new Vector3(14f, 17.4f, 117.2f), new Vector3(15f, 0.65f, 6.8f), accent);
        BuildBattlements(root, "RearKeep", new Vector3(14f, 18.2f, 113.9f), 14f, true, stone);
    }

    static void BuildFacadeDetails(Transform root, Material accent, Material recess)
    {
        MakeBlock(root, "LeftCorner_Quoin_A", new Vector3(-9.9f, 7.2f, 104.2f), new Vector3(0.7f, 10.5f, 0.4f), accent);
        MakeBlock(root, "LeftCorner_Quoin_B", new Vector3(2.3f, 7.2f, 104.2f), new Vector3(0.7f, 10.5f, 0.4f), accent);
        MakeBlock(root, "RightCorner_Quoin_A", new Vector3(24.8f, 7.5f, 103.35f), new Vector3(0.75f, 11f, 0.4f), accent);
        MakeBlock(root, "RightCorner_Quoin_B", new Vector3(38.6f, 7.5f, 103.35f), new Vector3(0.75f, 11f, 0.4f), accent);

        var leftSlots = new[] { -7f, -3.8f, -0.6f };
        var rightSlots = new[] { 27.7f, 31.7f, 35.7f };
        foreach (var x in leftSlots)
            MakeBlock(root, $"LeftArrowSlit_{x:0.0}", new Vector3(x, 9f, 104.15f), new Vector3(0.48f, 1.8f, 0.24f), recess);
        foreach (var x in rightSlots)
            MakeBlock(root, $"RightArrowSlit_{x:0.0}", new Vector3(x, 9.5f, 103.25f), new Vector3(0.52f, 1.9f, 0.24f), recess);
        MakeBlock(root, "Gatehouse_Slit_Left", new Vector3(7.9f, 10.3f, 105.85f), new Vector3(0.45f, 1.6f, 0.24f), recess);
        MakeBlock(root, "Gatehouse_Slit_Right", new Vector3(20.1f, 10.3f, 105.85f), new Vector3(0.45f, 1.6f, 0.24f), recess);
    }

    static void PlaceRightCannon(Transform root, GameObject cannon)
    {
        if (cannon == null) return;
        cannon.name = "Concept_RightTower_Cannon_ExistingAsset";
        cannon.transform.SetParent(root, true);
        cannon.transform.position = new Vector3(31.7f, 15.3f, 104.8f);
        cannon.transform.rotation = Quaternion.Euler(0f, 188f, 0f);
        cannon.SetActive(true);
    }

    static void BuildBattlements(Transform root, string prefix, Vector3 center, float span, bool alongX, Material material)
    {
        var count = Mathf.Max(3, Mathf.FloorToInt(span / 2.2f));
        for (var i = 0; i < count; i++)
        {
            var offset = Mathf.Lerp(-span * 0.42f, span * 0.42f, count == 1 ? 0.5f : i / (float)(count - 1));
            var position = center + (alongX ? Vector3.right : Vector3.forward) * offset;
            MakeBlock(root, $"{prefix}_Merlon_{i + 1:00}", position, new Vector3(1.25f, 1.5f, 1.25f), material);
        }
    }

    static GameObject MakeBlock(Transform root, string name, Vector3 position, Vector3 size, Material material, Vector3 rotation = default)
    {
        var block = GameObject.CreatePrimitive(PrimitiveType.Cube);
        block.name = name;
        block.transform.SetParent(root, false);
        block.transform.SetPositionAndRotation(position, Quaternion.Euler(rotation));
        block.transform.localScale = size;
        var renderer = block.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        UnityEngine.Object.DestroyImmediate(block.GetComponent<Collider>());
        return block;
    }

    static GameObject FindSceneObject(string name)
    {
        return Resources.FindObjectsOfTypeAll<GameObject>()
            .FirstOrDefault(go => go.scene.IsValid() && go.scene == SceneManager.GetActiveScene() && go.name == name);
    }

    static Bounds CombinedBounds(GameObject gameObject)
    {
        var renderers = gameObject.GetComponentsInChildren<Renderer>(true);
        if (renderers.Length == 0) return new Bounds(gameObject.transform.position, Vector3.one);
        var bounds = renderers[0].bounds;
        for (var i = 1; i < renderers.Length; i++) bounds.Encapsulate(renderers[i].bounds);
        return bounds;
    }

    static Material EnsureReviewMaterial(string path, string sourcePath, Color color)
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            var source = AssetDatabase.LoadAssetAtPath<Material>(sourcePath);
            if (source == null) throw new InvalidDataException($"Missing source material: {sourcePath}");
            material = new Material(source) { name = Path.GetFileNameWithoutExtension(path) };
            AssetDatabase.CreateAsset(material, path);
        }

        if (material.HasProperty("_BaseColor")) material.SetColor("_BaseColor", color);
        else material.color = color;
        EditorUtility.SetDirty(material);
        return material;
    }

    static void CaptureGameplayCamera()
    {
        var camera = Resources.FindObjectsOfTypeAll<Camera>()
            .FirstOrDefault(candidate => candidate.gameObject.scene == SceneManager.GetActiveScene() && candidate.name == "PORTRAIT_CAMERA__Gameplay");
        if (camera == null) throw new InvalidDataException("Gameplay camera was not found.");

        const int width = 720;
        const int height = 1280;
        var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            var absolutePath = Path.GetFullPath(CapturePath);
            Directory.CreateDirectory(Path.GetDirectoryName(absolutePath) ?? string.Empty);
            File.WriteAllBytes(absolutePath, texture.EncodeToPNG());
            UnityEngine.Object.DestroyImmediate(texture);
        }
        finally
        {
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            RenderTexture.ReleaseTemporary(target);
        }
    }
}
