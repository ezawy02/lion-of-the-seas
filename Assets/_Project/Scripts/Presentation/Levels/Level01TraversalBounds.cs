using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    public readonly struct HorizontalTravelRange
    {
        public readonly float Left;
        public readonly float Right;
        public bool IsValid => IsFinite(Left) && IsFinite(Right) && Left < Right;

        public HorizontalTravelRange(float left, float right)
        {
            Left = left;
            Right = right;
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }

    /// <summary>Derives world-space steering limits from the full on-screen flagship bounds.</summary>
    public static class Level01TraversalBounds
    {
        public static HorizontalTravelRange Calculate(Camera camera, Bounds presentationBounds,
            float controlledTransformX, float viewportMargin = .055f)
        {
            if (camera == null) return new HorizontalTravelRange(-3f, 3f);
            viewportMargin = Mathf.Clamp(viewportMargin, .01f, .25f);
            var leftDelta = float.NegativeInfinity;
            var rightDelta = float.PositiveInfinity;
            for (var index = 0; index < 8; index++)
            {
                var corner = presentationBounds.center + Vector3.Scale(presentationBounds.extents,
                    new Vector3((index & 1) == 0 ? -1f : 1f,
                        (index & 2) == 0 ? -1f : 1f, (index & 4) == 0 ? -1f : 1f));
                var viewport = camera.WorldToViewportPoint(corner);
                var shifted = camera.WorldToViewportPoint(corner + Vector3.right);
                var slope = shifted.x - viewport.x;
                if (viewport.z <= .01f || Mathf.Abs(slope) <= .00001f) continue;
                var first = (viewportMargin - viewport.x) / slope;
                var second = (1f - viewportMargin - viewport.x) / slope;
                leftDelta = Mathf.Max(leftDelta, Mathf.Min(first, second));
                rightDelta = Mathf.Min(rightDelta, Mathf.Max(first, second));
            }

            var result = new HorizontalTravelRange(controlledTransformX + leftDelta,
                controlledTransformX + rightDelta);
            if (!result.IsValid) return new HorizontalTravelRange(controlledTransformX - 3f,
                controlledTransformX + 3f);
            return result;
        }
    }
}
