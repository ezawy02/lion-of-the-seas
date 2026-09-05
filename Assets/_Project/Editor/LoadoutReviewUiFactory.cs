using System.IO;
using SeaLion.UI.Loadout;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public readonly struct LoadoutReviewBindings
{
    public Text Status { get; }
    public Text Summary { get; }
    public Text Readiness { get; }
    public Button Confirm { get; }

    public LoadoutReviewBindings(Text status, Text summary, Text readiness, Button confirm)
    {
        Status = status;
        Summary = summary;
        Readiness = readiness;
        Confirm = confirm;
    }
}

public static class LoadoutReviewUiFactory
{
    const string BackdropMaterialPath =
        "Assets/_Project/Materials/UI/SeaLion_LoadoutBackdrop_R2_REVIEW.mat";
    const string RoundedSpritePath =
        "Assets/_Project/Art/UI/Generated/LoadoutRounded_R2.png";
    const string CircleSpritePath =
        "Assets/_Project/Art/UI/Generated/LoadoutCircle_R2.png";

    static readonly Color Ink = Hex("06141C");
    static readonly Color DeepNavy = Hex("071E28");
    static readonly Color RaisedNavy = Hex("0D303B");
    static readonly Color CardTeal = Hex("104852");
    static readonly Color ActiveTeal = Hex("167F78");
    static readonly Color LockedSlate = Hex("222E35");
    static readonly Color SlateEdge = Hex("41515A");
    static readonly Color Cyan = Hex("9AD8DC");
    static readonly Color Gold = Hex("F2C14E");
    static readonly Color Bronze = Hex("8F5B24");
    static readonly Color White = Hex("F4F5F1");
    static readonly Color Muted = Hex("91A8AD");

    static Font font;
    static Sprite rounded;
    static Sprite circle;

    public static LoadoutReviewBindings Build(Transform root, LoadoutScreenPresenter presenter)
    {
        CacheBuiltins();
        BuildBackdrop(root);
        var safe = Container(root, "SafeArea", Vector2.zero, new Vector2(672f, 1248f));
        var header = BuildHeader(safe, out var summary, out var readiness);
        BuildCompassSeal(header.transform);

        BuildSlot(safe, presenter, LoadoutSlot.Flagship, "I", "FLAGSHIP",
            "DEPLOYMENT DOCTRINE", -168f,
            new[] { "default-flagship", "flagship-lateen-raider" },
            new[] { "Lion Vanguard", "Lateen Raider" },
            new[] { "CADENCE FORMATION", "BURST FORMATION" },
            new[] { "1 craft every 0.9s", "3 craft • 1.5s recovery" },
            new[] { "LV", "LR" }, new[] { string.Empty, "LEVEL II BLUEPRINT" });

        BuildSlot(safe, presenter, LoadoutSlot.CrewRole, "II", "CREW ROLE",
            "BOARDING FORMATION", -400f,
            new[] { "default-crew", "loadout-crew-sailmakers" },
            new[] { "Sea Guard", "Sailmakers Corps" },
            new[] { "BALANCED CREW", "DEFENDER CREW" },
            new[] { "Damage 1.8 • durability 1.0", "Damage 1.6 • durability 1.5" },
            new[] { "SG", "SC" }, new[] { string.Empty, "LEVEL I BLUEPRINT" });

        BuildSlot(safe, presenter, LoadoutSlot.CaptainAbility, "III", "CAPTAIN ABILITY",
            "TACTICAL COMMAND", -632f,
            new[] { "default-ability", "ability-powder-barrage" },
            new[] { "Captain's Rally", "Powder Barrage" },
            new[] { "REINFORCE +8", "STRIKE 18" },
            new[] { "Time charge • 5s cooldown", "Damage charge • 9s cooldown" },
            new[] { "CR", "PB" }, new[] { string.Empty, "REQUIRES BLUEPRINT" });

        BuildReward(safe);
        var confirm = BuildConfirm(safe);
        var status = TextElement(safe, "Status", "REVIEW R2  •  PLAYER APPROVAL PENDING",
            new Vector2(0f, -1134f), new Vector2(640f, 32f), 12, TextAnchor.MiddleCenter, Muted);
        status.fontStyle = FontStyle.Bold;
        return new LoadoutReviewBindings(status, summary, readiness, confirm);
    }

