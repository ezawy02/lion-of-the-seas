using UnityEngine;
using UnityEngine.InputSystem;

namespace SeaLion.Gameplay.Input
{
    /// <summary>Samples one-handed portrait drag input without changing gameplay state.</summary>
    public sealed class FlagshipInputAdapter : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float dragPixelsForFullIntent = 240f;

        private bool dragging;
        private bool uiSteering;
        private Vector2 dragStart;
        private float releasedIntentHold;

        public float HorizontalIntent { get; private set; }
        public bool IsDragging => dragging;
        public bool HasSteered { get; private set; }

        private void Update()
        {
            if (uiSteering) return;
            if (TryReadTouch(out var position, out var pressed, out var released))
            {
                SamplePointer(position, pressed, released);
                return;
            }

            if (TryReadMouse(out position, out pressed, out released))
            {
                SamplePointer(position, pressed, released);
                return;
            }

            if (releasedIntentHold > 0f)
            {
                releasedIntentHold -= Time.unscaledDeltaTime;
                if (releasedIntentHold > 0f) return;
            }
            Reset();
        }

        private void SamplePointer(Vector2 position, bool pressed, bool released)
        {
            if (pressed && !dragging)
            {
                dragging = true;
                dragStart = position;
            }

            if (dragging)
            {
                HorizontalIntent = MapHorizontalDrag(position.x - dragStart.x, dragPixelsForFullIntent);
                if (Mathf.Abs(HorizontalIntent) > 0.08f) HasSteered = true;
            }

            if (released)
            {
                dragging = false;
                HorizontalIntent = 0f;
                releasedIntentHold = 0f;
            }
        }

        private static bool TryReadTouch(out Vector2 position, out bool pressed, out bool released)
        {
            var touch = Touchscreen.current?.primaryTouch;
            if (touch == null)
            {
                position = default;
                pressed = released = false;
                return false;
            }

            position = touch.position.ReadValue();
            pressed = touch.press.wasPressedThisFrame;
            released = touch.press.wasReleasedThisFrame || touch.phase.ReadValue() == UnityEngine.InputSystem.TouchPhase.Canceled;
            return touch.press.isPressed || pressed || released;
        }

        private static bool TryReadMouse(out Vector2 position, out bool pressed, out bool released)
        {
            var mouse = Mouse.current;
            if (mouse == null)
            {
                position = default;
                pressed = released = false;
                return false;
            }

            position = mouse.position.ReadValue();
            pressed = mouse.leftButton.wasPressedThisFrame;
            released = mouse.leftButton.wasReleasedThisFrame;
            return mouse.leftButton.isPressed || pressed || released;
        }

        public void Reset()
        {
            dragging = false;
            uiSteering = false;
            releasedIntentHold = 0f;
            HorizontalIntent = 0f;
            HasSteered = false;
        }

        public void SetUiIntent(float value)
        {
            uiSteering = true;
            HorizontalIntent = Mathf.Clamp(value, -1f, 1f);
            if (Mathf.Abs(HorizontalIntent) > 0.08f) HasSteered = true;
        }

        public void ReleaseUiIntent()
        {
            uiSteering = false;
            releasedIntentHold = 0.18f;
        }

        private void OnApplicationFocus(bool hasFocus)
        {
            if (!hasFocus)
                Reset();
        }

        private void OnDisable() => Reset();

        public static float MapHorizontalDrag(float horizontalPixels, float pixelsForFullIntent)
        {
            if (pixelsForFullIntent <= 0f)
                return 0f;

            return Mathf.Clamp(horizontalPixels / pixelsForFullIntent, -1f, 1f);
        }
    }
}
