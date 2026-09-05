using UnityEngine;
using UnityEngine.UI;

namespace SeaLion.UI.Localization
{
    public sealed class LoadoutLanguageSwitcher : MonoBehaviour
    {
        [SerializeField] private Loadout.LoadoutScreenPresenter presenter;
        [SerializeField] private Button englishButton;
        [SerializeField] private Button arabicButton;
        [SerializeField] private Text englishLabel;
        [SerializeField] private Text arabicLabel;

        public void Configure(Loadout.LoadoutScreenPresenter owner, Button english, Button arabic,
            Text englishText, Text arabicText)
        {
            Unbind();
            presenter = owner;
            englishButton = english;
            arabicButton = arabic;
            englishLabel = englishText;
            arabicLabel = arabicText;
            Bind();
            ApplyLanguage(presenter == null ? GameLanguage.English : presenter.CurrentLanguage);
        }

        private void OnEnable() { Bind(); }

        private void OnDisable() { Unbind(); }

        private void Bind()
        {
            if (englishButton != null)
            {
                englishButton.onClick.RemoveListener(SelectEnglish);
                englishButton.onClick.AddListener(SelectEnglish);
            }
            if (arabicButton != null)
            {
                arabicButton.onClick.RemoveListener(SelectArabic);
                arabicButton.onClick.AddListener(SelectArabic);
            }
            if (presenter != null)
            {
                presenter.LanguageChanged -= ApplyLanguage;
                presenter.LanguageChanged += ApplyLanguage;
            }
        }

        private void Unbind()
        {
            if (englishButton != null) englishButton.onClick.RemoveListener(SelectEnglish);
            if (arabicButton != null) arabicButton.onClick.RemoveListener(SelectArabic);
            if (presenter != null) presenter.LanguageChanged -= ApplyLanguage;
        }

        private void SelectEnglish() { presenter?.SetLanguage(GameLanguage.English); }

        private void SelectArabic() { presenter?.SetLanguage(GameLanguage.Arabic); }

        public void ApplyLanguage(GameLanguage language)
        {
            ApplyPanelLayout(language);
            var gold = new Color(0.95f, 0.76f, 0.31f, 1f);
            var cyan = new Color(0.60f, 0.85f, 0.86f, 1f);
            var ink = new Color(0.025f, 0.08f, 0.11f, 1f);
            if (englishLabel != null) englishLabel.text = "EN";
            if (arabicLabel != null)
            {
                arabicLabel.text = ArabicTextShaper.Shape("عربي");
                arabicLabel.font = RuntimeArabicFont.Resolve(englishLabel == null ? null : englishLabel.font);
                arabicLabel.fontSize = 9;
                arabicLabel.verticalOverflow = VerticalWrapMode.Overflow;
            }
            if (englishButton != null && englishButton.targetGraphic != null)
                englishButton.targetGraphic.color = language == GameLanguage.English ? gold : ink;
            if (arabicButton != null && arabicButton.targetGraphic != null)
                arabicButton.targetGraphic.color = language == GameLanguage.Arabic ? gold : ink;
            if (englishLabel != null) englishLabel.color = language == GameLanguage.English ? ink : cyan;
            if (arabicLabel != null) arabicLabel.color = language == GameLanguage.Arabic ? ink : cyan;
        }

        private void ApplyPanelLayout(GameLanguage language)
        {
            var rtl = language == GameLanguage.Arabic;
            SetX("SafeArea/Section__FLAGSHIP/SectionCore/Numeral", rtl ? 299f : -299f);
            SetX("SafeArea/Section__CREW ROLE/SectionCore/Numeral", rtl ? 299f : -299f);
            SetX("SafeArea/Section__CAPTAIN ABILITY/SectionCore/Numeral", rtl ? 299f : -299f);
            SetX("SafeArea/RewardShell/RewardCore/BlueprintSeal", rtl ? 285f : -285f);
            SetX("SafeArea/RewardShell/RewardCore/RewardTag", rtl ? -242f : 242f);
            SetX("SafeArea/CommandDock/ConfirmShell/ConfirmCore/ArrowIsland", rtl ? -265f : 265f);
        }

        private void SetX(string path, float x)
        {
            var item = transform.Find(path) as RectTransform;
            if (item == null) return;
            var position = item.anchoredPosition;
            position.x = x;
            item.anchoredPosition = position;
        }
    }
}
