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

        public static float RouteTravel(float progress, float gateProgress = .4f,
            float distanceAtGate = 48f, float distanceAtShore = 70f)
        {
            if (!IsFinite(progress) || !IsFinite(gateProgress) || gateProgress <= 0f ||
                !IsFinite(distanceAtGate) || !IsFinite(distanceAtShore)) return 0f;
            progress = Mathf.Clamp01(progress);
            if (progress <= gateProgress)
                return SmoothProgress(progress, gateProgress) * distanceAtGate;
            var after = SmoothProgress(progress - gateProgress, 1f - gateProgress);
            return Mathf.Lerp(distanceAtGate, distanceAtShore, after);
        }

        public static float ShoreBlend01(float routeProgress, bool gateCommitted, float gateProgress = .4f)
        {
            if (!gateCommitted || !IsFinite(routeProgress) || !IsFinite(gateProgress)) return 0f;
            return Mathf.Clamp01((Mathf.Clamp01(routeProgress) - gateProgress) / Mathf.Max(.01f, 1f - gateProgress));
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
