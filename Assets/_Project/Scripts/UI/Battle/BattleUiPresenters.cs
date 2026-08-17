using System;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;

namespace SeaLion.UI.Battle
{
    public readonly struct ForceCountView
    {
        public readonly int Logical; public readonly int Displayed;
        public ForceCountView(int logical, int displayed) { Logical = logical; Displayed = displayed; }
    }

    public readonly struct GateResultView
    {
        public readonly bool Visible; public readonly int Before, After, Displayed;
        public readonly GateOutcome Outcome;
        public GateResultView(bool visible, int before, int after, int displayed, GateOutcome outcome)
        { Visible = visible; Before = before; After = after; Displayed = displayed; Outcome = outcome; }
    }

    public readonly struct BossHealthView
    {
        public readonly bool Visible; public readonly int Current, Maximum;
        public float Normalized { get { return Maximum == 0 ? 0f : (float)Current / Maximum; } }
        public BossHealthView(bool visible, int current, int maximum) { Visible = visible; Current = current; Maximum = maximum; }
    }

    public readonly struct AbilityPlaceholderView
    {
        public readonly bool Ready, Active; public readonly string Label;
        public AbilityPlaceholderView(bool ready, bool active, string label) { Ready = ready; Active = active; Label = label ?? string.Empty; }
    }

    public readonly struct BattleResultView
    {
        public readonly bool Visible, Victory; public readonly string Reason;
        public BattleResultView(bool visible, bool victory, string reason) { Visible = visible; Victory = victory; Reason = reason ?? string.Empty; }
    }

    public sealed class ForceCountPresenter
    {
        public ForceCountView View { get; private set; }
        public ForceCountPresenter(int logical = 0, int displayed = 0) { Set(logical, displayed); }
        public void Set(int logical, int displayed)
        { if (logical < 0 || displayed < 0 || displayed > logical) throw new ArgumentOutOfRangeException(); View = new ForceCountView(logical, displayed); }
        public void Handle(BattleEvent e)
        { if (e.Type == BattleEventType.ForceChanged) Set(e.Payload.After, e.Payload.After); }
    }

    public sealed class GateResultPresenter
    {
        public GateResultView View { get; private set; }
        public GateResultPresenter() { View = new GateResultView(false, 0, 0, 0, default); }
        public void Handle(BattleEvent e)
        { if (e.Type == BattleEventType.GateResolved) View = new GateResultView(true, e.Payload.Before, e.Payload.After, e.Payload.After, e.Payload.Outcome); }
        public void Clear() { View = new GateResultView(false, 0, 0, 0, default); }
    }

    public sealed class BossHealthPresenter
    {
        public BossHealthView View { get; private set; }
        public void Set(int current, int maximum)
        { if (maximum < 0 || current < 0 || current > maximum) throw new ArgumentOutOfRangeException(); View = new BossHealthView(true, current, maximum); }
        public void Handle(BattleEvent e)
        { if (e.Type == BattleEventType.BossPhaseChanged) Set(Math.Max(0, e.Payload.After), Math.Max(0, e.Payload.Before)); }
        public void Hide() { View = new BossHealthView(false, 0, 0); }
    }

    public sealed class AbilityPlaceholderPresenter
    {
        public AbilityPlaceholderView View { get; private set; }
        public AbilityPlaceholderPresenter(string label = "Ability") { View = new AbilityPlaceholderView(true, false, label); }
        public void SetReady(bool ready) { View = new AbilityPlaceholderView(ready, View.Active, View.Label); }
        public void Handle(BattleEvent e)
        { if (e.Type == BattleEventType.AbilityActivated) View = new AbilityPlaceholderView(false, true, View.Label); }
    }

    public sealed class BattleResultPresenter
    {
        public BattleResultView View { get; private set; }
        public BattleResultPresenter() { View = new BattleResultView(false, false, string.Empty); }
        public void Handle(BattleEvent e)
        { if (e.Type == BattleEventType.BattleEnded) View = new BattleResultView(true, e.Payload.Result.IsVictory, e.Payload.Result.Reason); }
        public void Clear() { View = new BattleResultView(false, false, string.Empty); }
    }
}