    static void BuildBackdrop(Transform root)
    {
        var background = Panel(root, "Backdrop_R2", Vector2.zero, new Vector2(720f, 1280f), Color.white, false);
        background.material = EnsureBackdropMaterial();
        AddLine(root, "TopGoldRail", new Vector2(0f, -5f), new Vector2(720f, 3f), Gold.WithAlpha(0.75f));
        AddLine(root, "LeftChartLine", new Vector2(-298f, -72f), new Vector2(2f, 1170f), Cyan.WithAlpha(0.06f));
        AddLine(root, "RightChartLine", new Vector2(298f, -72f), new Vector2(2f, 1170f), Cyan.WithAlpha(0.06f));
        AddLine(root, "NorthEastBearing", new Vector2(260f, -38f), new Vector2(190f, 2f), Gold.WithAlpha(0.08f), -32f);
        AddLine(root, "SouthWestBearing", new Vector2(-272f, -1110f), new Vector2(170f, 2f), Cyan.WithAlpha(0.07f), -32f);
    }

    static Image BuildHeader(Transform parent, out Text summary, out Text readiness)
    {
        var shell = Panel(parent, "HeaderShell", new Vector2(0f, -12f), new Vector2(664f, 142f), Gold.WithAlpha(0.34f));
        AddSoftShadow(shell);
        var core = Panel(shell.transform, "HeaderCore", new Vector2(0f, -4f), new Vector2(656f, 134f), DeepNavy.WithAlpha(0.98f));
        AddLine(core.transform, "HeaderAccent", new Vector2(42f, -4f), new Vector2(360f, 2f), Gold.WithAlpha(0.72f));
        var eyebrow = TextElement(core.transform, "Eyebrow", "LION OF THE SEAS  •  FLEET DOCTRINE",
            new Vector2(-40f, -15f), new Vector2(390f, 20f), 11, TextAnchor.MiddleLeft, Cyan);
        eyebrow.fontStyle = FontStyle.Bold;
        var title = TextElement(core.transform, "Title", "COMMANDER'S LOADOUT",
            new Vector2(-14f, -35f), new Vector2(440f, 42f), 29, TextAnchor.MiddleLeft, White);
        title.fontStyle = FontStyle.Bold;
        AddTextShadow(title, new Vector2(0f, -2f), Ink.WithAlpha(0.8f));
        TextElement(core.transform, "Subtitle", "Choose one doctrine for each battle system",
            new Vector2(-14f, -77f), new Vector2(440f, 25f), 14, TextAnchor.MiddleLeft, Muted);

        var readyShell = Panel(core.transform, "ReadinessShell", new Vector2(235f, -21f),
            new Vector2(154f, 34f), Gold.WithAlpha(0.6f));
        var readyCore = Panel(readyShell.transform, "ReadinessCore", new Vector2(0f, -2f),
            new Vector2(150f, 30f), Ink.WithAlpha(0.96f));
        readiness = TextElement(readyCore.transform, "Readiness", "3 / 3  READY",
            new Vector2(0f, -2f), new Vector2(140f, 25f), 12, TextAnchor.MiddleCenter, Gold);
        readiness.fontStyle = FontStyle.Bold;

        summary = TextElement(core.transform, "SelectionSummary",
            "VANGUARD  •  SEA GUARD  •  CAPTAIN'S RALLY",
            new Vector2(84f, -106f), new Vector2(470f, 20f), 11, TextAnchor.MiddleRight, Cyan);
        summary.fontStyle = FontStyle.Bold;
        return core;
    }

