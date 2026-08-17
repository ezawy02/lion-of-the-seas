using System.IO;
using SeaLion.Core.Persistence;
using SeaLion.Presentation.Quality;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeaLion.Core.Bootstrap
{
    public sealed class BootstrapEntryPoint : MonoBehaviour, ISceneTransition, IQualitySelector
    {
        [SerializeField] private string frontendScene = "Frontend"; [SerializeField] private string directLevelScene; [SerializeField] private QualityProfileController qualityController;
        private BootstrapComposition composition; public BootstrapComposition Composition => composition;
        private void Awake()
        { DontDestroyOnLoad(gameObject); if (qualityController == null) qualityController = GetComponent<QualityProfileController>(); var path = Path.Combine(Application.persistentDataPath, "player-save.json"); composition = new BootstrapComposition(new LocalSaveRepository(path), this, this); composition.Start(frontendScene, directLevelScene); }
        public bool TryLoad(string sceneId)
        { if (string.IsNullOrEmpty(sceneId)) return false; for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++) if (Path.GetFileNameWithoutExtension(SceneUtility.GetScenePathByBuildIndex(i)) == sceneId) { SceneManager.LoadSceneAsync(i); return true; } return false; }
        public void Apply(string preference)
        { if (qualityController == null) return; QualityPreference value; if (!System.Enum.TryParse(preference, true, out value)) value = QualityPreference.Auto; qualityController.SetPreference(value); }
        public void Save() => composition?.Save();
    }
}
