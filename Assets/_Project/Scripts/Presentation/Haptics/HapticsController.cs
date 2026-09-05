using System;
using UnityEngine;
using SeaLion.Core.Events;

namespace SeaLion.Presentation.Haptics
{
    public enum HapticCue { None, Gate, Broadside, ArmorBreak, Victory }

    public interface IHapticsPlatform
    {
        bool IsSupported { get; }
        void Vibrate();
    }

    public sealed class HandheldHapticsPlatform : IHapticsPlatform
    {
        public bool IsSupported
        {
            get
            {
                return Application.platform == RuntimePlatform.Android ||
                    Application.platform == RuntimePlatform.IPhonePlayer;
            }
        }

        public void Vibrate()
        {
#if UNITY_ANDROID || UNITY_IOS
            if (IsSupported) Handheld.Vibrate();
#endif
        }
    }

    public sealed class HapticsController : MonoBehaviour
    {
        [SerializeField] private bool enabledBySetting = true;
        private IHapticsPlatform platform;
        private BattleEventStream stream;
        private IDisposable subscription;

        public bool EnabledBySetting { get { return enabledBySetting; } set { enabledBySetting = value; } }
        public bool IsAvailable { get { return platform != null && platform.IsSupported; } }
        public HapticCue LastCue { get; private set; }

        public void Initialize(IHapticsPlatform hapticsPlatform)
        {
            platform = hapticsPlatform;
        }

        public void Bind(BattleEventStream eventStream)
        {
            if (eventStream == null) throw new ArgumentNullException(nameof(eventStream));
            subscription?.Dispose();
            stream = eventStream;
            subscription = isActiveAndEnabled ? stream.Subscribe(HandleEvent) : null;
        }

        public bool TryPulse()
        {
            if (!enabledBySetting || platform == null || !platform.IsSupported) return false;
            platform.Vibrate();
            return true;
        }

        public bool TryPulse(HapticCue cue)
        {
            if (cue == HapticCue.None) return false;
            var pulsed = TryPulse();
            if (pulsed) LastCue = cue;
            return pulsed;
        }

        public bool Handle(BattleEvent value)
        {
            if (value.Type == BattleEventType.GateResolved) return TryPulse(HapticCue.Gate);
            if (value.Type == BattleEventType.BossPhaseChanged) return TryPulse(HapticCue.ArmorBreak);
            if (value.Type == BattleEventType.BattleEnded && value.Payload.Result.IsVictory)
                return TryPulse(HapticCue.Victory);
            return false;
        }

        private void Awake()
        {
            if (platform == null) platform = new HandheldHapticsPlatform();
        }

        private void OnEnable()
        {
            if (subscription == null && stream != null) subscription = stream.Subscribe(HandleEvent);
        }

        private void OnDisable()
        {
            subscription?.Dispose();
            subscription = null;
        }

        private void OnDestroy()
        {
            subscription?.Dispose();
            subscription = null;
            stream = null;
        }

        private void HandleEvent(BattleEvent value) => Handle(value);
    }
}