    static void BuildCompassSeal(Transform parent)
    {
        var seal = Panel(parent, "CompassSeal", new Vector2(-282f, -29f), new Vector2(70f, 70f), Gold.WithAlpha(0.82f), true);
        var core = Panel(seal.transform, "CompassCore", new Vector2(0f, -3f), new Vector2(64f, 64f), Ink, true);
        AddLine(core.transform, "NorthSouth", new Vector2(0f, -13f), new Vector2(3f, 38f), Gold.WithAlpha(0.95f));
        AddLine(core.transform, "EastWest", new Vector2(0f, -13f), new Vector2(38f, 3f), Cyan.WithAlpha(0.82f));
        AddLine(core.transform, "BearingA", new Vector2(0f, -13f), new Vector2(30f, 2f), Gold.WithAlpha(0.55f), 45f);
        AddLine(core.transform, "BearingB", new Vector2(0f, -13f), new Vector2(30f, 2f), Gold.WithAlpha(0.55f), -45f);
        TextElement(core.transform, "North", "N", new Vector2(0f, -1f), new Vector2(20f, 18f), 10,
            TextAnchor.MiddleCenter, White).fontStyle = FontStyle.Bold;
    }

    static void BuildSlot(Transform parent, LoadoutScreenPresenter presenter, LoadoutSlot slot,
        string numeral, string heading, string kicker, float top, string[] ids, string[] names,
        string[] roles, string[] tradeOffs, string[] monograms, string[] lockedReasons)
    {
        var shell = Panel(parent, "Section__" + heading, new Vector2(0f, top),
            new Vector2(664f, 220f), Cyan.WithAlpha(0.22f));
        var core = Panel(shell.transform, "SectionCore", new Vector2(0f, -4f),
            new Vector2(656f, 212f), DeepNavy.WithAlpha(0.94f));
        var numeralSeal = Panel(core.transform, "Numeral", new Vector2(-299f, -10f),
            new Vector2(30f, 30f), Gold.WithAlpha(0.82f), true);
        var numeralText = TextElement(numeralSeal.transform, "Value", numeral, new Vector2(0f, -2f),
            new Vector2(24f, 22f), 11, TextAnchor.MiddleCenter, Ink);
        numeralText.fontStyle = FontStyle.Bold;
        var headingText = TextElement(core.transform, "Heading", heading, new Vector2(-175f, -12f),
            new Vector2(210f, 28f), 17, TextAnchor.MiddleLeft, White);
        headingText.fontStyle = FontStyle.Bold;
        var kickerText = TextElement(core.transform, "Kicker", kicker, new Vector2(200f, -13f),
            new Vector2(220f, 24f), 10, TextAnchor.MiddleRight, Muted);
        kickerText.fontStyle = FontStyle.Bold;
        AddLine(core.transform, "HeaderRule", new Vector2(0f, -43f), new Vector2(616f, 1f), Cyan.WithAlpha(0.16f));

        for (var index = 0; index < 2; index++)
            BuildOption(core.transform, presenter, slot, ids[index], names[index], roles[index],
                tradeOffs[index], monograms[index], lockedReasons[index], index == 0,
                index == 0 ? -158f : 158f);
    }

