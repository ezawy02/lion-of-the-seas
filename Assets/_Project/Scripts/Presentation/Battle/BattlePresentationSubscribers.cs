using System;
using SeaLion.Core.Events;
using SeaLion.Presentation.Pooling;

namespace SeaLion.Presentation.Battle
{
    public enum BattlePresentationKind { Gate, Hit, Loss, Landing, Boss, Destruction, Victory, Failure }

    public sealed class BattlePresentationEffect
    {
        public BattlePresentationKind Kind { get; internal set; }
        public BattleEvent Event { get; internal set; }
        internal void Clear() { Kind = default(BattlePresentationKind); Event = default(BattleEvent); }
    }

    /// <summary>Routes authoritative battle events to pooled, presentation-only effects.</summary>
    public sealed class BattlePresentationSubscribers : IDisposable
    {
        private readonly CraftPool<BattlePresentationEffect> gate;
        private readonly VfxPool<BattlePresentationEffect> hit;
        private readonly UiNumberPool<BattlePresentationEffect> loss;
        private readonly VfxPool<BattlePresentationEffect> landing;
        private readonly VfxPool<BattlePresentationEffect> boss;
        private readonly DebrisPool<BattlePresentationEffect> destruction;
        private readonly UiNumberPool<BattlePresentationEffect> victory;
        private readonly UiNumberPool<BattlePresentationEffect> failure;
        private readonly Action<BattlePresentationEffect> present;
        private readonly IDisposable subscription;
        private bool disposed;

        public BattlePresentationSubscribers(BattleEventStream stream, Action<BattlePresentationEffect> present, int capacity = 8)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            this.present = present ?? throw new ArgumentNullException(nameof(present));
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            Func<BattlePresentationEffect> create = () => new BattlePresentationEffect();
            gate = new CraftPool<BattlePresentationEffect>(capacity, create, Reset);
            hit = new VfxPool<BattlePresentationEffect>(capacity, create, Reset);
            loss = new UiNumberPool<BattlePresentationEffect>(capacity, create, Reset);
            landing = new VfxPool<BattlePresentationEffect>(capacity, create, Reset);
            boss = new VfxPool<BattlePresentationEffect>(capacity, create, Reset);
            destruction = new DebrisPool<BattlePresentationEffect>(capacity, create, Reset);
            victory = new UiNumberPool<BattlePresentationEffect>(capacity, create, Reset);
            failure = new UiNumberPool<BattlePresentationEffect>(capacity, create, Reset);
            subscription = stream.Subscribe(OnEvent);
        }

        public void Dispose()
        {
            if (disposed) return;
            disposed = true; subscription.Dispose();
            gate.Dispose(); hit.Dispose(); loss.Dispose(); landing.Dispose();
            boss.Dispose(); destruction.Dispose(); victory.Dispose(); failure.Dispose();
        }

        private void OnEvent(BattleEvent item)
        {
            BattlePresentationKind kind;
            switch (item.Type)
            {
                case BattleEventType.GateResolved: kind = BattlePresentationKind.Gate; break;
                case BattleEventType.ForceChanged:
                    kind = item.Payload.After == 0 ? BattlePresentationKind.Destruction :
                        (item.Payload.After < item.Payload.Before ? BattlePresentationKind.Loss : BattlePresentationKind.Hit); break;
                case BattleEventType.LandingStarted:
                case BattleEventType.LandingCompleted: kind = BattlePresentationKind.Landing; break;
                case BattleEventType.BossPhaseChanged: kind = BattlePresentationKind.Boss; break;
                case BattleEventType.BattleEnded:
                    kind = item.Payload.Result.IsVictory ? BattlePresentationKind.Victory : BattlePresentationKind.Failure; break;
                default: return;
            }
            var pool = PoolFor(kind);
            var effect = pool.Rent();
            effect.Kind = kind; effect.Event = item;
            try { present(effect); }
            finally { pool.Release(effect); }
        }

        private TypedPresentationPool<BattlePresentationEffect> PoolFor(BattlePresentationKind kind)
        {
            switch (kind)
            {
                case BattlePresentationKind.Gate: return gate;
                case BattlePresentationKind.Hit: return hit;
                case BattlePresentationKind.Loss: return loss;
                case BattlePresentationKind.Landing: return landing;
                case BattlePresentationKind.Boss: return boss;
                case BattlePresentationKind.Destruction: return destruction;
                case BattlePresentationKind.Victory: return victory;
                default: return failure;
            }
        }

        private static void Reset(BattlePresentationEffect effect) { effect.Clear(); }
    }
}
