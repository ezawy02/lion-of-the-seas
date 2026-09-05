using SeaLion.Core.Definitions;
using SeaLion.UI.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace SeaLion.UI.Loadout
{
    [RequireComponent(typeof(Button))]
    public sealed class LoadoutOptionButton : MonoBehaviour
    {
        [SerializeField] private LoadoutSlot slot;
        [SerializeField] private string optionId;
        [SerializeField] private Text nameLabel;
        [SerializeField] private Text roleLabel;
        [SerializeField] private Text tradeOffLabel;
        [SerializeField] private Text stateLabel;
        [SerializeField] private Image background;
        [SerializeField] private Image shell;
        [SerializeField] private Image accent;
        [SerializeField] private Image stateBadge;
        [SerializeField] private Image sigil;
        [SerializeField] private Text sigilLabel;
        [SerializeField] private Text lockedReasonLabel;
        [SerializeField] private bool initialActive;
        [SerializeField] private bool initialLocked;
        private LoadoutScreenPresenter presenter;
        private Button button;
        private Font englishFont;
        private bool stateKnown;
        private bool isActive;
        private bool isLocked;
        private bool layoutCaptured;
        private Vector2 namePosition;
        private Vector2 rolePosition;
        private Vector2 tradeOffPosition;
        private Vector2 lockedReasonPosition;
        private Vector2 stateBadgePosition;
        private Vector2 sigilPosition;
        private int nameFontSize;
        private int roleFontSize;
        private int tradeOffFontSize;
        private int stateFontSize;
        private int lockedReasonFontSize;

        public void Configure(LoadoutSlot valueSlot, string valueId, Text title, Text role,
            Text tradeOff, Text state, Image panel, Image outerShell, Image accentRail,
            Image badge, Image optionSigil, Text optionSigilLabel, Text lockedReason,
            bool activeAtStart, bool lockedAtStart)
        {
            slot = valueSlot;
            optionId = valueId;
            nameLabel = title;
            roleLabel = role;
            tradeOffLabel = tradeOff;
            stateLabel = state;
            background = panel;
            shell = outerShell;
            accent = accentRail;
            stateBadge = badge;
            sigil = optionSigil;
            sigilLabel = optionSigilLabel;
            lockedReasonLabel = lockedReason;
            initialActive = activeAtStart;
            initialLocked = lockedAtStart;
            CaptureEnglishFont();
            CaptureEnglishLayout();
            InferInitialState();
        }

        public void Bind(LoadoutScreenPresenter owner) { presenter = owner; }

        public void Refresh(LoadoutScreenView view)
        {
            var options = view.GetOptions(slot);
            for (var index = 0; index < options.Count; index++)
            {
                var card = options[index];
                if (card.Option.Id.Value != optionId) continue;
                isActive = card.Option.IsActive;
                isLocked = card.Option.IsLocked;
                stateKnown = true;
                ApplyLanguage(presenter == null ? GameLanguage.English : presenter.CurrentLanguage);
                ApplyVisualState(card.Option.IsActive, card.Option.IsLocked);
                EnsureButton();
                button.interactable = card.CanSelect && !card.Option.IsActive;
                return;
            }
        }

        public void ApplyLanguage(GameLanguage language)
        {
            if (!stateKnown) InferInitialState();
            CaptureEnglishFont();
            CaptureEnglishLayout();
            ApplyLayout(language);
            ApplyText(nameLabel, LoadoutLocalization.GetOption(optionId, "name", language), language, false);
            ApplyText(roleLabel, LoadoutLocalization.GetOption(optionId, "role", language), language, false);
            ApplyText(tradeOffLabel, LoadoutLocalization.GetOption(optionId, "tradeoff", language), language, false);
            ApplyText(stateLabel, LoadoutLocalization.Get(isActive ? "state.equipped" :
                isLocked ? "state.locked" : "state.select", language), language, true);
            if (lockedReasonLabel != null)
            {
                var key = isLocked ? "option." + optionId + ".lock" : "state.ready";
                ApplyText(lockedReasonLabel, LoadoutLocalization.Get(key, language), language, false);
            }
        }

        private void OnEnable()
        {
            EnsureButton();
            button.onClick.AddListener(HandleClick);
        }

        private void OnDisable()
        {
            if (button != null) button.onClick.RemoveListener(HandleClick);
        }

        private void HandleClick()
        {
            presenter?.TrySelect(slot, new StableId(optionId));
        }

        private void EnsureButton()
        {
            if (button == null) button = GetComponent<Button>();
        }

        private void ApplyVisualState(bool isActive, bool isLocked)
        {
            var gold = new Color(0.95f, 0.76f, 0.31f, 1f);
            var cyan = new Color(0.60f, 0.85f, 0.86f, 1f);
            var ink = new Color(0.025f, 0.08f, 0.11f, 1f);
            var muted = new Color(0.57f, 0.66f, 0.68f, 1f);
            if (background != null)
                background.color = isActive ? new Color(0.06f, 0.28f, 0.32f, 1f) :
                    isLocked ? new Color(0.13f, 0.18f, 0.21f, 1f) :
                    new Color(0.05f, 0.23f, 0.27f, 1f);
            if (shell != null)
                shell.color = isActive ? gold : isLocked ?
                    new Color(0.25f, 0.32f, 0.35f, 0.78f) : cyan;
            if (accent != null) accent.color = isActive ? gold : isLocked ? muted : cyan;
            if (stateBadge != null) stateBadge.color = isActive ? gold : ink;
            if (stateLabel != null) stateLabel.color = isActive ? ink : isLocked ? muted : cyan;
            if (sigil != null)
                sigil.color = isActive ? new Color(0.09f, 0.50f, 0.47f, 1f) : ink;
            if (sigilLabel != null) sigilLabel.color = isActive ? gold : isLocked ? muted : cyan;
            if (roleLabel != null) roleLabel.color = isActive ? gold : cyan;
            if (tradeOffLabel != null) tradeOffLabel.color = isLocked ? muted : Color.white;
            if (lockedReasonLabel != null)
            {
                lockedReasonLabel.gameObject.SetActive(!isActive);
                lockedReasonLabel.color = isLocked ? muted : cyan;
            }
        }

        private void InferInitialState()
        {
            isActive = initialActive;
            isLocked = initialLocked;
            stateKnown = true;
        }

        private void CaptureEnglishFont()
        {
            if (englishFont == null && nameLabel != null) englishFont = nameLabel.font;
        }

        private void CaptureEnglishLayout()
        {
            if (layoutCaptured || nameLabel == null) return;
            namePosition = nameLabel.rectTransform.anchoredPosition;
            rolePosition = roleLabel.rectTransform.anchoredPosition;
            tradeOffPosition = tradeOffLabel.rectTransform.anchoredPosition;
            lockedReasonPosition = lockedReasonLabel == null ? Vector2.zero :
                lockedReasonLabel.rectTransform.anchoredPosition;
            stateBadgePosition = stateBadge == null ? Vector2.zero :
                stateBadge.rectTransform.anchoredPosition;
            sigilPosition = sigil == null ? Vector2.zero : sigil.rectTransform.anchoredPosition;
            nameFontSize = nameLabel.fontSize;
            roleFontSize = roleLabel.fontSize;
            tradeOffFontSize = tradeOffLabel.fontSize;
            stateFontSize = stateLabel == null ? 0 : stateLabel.fontSize;
            lockedReasonFontSize = lockedReasonLabel == null ? 0 : lockedReasonLabel.fontSize;
            layoutCaptured = true;
        }

        private void ApplyLayout(GameLanguage language)
        {
            var rtl = language == GameLanguage.Arabic;
            SetMirroredPosition(nameLabel == null ? null : nameLabel.rectTransform, namePosition, rtl);
            SetMirroredPosition(roleLabel == null ? null : roleLabel.rectTransform, rolePosition, rtl);
            SetMirroredPosition(tradeOffLabel == null ? null : tradeOffLabel.rectTransform,
                tradeOffPosition, rtl);
            SetMirroredPosition(lockedReasonLabel == null ? null : lockedReasonLabel.rectTransform,
                lockedReasonPosition, rtl);
            SetMirroredPosition(stateBadge == null ? null : stateBadge.rectTransform,
                stateBadgePosition, rtl);
            SetMirroredPosition(sigil == null ? null : sigil.rectTransform, sigilPosition, rtl);
            nameLabel.fontSize = rtl ? Mathf.Max(10, nameFontSize - 2) : nameFontSize;
            roleLabel.fontSize = rtl ? Mathf.Max(8, roleFontSize - 1) : roleFontSize;
            tradeOffLabel.fontSize = rtl ? Mathf.Max(8, tradeOffFontSize - 2) : tradeOffFontSize;
            if (stateLabel != null)
                stateLabel.fontSize = rtl ? Mathf.Max(8, stateFontSize - 1) : stateFontSize;
            if (lockedReasonLabel != null)
                lockedReasonLabel.fontSize = rtl ? Mathf.Max(8, lockedReasonFontSize - 1) :
                    lockedReasonFontSize;
        }

        private static void SetMirroredPosition(RectTransform value, Vector2 english, bool rtl)
        {
            if (value == null) return;
            value.anchoredPosition = new Vector2(rtl ? -english.x : english.x, english.y);
        }

        private void ApplyText(Text label, string value, GameLanguage language, bool centered)
        {
            if (label == null) return;
            label.text = LoadoutLocalization.FormatForDisplay(value, language);
            label.font = language == GameLanguage.Arabic ? RuntimeArabicFont.Resolve(englishFont) : englishFont;
            label.verticalOverflow = language == GameLanguage.Arabic ?
                VerticalWrapMode.Overflow : VerticalWrapMode.Truncate;
            if (!centered) label.alignment = language == GameLanguage.Arabic ?
                TextAnchor.MiddleRight : TextAnchor.MiddleLeft;
        }
    }
}
