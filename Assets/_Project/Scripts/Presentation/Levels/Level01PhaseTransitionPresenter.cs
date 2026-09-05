using System.Collections;
using SeaLion.Gameplay.Levels;
using UnityEngine;
using UnityEngine.UI;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Hides phase-root swaps behind a short nautical transition instead of a visible pop.</summary>
    [DisallowMultipleComponent]
    public sealed class Level01PhaseTransitionPresenter : MonoBehaviour
    {
        private const float TransitionSeconds = 1.15f;
        private GameObject opening;
        private GameObject traversal;
        private GameObject landing;
        private GameObject assault;
        private GameObject victory;
        private CanvasGroup veil;
        private Image veilImage;
        private Coroutine routine;
        private Level01TrialPhase visiblePhase = Level01TrialPhase.Loading;
        private bool initialized;

        public bool IsTransitioning => routine != null;

        public void Bind(GameObject openingRoot, GameObject traversalRoot, GameObject landingRoot,
            GameObject assaultRoot, GameObject victoryRoot)
        {
            opening = openingRoot;
            traversal = traversalRoot;
            landing = landingRoot;
            assault = assaultRoot;
            victory = victoryRoot;
            EnsureVeil();
            ApplyRoots(Level01TrialPhase.Loading);
            initialized = true;
        }

        public void Present(Level01TrialPhase phase)
        {
            if (!initialized) return;
            if (SameVisualPhase(visiblePhase, phase))
            {
                visiblePhase = phase;
                return;
            }
            if (routine != null) StopCoroutine(routine);
            if (visiblePhase == Level01TrialPhase.Loading)
            {
                ApplyRoots(phase);
                veil.alpha = 0f;
                veil.blocksRaycasts = false;
                visiblePhase = phase;
                return;
            }
            routine = StartCoroutine(TransitionTo(phase));
        }

        private IEnumerator TransitionTo(Level01TrialPhase phase)
        {
            veil.blocksRaycasts = true;
            var elapsed = 0f;
            var switched = false;
            while (elapsed < TransitionSeconds)
            {
                elapsed += Time.unscaledDeltaTime;
                var progress = Mathf.Clamp01(elapsed / TransitionSeconds);
                var half = progress < .5f ? progress * 2f : (1f - progress) * 2f;
                veil.alpha = half * half * (3f - 2f * half);
                veilImage.color = Color.Lerp(new Color(.015f, .08f, .1f, 1f),
                    new Color(.02f, .2f, .22f, 1f), Mathf.Sin(progress * Mathf.PI) * .35f);
                if (!switched && progress >= .5f)
                {
                    ApplyRoots(phase);
                    visiblePhase = phase;
                    switched = true;
                }
                yield return null;
            }
            if (!switched) ApplyRoots(phase);
            visiblePhase = phase;
            veil.alpha = 0f;
            veil.blocksRaycasts = false;
            routine = null;
        }

        private void EnsureVeil()
        {
            if (veil != null) return;
            var root = new GameObject("Level 1 Nautical Transition", typeof(RectTransform),
                typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster), typeof(CanvasGroup));
            root.transform.SetParent(transform, false);
            var canvas = root.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 32000;
            var scaler = root.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(720f, 1280f);
            veil = root.GetComponent<CanvasGroup>();
            veil.alpha = 0f;
            veil.blocksRaycasts = false;
            var panel = new GameObject("Deep Water Veil", typeof(RectTransform), typeof(Image));
            panel.transform.SetParent(root.transform, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            veilImage = panel.GetComponent<Image>();
            veilImage.color = new Color(.015f, .08f, .1f, 1f);
        }

        private void ApplyRoots(Level01TrialPhase phase)
        {
            SetActive(opening, phase == Level01TrialPhase.Opening);
            SetActive(traversal, phase == Level01TrialPhase.Traversal);
            SetActive(landing, phase == Level01TrialPhase.Landing);
            SetActive(assault, phase == Level01TrialPhase.Assault || phase == Level01TrialPhase.Failure);
            SetActive(victory, phase == Level01TrialPhase.Victory);
        }

        private static bool SameVisualPhase(Level01TrialPhase first, Level01TrialPhase second)
        {
            if (first == second) return true;
            return (first == Level01TrialPhase.Assault || first == Level01TrialPhase.Failure) &&
                (second == Level01TrialPhase.Assault || second == Level01TrialPhase.Failure);
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active) target.SetActive(active);
        }
    }
}
