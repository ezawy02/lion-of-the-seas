using System.IO;
using UnityEditor;
using UnityEngine;

public sealed class Level01LocalArtReviewWindow : EditorWindow
{
    const string TraversalReferencePath =
        "ArtSource/References/Level01/REF_Level01_Traversal_GateRescue.png";
    const string TraversalUnityPath =
        "Artifacts/Local/Approval/Level01Traversal/01_Traversal_Full_REVIEW.png";

    string referencePath;
    string unityPath;
    string heading;
    string referenceLabel;
    string unityLabel;
    string reviewNote;
    Texture2D reference;
    Texture2D unityCapture;

    [MenuItem("Lion of the Seas/Review Level 01 Traversal R15 Gate Energy")]
    public static void ShowTraversalReview()
    {
        ShowReview("Level 01 Traversal R15 Gate Energy — User Review",
            "LEVEL 01 — TRAVERSAL / GATE RESCUE — R15 GATE ENERGY",
            TraversalReferencePath, TraversalUnityPath, "EXECUTION REFERENCE", "UNITY R15",
            "Left: execution reference. Right: exact local Unity R15 capture with the blue gate " +
            "field, center beam, and water halo added to the existing L01-GAT-001 model. " +
            "This revision is not approved until the user confirms it.");
    }

    [MenuItem("Lion of the Seas/Review Level 01 Boss Battle R2")]
    public static void ShowBossBattleReview()
    {
        ShowReview("Level 01 Boss Battle R2 — User Review",
            "LEVEL 01 — BOSS BATTLE — RESTORED FORTRESS SCALE R2",
            "ArtSource/References/Level01/REF_Level01_BossBattle.png",
            "Artifacts/Local/Approval/Level01BossBattle/01_BossBattle_Full_REVIEW.png",
            "POSTER / PALETTE REFERENCE", "UNITY R2 — 38-UNIT FORTRESS",
            "Left: poster reference for palette, mood, silhouette, and encounter scale only. " +
            "Right: exact local Unity R2 capture with the agreed 38-unit fortress width restored. " +
            "Camera/layout are not copied from poster art. User approval is required.");
    }

    [MenuItem("Lion of the Seas/Review Benchmark Art R3 Gate Energy")]
    public static void ShowBenchmarkArtReview()
    {
        ShowReview("Benchmark Art R3 Gate Energy — User Review",
            "BENCHMARK ART — X4 GATE ENERGY R3",
            "ArtSource/References/Level01/REF_Level01_BossBattle.png",
            "Artifacts/Local/Approval/BenchmarkArt/01_Primary_REVIEW.png",
            "PORTAL / COLOR REFERENCE", "UNITY BENCHMARK R3 — GATE ENERGY",
            "Left: poster reference for the blue portal aura and color intent. Right: exact local Unity R3 " +
            "candidate with a blue portal field, center beam, and water halo added to the existing gate. " +
            "The gate model and agreed fortress scale are unchanged. Gate energy was user-approved " +
            "on 2026-08-30; the remaining benchmark contract is not yet approved.");
    }

    [MenuItem("Lion of the Seas/Review Reference Match R5/01 Opening")]
    public static void ShowReferenceMatchR5Opening()
    {
        ShowReferenceMatchR5("OPENING", "REF_Level01_Opening.png",
            "01_Opening_ReferenceMatch_R5_REVIEW.png");
    }

    [MenuItem("Lion of the Seas/Review Reference Match R5/02 Traversal")]
    public static void ShowReferenceMatchR5Traversal()
    {
        ShowReferenceMatchR5("TRAVERSAL / GATE RESCUE", "REF_Level01_Traversal_GateRescue.png",
            "02_Traversal_ReferenceMatch_R5_REVIEW.png");
    }

    [MenuItem("Lion of the Seas/Review Reference Match R5/03 Beach Landing")]
    public static void ShowReferenceMatchR5Beach()
    {
        ShowReferenceMatchR5("BEACH LANDING", "REF_Level01_BeachLanding.png",
            "03_BeachLanding_ReferenceMatch_R5_REVIEW.png");
    }