    static void BuildOption(Transform parent, LoadoutScreenPresenter presenter, LoadoutSlot slot,
        string id, string displayName, string role, string tradeOff, string monogram,
        string lockedReason, bool active, float x)
    {
        var shellColor = active ? Gold : SlateEdge;
        var shell = Panel(parent, "Option__" + id, new Vector2(x, -50f),
            new Vector2(306f, 154f), shellColor.WithAlpha(active ? 0.92f : 0.72f));
        AddSoftShadow(shell);
        var core = Panel(shell.transform, "CardCore", new Vector2(0f, -3f), new Vector2(300f, 148f),
            active ? CardTeal : LockedSlate);
        core.raycastTarget = true;
        var accent = AddLine(core.transform, "AccentRail", new Vector2(0f, -2f),
            new Vector2(122f, 3f), active ? Gold : SlateEdge);

        var sigil = Panel(core.transform, "Sigil", new Vector2(-116f, -17f), new Vector2(48f, 48f),
            active ? ActiveTeal : DeepNavy, true);
        var sigilText = TextElement(sigil.transform, "Monogram", monogram, new Vector2(0f, -3f),
            new Vector2(42f, 34f), 17, TextAnchor.MiddleCenter, active ? Gold : Muted);
        sigilText.fontStyle = FontStyle.Bold;

        var title = TextElement(core.transform, "Name", displayName, new Vector2(31f, -15f),
            new Vector2(205f, 26f), 16, TextAnchor.MiddleLeft, White);
        title.fontStyle = FontStyle.Bold;
        var roleLabel = TextElement(core.transform, "Role", role, new Vector2(31f, -43f),
            new Vector2(205f, 20f), 11, TextAnchor.MiddleLeft, active ? Gold : Cyan);
        roleLabel.fontStyle = FontStyle.Bold;
        AddLine(core.transform, "CardRule", new Vector2(0f, -75f), new Vector2(264f, 1f), White.WithAlpha(0.10f));
        var tradeLabel = TextElement(core.transform, "TradeOff", tradeOff, new Vector2(-6f, -82f),
            new Vector2(254f, 24f), 12, TextAnchor.MiddleLeft, active ? White : Muted);

        var lockedReasonLabel = TextElement(core.transform, "LockedReason", lockedReason,
            new Vector2(-46f, -113f), new Vector2(180f, 22f), 9, TextAnchor.MiddleLeft, Muted);
        lockedReasonLabel.fontStyle = FontStyle.Bold;
        lockedReasonLabel.gameObject.SetActive(!active);

        var badge = Panel(core.transform, "StateBadge", new Vector2(98f, -111f),
            new Vector2(82f, 25f), active ? Gold : Ink);
        var state = TextElement(badge.transform, "State", active ? "EQUIPPED" : "LOCKED",
            new Vector2(0f, -2f), new Vector2(74f, 20f), 9, TextAnchor.MiddleCenter,
            active ? Ink : Muted);
        state.fontStyle = FontStyle.Bold;

        var button = core.gameObject.AddComponent<Button>();
        button.targetGraphic = core;
        button.interactable = false;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Hex("E6FAF6");
        colors.pressedColor = Hex("BBDDD8");
        colors.disabledColor = Color.white;
        colors.colorMultiplier = 1f;
        colors.fadeDuration = 0.12f;
        button.colors = colors;

        var option = core.gameObject.AddComponent<LoadoutOptionButton>();
        option.Configure(slot, id, title, roleLabel, tradeLabel, state, core, shell, accent,
            badge, sigil, sigilText, lockedReasonLabel, active, !active);
        option.Bind(presenter);
    }

    static void BuildReward(Transform parent)
    {
        var shell = Panel(parent, "RewardShell", new Vector2(0f, -866f),
            new Vector2(664f, 92f), Gold.WithAlpha(0.58f));
        AddSoftShadow(shell);
        var core = Panel(shell.transform, "RewardCore", new Vector2(0f, -3f),
            new Vector2(658f, 86f), new Color(0.16f, 0.10f, 0.035f, 0.98f));
        var seal = Panel(core.transform, "BlueprintSeal", new Vector2(-285f, -15f),
            new Vector2(54f, 54f), Bronze.WithAlpha(0.95f), true);
        var mark = TextElement(seal.transform, "Mark", "BP", new Vector2(0f, -4f),
            new Vector2(44f, 34f), 15, TextAnchor.MiddleCenter, Gold);
        mark.fontStyle = FontStyle.Bold;
        var eyebrow = TextElement(core.transform, "RewardEyebrow", "LEVEL I REWARD",
            new Vector2(-118f, -11f), new Vector2(270f, 20f), 10, TextAnchor.MiddleLeft, Gold);
        eyebrow.fontStyle = FontStyle.Bold;
        var title = TextElement(core.transform, "RewardTitle", "SAILMAKERS BLUEPRINT",
            new Vector2(-90f, -31f), new Vector2(330f, 26f), 17, TextAnchor.MiddleLeft, White);
        title.fontStyle = FontStyle.Bold;
        TextElement(core.transform, "RewardBody", "Victory unlocks a durable crew doctrine",
            new Vector2(-72f, -59f), new Vector2(365f, 20f), 11, TextAnchor.MiddleLeft, Muted);
        var tag = Panel(core.transform, "RewardTag", new Vector2(242f, -29f),
            new Vector2(142f, 31f), Gold.WithAlpha(0.92f));
        var tagText = TextElement(tag.transform, "Value", "AFTER VICTORY", new Vector2(0f, -3f),
            new Vector2(130f, 24f), 10, TextAnchor.MiddleCenter, Ink);
        tagText.fontStyle = FontStyle.Bold;
    }

