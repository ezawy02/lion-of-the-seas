using NUnit.Framework;
using Unity.Mathematics;
using SeaLion.Crowd.Benchmark;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SeaLion.Tests.EditMode.Crowd
{
    public sealed class BenchmarkStressControllerTests
    {
        [Test]
        public void LayoutIsDeterministicAndDistinctAcrossAgents()
        {
            var a = BenchmarkStressController.PositionFor(17, 300, 2701u, 1.35f);
            var b = BenchmarkStressController.PositionFor(17, 300, 2701u, 1.35f);
            var c = BenchmarkStressController.PositionFor(18, 300, 2701u, 1.35f);
            Assert.That(math.distance(a, b), Is.EqualTo(0f));
            Assert.That(math.distance(a, c), Is.GreaterThan(0.01f));
        }

        [Test]
        public void BenchmarkSceneOpensWithHarnessCameraAndLight()
        {
            const string path = "Assets/_Project/Scenes/Benchmark_Stress.unity";
            var scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            try
            {
                Assert.That(scene.IsValid() && scene.isLoaded, Is.True);
                var roots = scene.GetRootGameObjects();
                Assert.That(roots, Has.Length.EqualTo(3));
                Assert.That(roots[0].GetComponent<BenchmarkStressController>(), Is.Not.Null);
                Assert.That(roots[1].GetComponent<UnityEngine.Camera>(), Is.Not.Null);
                Assert.That(roots[2].GetComponent<UnityEngine.Light>(), Is.Not.Null);
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
