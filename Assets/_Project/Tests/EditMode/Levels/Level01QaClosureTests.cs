using NUnit.Framework;
using SeaLion.Core.Bootstrap;
using SeaLion.Gameplay.Levels;
using SeaLion.Presentation.Levels;
using SeaLion.UI.Levels;
using SeaLion.UI.Localization;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Levels
{
    public sealed class Level01QaClosureTests
    {
        [Test]
        public void CrowdBudgetTracksAuthoritativeFriendlyAndHostileCounts()
        {
            Assert.That(Level01CrowdPresentationBudget.FriendlyVisibleCount(180, 120, 131, 1f), Is.EqualTo(120));
            Assert.That(Level01CrowdPresentationBudget.HostileVisibleCount(8, 8, 122, 1f), Is.EqualTo(122));
            Assert.That(Level01CrowdPresentationBudget.HostileVisibleCount(4, 8, 122, 1f), Is.EqualTo(61));
            Assert.That(Level01CrowdPresentationBudget.HostileVisibleCount(0, 8, 122, 1f), Is.Zero);
        }

        [Test]
        public void ReducedCrowdBudgetPreservesEndpointsAndSpreadsSelection()
        {
            Assert.That(Level01CrowdPresentationBudget.FriendlyVisibleCount(120, 120, 131, .5f), Is.EqualTo(60));
            Assert.That(Level01CrowdPresentationBudget.SourceIndex(0, 4, 100), Is.Zero);
            Assert.That(Level01CrowdPresentationBudget.SourceIndex(3, 4, 100), Is.EqualTo(99));
            Assert.That(Level01CrowdPresentationBudget.CommandedMarch(4.2f, 0f), Is.Zero);
            Assert.That(Level01CrowdPresentationBudget.CommandedMarch(4.2f, 0.5f), Is.EqualTo(2.1f).Within(0.0001f));
            Assert.That(Level01CrowdPresentationBudget.CommandedMarch(4.2f, float.NaN), Is.Zero);
        }

        [Test]
        public void TraversalBoundsKeepTheCompletePresentationInsidePortraitMargins()
        {
            var cameraObject = new GameObject("portrait-camera");
            var camera = cameraObject.AddComponent<Camera>();
            camera.aspect = 720f / 1280f;
            camera.fieldOfView = 39f;
            camera.transform.position = new Vector3(0f, 4f, -12f);
            var bounds = new Bounds(new Vector3(0f, 2f, 8f), new Vector3(5f, 5f, 6f));
            try
            {
                var range = Level01TraversalBounds.Calculate(camera, bounds, 0f, .055f);
                Assert.That(range.IsValid, Is.True);
                Assert.That(range.Left, Is.LessThan(range.Right));
                Assert.That(range.Left, Is.GreaterThan(-5f));
                Assert.That(range.Right, Is.LessThan(5f));
            }
            finally
            {
                Object.DestroyImmediate(cameraObject);
            }
        }

        [Test]
        public void PhaseCameraMovesCloserAsObjectivesAdvance()
        {
            var opening = Level01PhaseCameraPresenter.PresetFor(Level01TrialPhase.Opening);
            var traversal = Level01PhaseCameraPresenter.PresetFor(Level01TrialPhase.Traversal);
            var landing = Level01PhaseCameraPresenter.PresetFor(Level01TrialPhase.Landing);
            var assault = Level01PhaseCameraPresenter.PresetFor(Level01TrialPhase.Assault);
            var victory = Level01PhaseCameraPresenter.PresetFor(Level01TrialPhase.Victory);

            Assert.That(traversal.Position.z, Is.GreaterThan(opening.Position.z));
            Assert.That(landing.Position.z, Is.GreaterThan(traversal.Position.z));
            Assert.That(assault.Position.z, Is.GreaterThan(landing.Position.z));
            Assert.That(victory.Position.z, Is.GreaterThan(assault.Position.z));
            Assert.That(assault.FieldOfView, Is.LessThan(opening.FieldOfView));
            Assert.That(victory.FieldOfView, Is.LessThanOrEqualTo(assault.FieldOfView));
            Assert.That(Level01PhaseCameraPresenter.FollowFactor(Level01TrialPhase.Traversal),
                Is.GreaterThan(.7f));
            Assert.That(Level01PhaseCameraPresenter.FollowFactor(Level01TrialPhase.Landing),
                Is.GreaterThan(Level01PhaseCameraPresenter.FollowFactor(Level01TrialPhase.Traversal)));
            Assert.That(Level01PhaseCameraPresenter.FollowFactor(Level01TrialPhase.Victory),
                Is.GreaterThan(0f));
            Assert.That(Level01PhaseCameraPresenter.CombatPushIn(0, 0f), Is.EqualTo(1f));
            Assert.That(Level01PhaseCameraPresenter.CombatPushIn(8, 1f), Is.Zero);
            Assert.That(Level01SeaMotion.ShoreBlend01(.4f, false), Is.Zero);
            Assert.That(Level01SeaMotion.ShoreBlend01(1f, true), Is.EqualTo(1f));
            Assert.That(Level01SeaMotion.RouteTravel(.4f), Is.EqualTo(48f).Within(.01f));
            Assert.That(Level01SeaMotion.RouteTravel(1f), Is.EqualTo(70f).Within(.01f));
            Assert.That(Level01SeaMotion.RouteTravel(.2f), Is.GreaterThan(0f));
            Assert.That(Level01SeaMotion.RouteTravel(.2f), Is.LessThan(48f));
        }

        [Test]
        public void MaritimeTravelMovesForwardWithSmoothEndpoints()
        {
            Assert.That(Level01SeaMotion.ForwardDistance(0f, 10f, 20f), Is.Zero);
            Assert.That(Level01SeaMotion.ForwardDistance(5f, 10f, 20f), Is.EqualTo(10f).Within(.001f));
            Assert.That(Level01SeaMotion.ForwardDistance(10f, 10f, 20f), Is.EqualTo(20f).Within(.001f));
            Assert.That(Level01SeaMotion.ForwardDistance(2f, 10f, 20f), Is.GreaterThan(0f));
            Assert.That(Level01SeaMotion.ForwardDistance(8f, 10f, 20f), Is.LessThan(20f));
        }

        [Test]
        public void RiskyGateAndFailureReasonsTellTheTruthInBothLanguages()
        {
            StringAssert.Contains("LOSE 1", Level01TrialLocalization.Get("gateRisk", GameLanguage.English));
            StringAssert.Contains("خسارة", Level01TrialLocalization.Get("gateRisk", GameLanguage.Arabic));
            Assert.That(Level01TrialLocalization.FailureKey("guardian-timeout"), Is.EqualTo("failureTimeout"));
            Assert.That(Level01TrialLocalization.FailureKey("force-depleted"), Is.EqualTo("failureDepleted"));
        }

        [Test]
        public void ArabicForceFormattingShapesTheWholeFractionAsOneRun()
        {
            var value = Level01TrialLocalization.FormatForce(83, 120, GameLanguage.Arabic);
            StringAssert.Contains("٨٣", value);
            StringAssert.Contains("١٢٠", value);
            StringAssert.Contains("/", value);
            Assert.That(value, Is.Not.EqualTo(Level01TrialLocalization.Display("force", GameLanguage.Arabic) + "  ٨٣ / ١٢٠"));
        }

        [Test]
        public void ControlHudCopyAndCooldownAreReadableInBothLanguages()
        {
            Assert.That(Level01TrialLocalization.Get("steer", GameLanguage.English), Does.Contain("DRAG"));
            Assert.That(Level01TrialLocalization.Get("steer", GameLanguage.Arabic), Does.Contain("اسحب"));
            Assert.That(Level01TrialLocalization.Get("holdBeach", GameLanguage.English), Does.Contain("HOLD"));
            Assert.That(Level01TrialLocalization.Get("holdBeach", GameLanguage.Arabic), Does.Contain("اثبت"));
            Assert.That(Level01TrialLocalization.Get("clearDefenders", GameLanguage.English), Does.Contain("CLEAR"));
            Assert.That(Level01TrialLocalization.Get("strikeGuardian", GameLanguage.English), Does.Contain("STRIKE"));
            Assert.That(Level01TrialLocalization.Get("engage", GameLanguage.Arabic), Does.Contain("اشتبك"));
            Assert.That(Level01TrialLocalization.Get("sailToShore", GameLanguage.English), Does.Contain("SAIL"));
            Assert.That(Level01TrialLocalization.Get("sailToShore", GameLanguage.Arabic), Does.Contain("أبحر"));
            Assert.That(Level01TrialLocalization.FormatPercent(.67f, GameLanguage.English), Is.EqualTo("67%"));
            Assert.That(Level01TrialLocalization.FormatPercent(.67f, GameLanguage.Arabic), Does.Contain("٦٧"));
        }

        [Test]
        public void JourneyHudFormatsForceGateAndResultInBothLanguages()
        {
            var landing = Level01TrialLocalization.FormatJourneyForce(32, 8, 2, 7, false, true,
                GameLanguage.English);
            Assert.That(landing, Does.Contain("32"));
            Assert.That(landing, Does.Contain("2/7"));
            var assault = Level01TrialLocalization.FormatJourneyForce(28, 5, 0, 0, true, false,
                GameLanguage.English, -3, true);
            Assert.That(assault, Does.Contain("−3"));
            Assert.That(assault, Does.Contain("ENEMY"));
            Assert.That(assault, Does.Contain("5"));
            var pending = Level01TrialLocalization.FormatJourneyGate(false, true, 0, 0,
                GameLanguage.English);
            Assert.That(pending, Does.Contain("SAFE"));
            var committed = Level01TrialLocalization.FormatJourneyGate(true, true, 8, 32,
                GameLanguage.English);
            Assert.That(committed, Does.Contain("8 → 32"));
            var arabicForce = Level01TrialLocalization.FormatJourneyForce(32, 8, 2, 7, false, true,
                GameLanguage.Arabic);
            Assert.That(arabicForce, Does.Contain("٣٢"));
            Assert.That(arabicForce, Does.Contain("٢"));
            Assert.That(arabicForce, Does.Contain("٧"));
            var result = Level01TrialLocalization.FormatResultBody(false, 0, 36, "force-depleted",
                GameLanguage.English);
            Assert.That(result, Does.Contain("depleted"));
            Assert.That(result, Does.Contain("PEAK"));
            Assert.That(result, Does.Contain("36"));
            var victory = Level01TrialLocalization.FormatResultBody(true, 22, 44, string.Empty,
                GameLanguage.Arabic);
            Assert.That(victory, Does.Contain("٢٢"));
            Assert.That(victory, Does.Contain("٤٤"));
        }

        [Test]
        public void ReferenceShaderExposesInstancingAndHighlightRecoveryControls()
        {
            var shader = Shader.Find("Sea Lion/Art/Reference Lit");
            Assert.That(shader, Is.Not.Null);
            var material = new Material(shader);
            try
            {
                Assert.That(material.HasProperty("_HighlightCompression"), Is.True);
                Assert.That(material.HasProperty("_HighlightTint"), Is.True);
            }
            finally
            {
                Object.DestroyImmediate(material);
            }
        }

        [Test]
        public void LoadingCoverWaitsForExplicitSceneReadiness()
        {
            var host = new GameObject("loading-cover-test");
            var overlay = host.AddComponent<BootstrapLoadingOverlay>();
            try
            {
                overlay.Begin("Level_01_Playable_Trial");
                Assert.That(overlay.Visible, Is.True);
                Assert.That(overlay.IsCompleting, Is.False);
                overlay.MarkReady();
                Assert.That(overlay.IsCompleting, Is.True);
            }
            finally
            {
                Object.DestroyImmediate(host);
            }
        }
    }
}
