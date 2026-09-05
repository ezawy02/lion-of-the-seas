using System;
using SeaLion.Core.Definitions;
using SeaLion.UI.Localization;
using UnityEngine;
using UnityEngine.UI;

namespace SeaLion.UI.Loadout
{
    /// <summary>Small uGUI binder for the three-slot loadout screen.</summary>
    public sealed class LoadoutScreenPresenter : MonoBehaviour
    {
        [SerializeField] private LoadoutScreenController controller;
        [SerializeField] private Text statusLabel;
        [SerializeField] private Text summaryLabel;
        [SerializeField] private Text readinessLabel;
        [SerializeField] private Button confirmButton;
        private LoadoutOptionButton[] optionButtons;
        private GameLanguage currentLanguage = GameLanguage.English;
        private string statusKey = "status.review";
        private Font statusEnglishFont;
        private Font summaryEnglishFont;
        private Font readinessEnglishFont;
        public event Action Confirmed;
        public event Action<GameLanguage> LanguageChanged;
        public GameLanguage CurrentLanguage => currentLanguage;

        public void Configure(LoadoutScreenController screenController, Text status,
            Text summary, Text readiness, Button confirm)
        {
            UnbindConfirm();
            controller = screenController;
            statusLabel = status;
            summaryLabel = summary;
            readinessLabel = readiness;
            confirmButton = confirm;
            statusEnglishFont = statusLabel == null ? null : statusLabel.font;
            summaryEnglishFont = summaryLabel == null ? null : summaryLabel.font;
            readinessEnglishFont = readinessLabel == null ? null : readinessLabel.font;
            CacheButtons();
            BindConfirm();
        }

        private void Start()
        {
            if (controller == null) return;
            if (controller.View == null) controller.InitializeForRuntime();
            currentLanguage = GameLanguagePreference.Parse(controller.LanguagePreference);
            Refresh();
        }

        private void OnEnable() { BindConfirm(); }

        private void OnDisable() { UnbindConfirm(); }

        public bool TrySelect(LoadoutSlot slot, StableId optionId)
        {
            if (controller == null) return false;
            var selected = controller.TrySelect(slot, optionId);
            statusKey = selected ? "status.saved" : "status.failure";
            Refresh();
            return selected;
        }

        public void SetLanguage(GameLanguage language, bool persist = true)
        {
            currentLanguage = language;
            if (persist && controller != null && !controller.TrySetLanguagePreference(
                GameLanguagePreference.ToStoredValue(language))) statusKey = "status.failure";
            Refresh();
            LanguageChanged?.Invoke(currentLanguage);
        }

        public void Refresh()
        {
            if (optionButtons == null) CacheButtons();
            if (controller != null && controller.View != null)
            {
                for (var index = 0; index < optionButtons.Length; index++)
                    optionButtons[index].Refresh(controller.View);
                RefreshHeader();
            }
            else
            {
                for (var index = 0; index < optionButtons.Length; index++)
                    optionButtons[index].ApplyLanguage(currentLanguage);
                if (readinessLabel != null)
                    SetDisplay(readinessLabel, LoadoutLocalization.FormatReadiness(0, currentLanguage),
                        readinessEnglishFont);
                if (summaryLabel != null)
                    SetDisplay(summaryLabel, LoadoutLocalization.Get("header.summary.incomplete", currentLanguage),
                        summaryEnglishFont);
                if (confirmButton != null) confirmButton.interactable = false;
            }
            var labels = GetComponentsInChildren<LocalizedTextLabel>(true);
            for (var index = 0; index < labels.Length; index++) labels[index].Apply(currentLanguage);
            var switchers = GetComponentsInChildren<LoadoutLanguageSwitcher>(true);
            for (var index = 0; index < switchers.Length; index++)
                switchers[index].ApplyLanguage(currentLanguage);
            RefreshStatus();
        }

        private void CacheButtons()
        {
            optionButtons = GetComponentsInChildren<LoadoutOptionButton>(true);
            for (var index = 0; index < optionButtons.Length; index++)
                optionButtons[index].Bind(this);
        }

        private void RefreshHeader()
        {
            var ready = 0;
            OptionCard active;
            if (controller.View.TryGetActive(LoadoutSlot.Flagship, out active)) ready++;
            if (controller.View.TryGetActive(LoadoutSlot.CrewRole, out active)) ready++;
            if (controller.View.TryGetActive(LoadoutSlot.CaptainAbility, out active)) ready++;
            if (readinessLabel != null)
                SetDisplay(readinessLabel, LoadoutLocalization.FormatReadiness(ready, currentLanguage),
                    readinessEnglishFont);
            if (summaryLabel != null)
                SetDisplay(summaryLabel, LoadoutLocalization.Get(ready == 3 ?
                    "header.summary.ready" : "header.summary.incomplete", currentLanguage),
                    summaryEnglishFont);
            if (confirmButton != null) confirmButton.interactable = ready == 3;
        }

        private void HandleConfirm()
        {
            statusKey = "status.confirmed";
            RefreshStatus();
            Confirmed?.Invoke();
        }

        private void RefreshStatus()
        {
            if (statusLabel != null)
            {
                var value = statusKey == "status.failure" && controller != null &&
                    !string.IsNullOrEmpty(controller.LastFailure) ? controller.LastFailure : statusKey;
                SetDisplay(statusLabel, LoadoutLocalization.Get(value, currentLanguage), statusEnglishFont);
            }
        }

        private void SetDisplay(Text label, string value, Font englishFont)
        {
            label.text = LoadoutLocalization.FormatForDisplay(value, currentLanguage);
            var fallback = englishFont != null ? englishFont :
                Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.font = currentLanguage == GameLanguage.Arabic ?
                RuntimeArabicFont.Resolve(fallback) : fallback;
            label.verticalOverflow = currentLanguage == GameLanguage.Arabic ?
                VerticalWrapMode.Overflow : VerticalWrapMode.Truncate;
        }

        private void BindConfirm()
        {
            if (confirmButton == null) return;
            confirmButton.onClick.RemoveListener(HandleConfirm);
            confirmButton.onClick.AddListener(HandleConfirm);
        }

        private void UnbindConfirm()
        {
            if (confirmButton != null) confirmButton.onClick.RemoveListener(HandleConfirm);
        }
    }

}
