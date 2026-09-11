using SeaLion.UI.Localization;

namespace SeaLion.UI.Levels
{
    public static partial class Level01TrialLocalization
    {
        public static string FormatJourneyForce(int crew, int enemies, int landedCraft,
            int landingTotal, bool showEnemies, bool showLanding, GameLanguage language)
        {
            var raw = Get("force", language) + "  " + Digits(crew, language);
            if (showLanding)
                raw += "  •  " + Get("landingCount", language) + "  " +
                    Digits(landedCraft, language) + "/" + Digits(landingTotal, language);
            else if (showEnemies)
                raw += "  •  " + Get("enemies", language) + "  " + Digits(enemies, language);
            return language == GameLanguage.Arabic ? ArabicTextShaper.Shape(raw) : raw;
        }

        public static string FormatJourneyGate(bool committed, bool easy, int before, int after,
            GameLanguage language)
        {
            if (!committed) return Display("gatePending", language);
            var raw = Get(easy ? "gateSafe" : "gateRisk", language) + "  •  " +
                Digits(before, language) + " → " + Digits(after, language);
            return language == GameLanguage.Arabic ? ArabicTextShaper.Shape(raw) : raw;
        }

        public static string FormatResultBody(bool victory, int remaining, int peak,
            string failureReason, GameLanguage language)
        {
            var counts = Get("remaining", language) + "  " + Digits(remaining, language) +
                "  •  " + Get("peakForce", language) + "  " + Digits(peak, language);
            if (language == GameLanguage.Arabic)
            {
                counts = ArabicTextShaper.Shape(counts);
                return victory ? counts :
                    ArabicTextShaper.Shape(Get(FailureKey(failureReason), language)) + "\n" + counts;
            }
            return victory ? counts : Get(FailureKey(failureReason), language) + "\n" + counts;
        }

        private static string Digits(int value, GameLanguage language)
        {
            return language == GameLanguage.Arabic ? ArabicDigits(value) : value.ToString();
        }
    }
}
