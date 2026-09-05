using System.Collections.Generic;
using System.IO;
using SeaLion.Core.Persistence;
using SeaLion.UI.Localization;
using UnityEngine;

namespace SeaLion.UI.Levels
{
    /// <summary>Compact Level 1 copy and the shared, atomic language preference seam.</summary>
    public static class Level01TrialLocalization
    {
        private static readonly Dictionary<string, string[]> Text = new Dictionary<string, string[]>
        {
            { "stage", new[] { "LEVEL 1  •  THE HUNDRED SAILS", "المستوى الأول  •  أسطول المئة شراع" } },
            { "opening", new[] { "FORM THE VANGUARD", "تشكيل الطليعة" } },
            { "traversal", new[] { "CHOOSE YOUR PASSAGE", "اختر مسارك عبر المضيق" } },
            { "landing", new[] { "MAKE LANDFALL", "أنزل القوات إلى الشاطئ" } },
            { "assault", new[] { "STORM THE HARBOR FORTRESS", "اقتحم حصن الميناء" } },
            { "victory", new[] { "VICTORY AT THE HARBOR", "النصر في الميناء" } },
            { "failure", new[] { "THE FLEET HAS FALLEN", "سقط الأسطول" } },
            { "force", new[] { "CREW", "القوات" } },
            { "guardian", new[] { "HARBOR FORTRESS GUARDIAN", "حارس حصن الميناء" } },
            { "gatePending", new[] { "◀ ×4 SAFE  |  RISKY -1 ▶", "◀ آمن ×٤  |  خطر -١ ▶" } },
            { "gateSafe", new[] { "SAFE PASSAGE ×4  •  REINFORCED", "الممر الآمن ×٤  •  تعزيز القوات" } },
            { "gateRisk", new[] { "DANGER PASSAGE  •  LOSE 1 CREW (-1)", "الممر الخطر  •  خسارة فرد واحد (-١)" } },
            { "ability", new[] { "CAPTAIN'S RALLY", "نداء القبطان" } },
            { "abilityShort", new[] { "RALLY", "النداء" } },
            { "steer", new[] { "DRAG TO STEER", "اسحب يمينًا أو يسارًا" } },
            { "steerAnywhere", new[] { "HOLD ANYWHERE", "المس أي مكان" } },
            { "steerToChoose", new[] { "STEER LEFT ×4 OR RIGHT -1", "انحرف يسارا ×٤ أو يمينا -١" } },
            { "landingAssist", new[] { "TAP FIRE TO LAND FASTER", "اضغط إطلاق للإنزال الأسرع" } },
            { "dodgeHint", new[] { "STEER TO DODGE", "انحرف لتفادي الضرب" } },
            { "ready", new[] { "TAP  •  READY", "اضغط  •  جاهزة" } },
            { "charging", new[] { "RECHARGING", "جاري الاستعداد" } },
            { "retry", new[] { "SAIL AGAIN", "أبحر من جديد" } },
            { "reward", new[] { "NEW BLUEPRINT UNLOCKED", "تم فتح مخطط جديد" } },
            { "rewardBody", new[] { "Sailmakers Crew is ready for your next voyage", "طاقم صناع الأشرعة جاهز للرحلة القادمة" } },
            { "rewardFailure", new[] { "Could not save the reward — sail again to retry", "تعذر حفظ المكافأة — أبحر من جديد للمحاولة" } },
            { "failureTimeout", new[] { "The guardian held the harbor — strike faster", "صمد الحارس في الميناء — اضرب بصورة أسرع" } },
            { "failureDepleted", new[] { "The landing force was depleted", "نفدت قوات الإنزال" } },
            { "failureGeneric", new[] { "The assault failed — regroup and sail again", "فشل الهجوم — أعد التجمع وأبحر من جديد" } }
        };

        public static string Get(string key, GameLanguage language)
        {
            if (!Text.TryGetValue(key, out var values)) return key ?? string.Empty;
            return values[language == GameLanguage.Arabic ? 1 : 0];
        }

        public static string Display(string key, GameLanguage language)
        {
            var value = Get(key, language);
            return language == GameLanguage.Arabic ? ArabicTextShaper.Shape(value) : value;
        }

        public static string FormatForce(int count, int cap, GameLanguage language)
        {
            if (language == GameLanguage.English) return Get("force", language) + "  " + count + " / " + cap;
            var raw = Get("force", language) + "  " + ArabicDigits(count) + " / " + ArabicDigits(cap);
            return ArabicTextShaper.Shape(raw);
        }

        public static string FormatPercent(float value01, GameLanguage language)
        {
            var percent = Mathf.RoundToInt(Mathf.Clamp01(value01) * 100f).ToString() + "%";
            return language == GameLanguage.Arabic
                ? ArabicTextShaper.Shape(ArabicDigits(percent))
                : percent;
        }

        public static string FailureKey(string reason)
        {
            if (reason == "guardian-timeout") return "failureTimeout";
            if (reason == "force-depleted") return "failureDepleted";
            return "failureGeneric";
        }

        public static GameLanguage LoadLanguage()
        {
            var result = Repository().Load();
            return result.Succeeded && result.Data != null
                ? GameLanguagePreference.Parse(result.Data.settings.languagePreference)
                : GameLanguage.English;
        }

        public static bool SaveLanguage(GameLanguage language)
        {
            var repository = Repository();
            var result = repository.Load();
            if (!result.Succeeded || result.Data == null) return false;
            result.Data.settings.languagePreference = GameLanguagePreference.ToStoredValue(language);
            return repository.Save(result.Data, out _);
        }

        private static LocalSaveRepository Repository()
        {
            return new LocalSaveRepository(Path.Combine(Application.persistentDataPath,
                LocalSaveRepository.DefaultFileName));
        }

        private static string ArabicDigits(int value) => ArabicDigits(value.ToString());

        private static string ArabicDigits(string value)
        {
            var characters = value.ToCharArray();
            for (var index = 0; index < characters.Length; index++)
                if (characters[index] >= '0' && characters[index] <= '9')
                    characters[index] = (char)('٠' + characters[index] - '0');
            return new string(characters);
        }
    }
}
