using SeaLion.Combat.Bosses;
using UnityEngine;

namespace SeaLion.Gameplay.Levels
{
    public sealed partial class Level01TrialRuntime
    {
        public const int MaxShieldCharges = 3;
        public const int MaxFireRank = 3;
        private const float TelegraphLeadSeconds = 1.35f;
        private const float FireDropLockSeconds = 2.2f;
        private int shieldCharges;
        private int fireRank = 1;
        private bool guardianTelegraphed;
        private float fireDropLock;

        public int ShieldCharges => shieldCharges;
        public int FireRank => fireRank;
        public float FireDensity => fireRank <= 0 ? .62f : fireRank == 1 ? 1f : fireRank == 2 ? 1.28f : 1.52f;
        public float FireCadence => fireRank <= 0 ? 1.32f : fireRank == 1 ? 1f : fireRank == 2 ? .82f : .66f;
        public bool GuardianTelegraphing => guardianTelegraphed && Phase == Level01TrialPhase.Assault;
        public bool EliteGuardActive => Phase == Level01TrialPhase.Assault && hostileRemaining == 1;
        public int GuardianPhase => guardian == null ? 0 : guardian.PhaseIndex;
        public bool ShowsPowerStatus => LevelNumber == 1 && !CanRetry &&
            (Phase == Level01TrialPhase.Landing || Phase == Level01TrialPhase.Assault);

        private void ResetPower()
        {
            shieldCharges = 0;
            fireRank = 1;
            guardianTelegraphed = false;
            fireDropLock = 0f;
        }

        private void TickPower(float step) => fireDropLock = Mathf.Max(0f, fireDropLock - step);

        private void GainSafePassageBuff()
        {
            AddShield(1);
            AddFire(1);
        }

        private void GainRescueBuff()
        {
            AddShield(1);
            AddFire(1);
        }

        private void ApplyRiskyPassageDebuff() => AddFire(-1);

        private void SpendEngageVolley()
        {
            if (fireRank >= 2) AddFire(-1);
        }

        private void OnFriendlyHit()
        {
            if (fireDropLock > 0f) return;
            AddFire(-1);
            fireDropLock = FireDropLockSeconds;
        }

        private void OnEliteBroken() => AddFire(1);

        private int AbsorbGuardianSlam(int loss)
        {
            if (shieldCharges > 0)
            {
                AddShield(-1);
                return Mathf.Max(1, Mathf.CeilToInt(loss / 3f));
            }
            AddFire(-1);
            return loss;
        }

        private bool TryStartGuardianTelegraph()
        {
            if (guardianTelegraphed || guardian == null ||
                guardian.State != HarborGuardianState.Active) return false;
            guardianTelegraphed = guardian.TryTelegraphAttack(FirstAttack(), clock.Tick);
            return guardianTelegraphed;
        }

        private void ClearGuardianTelegraph() => guardianTelegraphed = false;

        private bool GuardianShouldTelegraph(float interval)
        {
            return !guardianTelegraphed && guardian != null &&
                guardian.State == HarborGuardianState.Active &&
                guardianAttackAccumulator >= interval - TelegraphLeadSeconds;
        }

        private void AddShield(int delta) =>
            shieldCharges = Mathf.Clamp(shieldCharges + delta, 0, MaxShieldCharges);

        private void AddFire(int delta) =>
            fireRank = Mathf.Clamp(fireRank + delta, 0, MaxFireRank);
    }
}
