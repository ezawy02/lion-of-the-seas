using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Presentation.Quality;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Presentation.Quality
{
    public sealed class QualityProfileControllerTests
    {
        private GameObject host;
        private QualityProfile primary;
        private QualityProfile reduced;
        private QualityProfileController controller;
        private float previousLodBias;
        private float previousShadowDistance;

        [SetUp]
        public void SetUp()
        {
            previousLodBias = QualitySettings.lodBias;
            previousShadowDistance = QualitySettings.shadowDistance;
            primary = ScriptableObject.CreateInstance<QualityProfile>();
            reduced = ScriptableObject.CreateInstance<QualityProfile>();
            SetPrivate(primary, "profileKind", QualityProfileKind.Primary);
            SetPrivate(reduced, "profileKind", QualityProfileKind.Reduced);
            SetPrivate(primary, "lodBias", 1f);
            SetPrivate(reduced, "lodBias", .5f);
            host = new GameObject("quality-test");
            controller = host.AddComponent<QualityProfileController>();
            SetPrivate(controller, "primary", primary);
            SetPrivate(controller, "reduced", reduced);
            controller.Apply(primary);
        }

        [TearDown]
        public void TearDown()
        {
            QualitySettings.lodBias = previousLodBias;
            QualitySettings.shadowDistance = previousShadowDistance;
            Object.DestroyImmediate(host);
            Object.DestroyImmediate(primary);
            Object.DestroyImmediate(reduced);
        }

        [Test] public void ExplicitPreferenceAppliesRequestedProfile()
        {
            controller.SetPreference(QualityPreference.Reduced);
            Assert.That(controller.ActiveProfile, Is.SameAs(reduced));
            controller.SetPreference(QualityPreference.Primary);
            Assert.That(controller.ActiveProfile, Is.SameAs(primary));
        }

        [Test] public void AutoUsesHysteresisAndDoesNotFlicker()
        {
            controller.SetPreference(QualityPreference.Auto);
            controller.EvaluateFrameTime(.034f);
            Assert.That(controller.ActiveProfile, Is.SameAs(reduced));
            controller.EvaluateFrameTime(.030f);
            Assert.That(controller.ActiveProfile, Is.SameAs(reduced));
            controller.EvaluateFrameTime(.024f);
            Assert.That(controller.ActiveProfile, Is.SameAs(primary));
        }

        private static void SetPrivate(Object target, string field, object value)
        {
            var info = target.GetType().GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            info.SetValue(target, value);
        }
    }
}
