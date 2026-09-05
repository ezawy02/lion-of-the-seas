using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Art
{
    public sealed class BenchmarkArtAssetTests
    {
        private static readonly string[] Required =
        {
            "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2_Optimized_REVIEW.fbx",
            "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2_LOD1_REVIEW.fbx",
            "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2_LOD2_REVIEW.fbx",
            "Assets/_Project/Art/Characters/FriendlyCrew.fbx",
            "Assets/_Project/Art/Characters/HostileEnemy.fbx",
            "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized.fbx",
            "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized_LOD1_REVIEW.fbx",
            "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized_LOD2_REVIEW.fbx",
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
                var renderers = asset.GetComponentsInChildren<Renderer>(true);
                Assert.That(renderers.Length, Is.GreaterThan(0), path);
                var triangles = 0;
                triangles = Triangles(path);
                Assert.That(triangles, Is.GreaterThan(24), path);
                Assert.That(renderers[0].sharedMaterials.Length, Is.GreaterThan(0), path);
            }
        }

        [Test]
        public void HeroLodsReduceGeometryMonotonically()
        {
            AssertLods(
                "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2_Optimized_REVIEW.fbx",
                "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2_LOD1_REVIEW.fbx",
                "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2_LOD2_REVIEW.fbx");
            AssertLods(
                "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized.fbx",
                "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized_LOD1_REVIEW.fbx",
                "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized_LOD2_REVIEW.fbx");
        }

        private static void AssertLods(string lod0Path, string lod1Path, string lod2Path)
        {
            var lod0 = Triangles(lod0Path);
            var lod1 = Triangles(lod1Path);
            var lod2 = Triangles(lod2Path);
            Assert.That(lod1, Is.LessThan(lod0), lod0Path);
            Assert.That(lod2, Is.LessThan(lod1), lod0Path);
            Assert.That((float)lod1 / lod0, Is.InRange(0.5f, 0.66f), lod1Path);
            Assert.That((float)lod2 / lod0, Is.InRange(0.2f, 0.36f), lod2Path);
        }

        private static int Triangles(string path)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var total = 0;
            foreach (var filter in asset.GetComponentsInChildren<MeshFilter>(true))
                if (filter.sharedMesh != null) total += filter.sharedMesh.triangles.Length / 3;
            foreach (var renderer in asset.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                if (renderer.sharedMesh != null) total += renderer.sharedMesh.triangles.Length / 3;
            return total;
        }
    }
}
