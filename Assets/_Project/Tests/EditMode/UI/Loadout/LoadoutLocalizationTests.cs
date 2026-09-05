using NUnit.Framework;
using SeaLion.UI.Localization;

namespace SeaLion.Tests.EditMode.UI.Loadout
{
    public sealed class LoadoutLocalizationTests
    {
        [Test]
        public void CatalogProvidesDistinctEnglishAndArabicValues()
        {
            Assert.AreEqual("COMMANDER'S LOADOUT",
                LoadoutLocalization.Get("header.title", GameLanguage.English));
            Assert.AreEqual("تجهيزات القائد",
                LoadoutLocalization.Get("header.title", GameLanguage.Arabic));
        }

        [Test]
        public void EveryVisibleOptionFieldHasBothLanguages()
        {
            var ids = new[]
            {
                "default-flagship", "flagship-lateen-raider", "default-crew",
                "loadout-crew-sailmakers", "default-ability", "ability-powder-barrage"
            };
            var fields = new[] { "name", "role", "tradeoff" };
            for (var idIndex = 0; idIndex < ids.Length; idIndex++)
            for (var fieldIndex = 0; fieldIndex < fields.Length; fieldIndex++)
            {
                var key = "option." + ids[idIndex] + "." + fields[fieldIndex];
                Assert.AreNotEqual(key, LoadoutLocalization.Get(key, GameLanguage.English), key);
                Assert.AreNotEqual(key, LoadoutLocalization.Get(key, GameLanguage.Arabic), key);
            }
        }

        [Test]
        public void ArabicShaperUsesPresentationFormsAndKeepsNumberOrder()
        {
            var shaped = ArabicTextShaper.Shape("جاهز ١٢٣");
            StringAssert.DoesNotContain("جاهز", shaped);
            StringAssert.Contains("١٢٣", shaped);
            Assert.IsTrue(ContainsPresentationForm(shaped));
        }

        [Test]
        public void ArabicShaperKeepsMixedStatTokensPresent()
        {
            var shaped = ArabicTextShaper.Shape("الضرر ١٫٨ • المتانة ١٫٠");
            StringAssert.Contains("١٫٨", shaped);
            StringAssert.Contains("١٫٠", shaped);
            StringAssert.Contains("•", shaped);
            Assert.IsTrue(ContainsPresentationForm(shaped));
        }

        [Test]
        public void ControllerErrorKeysHaveArabicAndEnglishValues()
        {
            var keys = new[] { "error.notConfigured", "error.notInitialized", "error.invalidLanguage" };
            for (var index = 0; index < keys.Length; index++)
            {
                Assert.AreNotEqual(keys[index], LoadoutLocalization.Get(keys[index], GameLanguage.English));
                Assert.AreNotEqual(keys[index], LoadoutLocalization.Get(keys[index], GameLanguage.Arabic));
            }
        }

        [Test]
        public void StoredLanguageValuesAreStable()
        {
            Assert.AreEqual(GameLanguage.Arabic, GameLanguagePreference.Parse("Arabic"));
            Assert.AreEqual(GameLanguage.English, GameLanguagePreference.Parse("unknown"));
            Assert.AreEqual("Arabic", GameLanguagePreference.ToStoredValue(GameLanguage.Arabic));
        }

        private static bool ContainsPresentationForm(string value)
        {
            for (var index = 0; index < value.Length; index++)
                if (value[index] >= '\uFE70' && value[index] <= '\uFEFF') return true;
            return false;
        }
    }
}
