using NUnit.Framework;
using SeaLion.Gameplay.Levels;
using SeaLion.Presentation.Levels;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Presentation
{
    public sealed class Level01SharedHorizonPresenterTests
    {
        [Test]
        public void OpeningCoastStaysVisibleAfterTraversalHidesTheOpeningRoot()
        {
            var opening = new GameObject("PHASE__Opening");
            var cliff = new GameObject("ENV__LeftCoastalCliff");
            cliff.transform.SetParent(opening.transform);
            var wall = new GameObject("CITY__FortressWall");
            wall.transform.SetParent(opening.transform);
            var traversal = new GameObject("PHASE__Traversal");
            var duplicate = new GameObject("ENV__LeftCoastalCliff");
            duplicate.transform.SetParent(traversal.transform);
            var host = new GameObject("horizon-host");
            var presenter = host.AddComponent<Level01SharedHorizonPresenter>();
            try
            {
                presenter.Bind(opening, traversal);
                Assert.That(presenter.SharedCount, Is.EqualTo(2));
                opening.SetActive(false);
                traversal.SetActive(true);
                presenter.Present(Level01TrialPhase.Traversal);
                Assert.That(Level01SharedHorizonPresenter.ShowsSeaHorizon(Level01TrialPhase.Traversal),
                    Is.True);
                Assert.That(cliff.activeInHierarchy, Is.True);
                Assert.That(wall.activeInHierarchy, Is.True);
                Assert.That(duplicate.activeSelf, Is.False);
                presenter.Present(Level01TrialPhase.Landing);
                Assert.That(presenter.IsSeaHorizonVisible, Is.False);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(opening);
                Object.DestroyImmediate(traversal);
            }
        }
    }
}
