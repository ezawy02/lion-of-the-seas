using System;
using System.Collections.Generic;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using UnityEngine;

namespace SeaLion.Gameplay.Deployment
{
    public interface ILandingCraft
    {
        void Activate(Vector3 position, int contribution, int sequence);
        void Deactivate();
    }

    public readonly struct LandingCraftDeployment
    {
        public readonly int Sequence;
        public readonly int Contribution;
        public readonly Vector3 Position;
        public LandingCraftDeployment(int sequence, int contribution, Vector3 position)
        { Sequence = sequence; Contribution = contribution; Position = position; }
    }

    /// <summary>Fixed-step, bounded deployment coordinator. Craft lifetime belongs to its consumer.</summary>
    public sealed class LandingCraftDeployer : MonoBehaviour, IDisposable
    {
        [SerializeField, Min(1)] private int poolCapacity = 32;
        [SerializeField] private float spreadWidth = 1f;
        private BoundedPool pool;
        private FlagshipDefinition definition;
        private BattleSession session;
        private Vector3 origin;
        private float elapsed;
        private int sequence;
        private bool paused;
        private bool terminal;

        public event Action<LandingCraftDeployment> Deployed;
        public int AvailableCount => pool == null ? 0 : pool.AvailableCount;
        public int InUseCount => pool == null ? 0 : pool.InUseCount;
        public int Capacity => pool == null ? 0 : pool.Capacity;

        public void Configure(FlagshipDefinition source, BattleSession battle, Func<ILandingCraft> factory,
            int capacity, Vector3 deploymentOrigin, float width = 1f, int warmCount = 0)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (capacity < 1) throw new ArgumentOutOfRangeException(nameof(capacity));
            Dispose();
            definition = source; session = battle; origin = deploymentOrigin;
            poolCapacity = capacity; spreadWidth = IsFinite(width) ? Mathf.Max(0f, width) : 0f;
            pool = new BoundedPool(capacity, factory);
            pool.WarmUp(Mathf.Clamp(warmCount, 0, capacity));
            ResetRuntime();
        }

        public void SetPaused(bool value) { paused = value; }
        public void SetTerminal(bool value) { terminal = value; if (value) elapsed = 0f; }
        public void Tick(float deltaSeconds)
        {
            if (paused || terminal || pool == null || definition == null || !IsFinite(deltaSeconds) || deltaSeconds <= 0f) return;
            if (session != null && (session.State != BattleState.Active && session.State != BattleState.Landing && session.State != BattleState.Assault)) return;
            var interval = ComputeInterval(definition.DeploymentCadence);
            elapsed += deltaSeconds;
            while (elapsed >= interval)
            {
                elapsed -= interval;
                DeployBurst();
                if (terminal || pool.AvailableCount == 0 && pool.InUseCount >= pool.Capacity) break;
            }
        }

        public bool Release(ILandingCraft craft) => pool != null && pool.Release(craft);
        public static float ComputeInterval(float cadence) => IsFinite(cadence) && cadence > 0f ? cadence : float.PositiveInfinity;
        public static int ComputeBurstSize(DeployPattern pattern, int configuredBurst)
            => pattern == DeployPattern.Cadence ? 1 : Mathf.Max(1, configuredBurst);
        public static int ComputeContribution(float configuredContribution)
            => IsFinite(configuredContribution)
                ? Mathf.Max(1, (int)Math.Round(configuredContribution, MidpointRounding.AwayFromZero))
                : 1;
        public static float ComputeSpreadOffset(DeployPattern pattern, int index, int count, float width)
        {
            if (pattern != DeployPattern.Spread || count <= 1) return 0f;
            var safeWidth = IsFinite(width) ? Mathf.Max(0f, width) : 0f;
            return (index / (float)(count - 1) - 0.5f) * safeWidth;
        }

        private void DeployBurst()
        {
            var count = ComputeBurstSize(definition.DeployPattern, definition.BurstSize);
            for (var i = 0; i < count; i++)
            {
                ILandingCraft craft;
                try { craft = pool.Rent(); } catch (InvalidOperationException) { return; }
                var contribution = ComputeContribution(definition.BaseDeployment);
                var position = origin + Vector3.right * ComputeSpreadOffset(definition.DeployPattern, i, count, spreadWidth);
                craft.Activate(position, contribution, sequence++);
                Deployed?.Invoke(new LandingCraftDeployment(sequence - 1, contribution, position));
            }
        }

        private void ResetRuntime() { elapsed = 0f; sequence = 0; paused = false; terminal = false; }
        public void Dispose() { if (pool != null) { pool.Dispose(); pool = null; } }
        private void OnDestroy() { Dispose(); }
        private static bool IsFinite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

        private sealed class BoundedPool : IDisposable
        {
            private readonly Func<ILandingCraft> factory;
            private readonly Stack<ILandingCraft> available;
            private readonly HashSet<ILandingCraft> owned = new HashSet<ILandingCraft>();
            private readonly HashSet<ILandingCraft> inUse = new HashSet<ILandingCraft>();
            public int Capacity { get; }
            public int AvailableCount => available.Count;
            public int InUseCount => inUse.Count;
            public BoundedPool(int capacity, Func<ILandingCraft> factory)
            { Capacity = capacity; this.factory = factory; available = new Stack<ILandingCraft>(capacity); }
            public void WarmUp(int count) { while (owned.Count < count) Add(); }
            public ILandingCraft Rent()
            {
                if (available.Count == 0 && owned.Count < Capacity) Add();
                if (available.Count == 0) throw new InvalidOperationException("Landing-craft pool exhausted.");
                var craft = available.Pop(); inUse.Add(craft); return craft;
            }
            public bool Release(ILandingCraft craft)
            { if (craft == null || !owned.Contains(craft) || !inUse.Remove(craft)) return false; craft.Deactivate(); available.Push(craft); return true; }
            public void Dispose() { foreach (var craft in owned) craft.Deactivate(); available.Clear(); owned.Clear(); inUse.Clear(); }
            private void Add()
            { var craft = factory(); if (craft == null || !owned.Add(craft)) throw new InvalidOperationException("Invalid landing-craft factory result."); available.Push(craft); }
        }
    }
}
