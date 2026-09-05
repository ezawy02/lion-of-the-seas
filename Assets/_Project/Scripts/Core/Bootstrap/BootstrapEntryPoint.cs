using System.IO;
using SeaLion.Core.Persistence;
using SeaLion.Presentation.Quality;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeaLion.Core.Bootstrap
{
    public sealed class BootstrapEntryPoint : MonoBehaviour, ISceneTransition, IQualitySelector
    {
        [SerializeField] private string frontendScene = "Frontend";
        [SerializeField] private string directLevelScene;
        [SerializeField] private QualityProfileController qualityController;
        private BootstrapComposition composition;
        private BootstrapLoadingOverlay loadingOverlay;

        public BootstrapComposition Composition => composition;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            if (qualityController == null) qualityController = GetComponent<QualityProfileController>();
            loadingOverlay = GetComponent<BootstrapLoadingOverlay>();
            if (loadingOverlay == null) loadingOverlay = gameObject.AddComponent<BootstrapLoadingOverlay>();
            loadingOverlay.Begin(directLevelScene);
            var path = Path.Combine(Application.persistentDataPath, LocalSaveRepository.DefaultFileName);
            composition = new BootstrapComposition(new LocalSaveRepository(path), this, this);
            composition.Start(frontendScene, directLevelScene);
        }

        public bool TryLoad(string sceneId)
        {
            if (string.IsNullOrEmpty(sceneId))
            {
                loadingOverlay?.MarkReady();
                return false;
            }
            for (var index = 0; index < SceneManager.sceneCountInBuildSettings; index++)
            {
                if (Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(index)) != sceneId)
                    continue;
                loadingOverlay?.Begin(sceneId);
                var operation = SceneManager.LoadSceneAsync(index);
                if (operation == null)
                {
                    loadingOverlay?.MarkReady();
                    return false;
                }
                return true;
            }
            loadingOverlay?.MarkReady();
            return false;
        }

        public void Apply(string preference)
        {
            if (qualityController == null) return;
            if (!System.Enum.TryParse(preference, true, out QualityPreference value))
                value = QualityPreference.Auto;
            qualityController.SetPreference(value);
        }

        public void Save() => composition?.Save();
    }
}
