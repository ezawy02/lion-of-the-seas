using System.IO;
using SeaLion.Core.Persistence;
using SeaLion.Gameplay.Levels;
using SeaLion.Gameplay.Rewards;
using SeaLion.UI.Localization;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace SeaLion.UI.Levels
{
    public sealed partial class Level01TrialHud
    {
        private Text continueLabel;
        private void BuildCampaignControls(Transform card)
        {
            continueLabel = Button(card, "Continue Campaign", new Vector2(.5f, -.06f),
                new Vector2(.5f, -.06f), new Vector2(-145f, -22f), new Vector2(145f, 22f),
                "NEXT ENCOUNTER", ContinueCampaign);
        }

        private string Bilingual(string english, string arabic) => language == GameLanguage.Arabic
            ? ArabicTextShaper.Shape(arabic) : english;

        private void RefreshCampaign()
        {
            if (continueLabel != null)
            {
                continueLabel.transform.parent.gameObject.SetActive(runtime.Phase == Level01TrialPhase.Victory && runtime.LevelNumber < 3);
                continueLabel.text = Bilingual("NEXT ENCOUNTER", "المعركة التالية");
            }
            if (runtime.LevelNumber == 1) return;
            stage.text = runtime.LevelNumber == 2 ? Bilingual("LEVEL 2 · CHAIN STRAIT", "المستوى الثاني · مضيق السلاسل") :
                Bilingual("LEVEL 3 · STORM FORTRESS", "المستوى الثالث · قلعة العاصفة");
            boss.text = runtime.LevelNumber == 2 ? Bilingual("ARMORED WARSHIP", "السفينة المدرعة") :
                runtime.AssaultStage == 1 ? Bilingual("BREACH THE OUTER GATE", "اقتحم البوابة الخارجية") :
                Bilingual("DEFEAT THE COMMANDER", "اهزم قائد القلعة");
            gate.text = runtime.LevelNumber == 2 ? Bilingual("LEFT ×3 · CENTER +12 · RIGHT ×5", "يسار ×٣ · وسط +١٢ · يمين ×٥") :
                Bilingual("LEFT ×4 CREW · RIGHT 2 CREW → 1 POWDER", "يسار ×٤ قوات · يمين كل فردين مقابل بارود");
            if (runtime.BlockadeActive) phase.text = Bilingual("FIRE TO BREAK THE CHAIN", "أطلق لتحطيم السلسلة");
            else if (runtime.HazardWarning) phase.text = Bilingual("INCOMING: " + (runtime.HazardLane < -.33f ? "LEFT" : runtime.HazardLane > .33f ? "RIGHT" : "CENTER"), "قصف قادم: " + (runtime.HazardLane < -.33f ? "يسار" : runtime.HazardLane > .33f ? "يمين" : "وسط"));
            else if (runtime.Phase == Level01TrialPhase.Assault && runtime.LevelNumber == 3)
                phase.text = Bilingual("ASSAULT " + runtime.AssaultStage + " / 2 · POWDER " + runtime.Powder,
                    "الهجوم " + runtime.AssaultStage + " / ٢ · بارود " + runtime.Powder);
            if (runtime.BlockadeActive) { bossCard.SetActive(true); boss.text = Bilingual("CHAIN BLOCKADE", "حاجز السلاسل"); bossHealth.value = runtime.BlockadeHealth01; }
        }

        private string CampaignReward(RewardGrantResult item)
        {
            var name = runtime.LevelNumber == 2 ? Bilingual("LATEEN RAIDER FLAGSHIP", "سفينة المغير") :
                Bilingual("POWDER BARRAGE ABILITY", "قدرة قصف البارود");
            return (item.AlreadyGranted ? Bilingual("ALREADY OWNED", "مملوك بالفعل") :
                Bilingual("BLUEPRINT UNLOCKED", "تم فتح المخطط")) + "\n" + name;
        }

        private void ContinueCampaign()
        {
            if (runtime.Phase != Level01TrialPhase.Victory || runtime.LevelNumber >= 3) return;
            var saved = new LocalSaveRepository(Path.Combine(Application.persistentDataPath, runtime.SaveFileName)).Load();
            var next = runtime.LevelNumber + 1;
            if (!saved.Succeeded || saved.Data.highestUnlockedLevel < next) return;
            SceneManager.LoadSceneAsync("Level_0" + next + "_Playable_Trial", LoadSceneMode.Single);
        }
    }
}