    static Button BuildConfirm(Transform parent)
    {
        var dock = Panel(parent, "CommandDock", new Vector2(0f, -972f),
            new Vector2(664f, 148f), Ink.WithAlpha(0.88f));
        var shell = Panel(dock.transform, "ConfirmShell", new Vector2(0f, -16f),
            new Vector2(632f, 70f), Gold.WithAlpha(0.95f));
        AddSoftShadow(shell);
        var core = Panel(shell.transform, "ConfirmCore", new Vector2(0f, -4f),
            new Vector2(624f, 62f), Gold);
        core.raycastTarget = true;
        var eyebrow = TextElement(core.transform, "Eyebrow", "LOCK IN LOADOUT",
            new Vector2(-38f, -7f), new Vector2(350f, 18f), 9, TextAnchor.MiddleCenter, Bronze);
        eyebrow.fontStyle = FontStyle.Bold;
        var label = TextElement(core.transform, "Label", "SET SAIL",
            new Vector2(-38f, -23f), new Vector2(350f, 29f), 20, TextAnchor.MiddleCenter, Ink);
        label.fontStyle = FontStyle.Bold;
        var arrow = Panel(core.transform, "ArrowIsland", new Vector2(265f, -9f),
            new Vector2(44f, 44f), Ink, true);
        var arrowText = TextElement(arrow.transform, "Arrow", ">", new Vector2(0f, -5f),
            new Vector2(34f, 30f), 23, TextAnchor.MiddleCenter, Gold);
        arrowText.fontStyle = FontStyle.Bold;
        var button = core.gameObject.AddComponent<Button>();
        button.targetGraphic = core;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors;
        colors.normalColor = Color.white;
        colors.highlightedColor = Hex("FFF1B0");
        colors.pressedColor = Hex("D9A93D");
        colors.disabledColor = Hex("607078");
        colors.fadeDuration = 0.12f;
        button.colors = colors;
        TextElement(dock.transform, "Autosave", "LOADOUT SAVES AUTOMATICALLY",
            new Vector2(0f, -100f), new Vector2(520f, 22f), 10, TextAnchor.MiddleCenter, Muted);
        return button;
    }

    static Material EnsureBackdropMaterial()
    {
        var material = AssetDatabase.LoadAssetAtPath<Material>(BackdropMaterialPath);
        if (material != null) return material;
        var shader = Shader.Find("SeaLion/UI/LoadoutBackdrop");
        if (shader == null) throw new MissingReferenceException("Loadout backdrop shader is missing.");
        material = new Material(shader) { name = "SeaLion_LoadoutBackdrop_R2_REVIEW" };
        AssetDatabase.CreateAsset(material, BackdropMaterialPath);
        return material;
    }

    static Image Panel(Transform parent, string name, Vector2 position, Vector2 size,
        Color color, bool useCircle = false)
    {
        var value = new GameObject(name, typeof(RectTransform), typeof(Image));
        value.transform.SetParent(parent, false);
        SetRect(value.GetComponent<RectTransform>(), position, size);
        var image = value.GetComponent<Image>();
        image.color = color;
        image.sprite = useCircle ? circle : rounded;
        if (image.sprite != null) image.type = useCircle ? Image.Type.Simple : Image.Type.Sliced;
        image.raycastTarget = false;
        return image;
    }

    static RectTransform Container(Transform parent, string name, Vector2 position, Vector2 size)
    {
        var value = new GameObject(name, typeof(RectTransform));
        value.transform.SetParent(parent, false);
        var rect = value.GetComponent<RectTransform>();
        SetRect(rect, position, size);
        return rect;
    }

    static Text TextElement(Transform parent, string name, string contents, Vector2 position,
        Vector2 size, int fontSize, TextAnchor alignment, Color color)
    {
        var value = new GameObject(name, typeof(RectTransform), typeof(Text));
        value.transform.SetParent(parent, false);
        SetRect(value.GetComponent<RectTransform>(), position, size);
        var text = value.GetComponent<Text>();
        text.text = contents;
        text.font = font;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.horizontalOverflow = HorizontalWrapMode.Wrap;
        text.verticalOverflow = VerticalWrapMode.Truncate;
        text.raycastTarget = false;
        return text;
    }

