using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace SeaLion.UI.Localization
{
    public sealed class LocalizedTextLabel : MonoBehaviour
    {
        [SerializeField] private string key;
        [SerializeField] private Text label;
        [SerializeField] private TextAnchor englishAlignment;
        [SerializeField] private Font englishFont;
        [SerializeField] private int englishFontSize;
        [SerializeField] private VerticalWrapMode englishVerticalOverflow;
        [SerializeField] private int arabicFontSizeDelta;
        [SerializeField] private RectTransform labelRect;
        [SerializeField] private Vector2 englishAnchoredPosition;
        [SerializeField] private Vector2 englishSizeDelta;
        [SerializeField] private bool layoutCaptured;

        public void Configure(string localizationKey, Text target, int arabicSizeDelta = 0)
        {
            key = localizationKey;
            label = target;
            arabicFontSizeDelta = arabicSizeDelta;
            if (label == null) return;
            englishAlignment = label.alignment;
            englishFont = label.font;
            englishFontSize = label.fontSize;
            englishVerticalOverflow = label.verticalOverflow;
            CaptureLayout();
        }

        public void Apply(GameLanguage language)
        {
            if (label == null) label = GetComponent<Text>();
            if (label == null) return;
            CaptureLayout();
            var value = LoadoutLocalization.Get(key, language);
            label.text = LoadoutLocalization.FormatForDisplay(value, language);
            if (language == GameLanguage.Arabic)
            {
                label.font = RuntimeArabicFont.Resolve(englishFont);
                label.fontSize = Mathf.Max(8, englishFontSize + arabicFontSizeDelta);
                label.alignment = Mirror(englishAlignment);
                label.verticalOverflow = VerticalWrapMode.Overflow;
                SetHorizontalPosition(-englishAnchoredPosition.x);
                ApplyArabicLayoutOverride();
            }
            else
            {
                label.font = englishFont;
                label.fontSize = englishFontSize;
                label.alignment = englishAlignment;
                label.verticalOverflow = englishVerticalOverflow;
                SetHorizontalPosition(englishAnchoredPosition.x);
                labelRect.sizeDelta = englishSizeDelta;
            }
        }

        private void CaptureLayout()
        {
            if (layoutCaptured) return;
            labelRect = label == null ? null : label.rectTransform;
            if (labelRect == null) return;
            englishAnchoredPosition = labelRect.anchoredPosition;
            englishSizeDelta = labelRect.sizeDelta;
            layoutCaptured = true;
        }

        private void ApplyArabicLayoutOverride()
        {
            switch (key)
            {
                case "header.brand":
                case "header.title":
                case "header.subtitle":
                    SetRect(-45f, 360f);
                    break;
                case "slot.flagship.kicker":
                case "slot.crew.kicker":
                case "slot.ability.kicker":
                    SetRect(-180f, 180f);
                    break;
                case "reward.eyebrow":
                case "reward.title":
                case "reward.body":
                    SetRect(40f, 360f);
                    break;
            }
        }

        private void SetRect(float x, float width)
        {
            SetHorizontalPosition(x);
            if (labelRect == null) return;
            labelRect.sizeDelta = new Vector2(width, englishSizeDelta.y);
        }

        private void SetHorizontalPosition(float x)
        {
            if (labelRect == null) return;
            var position = labelRect.anchoredPosition;
            position.x = x;
            labelRect.anchoredPosition = position;
        }

        private static TextAnchor Mirror(TextAnchor value)
        {
            switch (value)
            {
                case TextAnchor.UpperLeft: return TextAnchor.UpperRight;
                case TextAnchor.UpperRight: return TextAnchor.UpperLeft;
                case TextAnchor.MiddleLeft: return TextAnchor.MiddleRight;
                case TextAnchor.MiddleRight: return TextAnchor.MiddleLeft;
                case TextAnchor.LowerLeft: return TextAnchor.LowerRight;
                case TextAnchor.LowerRight: return TextAnchor.LowerLeft;
                default: return value;
            }
        }
    }

    public static class RuntimeArabicFont
    {
        private const string BundledFontResource = "Fonts/NotoSansArabic/NotoSansArabic-Variable";

        private static readonly string[] PreferredNames =
        {
            ".SF Arabic", "SF Arabic", "Geeza Pro", "Noto Kufi Arabic",
            "Noto Sans Arabic", "Noto Naskh Arabic UI", "Noto Naskh Arabic",
            "Arial Unicode MS", "Arial"
        };

        private static Font cached;

        public static Font Resolve(Font fallback)
        {
            if (cached != null) return cached;
            cached = Resources.Load<Font>(BundledFontResource);
            if (cached != null) return cached;
            try
            {
                var installed = new HashSet<string>(Font.GetOSInstalledFontNames(),
                    StringComparer.OrdinalIgnoreCase);
                for (var index = 0; index < PreferredNames.Length; index++)
                {
                    if (!installed.Contains(PreferredNames[index])) continue;
                    cached = Font.CreateDynamicFontFromOSFont(PreferredNames[index], 32);
                    if (cached != null) return cached;
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Arabic system font lookup failed; using the UI fallback. " + exception.Message);
            }
            return fallback;
        }
    }
}