    [MenuItem("Lion of the Seas/Review Reference Match R5/04 Boss Battle")]
    public static void ShowReferenceMatchR5Boss()
    {
        ShowReferenceMatchR5("BOSS BATTLE", "REF_Level01_BossBattle.png",
            "04_BossBattle_ReferenceMatch_R5_REVIEW.png");
    }

    [MenuItem("Lion of the Seas/Review Reference Match R5/05 Benchmark")]
    public static void ShowReferenceMatchR5Benchmark()
    {
        ShowReferenceMatchR5("BENCHMARK", "REF_Level01_BossBattle.png",
            "05_Benchmark_ReferenceMatch_R5_REVIEW.png");
    }

    [MenuItem("Lion of the Seas/Review Loadout R2/Prototype vs Game UI")]
    public static void ShowLoadoutR2Review()
    {
        ShowReview("Loadout R2 — User Review",
            "LOADOUT — R1 PROTOTYPE VS R2 PROFESSIONAL GAME UI",
            "Artifacts/Local/Approval/Loadout/R1/Loadout_Reward_REVIEW.png",
            "Artifacts/Local/Approval/Loadout/R2/Loadout_Reward_R2_REVIEW.png",
            "R1 — TECHNICAL PROTOTYPE", "UNITY R2 — REVIEW CANDIDATE",
            "Left: the original flat technical prototype. Right: the exact local Unity R2 " +
            "review candidate with a nautical command-table identity, double-bezel cards, " +
            "clear equipped/locked states, reward hierarchy, and a primary Set Sail action. " +
            "R2 is not approved until the user explicitly accepts this exact revision.");
    }

    static void ShowReferenceMatchR5(string phase, string referenceFile, string captureFile)
    {
        ShowReview("Level 01 " + phase + " R5 — User Review",
            "LEVEL 01 — " + phase + " — REFERENCE MATCH R5",
            "ArtSource/References/Level01/" + referenceFile,
            "Artifacts/Local/Approval/Level01ReferenceMatch/R5/" + captureFile,
            "EXECUTION REFERENCE", "UNITY R5 — USER APPROVED",
            "Left: local execution reference. Right: exact local Unity R5 candidate. " +
            "The user explicitly approved this exact R5 capture package on 2026-08-31. " +
            "Later visible revisions still require a new Unity review and explicit approval.");
    }

    static void ShowReview(string windowTitle, string reviewHeading, string leftPath,
        string rightPath, string leftLabel, string rightLabel, string note)
    {
        var window = GetWindow<Level01LocalArtReviewWindow>(true, windowTitle, true);
        window.minSize = new Vector2(900f, 620f);
        window.position = new Rect(80f, 60f, 1200f, 760f);
        window.heading = reviewHeading;
        window.referencePath = leftPath;
        window.unityPath = rightPath;
        window.referenceLabel = leftLabel;
        window.unityLabel = rightLabel;
        window.reviewNote = note;
        window.LoadImages();
        window.Show();
        window.Focus();
    }

    void OnEnable()
    {
        if (string.IsNullOrEmpty(referencePath))
        {
            referencePath = TraversalReferencePath;
            unityPath = TraversalUnityPath;
            heading = "LEVEL 01 — TRAVERSAL / GATE RESCUE — R15 GATE ENERGY";
            referenceLabel = "EXECUTION REFERENCE";
            unityLabel = "UNITY R15";
            reviewNote = "This exact local revision requires user approval inside Unity.";
        }
        LoadImages();
    }

    void OnDisable()
    {
        if (reference != null) DestroyImmediate(reference);
        if (unityCapture != null) DestroyImmediate(unityCapture);
    }

    void LoadImages()
    {
        if (reference != null) DestroyImmediate(reference);
        if (unityCapture != null) DestroyImmediate(unityCapture);
        reference = LoadLocalTexture(referencePath);
        unityCapture = LoadLocalTexture(unityPath);
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
        EditorGUILayout.LabelField(heading, EditorStyles.boldLabel);
        EditorGUILayout.HelpBox(reviewNote, MessageType.Info);
        EditorGUILayout.Space(6f);

        using (new EditorGUILayout.HorizontalScope())
        {
            DrawReviewImage(referenceLabel, reference);
            DrawReviewImage(unityLabel, unityCapture);
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