    static Image AddLine(Transform parent, string name, Vector2 position, Vector2 size,
        Color color, float rotation = 0f)
    {
        var line = Panel(parent, name, position, size, color, false);
        line.sprite = null;
        line.rectTransform.localRotation = Quaternion.Euler(0f, 0f, rotation);
        return line;
    }

    static void AddSoftShadow(Graphic graphic)
    {
        var shadow = graphic.gameObject.AddComponent<Shadow>();
        shadow.effectColor = new Color(0f, 0f, 0f, 0.24f);
        shadow.effectDistance = new Vector2(0f, -5f);
        shadow.useGraphicAlpha = true;
    }

    static void AddTextShadow(Graphic graphic, Vector2 distance, Color color)
    {
        var shadow = graphic.gameObject.AddComponent<Shadow>();
        shadow.effectColor = color;
        shadow.effectDistance = distance;
        shadow.useGraphicAlpha = true;
    }

    static void SetRect(RectTransform value, Vector2 position, Vector2 size)
    {
        value.anchorMin = new Vector2(0.5f, 1f);
        value.anchorMax = new Vector2(0.5f, 1f);
        value.pivot = new Vector2(0.5f, 1f);
        value.anchoredPosition = position;
        value.sizeDelta = size;
    }

    static void CacheBuiltins()
    {
        if (font == null) font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (rounded == null) rounded = EnsureShapeSprite(RoundedSpritePath, false);
        if (circle == null) circle = EnsureShapeSprite(CircleSpritePath, true);
    }

    static Sprite EnsureShapeSprite(string path, bool isCircle)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Sprite>(path);
        if (existing != null) return existing;
        const int size = 64;
        var texture = new Texture2D(size, size, TextureFormat.RGBA32, false, true)
        {
            name = Path.GetFileNameWithoutExtension(path)
        };
        var pixels = new Color32[size * size];
        for (var y = 0; y < size; y++)
        for (var x = 0; x < size; x++)
        {
            var px = x + 0.5f - size * 0.5f;
            var py = y + 0.5f - size * 0.5f;
            float distance;
            if (isCircle) distance = Mathf.Sqrt(px * px + py * py) - 30.5f;
            else
            {
                const float radius = 11.5f;
                var qx = Mathf.Abs(px) - (31.5f - radius);
                var qy = Mathf.Abs(py) - (31.5f - radius);
                distance = Mathf.Sqrt(Mathf.Max(qx, 0f) * Mathf.Max(qx, 0f) +
                    Mathf.Max(qy, 0f) * Mathf.Max(qy, 0f)) + Mathf.Min(Mathf.Max(qx, qy), 0f) - radius;
            }
            var alpha = (byte)Mathf.RoundToInt(Mathf.Clamp01(0.5f - distance) * 255f);
            pixels[y * size + x] = new Color32(255, 255, 255, alpha);
        }
        texture.SetPixels32(pixels);
        texture.Apply();
        File.WriteAllBytes(path, texture.EncodeToPNG());
        Object.DestroyImmediate(texture);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var importer = (TextureImporter)AssetImporter.GetAtPath(path);
        importer.textureType = TextureImporterType.Sprite;
        importer.spriteImportMode = SpriteImportMode.Single;
        importer.spritePixelsPerUnit = size;
        importer.spriteBorder = isCircle ? Vector4.zero : new Vector4(12f, 12f, 12f, 12f);
        importer.mipmapEnabled = false;
        importer.alphaIsTransparency = true;
        importer.filterMode = FilterMode.Bilinear;
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(path);
    }

    static Color Hex(string value)
    {
        Color parsed;
        return ColorUtility.TryParseHtmlString("#" + value, out parsed) ? parsed : Color.magenta;
    }

    static Color WithAlpha(this Color value, float alpha)
    {
        value.a = alpha;
        return value;
    }
}
