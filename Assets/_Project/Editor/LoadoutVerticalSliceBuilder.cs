using System.IO;
using SeaLion.Core.Definitions;
using SeaLion.UI.Loadout;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public static class LoadoutVerticalSliceBuilder
{
    const string DataRoot = "Assets/_Project/Data/Loadouts/VerticalSlice";
    const string RewardPath = "Assets/_Project/Data/Rewards/Level01Blueprint.asset";
    const string PrefabPath = "Assets/_Project/Prefabs/UI/Loadout/LoadoutScreen_R2_REVIEW.prefab";
    const string ScenePath = "Assets/_Project/Scenes/Review/Loadout_Reward_R2_REVIEW.unity";
    const string CapturePath = "Artifacts/Local/Approval/Loadout/R2/Loadout_Reward_R2_REVIEW.png";

    [MenuItem("Lion of the Seas/Build Loadout Vertical Slice R2 REVIEW")]
    public static void BuildReview()
    {
        EnsureFolders();
        var flagships = BuildFlagships();
        var crew = BuildCrew();
        var abilities = BuildAbilities();
        BuildReward();
        BuildReviewScene(flagships, crew, abilities);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("Loadout vertical slice REVIEW built. Runtime checks and user review are still required.");
    }

    [MenuItem("Lion of the Seas/Open Loadout Vertical Slice R2 REVIEW")]
    public static void OpenReview()
    {
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        EditorApplication.ExecuteMenuItem("Window/General/Game");
    }

    static FlagshipDefinition[] BuildFlagships()
    {
        var standard = EnsureAsset<FlagshipDefinition>(DataRoot + "/DefaultFlagship.asset");
        ConfigureFlagship(standard, "default-flagship", DeployPattern.Cadence, 0.9f, 1, 1f, true,
            "hero-flagship", "wake-standard", "recoil-standard", "audio-broadside-standard", string.Empty);
        var raider = EnsureAsset<FlagshipDefinition>(DataRoot + "/LateenRaiderFlagship.asset");
        ConfigureFlagship(raider, "flagship-lateen-raider", DeployPattern.Burst, 1.45f, 3, 0.86f, false,
            "hero-flagship-raider", "wake-raider", "recoil-heavy", "audio-broadside-heavy",
            "reward-level02-raider-blueprint");
        return new[] { standard, raider };
    }

    static UnitRoleDefinition[] BuildCrew()
    {
        var sailors = EnsureAsset<UnitRoleDefinition>(DataRoot + "/DefaultSailorCrew.asset");
        ConfigureCrew(sailors, "default-crew", UnitRole.Sailor, 4.8f, 1.8f, 8f, 1f, 1f,
            "crew-sailor-mesh", "crew-sailor-material", "crew-sailor-pose", "crew-sailor-vfx", "crew-sailor-audio");
        var sailmakers = EnsureAsset<UnitRoleDefinition>(DataRoot + "/SailmakersCrew.asset");
        ConfigureCrew(sailmakers, "loadout-crew-sailmakers", UnitRole.Defender, 4.1f, 1.62f, 6f, 1.5f, 1.1f,
            "crew-sailmaker-mesh", "crew-sailmaker-material", "crew-sailmaker-pose", "crew-sailmaker-vfx", "crew-sailmaker-audio");
        return new[] { sailors, sailmakers };
    }

    static CaptainAbilityDefinition[] BuildAbilities()
    {
        var rally = EnsureAsset<CaptainAbilityDefinition>(DataRoot + "/RallyAbility.asset");
        ConfigureAbility(rally, "default-ability", AbilityChargeRule.Time, GateOutcome.Add, 8f, 10f, 5f,
            "hero-hayreddin", "vfx-rally", "audio-rally", "camera-rally");
        var barrage = EnsureAsset<CaptainAbilityDefinition>(DataRoot + "/PowderBarrageAbility.asset");
        ConfigureAbility(barrage, "ability-powder-barrage", AbilityChargeRule.Damage, GateOutcome.Damage, 18f, 14f, 9f,
            "hero-hayreddin", "vfx-powder-barrage", "audio-powder-barrage", "camera-barrage");
        return new[] { rally, barrage };
    }

    static void BuildReward()
    {
        var reward = EnsureAsset<RewardDefinition>(RewardPath);
        var serialized = new SerializedObject(reward);
        SetStable(serialized, "id", "reward-level01-loadout-blueprint");
        serialized.FindProperty("grantType").enumValueIndex = (int)RewardGrantType.Ownership;
        SetStable(serialized, "grantTargetId", "loadout-crew-sailmakers");
        serialized.FindProperty("amount").intValue = 1;
        serialized.FindProperty("firstCompletionOnly").boolValue = true;
        SetStable(serialized, "iconId", "icon-blueprint-sailmakers");
        SetStable(serialized, "revealId", "reveal-blueprint-sailmakers");
        SetStable(serialized, "audioId", "audio-reward-sailmakers");
        SetStable(serialized, "descriptionId", "reward-level01-sailmakers-description");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(reward);

        var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(
            "Assets/_Project/Data/Levels/Level01/Level01.asset");
        if (level == null) throw new MissingReferenceException("Level 01 definition is missing.");
        serialized = new SerializedObject(level);
        SetStable(serialized, "rewardId", "reward-level01-loadout-blueprint");
        serialized.ApplyModifiedPropertiesWithoutUndo();
        EditorUtility.SetDirty(level);
    }

    static void BuildReviewScene(FlagshipDefinition[] flagships, UnitRoleDefinition[] crew,
        CaptainAbilityDefinition[] abilities)
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        var cameraObject = new GameObject("Main Camera", typeof(Camera));
        cameraObject.tag = "MainCamera";
        var camera = cameraObject.GetComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.005f, 0.025f, 0.04f, 1f);
        camera.orthographic = true;

        var canvasObject = new GameObject("UI__LoadoutReward_REVIEW", typeof(RectTransform),
            typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster),
            typeof(LoadoutScreenController), typeof(LoadoutScreenPresenter));
        var canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera;
        canvas.planeDistance = 1f;
        var scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(720f, 1280f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        var controller = canvasObject.GetComponent<LoadoutScreenController>();
        AssignDefinitions(controller, flagships, crew, abilities);
        var presenter = canvasObject.GetComponent<LoadoutScreenPresenter>();
        var bindings = LoadoutReviewUiFactory.Build(canvasObject.transform, presenter);
        presenter.Configure(controller, bindings.Status, bindings.Summary,
            bindings.Readiness, bindings.Confirm);

        new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        PrefabUtility.SaveAsPrefabAsset(canvasObject, PrefabPath);
        Object.DestroyImmediate(canvasObject);
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
        instance.GetComponent<Canvas>().worldCamera = camera;
        EditorSceneManager.SaveScene(scene, ScenePath);
        Directory.CreateDirectory(Path.GetDirectoryName(CapturePath));
        Capture(camera, CapturePath);
        Selection.activeGameObject = instance;
    }

    static void ConfigureFlagship(FlagshipDefinition asset, string id, DeployPattern pattern,
        float cadence, int burst, float baseDeployment, bool isDefault, string ship, string wake,
        string recoil, string audio, string reward)
    {
        var value = new SerializedObject(asset); SetStable(value, "id", id);
        var bounds = value.FindProperty("controlBounds");
        bounds.FindPropertyRelative("left").floatValue = 0.08f;
        bounds.FindPropertyRelative("right").floatValue = 0.92f;
        value.FindProperty("deployPattern").enumValueIndex = (int)pattern;
        value.FindProperty("deploymentCadence").floatValue = cadence;
        value.FindProperty("burstSize").intValue = burst;
        value.FindProperty("baseDeployment").floatValue = baseDeployment;
        SetStable(value, "presentationShipId", ship); SetStable(value, "wakeId", wake);
        SetStable(value, "recoilId", recoil); SetStable(value, "audioId", audio);
        value.FindProperty("defaultUnlock").boolValue = isDefault;
        SetStable(value, "unlockRewardId", reward); Apply(value, asset);
    }

    static void ConfigureCrew(UnitRoleDefinition asset, string id, UnitRole role, float speed,
        float damage, float range, float durability, float cadence, params string[] presentation)
    {
        var value = new SerializedObject(asset); SetStable(value, "id", id);
        value.FindProperty("allegiance").enumValueIndex = (int)Allegiance.Friendly;
        value.FindProperty("role").enumValueIndex = (int)role;
        var movement = value.FindProperty("movement"); movement.FindPropertyRelative("speed").floatValue = speed;
        movement.FindPropertyRelative("steering").floatValue = 1.2f;
        var combat = value.FindProperty("combat"); combat.FindPropertyRelative("damage").floatValue = damage;
        combat.FindPropertyRelative("cadence").floatValue = cadence; combat.FindPropertyRelative("range").floatValue = range;
        value.FindProperty("durability").floatValue = durability;
        SetStable(value, "meshId", presentation[0]); SetStable(value, "materialId", presentation[1]);
        SetStable(value, "poseId", presentation[2]); SetStable(value, "vfxId", presentation[3]);
        SetStable(value, "audioId", presentation[4]); Apply(value, asset);
    }

    static void ConfigureAbility(CaptainAbilityDefinition asset, string id, AbilityChargeRule charge,
        GateOutcome outcome, float effect, float duration, float cooldown, params string[] presentation)
    {
        var value = new SerializedObject(asset); SetStable(value, "id", id);
        value.FindProperty("chargeRule").enumValueIndex = (int)charge;
        value.FindProperty("activation").enumValueIndex = (int)AbilityActivation.PlayerTap;
        var typed = value.FindProperty("gameplayEffect");
        typed.FindPropertyRelative("outcome").enumValueIndex = (int)outcome;
        typed.FindPropertyRelative("value").floatValue = effect;
        SetStable(typed.FindPropertyRelative("conversionId"), string.Empty);
        value.FindProperty("duration").floatValue = duration; value.FindProperty("cooldown").floatValue = cooldown;
        SetStable(value, "heroId", presentation[0]); SetStable(value, "vfxId", presentation[1]);
        SetStable(value, "audioId", presentation[2]); SetStable(value, "cameraProfileId", presentation[3]);
        Apply(value, asset);
    }

    static void AssignDefinitions(LoadoutScreenController controller, FlagshipDefinition[] flagships,
        UnitRoleDefinition[] crew, CaptainAbilityDefinition[] abilities)
    {
        var value = new SerializedObject(controller);
        AssignArray(value.FindProperty("flagships"), flagships);
        AssignArray(value.FindProperty("crewRoles"), crew);
        AssignArray(value.FindProperty("captainAbilities"), abilities);
        value.ApplyModifiedPropertiesWithoutUndo();
    }

    static void AssignArray<T>(SerializedProperty property, T[] values) where T : Object
    {
        property.arraySize = values.Length;
        for (var index = 0; index < values.Length; index++)
            property.GetArrayElementAtIndex(index).objectReferenceValue = values[index];
    }

    static void Capture(Camera camera, string path)
    {
        var target = new RenderTexture(720, 1280, 24, RenderTextureFormat.ARGB32);
        var texture = new Texture2D(720, 1280, TextureFormat.RGB24, false);
        camera.targetTexture = target; Canvas.ForceUpdateCanvases(); camera.Render();
        RenderTexture.active = target; texture.ReadPixels(new Rect(0, 0, 720, 1280), 0, 0); texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG()); camera.targetTexture = null; RenderTexture.active = null;
        Object.DestroyImmediate(texture); Object.DestroyImmediate(target);
    }

    static T EnsureAsset<T>(string path) where T : ScriptableObject
    {
        var value = AssetDatabase.LoadAssetAtPath<T>(path);
        if (value != null) return value;
        value = ScriptableObject.CreateInstance<T>(); value.name = Path.GetFileNameWithoutExtension(path);
        AssetDatabase.CreateAsset(value, path); return value;
    }

    static void Apply(SerializedObject serialized, Object asset)
    { serialized.ApplyModifiedPropertiesWithoutUndo(); EditorUtility.SetDirty(asset); }

    static void SetStable(SerializedObject serialized, string property, string id)
    { SetStable(serialized.FindProperty(property), id); }

    static void SetStable(SerializedProperty property, string id)
    { property.FindPropertyRelative("value").stringValue = id; }

    static void EnsureFolders()
    {
        EnsureFolder(DataRoot); EnsureFolder("Assets/_Project/Data/Rewards");
        EnsureFolder("Assets/_Project/Prefabs/UI/Loadout");
        EnsureFolder("Assets/_Project/Scenes/Review");
        EnsureFolder("Assets/_Project/Materials/UI");
        EnsureFolder("Assets/_Project/Art/UI/Generated");
    }

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        var parent = Path.GetDirectoryName(path).Replace('\\', '/');
        EnsureFolder(parent); AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
    }
}
