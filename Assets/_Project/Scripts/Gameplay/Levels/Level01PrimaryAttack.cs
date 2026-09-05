using System;

namespace SeaLion.Gameplay.Levels
{
    public readonly struct Level01PrimaryAttackEvent
    {
        public readonly Level01PrimaryAttackResult Result;
        public bool HitGuardian => Result.Fired && Result.Hit && Result.TargetIndex < 0;
        public Level01PrimaryAttackEvent(Level01PrimaryAttackResult result) { Result = result; }
    }

    public readonly struct Level01PrimaryAttackResult
    {
        public readonly bool Fired;
        public readonly bool Hit;
        public readonly float Damage;
        public readonly int TargetIndex;
        public Level01PrimaryAttackResult(bool fired, bool hit, float damage, int targetIndex)
        { Fired = fired; Hit = hit; Damage = damage; TargetIndex = targetIndex; }
        public static Level01PrimaryAttackResult Rejected => new Level01PrimaryAttackResult(false, false, 0f, -1);
    }
}
