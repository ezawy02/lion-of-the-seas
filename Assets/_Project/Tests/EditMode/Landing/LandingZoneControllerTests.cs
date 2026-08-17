using NUnit.Framework;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Crowd.Simulation;
using SeaLion.Gameplay.Deployment;
using SeaLion.Gameplay.Landing;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Landing
{
    public sealed class LandingZoneControllerTests
    {
        [Test]
        public void OutOfOrderArrivalsTransferInSequenceAndCompleteOnce()
        {
            var host = new GameObject("landing-zone-test");
            try
            {
                var session = ActiveSession();
                var force = new ForceRuntime(0, 100);
                var zone = host.AddComponent<LandingZoneController>();
                zone.Configure(session, force, new StableId("landing-zone-01"), 2);
                var later = new Craft();
                var first = new Craft();
                Assert.That(zone.TryAccept(later, 1, 3), Is.True);
                Assert.That(zone.IsStarted, Is.False);
                Assert.That(force.LogicalCount, Is.Zero);
                Assert.That(zone.TryAccept(first, 0, 2), Is.True);
                Assert.That(force.LogicalCount, Is.EqualTo(5));
                Assert.That(force.RoleCounts[UnitRole.Sailor], Is.EqualTo(5));
                Assert.That(zone.IsCompleted, Is.True);
                Assert.That(session.Events.Events[2].Type, Is.EqualTo(BattleEventType.LandingStarted));
                Assert.That(session.Events.Events[3].Type, Is.EqualTo(BattleEventType.LandingCompleted));
                Assert.That(zone.TryAccept(first, 0, 2), Is.False);
            }
            finally { Object.DestroyImmediate(host); }
        }

        [Test]
        public void DestroyedCraftResolvesWithoutContribution()
        {
            var host = new GameObject("landing-zone-destroyed-test");
            try
            {
                var force = new ForceRuntime(0, 100);
                var zone = host.AddComponent<LandingZoneController>();
                zone.Configure(ActiveSession(), force, new StableId("landing-zone-01"), 2);
                Assert.That(zone.NotifyCraftDestroyed(new Craft(), 0), Is.True);
                Assert.That(zone.TryAccept(new Craft(), 1, 4), Is.True);
                Assert.That(zone.TransferredContribution, Is.EqualTo(4));
                Assert.That(force.LogicalCount, Is.EqualTo(4));
                Assert.That(zone.IsCompleted, Is.True);
            }
            finally { Object.DestroyImmediate(host); }
        }

        [Test]
        public void DynamicBatchRequiresExplicitCompletionAndPauseRejectsArrival()
        {
            var host = new GameObject("landing-zone-dynamic-test");
            try
            {
                var zone = host.AddComponent<LandingZoneController>();
                zone.Configure(ActiveSession(), new ForceRuntime(0, 100), new StableId("landing-zone-01"));
                zone.SetPaused(true);
                Assert.That(zone.TryAccept(new Craft(), 0, 2), Is.False);
                zone.SetPaused(false);
                Assert.That(zone.TryAccept(new Craft(), 0, 2), Is.True);
                Assert.That(zone.IsCompleted, Is.False);
                zone.Complete();
                Assert.That(zone.IsCompleted, Is.True);
            }
            finally { Object.DestroyImmediate(host); }
        }

        private static BattleSession ActiveSession()
        {
            var session = new BattleSession(new StableId("level-01"), new StableId("landing"), default);
            session.TryTransition(BattleState.Ready);
            session.TryTransition(BattleState.Active);
            return session;
        }

        private sealed class Craft : ILandingCraft
        {
            public void Activate(Vector3 position, int contribution, int sequence) { }
            public void Deactivate() { }
        }
    }
}
