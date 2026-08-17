using UnityEngine;
using UnityEngine.InputSystem;

namespace SeaLion.Gameplay.Input
{
    /// <summary>Samples one-handed portrait drag input without changing gameplay state.</summary>
    public sealed class FlagshipInputAdapter : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float dragPixelsForFullIntent = 240f;

        private bool dragging;
        private Vector2 dragStart;

        public float HorizontalIntent { get; private set; }

        private void Update()
        {
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

            Reset();
        }

        private void SamplePointer(Vector2 position, bool pressed, bool released)
        {
            if (pressed && !dragging)
            {
                dragging = true;
                dragStart = position;
            }

            if (dragging && !released)
                HorizontalIntent = MapHorizontalDrag(position.x - dragStart.x, dragPixelsForFullIntent);

            if (released)
                Reset();
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
            HorizontalIntent = 0f;
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
