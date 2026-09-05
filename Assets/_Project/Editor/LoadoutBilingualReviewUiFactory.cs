using System.Collections.Generic;
using SeaLion.UI.Loadout;
using SeaLion.UI.Localization;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static class LoadoutBilingualReviewUiFactory
{
    private const string RoundedSpritePath =
        "Assets/_Project/Art/UI/Generated/LoadoutRounded_R2.png";

    private static readonly Dictionary<string, string> StaticKeys =
        new Dictionary<string, string>
        {
            { "LION OF THE SEAS  •  FLEET DOCTRINE", "header.brand" },
            { "COMMANDER'S LOADOUT", "header.title" },
            { "Choose one doctrine for each battle system", "header.subtitle" },
            { "N", "compass.north" },
            { "FLAGSHIP", "slot.flagship.heading" },
            { "DEPLOYMENT DOCTRINE", "slot.flagship.kicker" },
            { "CREW ROLE", "slot.crew.heading" },
            { "BOARDING FORMATION", "slot.crew.kicker" },
            { "CAPTAIN ABILITY", "slot.ability.heading" },
            { "TACTICAL COMMAND", "slot.ability.kicker" },
            { "I", "slot.one" },
            { "II", "slot.two" },
            { "III", "slot.three" },
            { "LEVEL I REWARD", "reward.eyebrow" },
            { "SAILMAKERS BLUEPRINT", "reward.title" },
            { "Victory unlocks a durable crew doctrine", "reward.body" },
            { "AFTER VICTORY", "reward.tag" },
            { "BP", "reward.mark" },
            { "LOCK IN LOADOUT", "confirm.eyebrow" },
            { "SET SAIL", "confirm.label" },
            { "LOADOUT SAVES AUTOMATICALLY", "confirm.autosave" },
            { ">", "confirm.arrow" }
        };

    public static LoadoutLanguageSwitcher Enhance(Transform root, LoadoutScreenPresenter presenter)
    {
        BindStaticLabels(root);
        var header = Find(root, "SafeArea/HeaderShell/HeaderCore");
        if (header == null) throw new MissingReferenceException("Loadout R2 header hierarchy changed.");

        var subtitle = header.Find("Subtitle") as RectTransform;
        if (subtitle != null)
        {
            subtitle.anchoredPosition = new Vector2(-55f, -77f);
            subtitle.sizeDelta = new Vector2(355f, 25f);
        }

        var shell = Panel(header, "LanguageSwitch_R3", new Vector2(242f, -67f),
            new Vector2(142f, 30f), Hex("8F5B24"));
        var core = Panel(shell.transform, "LanguageSwitchCore", new Vector2(0f, -2f),
            new Vector2(138f, 26f), Hex("06141C"));
        var english = LanguageButton(core.transform, "English", "EN", -34f, false, out var englishLabel);
        var arabic = LanguageButton(core.transform, "Arabic", "عربي", 34f, true, out var arabicLabel);

        var switcher = root.gameObject.AddComponent<LoadoutLanguageSwitcher>();
        switcher.Configure(presenter, english, arabic, englishLabel, arabicLabel);
        return switcher;
    }

    private static void BindStaticLabels(Transform root)
    {
        var labels = root.GetComponentsInChildren<Text>(true);
        for (var index = 0; index < labels.Length; index++)
        {
            var label = labels[index];
            if (!StaticKeys.TryGetValue(label.text, out var key)) continue;
            var delta = ArabicFontDelta(key);
            label.gameObject.AddComponent<LocalizedTextLabel>().Configure(key, label, delta);
        }
    }

    private static int ArabicFontDelta(string key)
    {
        switch (key)
        {
            case "header.title": return -9;
            case "slot.flagship.heading":
            case "slot.crew.heading":
            case "slot.ability.heading":
            case "reward.title": return -3;
            case "header.brand": return -3;
            case "header.subtitle": return -4;
            case "slot.flagship.kicker":
            case "slot.crew.kicker":
            case "slot.ability.kicker":
            case "reward.body": return -2;
            default: return -1;
        }
    }

    private static Button LanguageButton(Transform parent, string name, string value, float x,
        bool arabic, out Text label)
    {
        var panel = Panel(parent, name, new Vector2(x, -1f), new Vector2(66f, 22f), Hex("06141C"));
        panel.raycastTarget = true;
        var button = panel.gameObject.AddComponent<Button>();
        button.targetGraphic = panel;
        button.navigation = new Navigation { mode = Navigation.Mode.None };
        var colors = button.colors;
        colors.highlightedColor = Hex("F5D882");
        colors.pressedColor = Hex("D7A83D");
        colors.disabledColor = Hex("41515A");
        colors.fadeDuration = 0.08f;
        button.colors = colors;

        var textObject = new GameObject("Label", typeof(RectTransform), typeof(Text));
        textObject.transform.SetParent(panel.transform, false);
        SetRect(textObject.GetComponent<RectTransform>(), Vector2.zero, new Vector2(62f, 20f));
        label = textObject.GetComponent<Text>();
        label.font = arabic ? RuntimeArabicFont.Resolve(
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf")) :
            Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        label.text = arabic ? ArabicTextShaper.Shape(value) : value;
        label.fontSize = 10;
        label.fontStyle = FontStyle.Bold;
        label.alignment = TextAnchor.MiddleCenter;
        label.color = Hex("9AD8DC");
        label.raycastTarget = false;
        return button;
    }

    private static Image Panel(Transform parent, string name, Vector2 position,
        Vector2 size, Color color)
    {
        var value = new GameObject(name, typeof(RectTransform), typeof(Image));
        value.transform.SetParent(parent, false);
        SetRect(value.GetComponent<RectTransform>(), position, size);
        var image = value.GetComponent<Image>();
        image.color = color;
        image.sprite = AssetDatabase.LoadAssetAtPath<Sprite>(RoundedSpritePath);
        if (image.sprite != null) image.type = Image.Type.Sliced;
        image.raycastTarget = false;
        return image;
    }

    private static Transform Find(Transform root, string path)
    {
        return root.Find(path);
    }

    private static void SetRect(RectTransform value, Vector2 position, Vector2 size)
    {
        value.anchorMin = new Vector2(0.5f, 1f);
        value.anchorMax = new Vector2(0.5f, 1f);
        value.pivot = new Vector2(0.5f, 1f);
        value.anchoredPosition = position;
        value.sizeDelta = size;
    }

    private static Color Hex(string value)
    {
        return ColorUtility.TryParseHtmlString("#" + value, out var parsed) ? parsed : Color.magenta;
    }
}
