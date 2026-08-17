using NUnit.Framework;
using SeaLion.Core.Bootstrap;
using SeaLion.Core.Persistence;
using SeaLion.Presentation.Quality;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SeaLion.Tests.EditMode
{
    public sealed class BootstrapCompositionTests
    {
        private sealed class Files : ILocalSaveFileSystem
        { public string Value; public bool Exists(string p) => Value != null; public string ReadAllText(string p) => Value; public void WriteAllText(string p, string c) => Value = c; public void Replace(string t, string d, string b) { } public void Delete(string p) { } }
        private sealed class Scenes : ISceneTransition { public string Loaded; public bool TryLoad(string id) { Loaded = id; return true; } }
        private sealed class Quality : IQualitySelector { public string Value; public void Apply(string value) => Value = value; }
        [Test] public void StartLoadsSaveQualityAndFrontend()
        { var scenes = new Scenes(); var quality = new Quality(); var composition = new BootstrapComposition(new LocalSaveRepository("save", new Files()), scenes, quality); Assert.IsTrue(composition.Start("Frontend", null)); Assert.AreEqual("Frontend", scenes.Loaded); Assert.AreEqual("Auto", quality.Value); Assert.NotNull(composition.Player); }
        [Test] public void DirectLaunchWinsAndSessionStartsReady()
        { var scenes = new Scenes(); var composition = new BootstrapComposition(new LocalSaveRepository("save", new Files()), scenes, new Quality()); composition.Start("Frontend", "Level_01_HundredSails"); Assert.AreEqual("Level_01_HundredSails", scenes.Loaded); Assert.IsTrue(composition.TryCreateSession("level-01", "opening", out var session)); Assert.AreEqual(SeaLion.Core.Battle.BattleState.Ready, session.State); }

        [Test] public void BootstrapAndFrontendScenesOpenWithRequiredComposition()
        {
            var bootstrap = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Bootstrap.unity", OpenSceneMode.Additive);
            var frontend = EditorSceneManager.OpenScene("Assets/_Project/Scenes/Frontend.unity", OpenSceneMode.Additive);
            try
            {
                var root = bootstrap.GetRootGameObjects()[0];
                Assert.NotNull(root.GetComponent<BootstrapEntryPoint>());
                Assert.NotNull(root.GetComponent<QualityProfileController>());
                Assert.NotNull(frontend.GetRootGameObjects()[1].GetComponent<UnityEngine.Camera>());
            }
            finally
            {
                EditorSceneManager.CloseScene(frontend, true);
                EditorSceneManager.CloseScene(bootstrap, true);
            }
        }
    }
}
