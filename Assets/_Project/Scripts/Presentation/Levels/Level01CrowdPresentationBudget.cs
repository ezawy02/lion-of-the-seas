using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Pure count compression rules shared by the Level 1 crowd presenter and tests.</summary>
    public static class Level01CrowdPresentationBudget
    {
        public static int FriendlyVisibleCount(int logical, int displayCap, int available, float qualityScale)
        {
            var capped = Mathf.Min(Mathf.Max(0, logical), Mathf.Max(0, displayCap), Mathf.Max(0, available));
            return Scale(capped, qualityScale);
        }

        public static int HostileVisibleCount(int remaining, int initial, int available, float qualityScale)
        {
            if (remaining <= 0 || initial <= 0 || available <= 0) return 0;
            var ratio = Mathf.Clamp01(remaining / (float)initial);
            return Scale(Mathf.CeilToInt(available * ratio), qualityScale);
        }

        public static int SourceIndex(int visibleIndex, int visibleCount, int availableCount)
        {
            if (availableCount <= 1 || visibleCount <= 1) return 0;
            visibleIndex = Mathf.Clamp(visibleIndex, 0, visibleCount - 1);
            return Mathf.Clamp(Mathf.RoundToInt(visibleIndex * (availableCount - 1f) /
                (visibleCount - 1f)), 0, availableCount - 1);
        }

        private static int Scale(int count, float qualityScale)
        {
            if (count <= 0) return 0;
            if (float.IsNaN(qualityScale) || float.IsInfinity(qualityScale)) qualityScale = 1f;
            return Mathf.Clamp(Mathf.CeilToInt(count * Mathf.Clamp01(qualityScale)), 1, count);
        }
    }
}
