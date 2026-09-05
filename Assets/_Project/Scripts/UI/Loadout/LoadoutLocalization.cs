using System.Collections.Generic;

namespace SeaLion.UI.Localization
{
    /// <summary>Small, deterministic two-language catalog for the Level 1 loadout journey.</summary>
    public static class LoadoutLocalization
    {
        private readonly struct Entry
        {
            public readonly string English;
            public readonly string Arabic;

            public Entry(string english, string arabic)
            {
                English = english;
                Arabic = arabic;
            }
        }

        private static readonly Dictionary<string, Entry> Entries =
            new Dictionary<string, Entry>
            {
                { "header.brand", new Entry("LION OF THE SEAS  •  FLEET DOCTRINE", "أسد البحار  •  عقيدة الأسطول") },
                { "header.title", new Entry("COMMANDER'S LOADOUT", "تجهيزات القائد") },
                { "header.subtitle", new Entry("Choose one doctrine for each battle system", "اختر عقيدة لكل نظام قتالي") },
                { "header.summary.ready", new Entry("VANGUARD  •  SEA GUARD  •  CAPTAIN'S RALLY", "الطليعة  •  حرس البحر  •  نداء القبطان") },
                { "header.summary.incomplete", new Entry("COMPLETE ALL THREE BATTLE SYSTEMS", "أكمل أنظمة القتال الثلاثة") },
                { "compass.north", new Entry("N", "ش") },

                { "slot.flagship.heading", new Entry("FLAGSHIP", "سفينة القيادة") },
                { "slot.flagship.kicker", new Entry("DEPLOYMENT DOCTRINE", "عقيدة الإنزال") },
                { "slot.crew.heading", new Entry("CREW ROLE", "دور الطاقم") },
                { "slot.crew.kicker", new Entry("BOARDING FORMATION", "تشكيل الاقتحام") },
                { "slot.ability.heading", new Entry("CAPTAIN ABILITY", "قدرة القبطان") },
                { "slot.ability.kicker", new Entry("TACTICAL COMMAND", "القيادة التكتيكية") },
                { "slot.one", new Entry("I", "١") },
                { "slot.two", new Entry("II", "٢") },
                { "slot.three", new Entry("III", "٣") },

                { "option.default-flagship.name", new Entry("Lion Vanguard", "طليعة الأسد") },
                { "option.default-flagship.role", new Entry("CADENCE FORMATION", "تشكيل متتابع") },
                { "option.default-flagship.tradeoff", new Entry("1 craft every 0.9s", "مركب واحد كل ٠٫٩ ثانية") },
                { "option.flagship-lateen-raider.name", new Entry("Lateen Raider", "غارة الشراع اللاتيني") },
                { "option.flagship-lateen-raider.role", new Entry("BURST FORMATION", "تشكيل دفعي") },
                { "option.flagship-lateen-raider.tradeoff", new Entry("3 craft • 1.5s recovery", "٣ مراكب • تعاف ١٫٥ ثانية") },
                { "option.flagship-lateen-raider.lock", new Entry("LEVEL II BLUEPRINT", "مخطط المستوى الثاني") },

                { "option.default-crew.name", new Entry("Sea Guard", "حرس البحر") },
                { "option.default-crew.role", new Entry("BALANCED CREW", "طاقم متوازن") },
                { "option.default-crew.tradeoff", new Entry("Damage 1.8 • durability 1.0", "الضرر ١٫٨ • المتانة ١٫٠") },
                { "option.loadout-crew-sailmakers.name", new Entry("Sailmakers Corps", "فيلق صناع الأشرعة") },
                { "option.loadout-crew-sailmakers.role", new Entry("DEFENDER CREW", "طاقم دفاعي") },
                { "option.loadout-crew-sailmakers.tradeoff", new Entry("Damage 1.6 • durability 1.5", "الضرر ١٫٦ • المتانة ١٫٥") },
                { "option.loadout-crew-sailmakers.lock", new Entry("LEVEL I BLUEPRINT", "مخطط المستوى الأول") },

                { "option.default-ability.name", new Entry("Captain's Rally", "نداء القبطان") },
                { "option.default-ability.role", new Entry("REINFORCE +8", "تعزيز +٨") },
                { "option.default-ability.tradeoff", new Entry("Time charge • 5s cooldown", "شحن زمني • تبريد ٥ ثوان") },
                { "option.ability-powder-barrage.name", new Entry("Powder Barrage", "وابل البارود") },
                { "option.ability-powder-barrage.role", new Entry("STRIKE 18", "ضربة ١٨") },
                { "option.ability-powder-barrage.tradeoff", new Entry("Damage charge • 9s cooldown", "شحن بالضرر • تبريد ٩ ثوان") },
                { "option.ability-powder-barrage.lock", new Entry("REQUIRES BLUEPRINT", "يتطلب مخطط") },

                { "state.equipped", new Entry("EQUIPPED", "مجهز") },
                { "state.locked", new Entry("LOCKED", "مغلق") },
                { "state.select", new Entry("SELECT", "اختر") },
                { "state.ready", new Entry("READY TO EQUIP", "جاهز للتجهيز") },

                { "reward.eyebrow", new Entry("LEVEL I REWARD", "مكافأة المستوى الأول") },
                { "reward.title", new Entry("SAILMAKERS BLUEPRINT", "مخطط صناع الأشرعة") },
                { "reward.body", new Entry("Victory unlocks a durable crew doctrine", "الانتصار يفتح عقيدة طاقم أكثر صمودا") },
                { "reward.tag", new Entry("AFTER VICTORY", "بعد الانتصار") },
                { "reward.mark", new Entry("BP", "مخ") },

                { "confirm.eyebrow", new Entry("LOCK IN LOADOUT", "ثبت التجهيزات") },
                { "confirm.label", new Entry("SET SAIL", "أبحر الآن") },
                { "confirm.autosave", new Entry("LOADOUT SAVES AUTOMATICALLY", "تحفظ التجهيزات تلقائيا") },
                { "confirm.arrow", new Entry(">", "<") },

                { "status.review", new Entry("REVIEW R4  •  BILINGUAL APPROVAL PENDING", "مراجعة R4 ثنائية اللغة • بانتظار الاعتماد") },
                { "status.saved", new Entry("Loadout saved for the next battle", "تم حفظ التجهيزات للمعركة القادمة") },
                { "status.confirmed", new Entry("FLEET DOCTRINE LOCKED  •  READY TO SET SAIL", "تم تثبيت عقيدة الأسطول • جاهز للإبحار") },
                { "status.failure", new Entry("Unable to save the loadout", "تعذر حفظ التجهيزات") },
                { "error.notConfigured", new Entry("Loadout definitions are not configured.", "لم يتم إعداد تعريفات التجهيزات.") },
                { "error.notInitialized", new Entry("Loadout controller is not initialized.", "لم تتم تهيئة وحدة التجهيزات.") },
                { "error.invalidLanguage", new Entry("Language preference is invalid.", "تفضيل اللغة غير صالح.") }
            };

        public static string Get(string key, GameLanguage language)
        {
            if (string.IsNullOrEmpty(key) || !Entries.TryGetValue(key, out var entry))
                return key ?? string.Empty;
            return language == GameLanguage.Arabic ? entry.Arabic : entry.English;
        }

        public static string GetOption(string optionId, string field, GameLanguage language)
        {
            return Get("option." + optionId + "." + field, language);
        }

        public static string FormatReadiness(int ready, GameLanguage language)
        {
            if (language == GameLanguage.Arabic)
                return ToArabicDigits(ready.ToString()) + " / ٣  جاهز";
            return ready + " / 3  READY";
        }

        public static string FormatForDisplay(string value, GameLanguage language)
        {
            return language == GameLanguage.Arabic ? ArabicTextShaper.Shape(value) : value;
        }

        private static string ToArabicDigits(string value)
        {
            var chars = value.ToCharArray();
            for (var index = 0; index < chars.Length; index++)
                if (chars[index] >= '0' && chars[index] <= '9')
                    chars[index] = (char)('٠' + chars[index] - '0');
            return new string(chars);
        }
    }
}
