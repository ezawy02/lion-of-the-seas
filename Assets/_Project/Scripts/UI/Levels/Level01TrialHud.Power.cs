using SeaLion.Gameplay.Levels;
using UnityEngine;

namespace SeaLion.UI.Levels
{
    public sealed partial class Level01TrialHud
    {
        private static readonly Color PowerHurt = new Color(0.95f, 0.42f, 0.32f);
        private static readonly Color PowerIdle = new Color(0.65f, 0.88f, 0.9f);
        private static readonly Color FireHot = new Color(1f, 0.70f, 0.22f, 0.78f);
        private static readonly Color FireCold = new Color(0.55f, 0.28f, 0.22f, 0.78f);

        private void RefreshPower()
        {
            if (runtime == null || gate == null || runtime.LevelNumber != 1) return;
            if (runtime.ShowsPowerStatus)
            {
                gate.text = Level01TrialLocalization.FormatPowerStatus(
                    runtime.ShieldCharges, runtime.FireRank, language);
                gate.color = runtime.FireRank >= 2 || runtime.ShieldCharges > 0 ? Gold :
                    runtime.FireRank <= 0 ? PowerHurt : PowerIdle;
            }
            else gate.color = PowerIdle;
            if (reloadCharge == null || runtime.Phase != Level01TrialPhase.Assault) return;
            reloadCharge.color = runtime.FireRank >= 3 ? Gold :
                runtime.FireRank <= 0 ? FireCold : FireHot;
        }
    }
}
