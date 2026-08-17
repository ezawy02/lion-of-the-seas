using NUnit.Framework;
using UnityEngine;
using SeaLion.Crowd.Rendering;

namespace SeaLion.Tests.EditMode.Crowd
{
    public sealed class InstancedCrowdRendererTests
    {
        [TestCase(0f, 0f)]
        [TestCase(0.25f, 0.25f)]
        [TestCase(1f, 0f)]
        [TestCase(2.75f, 0.75f)]
        [TestCase(-0.25f, 0.75f)]
        public void AnimationPhaseWrapsToUnitInterval(float input, float expected)
        {
            Assert.That(InstancedCrowdRenderer.NormalizePhase(input), Is.EqualTo(expected).Within(0.0001f));
        }

        [Test]
        public void InvalidAnimationPhasesFallBackToZero()
        {
            Assert.That(InstancedCrowdRenderer.NormalizePhase(float.NaN), Is.Zero);
            Assert.That(InstancedCrowdRenderer.NormalizePhase(float.PositiveInfinity), Is.Zero);
        }

        [Test]
        public void ConfigureUsesSharedMeshAndMaterial()
        {
            var owner = new GameObject("InstancedCrowdRendererTest");
            var mesh = new Mesh();
            var shader = Shader.Find("Hidden/InternalErrorShader");
            var material = shader == null ? null : new Material(shader);
            try
            {
                var renderer = owner.AddComponent<InstancedCrowdRenderer>();
                renderer.Configure(mesh, material);
                Assert.That(renderer.Mesh, Is.SameAs(mesh));
                Assert.That(renderer.Material, Is.SameAs(material));
                Assert.That(InstancedCrowdRenderer.MaxInstancesPerBatch, Is.EqualTo(1023));
            }
            finally
            {
                if (material != null) Object.DestroyImmediate(material);
                Object.DestroyImmediate(mesh);
                Object.DestroyImmediate(owner);
            }
        }
    }
}
