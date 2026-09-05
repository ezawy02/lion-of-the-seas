using UnityEngine;
using SeaLion.Gameplay.Input;

namespace SeaLion.Gameplay.Flagship
{
    /// <summary>Presentation-safe kinematic flagship movement driven by normalized intent.</summary>
    public sealed class FlagshipController : MonoBehaviour
    {
        [SerializeField] private FlagshipInputAdapter input;
        [SerializeField] private float leftBound = -5f;
        [SerializeField] private float rightBound = 5f;
        [SerializeField, Min(0f)] private float moveSpeed = 8f;
        [SerializeField, Min(0.01f)] private float smoothing = 14f;

        private float smoothedIntent;
        private bool appPaused;
        private bool focusLost;
        private bool gameplayPaused;

        public float SmoothedIntent => smoothedIntent;
        public float LeftBound => Mathf.Min(leftBound, rightBound);
        public float RightBound => Mathf.Max(leftBound, rightBound);

        public void Configure(FlagshipInputAdapter inputSource, float firstBound, float secondBound,
            float speed = 8f, float response = 14f)
        {
            input = inputSource;
            leftBound = firstBound;
            rightBound = secondBound;
            moveSpeed = Mathf.Max(0f, speed);
            smoothing = Mathf.Max(0.01f, response);
        }

        private void Update()
        {
            if (appPaused || focusLost || gameplayPaused || !isActiveAndEnabled)
            {
                smoothedIntent = 0f;
                return;
            }

            var intent = input == null ? 0f : input.HorizontalIntent;
            smoothedIntent = SmoothNormalized(smoothedIntent, Mathf.Clamp(intent, -1f, 1f), smoothing, Time.deltaTime);
            var position = transform.position;
            var speed = IsFinite(moveSpeed) ? Mathf.Max(0f, moveSpeed) : 0f;
            position.x = ClampPosition(position.x + smoothedIntent * speed * Time.deltaTime, leftBound, rightBound);
            transform.position = position;
        }

        private void OnApplicationPause(bool pause) { appPaused = pause; if (pause) smoothedIntent = 0f; }
        private void OnApplicationFocus(bool focus) { focusLost = !focus; if (!focus) smoothedIntent = 0f; }
        private void OnDisable() { smoothedIntent = 0f; }

        public void SetPaused(bool value)
        {
            gameplayPaused = value;
            if (value) smoothedIntent = 0f;
        }

        public void Nudge(float normalizedDelta)
        {
            if (!IsFinite(normalizedDelta) || gameplayPaused || appPaused || focusLost) return;
            var position = transform.position;
            position.x = ClampPosition(position.x + Mathf.Clamp(normalizedDelta, -1f, 1f) *
                (RightBound - LeftBound), leftBound, rightBound);
            transform.position = position;
        }

        public static float SmoothNormalized(float current, float target, float sharpness, float deltaTime)
        {
            current = IsFinite(current) ? Mathf.Clamp(current, -1f, 1f) : 0f;
            target = IsFinite(target) ? Mathf.Clamp(target, -1f, 1f) : 0f;
            if (!IsFinite(deltaTime) || !IsFinite(sharpness) || deltaTime <= 0f || sharpness <= 0f)
                return current;
            var blend = 1f - Mathf.Exp(-sharpness * deltaTime);
            return Mathf.Clamp(Mathf.Lerp(current, target, blend), -1f, 1f);
        }

        public static float ClampPosition(float position, float firstBound, float secondBound)
        {
            position = IsFinite(position) ? position : 0f;
            if (!IsFinite(firstBound) || !IsFinite(secondBound)) return position;
            var low = Mathf.Min(firstBound, secondBound);
            var high = Mathf.Max(firstBound, secondBound);
            return Mathf.Clamp(position, low, high);
        }

        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
