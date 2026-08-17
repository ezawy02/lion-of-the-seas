using NUnit.Framework;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.UI.Battle;
using System;

namespace SeaLion.Tests.EditMode.UI.Battle
{
    public sealed class BattleUiPresentersTests
    {
        private static BattleEvent Event(BattleEventType type, int before = 0, int after = 0, GateOutcome outcome = GateOutcome.Add, BattleResult result = default)
        { return new BattleEvent(1, type, new BattleEventPayload(Guid.NewGuid(), default, default, Allegiance.Friendly, before, after, 0, outcome, result)); }

        [Test] public void ForceCountRejectsInvalidProjectionAndConsumesForceEvent()
        { var p = new ForceCountPresenter(4, 2); Assert.Throws<ArgumentOutOfRangeException>(() => p.Set(1, 2)); p.Handle(Event(BattleEventType.ForceChanged, 4, 7)); Assert.AreEqual(7, p.View.Logical); }
        [Test] public void GateResultIsHiddenUntilGateAndCanClear()
        { var p = new GateResultPresenter(); Assert.IsFalse(p.View.Visible); p.Handle(Event(BattleEventType.GateResolved, 8, 4, GateOutcome.Damage)); Assert.AreEqual(4, p.View.After); p.Clear(); Assert.IsFalse(p.View.Visible); }
        [Test] public void BossHealthClampsNormalizedProjectionToContract()
        { var p = new BossHealthPresenter(); p.Set(25, 100); Assert.AreEqual(.25f, p.View.Normalized, .0001f); Assert.Throws<ArgumentOutOfRangeException>(() => p.Set(101, 100)); }
        [Test] public void AbilityAndResultExposeDeterministicState()
        { var a = new AbilityPlaceholderPresenter("Powder"); a.Handle(Event(BattleEventType.AbilityActivated)); Assert.IsFalse(a.View.Ready); Assert.IsTrue(a.View.Active); var r = new BattleResultPresenter(); r.Handle(Event(BattleEventType.BattleEnded, result: new BattleResult(true, "Guardian defeated"))); Assert.IsTrue(r.View.Victory); Assert.AreEqual("Guardian defeated", r.View.Reason); }
    }
}
