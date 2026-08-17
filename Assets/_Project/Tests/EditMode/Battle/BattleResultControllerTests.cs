using System;
using NUnit.Framework;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Results;

namespace SeaLion.Tests.EditMode.Battle
{
    public sealed class BattleResultControllerTests
    {
        [Test]
        public void TerminalResultIsConsumedOnceAndOldSessionIsDetachedOnRetry()
        {
            var first = CreateActiveSession();
            var second = CreateActiveSession();
            var created = 0;
            var received = 0;
            using (var controller = new BattleResultController(first, () => { created++; return second; }))
            {
                controller.TerminalResultReceived += _ => received++;
                Assert.That(first.End(true, "won"), Is.True);
                Assert.That(controller.HasTerminalResult, Is.True);
                Assert.That(controller.TerminalEventCount, Is.EqualTo(1));
                Assert.That(received, Is.EqualTo(1));
                Assert.That(controller.TryRetry(out var retried), Is.True);
                Assert.That(retried, Is.SameAs(second));
                Assert.That(created, Is.EqualTo(1));
                Assert.That(controller.HasTerminalResult, Is.False);
                Assert.That(controller.TerminalResult, Is.Null);
                Assert.That(first.End(false, "stale"), Is.False);
                Assert.That(second.End(false, "lost"), Is.True);
                Assert.That(received, Is.EqualTo(2));
                Assert.That(controller.TerminalEventCount, Is.EqualTo(1));
            }
        }

        [Test]
        public void RetryClearsRuntimeAndRecordsSubThreeSecondBudget()
        {
            var cleared = 0;
            var first = CreateActiveSession();
            var second = CreateActiveSession();
            using (var controller = new BattleResultController(first, () => second, () => cleared++))
            {
                Assert.That(first.End(false, "lost"), Is.True);
                Assert.That(controller.TryRetry(), Is.True);
                Assert.That(cleared, Is.EqualTo(1));
                Assert.That(controller.RetryCount, Is.EqualTo(1));
                Assert.That(controller.LastRetryDurationSeconds, Is.GreaterThanOrEqualTo(0d));
                Assert.That(controller.LastRetryDurationSeconds, Is.LessThan(3d));
                Assert.That(controller.LastRetryWithinBudget, Is.True);
            }
        }

        [Test]
        public void RetryBeforeTerminalIsRejectedWithoutClearingRuntime()
        {
            var cleared = 0;
            using (var controller = new BattleResultController(CreateActiveSession(), CreateActiveSession, () => cleared++))
            {
                Assert.That(controller.TryRetry(), Is.False);
                Assert.That(cleared, Is.Zero);
                Assert.That(controller.RetryCount, Is.Zero);
            }
        }

        private static BattleSession CreateActiveSession()
        {
            var id = new StableId("test-session-" + Guid.NewGuid().ToString("N"));
            var session = new BattleSession(id, id, new LoadoutSnapshot(id, id, id));
            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);
            return session;
        }
    }
}
