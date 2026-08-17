using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Art
{
    public sealed class BenchmarkArtAssetTests
    {
        private static readonly string[] Required =
        {
            "Assets/_Project/Art/Ships/Flagship.fbx",
            "Assets/_Project/Art/Ships/Flagship_LOD1.fbx",
            "Assets/_Project/Art/Ships/Flagship_LOD2.fbx",
            "Assets/_Project/Art/Characters/FriendlyCrew.fbx",
            "Assets/_Project/Art/Characters/HostileEnemy.fbx",
            "Assets/_Project/Art/Characters/HarborGuardian.fbx",
            "Assets/_Project/Art/Characters/HarborGuardian_LOD1.fbx",
            "Assets/_Project/Art/Characters/HarborGuardian_LOD2.fbx",
            "Assets/_Project/Art/Environment/GateMultiplier.fbx",
            "Assets/_Project/Art/Environment/MediterraneanHarbor.fbx"
        };

        [Test]
        public void BenchmarkAssetsImportWithReadableGeometryAndMaterials()
        {
            foreach (var path in Required)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                Assert.That(asset, Is.Not.Null, path);
                var renderers = asset.GetComponentsInChildren<MeshRenderer>(true);
                Assert.That(renderers.Length, Is.GreaterThan(0), path);
                var triangles = 0;
                foreach (var filter in asset.GetComponentsInChildren<MeshFilter>(true))
                    if (filter.sharedMesh != null) triangles += filter.sharedMesh.triangles.Length / 3;
                Assert.That(triangles, Is.GreaterThan(24), path);
                Assert.That(renderers[0].sharedMaterials.Length, Is.GreaterThan(0), path);
            }
        }

        [Test]
        public void HeroLodsReduceGeometryMonotonically()
        {
            AssertLods("Assets/_Project/Art/Ships/Flagship");
            AssertLods("Assets/_Project/Art/Characters/HarborGuardian");
        }

        private static void AssertLods(string root)
        {
            var lod0 = Triangles(root + ".fbx");
            var lod1 = Triangles(root + "_LOD1.fbx");
            var lod2 = Triangles(root + "_LOD2.fbx");
            Assert.That(lod1, Is.LessThan(lod0), root);
            Assert.That(lod2, Is.LessThan(lod1), root);
        }

        private static int Triangles(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var total = 0;
            foreach (var filter in asset.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != null) total += filter.sharedMesh.triangles.Length / 3;
            return total;
        }
    }
}
