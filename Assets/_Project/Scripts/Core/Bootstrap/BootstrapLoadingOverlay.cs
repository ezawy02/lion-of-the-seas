using SeaLion.Presentation.Levels;
using SeaLion.UI.Localization;
using UnityEngine;

namespace SeaLion.Core.Bootstrap
{
    /// <summary>Immediate local loading cover that remains until Level 1 art is actually bound.</summary>
    [DisallowMultipleComponent]
    public sealed class BootstrapLoadingOverlay : MonoBehaviour
    {
        private string targetScene;
        private float alpha;
        private bool completing;
        private Camera loadingCamera;
        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private GUIStyle progressStyle;

        public bool Visible => alpha > .001f;
        public bool IsCompleting => completing;
        public string TargetScene => targetScene ?? string.Empty;

        private void Awake() { UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnSceneLoaded; }
        private void OnDestroy() { UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnSceneLoaded; }
        private void OnSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
        {
            if (mode == UnityEngine.SceneManagement.LoadSceneMode.Single && scene.name.EndsWith("_Playable_Trial"))
                Begin(scene.name);
        }

        public void Begin(string sceneName)
        {
            targetScene = sceneName ?? string.Empty;
            alpha = string.IsNullOrEmpty(targetScene) ? 0f : 1f;
            completing = false;
            enabled = alpha > 0f;
            if (enabled) EnsureCamera();
        }

        public void MarkReady()
        {
            if (alpha > 0f) completing = true;
        }

        private void Update()
        {
            if (!completing)
            {
                var presenter = FindFirstObjectByType<Level01TrialScenePresenter>(FindObjectsInactive.Include);
                if (presenter != null && presenter.IsReady) MarkReady();
                var campaign = FindFirstObjectByType<CampaignScenePresenter>(FindObjectsInactive.Include);
                if (campaign != null && campaign.IsReady) MarkReady();
            }
            if (!completing) return;
            alpha = Mathf.MoveTowards(alpha, 0f, Time.unscaledDeltaTime * 2.5f);
            if (alpha <= .001f)
            {
                if (loadingCamera != null) loadingCamera.enabled = false;
                enabled = false;
            }
        }

        private void OnGUI()
        {
            if (!Visible) return;
            EnsureStyles();
            var previous = GUI.color;
            GUI.color = new Color(.012f, .07f, .09f, alpha);
            GUI.DrawTexture(new Rect(0f, 0f, Screen.width, Screen.height), Texture2D.whiteTexture);
            var scale = Mathf.Min(Screen.width / 720f, Screen.height / 1280f);
            var centerY = Screen.height * .45f;
            GUI.color = new Color(1f, 1f, 1f, alpha);
            GUI.Label(new Rect(0f, centerY - 80f * scale, Screen.width, 64f * scale),
                "LION OF THE SEAS", titleStyle);
            GUI.Label(new Rect(0f, centerY - 20f * scale, Screen.width, 44f * scale),
                ArabicTextShaper.Shape("أسد البحار"), subtitleStyle);
            var dots = new string('•', 1 + Mathf.FloorToInt(Time.realtimeSinceStartup * 2f) % 4);
            GUI.Label(new Rect(0f, centerY + 58f * scale, Screen.width, 36f * scale),
                "PREPARING THE FLEET  " + dots, progressStyle);
            GUI.color = previous;
        }

        private void EnsureStyles()
        {
            if (titleStyle != null) return;
            titleStyle = CreateStyle(34, new Color(.98f, .72f, .24f), FontStyle.Bold);
            subtitleStyle = CreateStyle(26, new Color(.72f, .9f, .91f), FontStyle.Normal);
            progressStyle = CreateStyle(14, new Color(.58f, .78f, .8f), FontStyle.Normal);
        }

        private void EnsureCamera()
        {
            if (loadingCamera == null)
            {
                var cameraObject = new GameObject("CAMERA__BootstrapLoading");
                cameraObject.transform.SetParent(transform, false);
                loadingCamera = cameraObject.AddComponent<Camera>();
            }
            loadingCamera.clearFlags = CameraClearFlags.SolidColor;
            loadingCamera.backgroundColor = new Color(.012f, .07f, .09f, 1f);
            loadingCamera.cullingMask = 0;
            loadingCamera.depth = -100f;
            loadingCamera.enabled = true;
        }

        private static GUIStyle CreateStyle(int size, Color color, FontStyle style)
        {
            return new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = size,
                fontStyle = style,
                normal = { textColor = color }
            };
        }
    }
}
