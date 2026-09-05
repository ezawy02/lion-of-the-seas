using System.IO;
using SeaLion.Core.Definitions;
using SeaLion.UI.Loadout;
using SeaLion.UI.Localization;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class LoadoutBilingualReviewBuilder
{
    private const string DataRoot = "Assets/_Project/Data/Loadouts/VerticalSlice";
    private const string PrefabPath =
        "Assets/_Project/Prefabs/UI/Loadout/LoadoutScreen_R4_Bilingual_REVIEW.prefab";
    private const string ScenePath =
        "Assets/_Project/Scenes/Review/Loadout_Reward_R4_Bilingual_REVIEW.unity";
    private const string EvidenceRoot = "Artifacts/Local/Approval/Loadout/R4";
    private const string EnglishCapture = EvidenceRoot + "/01_Loadout_R4_EN_REVIEW.png";
    private const string ArabicCapture = EvidenceRoot + "/02_Loadout_R4_AR_REVIEW.png";

    [MenuItem("Lion of the Seas/Build Loadout Bilingual R4 REVIEW _F8")]
    public static void BuildReview()
    {
        var flagships = new[]
        {
            Require<FlagshipDefinition>(DataRoot + "/DefaultFlagship.asset"),
            Require<FlagshipDefinition>(DataRoot + "/LateenRaiderFlagship.asset")
        };
        var crew = new[]
        {
            Require<UnitRoleDefinition>(DataRoot + "/DefaultSailorCrew.asset"),
            Require<UnitRoleDefinition>(DataRoot + "/SailmakersCrew.asset")
        };
        var abilities = new[]
        {
            Require<CaptainAbilityDefinition>(DataRoot + "/RallyAbility.asset"),
            Require<CaptainAbilityDefinition>(DataRoot + "/PowderBarrageAbility.asset")
        };

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.005f, 0.025f, 0.04f, 1f);
        camera.orthographic = true;

        var root = new GameObject("UI__LoadoutReward_R4_Bilingual_REVIEW", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster),
            typeof(LoadoutScreenController), typeof(LoadoutScreenPresenter));
        ConfigureCanvas(root, camera);
        var controller = root.GetComponent<LoadoutScreenController>();
        AssignDefinitions(controller, flagships, crew, abilities);
        var presenter = root.GetComponent<LoadoutScreenPresenter>();
        var bindings = LoadoutReviewUiFactory.Build(root.transform, presenter);
        presenter.Configure(controller, bindings.Status, bindings.Summary, bindings.Readiness, bindings.Confirm);
        LoadoutBilingualReviewUiFactory.Enhance(root.transform, presenter);
        presenter.SetLanguage(GameLanguage.English, false);
        ApplyDeterministicReviewState(root, GameLanguage.English);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        PrefabUtility.SaveAsPrefabAsset(root, PrefabPath);
        Object.DestroyImmediate(root);
        var prefab = Require<GameObject>(PrefabPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.GetComponent<Canvas>().worldCamera = camera;
        presenter = instance.GetComponent<LoadoutScreenPresenter>();

        Directory.CreateDirectory(EvidenceRoot);
        presenter.SetLanguage(GameLanguage.English, false);
        ApplyDeterministicReviewState(instance, GameLanguage.English);
        Capture(camera, EnglishCapture);
        presenter.SetLanguage(GameLanguage.Arabic, false);
        ApplyDeterministicReviewState(instance, GameLanguage.Arabic);
        Capture(camera, ArabicCapture);
        presenter.SetLanguage(GameLanguage.English, false);
        ApplyDeterministicReviewState(instance, GameLanguage.English);

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Selection.activeGameObject = instance;
        LoadoutBilingualReviewWindow.ShowWindow();
        Debug.Log("Loadout R4 bilingual REVIEW built. Exact English and Arabic captures require user approval.");
    }

    private static void ApplyDeterministicReviewState(GameObject root, GameLanguage language)
    {
        var header = root.transform.Find("SafeArea/HeaderShell/HeaderCore");
        var readiness = header.Find("ReadinessShell/ReadinessCore/Readiness").GetComponent<Text>();
        var summary = header.Find("SelectionSummary").GetComponent<Text>();
        var status = root.transform.Find("SafeArea/Status").GetComponent<Text>();
        SetDynamicLabel(readiness, LoadoutLocalization.FormatReadiness(3, language), language);
        SetDynamicLabel(summary, LoadoutLocalization.Get("header.summary.ready", language), language);
        SetDynamicLabel(status, LoadoutLocalization.Get("status.review", language), language);
        summary.rectTransform.anchoredPosition = new Vector2(
            language == GameLanguage.Arabic ? -84f : 84f, -106f);
        summary.alignment = language == GameLanguage.Arabic ?
            TextAnchor.MiddleLeft : TextAnchor.MiddleRight;
        var confirm = root.transform.Find("SafeArea/CommandDock/ConfirmShell/ConfirmCore")
            .GetComponent<Button>();
        confirm.interactable = true;
    }

    private static void SetDynamicLabel(Text label, string value, GameLanguage language)
    {
        label.text = LoadoutLocalization.FormatForDisplay(value, language);
        if (language == GameLanguage.Arabic)
        {
            label.font = RuntimeArabicFont.Resolve(label.font);
            label.verticalOverflow = VerticalWrapMode.Overflow;
        }
        else label.verticalOverflow = VerticalWrapMode.Truncate;
    }

    [MenuItem("Lion of the Seas/Open Loadout Bilingual R4 REVIEW")]
    public static void OpenReview()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
    }

    [MenuItem("Lion of the Seas/Open Loadout Bilingual R4 Comparison")]
    public static void OpenComparison()
    {
        LoadoutBilingualReviewWindow.ShowWindow();
    }

    private static void ConfigureCanvas(GameObject root, Camera camera)
    {
        var canvas = root.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        var scaler = root.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720f, 1280f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
    }

    private static void AssignDefinitions(LoadoutScreenController controller,
        FlagshipDefinition[] flagships, UnitRoleDefinition[] crew,
        CaptainAbilityDefinition[] abilities)
    {
        var value = new SerializedObject(controller);
        AssignArray(value.FindProperty("flagships"), flagships);
        AssignArray(value.FindProperty("crewRoles"), crew);
        AssignArray(value.FindProperty("captainAbilities"), abilities);
        value.ApplyModifiedPropertiesWithoutUndo();
    }

    private static void AssignArray<T>(SerializedProperty property, T[] values) where T : Object
    {
        property.arraySize = values.Length;
        for (var index = 0; index < values.Length; index++)
            property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
    }

    private static void Capture(Camera camera, string path)
    {
        var target = new RenderTexture(720, 1280, 24, RenderTextureFormat.ARGB32);
        var texture = new Texture2D(720, 1280, TextureFormat.RGB24, false);
        camera.targetTexture = target;
        var labels = Object.FindObjectsByType<Text>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);
        for (var index = 0; index < labels.Length; index++) labels[index].SetAllDirty();
        for (var pass = 0; pass < 3; pass++)
        {
            Canvas.ForceUpdateCanvases();
            camera.Render();
        }
        RenderTexture.active = target;
        texture.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        camera.targetTexture = null;
        RenderTexture.active = null;
        Object.DestroyImmediate(texture);
        Object.DestroyImmediate(target);
    }

    private static T Require<T>(string path) where T : Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<T>(path);
        if (asset == null) throw new MissingReferenceException("Required asset is missing: " + path);
        return asset;
    }
}

