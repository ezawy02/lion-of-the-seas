using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public static partial class VerticalSliceBlockoutBuilder
{
    [MenuItem("Lion of the Seas/Show Level 03 Review Camera")]
    public static void ShowLevel03ReviewCamera()
    {
        EditorSceneManager.OpenScene(SceneRoot + "Level_03_StormFortress.unity", OpenSceneMode.Single);
        var camera = Camera.main;
        if (camera == null) throw new MissingComponentException("Level 03 MainCamera is missing.");
        camera.fieldOfView = 40;
        var position = new Vector3(-4, 24, 58);
        var target = new Vector3(0, 6, 108);
        camera.transform.position = position;
        camera.transform.rotation = Quaternion.LookRotation(target - position);
        Selection.activeGameObject = camera.gameObject;
        SceneView.RepaintAll();
    }

    static void SetPhase(string name, bool active)
    {
        foreach (var value in Resources.FindObjectsOfTypeAll<GameObject>())
            if (value.name == name && value.scene.IsValid()) value.SetActive(active);
    }

    static void SetOnlyLevel01Phase(string activeName)
    {
        foreach (var value in Resources.FindObjectsOfTypeAll<GameObject>())
            if (value.scene.IsValid() && value.name.StartsWith("PHASE__"))
                value.SetActive(value.name == activeName);
    }

    static void EnsureOpeningHasNoMultiplierGate()
    {
        foreach (var value in Resources.FindObjectsOfTypeAll<GameObject>())
            if (value.scene.IsValid() && value.activeInHierarchy && value.name.Contains("Multiplier_x4"))
                throw new InvalidDataException("The x4 gate belongs to the later boss-battle phase, not the opening reference.");
    }

    static void Capture(Camera camera, Vector3 position, Vector3 target, string output, string overlayPath = null)
    {
        camera.transform.position = position;
        camera.transform.rotation = Quaternion.LookRotation(target - position);
        var descriptor = new RenderTextureDescriptor(720, 1280, RenderTextureFormat.ARGB32, 24)
        {
            msaaSamples = 1,
            useMipMap = false,
            autoGenerateMips = false,
            sRGB = true
        };
        var renderTarget = new RenderTexture(descriptor);
        if (!renderTarget.Create())
            throw new System.InvalidOperationException("Unable to create the Level 01 evidence render target.");
        var texture = new Texture2D(720, 1280, TextureFormat.RGB24, false);
        camera.targetTexture = renderTarget;
        Canvas.ForceUpdateCanvases();
        camera.Render();
        RenderTexture.active = renderTarget;
        texture.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0);
        texture.Apply();
        if (!string.IsNullOrEmpty(overlayPath)) CompositeOverlay(texture, overlayPath);
        File.WriteAllBytes(output, texture.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(renderTarget);
    }

    static void CompositeOverlay(Texture2D target, string overlayPath)
    {
        var absolutePath = Path.GetFullPath(overlayPath);
        if (!File.Exists(absolutePath)) return;
        var overlay = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        if (!ImageConversion.LoadImage(overlay, File.ReadAllBytes(absolutePath), false))
        {
            Object.DestroyImmediate(overlay);
            return;
        }
        if (overlay.width != target.width || overlay.height != target.height)
        {
            Object.DestroyImmediate(overlay);
            throw new InvalidDataException("The Level 01 HUD overlay must match the portrait evidence resolution.");
        }
        var background = target.GetPixels32();
        var foreground = overlay.GetPixels32();
        for (var i = 0; i < background.Length; i++)
        {
            var alpha = foreground[i].a / 255f;
            if (alpha <= 0f) continue;
            background[i] = new Color32(
                (byte)Mathf.RoundToInt(Mathf.Lerp(background[i].r, foreground[i].r, alpha)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(background[i].g, foreground[i].g, alpha)),
                (byte)Mathf.RoundToInt(Mathf.Lerp(background[i].b, foreground[i].b, alpha)),
                255);
        }
        target.SetPixels32(background);
        target.Apply();
        Object.DestroyImmediate(overlay);
    }
}
