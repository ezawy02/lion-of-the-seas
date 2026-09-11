namespace SeaLion.Gameplay.Levels
{
    public sealed partial class Level01TrialRuntime
    {
        public int HostileRemaining => hostileRemaining;
        public int InitialHostileCombatants => HostileCombatants;
        public int HostileLost => Mathf.Max(0, InitialHostileCombatants - hostileRemaining);
        public int LandedCraftCount => landingIndex;
        public int LandingCraftTotal => fleet.Count;
        public bool ShowsEnemyCount => Phase == Level01TrialPhase.Assault && !CanRetry;
        public bool ShowsLandingCount => Phase == Level01TrialPhase.Landing && !CanRetry;
    }
}
