using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaLion.Tests.EditMode.Art
{
    public sealed class Level01PremiumRenderingTests
    {
        [Test]
        public void ProjectUsesUniversalRenderPipeline()
        {
            Assert.That(GraphicsSettings.defaultRenderPipeline, Is.Not.Null);
            Assert.That(GraphicsSettings.defaultRenderPipeline.GetType().Name,
                Is.EqualTo("UniversalRenderPipelineAsset"));
        }

        [Test]
        public void ReferenceMatchFlagshipUsesAuthoredReferenceMaterial()
        {
            const string path = "Assets/_Project/Materials/Imported/" +
                "L01-SHP-004_Hero_Flagship_ReferenceMatch.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            Assert.That(material, Is.Not.Null, path);
            Assert.That(material.shader.name, Is.EqualTo("Sea Lion/Art/Reference Lit"));
            Assert.That(material.GetTexture("_BaseMap"), Is.Not.Null);
            Assert.That(material.HasProperty("_Saturation"), Is.True);
            Assert.That(material.HasProperty("_Contrast"), Is.True);
            Assert.That(material.HasProperty("_ColorBoost"), Is.True);
            Assert.That(material.HasProperty("_LightResponse"), Is.True);
            Assert.That(material.HasProperty("_MetallicGlossMap"), Is.False,
                "The generated metallic mask must stay disabled because it double-lights baked highlights.");
        }

        [Test]
        public void WaterUsesTheAuthoredUniversalShader()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(
                "Assets/_Project/Materials/Water/SeaLion_Water_Primary.mat");
            Assert.That(material, Is.Not.Null);
            Assert.That(material.shader.name, Is.EqualTo("Sea Lion/Water/Styled Mobile"));
        }
    }
}
