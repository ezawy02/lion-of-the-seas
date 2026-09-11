using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Scenes
{
    public sealed class Level01PlayableCompositionTests
    {
        private const string Path = "Assets/_Project/Scenes/Level_01_HundredSails.unity";

        [Test]
        public void AuthoredCastUsesReadableScaleAndForwardComposition()
        {
            var scene = EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive);
            try
            {
                var nodes = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .ToArray();
                var flagship = Largest(nodes, "PLAYER__Flagship");
                var gate = First(nodes, "GATE__Multiplier_x4");
                var rescue = First(nodes, "RESCUE__CaptiveSailmakers");
                var landing = First(nodes, "FRIENDLY__LandingForce");
                var guardian = First(nodes, "BOSS__HarborGuardian");
                var marine = First(nodes, "FRIENDLY__LandingForce_Front");
                var hostile = First(nodes, "HOSTILE__Defenders_Front");

                AssertReadableScale(flagship, 5f, 20f);
                AssertReadableScale(gate, 3.5f, 16f);
                AssertReadableScale(guardian, 2f, 14f);
                AssertReadableScale(rescue, 1.2f, 12f);
                Assert.That(marine, Is.Not.Null);
                Assert.That(hostile, Is.Not.Null);
                Assert.That(flagship.position.z, Is.LessThan(gate.position.z));
                Assert.That(gate.position.z, Is.LessThan(landing.position.z));
                Assert.That(landing.position.z, Is.LessThanOrEqualTo(guardian.position.z + 8f));
                Assert.That(guardian.position.z - flagship.position.z, Is.GreaterThan(40f));
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        private static Transform First(Transform[] nodes, string name)
        {
            return nodes.FirstOrDefault(node => node.name == name);
        }

        private static Transform Largest(Transform[] nodes, string name)
        {
            Transform chosen = null;
            var best = 0f;
            for (var index = 0; index < nodes.Length; index++)
            {
                if (nodes[index].name != name) continue;
                var size = nodes[index].lossyScale.magnitude;
                if (size <= best) continue;
                best = size;
                chosen = nodes[index];
            }
            return chosen;
        }

        private static void AssertReadableScale(Transform target, float min, float max)
        {
            Assert.That(target, Is.Not.Null);
            var scale = target.lossyScale.x;
            Assert.That(scale, Is.GreaterThan(min), target.name);
            Assert.That(scale, Is.LessThan(max), target.name);
        }
    }
}
