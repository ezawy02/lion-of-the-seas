using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Shared eased-distance helpers for readable, forward maritime movement.</summary>
    public static class Level01SeaMotion
    {
        public static float SmoothProgress(float elapsed, float duration)
        {
            if (!IsFinite(elapsed) || !IsFinite(duration) || duration <= 0f) return 0f;
            var value = Mathf.Clamp01(elapsed / duration);
            return value * value * (3f - 2f * value);
        }

        public static float ForwardDistance(float elapsed, float duration, float distance)
        {
            if (!IsFinite(distance)) return 0f;
            return SmoothProgress(elapsed, duration) * distance;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
