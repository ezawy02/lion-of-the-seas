namespace SeaLion.UI.Localization
{
    public enum GameLanguage
    {
        English = 0,
        Arabic = 1
    }

    public static class GameLanguagePreference
    {
        public const string English = "English";
        public const string Arabic = "Arabic";

        public static GameLanguage Parse(string value)
        {
            return value == Arabic ? GameLanguage.Arabic : GameLanguage.English;
        }

        public static string ToStoredValue(GameLanguage language)
        {
            return language == GameLanguage.Arabic ? Arabic : English;
        }

        public static bool IsValid(string value)
        {
            return value == English || value == Arabic;
        }
    }
}