public sealed class LoadoutBilingualReviewWindow : EditorWindow
{
    private Texture2D english;
    private Texture2D arabic;

    public static void ShowWindow()
    {
        var window = GetWindow<LoadoutBilingualReviewWindow>(true,
            "Loadout R4 — English / Arabic Review", true);
        window.minSize = new Vector2(980f, 720f);
        window.Show();
    }

    private void OnEnable()
    {
        english = Load("Artifacts/Local/Approval/Loadout/R4/01_Loadout_R4_EN_REVIEW.png");
        arabic = Load("Artifacts/Local/Approval/Loadout/R4/02_Loadout_R4_AR_REVIEW.png");
    }

    private void OnGUI()
    {
        EditorGUILayout.HelpBox(
            "Exact local Unity R4 bilingual review. R4 mirrors the Arabic layout, tightens " +
            "Arabic typography, and requires approval of this exact revision.",
            MessageType.Info);
        EditorGUILayout.BeginHorizontal();
        Draw("ENGLISH", english);
        Draw("ARABIC", arabic);
        EditorGUILayout.EndHorizontal();
    }

    private static void Draw(string title, Texture2D texture)
    {
        EditorGUILayout.BeginVertical();
        EditorGUILayout.LabelField(title, EditorStyles.boldLabel);
        if (texture != null)
        {
            var width = Mathf.Max(320f, (EditorGUIUtility.currentViewWidth - 44f) * 0.5f);
            GUILayout.Label(texture, GUILayout.Width(width), GUILayout.Height(width * 16f / 9f));
        }
        EditorGUILayout.EndVertical();
    }

    private static Texture2D Load(string path)
    {
        if (!File.Exists(path)) return null;
        var texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
        texture.LoadImage(File.ReadAllBytes(path));
        texture.hideFlags = HideFlags.HideAndDontSave;
        return texture;
    }
}
