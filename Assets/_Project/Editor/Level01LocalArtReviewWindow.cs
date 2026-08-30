using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class Level01LocalArtReviewWindow : EditorWindow
{
    const string ReferencePath =
        "ArtSource/References/Level01/REF_Level01_Traversal_GateRescue.png";
    const string UnityPath =
        "Artifacts/Local/Approval/Level01Traversal/01_Traversal_Full_REVIEW.png";

    Texture2D reference;
    Texture2D unityCapture;

    [MenuItem("Lion of the Seas/Review Level 01 Traversal R11")]
    public static void ShowTraversalReview()
    {
        var window = GetWindow<Level01LocalArtReviewWindow>(true,
            "Level 01 Traversal R11 — User Review", true);
        window.minSize = new Vector2(900f, 620f);
        window.position = new Rect(80f, 60f, 1200f, 760f);
        window.LoadImages();
        window.Show();
        window.Focus();
    }

    void OnEnable() => LoadImages();

    void OnDisable()
    {
        if (reference != null) DestroyImmediate(reference);
        if (unityCapture != null) DestroyImmediate(unityCapture);
    }

    void LoadImages()
    {
        if (reference == null) reference = LoadLocalTexture(ReferencePath);
        if (unityCapture == null) unityCapture = LoadLocalTexture(UnityPath);
        Repaint();
    }

    static Texture2D LoadLocalTexture(string path)
    {
        if (!File.Exists(path)) return null;
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
        {
            name = Path.GetFileNameWithoutExtension(path),
            hideFlags = HideFlags.HideAndDontSave
        };
        if (texture.LoadImage(File.ReadAllBytes(path))) return texture;
        DestroyImmediate(texture);
        return null;
    }

    void OnGUI()
    {
        EditorGUILayout.Space(10f);
        EditorGUILayout.LabelField("LEVEL 01 — TRAVERSAL / GATE RESCUE — R11 REVIEW",
            EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(
            "Left: approved execution reference. Right: exact local Unity R11 capture. " +
            "This revision is not approved until the user confirms it.", MessageType.Info);
        EditorGUILayout.Space(6f);

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawReviewImage("REFERENCE", reference);
            DrawReviewImage("UNITY R11", unityCapture);
        }
    }

    static void DrawReviewImage(string label, Texture2D texture)
    {
        using (new EditorGUILayout.VerticalScope(GUILayout.ExpandWidth(true)))
        {
            EditorGUILayout.LabelField(label, EditorStyles.boldLabel);
            var available = GUILayoutUtility.GetRect(300f, 10000f, 480f, 10000f,
                GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
            if (texture == null)
            {
                EditorGUI.HelpBox(available, "Local review image is missing.", MessageType.Error);
                return;
            }
            var fitted = Fit(available, texture.width / (float)texture.height);
            EditorGUI.DrawPreviewTexture(fitted, texture, null, ScaleMode.ScaleToFit);
        }
    }

    static Rect Fit(Rect area, float aspect)
    {
        var width = area.height * aspect;
        if (width <= area.width)
            return new Rect(area.x + (area.width - width) * 0.5f, area.y, width, area.height);
        var height = area.width / aspect;
        return new Rect(area.x, area.y + (area.height - height) * 0.5f, area.width, height);
    }
}
