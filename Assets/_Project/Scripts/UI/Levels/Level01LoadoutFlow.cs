using System.IO;
using SeaLion.Core.Persistence;
using SeaLion.Gameplay.Levels;
using SeaLion.UI.Loadout;
using UnityEngine;

namespace SeaLion.UI.Levels
{
    /// <summary>Connects the authored loadout screen to the terminal battle and next attempt.</summary>
    public sealed class Level01LoadoutFlow : MonoBehaviour
    {
        [SerializeField] private GameObject loadoutPrefab;
        private GameObject screen;
        private LoadoutScreenController controller;
        private LoadoutScreenPresenter presenter;
        private Level01TrialRuntime runtime;

        public void Open()
        {
            runtime = GetComponent<Level01TrialRuntime>();
            if (runtime == null || !runtime.CanRetry || loadoutPrefab == null) return;
            if (screen == null)
            {
                screen = Instantiate(loadoutPrefab);
                controller = screen.GetComponentInChildren<LoadoutScreenController>(true);
                presenter = screen.GetComponentInChildren<LoadoutScreenPresenter>(true);
                if (controller == null || presenter == null)
                {
                    Debug.LogError("The loadout screen is missing its controller or presenter.", this);
                    Destroy(screen); return;
                }
                controller.Initialize(new LocalSaveRepository(Path.Combine(Application.persistentDataPath,
                    runtime.SaveFileName)), runtime.Flagships, runtime.CrewRoles, runtime.CaptainAbilities);
                foreach (var canvas in screen.GetComponentsInChildren<Canvas>(true))
                { canvas.renderMode = RenderMode.ScreenSpaceOverlay; canvas.sortingOrder = 200; }
                presenter.Confirmed += Replay;
            }
            controller.Refresh();
            screen.SetActive(true);
            presenter.SetLanguage(Level01TrialLocalization.LoadLanguage(), false);
        }

        private void Replay()
        {
            if (runtime == null || !runtime.CanRetry) return;
            screen.SetActive(false);
            runtime.Retry();
        }

        private void OnDestroy()
        {
            if (presenter != null) presenter.Confirmed -= Replay;
            if (screen != null) Destroy(screen);
        }
    }
}
