using SeaLion.Gameplay.Input;
using SeaLion.Gameplay.Flagship;
using UnityEngine;
using UnityEngine.EventSystems;

namespace SeaLion.UI.Levels
{
    public sealed class Level01SteeringHoldButton : MonoBehaviour,
        IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        private FlagshipInputAdapter input;
        private FlagshipController flagship;
        private float direction;
        private bool pressed;

        public void Bind(FlagshipInputAdapter source, FlagshipController controlledFlagship,
            float horizontalDirection)
        {
            input = source;
            flagship = controlledFlagship;
            direction = Mathf.Clamp(horizontalDirection, -1f, 1f);
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            pressed = true;
            input?.SetUiIntent(direction);
            flagship?.Nudge(direction * 0.3f);
        }

        public void OnPointerUp(PointerEventData eventData) => Release();
        public void OnPointerExit(PointerEventData eventData) => Release();

        private void OnDisable() => Release();

        private void Release()
        {
            if (!pressed) return;
            pressed = false;
            input?.ReleaseUiIntent();
        }
    }
}
