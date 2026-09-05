using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Presentation.Battle;
using SeaLion.Presentation.Pooling;
using UnityEngine;

namespace SeaLion.Presentation.Audio
{
    public enum Level01AudioMixSnapshot { Muted, Traversal, Assault, Victory, Failure }

    [DisallowMultipleComponent]
    public sealed class Level01AudioDirector : MonoBehaviour
    {
        [SerializeField] private Level01AudioLibrary library;
        [SerializeField, Range(4, 12)] private int oneShotCapacity = 8;
        [SerializeField, Range(0f, 1f)] private float masterVolume = 0.82f;
        [SerializeField, Range(0f, 1f)] private float musicVolume = 0.58f;
        [SerializeField, Range(0f, 1f)] private float ambienceVolume = 0.70f;
        [SerializeField, Range(0f, 1f)] private float effectsVolume = 0.86f;
        [SerializeField, Min(0.05f)] private float crossfadeSeconds = 1.25f;
        [SerializeField] private bool playTraversalOnStart = true;

        private readonly List<AudioSource> activeOneShots = new List<AudioSource>(8);
        private readonly Dictionary<int, float> lastCueTimes = new Dictionary<int, float>();
        private AudioSourcePool<AudioSource> oneShots;
        private BattlePresentationSubscribers battlePresentation;
        private AudioSource sea;
        private AudioSource wind;
        private AudioSource gate;
        private AudioSource traversalMusic;
        private AudioSource battleMusic;
        private float targetSea;
        private float targetWind;
        private float targetGate;
        private float targetTraversalMusic;
        private float targetBattleMusic;
        private bool initialized;
        private bool wasDisabled;
        private Level01AudioMixSnapshot resumeSnapshot;
        private float duckUntil;

        public Level01AudioLibrary Library => library;
        public Level01AudioMixSnapshot CurrentSnapshot { get; private set; }
        public int ActiveOneShotCount => activeOneShots.Count;
        public int OneShotCapacity => oneShotCapacity;
        public Level01AudioCue? LastPlayedCue { get; private set; }

        public void ApplyPreferences(float music, float effects)
        {
            musicVolume = Mathf.Clamp01(music);
            effectsVolume = Mathf.Clamp01(effects);
            ambienceVolume = effectsVolume;
            SetSnapshot(CurrentSnapshot);
        }

        public void Configure(Level01AudioLibrary value, bool autoStart)
        {
            library = value;
            playTraversalOnStart = autoStart;
            if (initialized) RefreshLoopClips();
        }

        public void Bind(BattleEventStream stream)
        {
            if (stream == null) throw new ArgumentNullException(nameof(stream));
            battlePresentation?.Dispose();
            battlePresentation = new BattlePresentationSubscribers(stream, PresentBattleEffect, oneShotCapacity);
        }

        public void EnterTraversal()
        {
            SetSnapshot(Level01AudioMixSnapshot.Traversal);
        }

        public void EnterAssault()
        {
            SetSnapshot(Level01AudioMixSnapshot.Assault);
        }

        public void EnterVictory()
        {
            SetSnapshot(Level01AudioMixSnapshot.Victory);
        }

        public void EnterFailure()
        {
            SetSnapshot(Level01AudioMixSnapshot.Failure);
        }

        public void Mute()
        {
            SetSnapshot(Level01AudioMixSnapshot.Muted);
        }

        public void SetGateEnergyActive(bool active)
        {
            EnsureInitialized();
            targetGate = active ? masterVolume * ambienceVolume * 0.62f : 0f;
        }

        public bool PlayBroadside() => PlayControlled(Level01AudioCue.BroadsideCannon,
            1f, 1f, 40, 0.7f, 101, .7f);
        public bool PlayGateMultiply() => PlayControlled(Level01AudioCue.GateMultiplyX4,
            .92f, 1f, 64, .5f, 102, .35f);
        public bool PlayGateDamage() => PlayControlled(Level01AudioCue.GateDamage,
            .72f, 1f, 76, .5f, 103, .28f);
        public bool PlayLanding() => PlayControlled(Level01AudioCue.LandingShallowWater,
            .9f, 1f, 72, .45f, 104, .22f);
        public bool PlayGuardianHit() => PlayControlled(Level01AudioCue.GuardianArmorHit,
            .68f, 1.04f, 86, .55f, 105, .16f);
        public bool PlayCrewLoss() => PlayControlled(Level01AudioCue.CrewLoss,
            .68f, 1f, 78, .8f, 106, .24f);
        public bool PlayGuardianDefeat() => PlayControlled(Level01AudioCue.GuardianDefeat,
            .9f, .94f, 32, 1f, 107, .7f);

        public void PlayReward()
        {
            EnterVictory();
            PlayCue(Level01AudioCue.RewardCorsair, 0.95f);
        }

        public void PlayFailure()
        {
            EnterFailure();
            PlayCue(Level01AudioCue.FailureMedieval, 0.92f);
        }

        public bool PlayCue(Level01AudioCue cue, float volumeScale = 1f)
        {
            return PlayControlled(cue, volumeScale, 1f, 96, 0f, (int)cue, 0f);
        }

        private bool PlayControlled(Level01AudioCue cue, float volumeScale, float pitch,
            int priority, float cooldown, int cooldownKey, float duckSeconds)
        {
            EnsureInitialized();
            if (library == null) return false;
            var clip = library.ClipFor(cue);
            if (clip == null || Level01AudioLibrary.IsLoopingCue(cue)) return false;
            var now = Time.unscaledTime;
            if (cooldown > 0f && lastCueTimes.TryGetValue(cooldownKey, out var last) &&
                now - last < cooldown) return false;
            if (activeOneShots.Count >= oneShotCapacity)
            {
                var oldest = activeOneShots[0];
                if (!oneShots.Release(oldest)) return false;
                activeOneShots.RemoveAt(0);
            }
            var source = oneShots.Rent();
            source.clip = clip;
            source.volume = masterVolume * effectsVolume * Mathf.Clamp01(volumeScale);
            source.pitch = Mathf.Clamp(pitch, .5f, 1.5f);
            source.priority = Mathf.Clamp(priority, 0, 256);
            source.Play();
            activeOneShots.Add(source);
            lastCueTimes[cooldownKey] = now;
            LastPlayedCue = cue;
            if (duckSeconds > 0f) duckUntil = Mathf.Max(duckUntil, now + duckSeconds);
            return true;
        }

        public void SetSnapshot(Level01AudioMixSnapshot snapshot)
        {
            EnsureInitialized();
            CurrentSnapshot = snapshot;
            var music = masterVolume * musicVolume;
            var ambience = masterVolume * ambienceVolume;
            targetGate = 0f;
            switch (snapshot)
            {
                case Level01AudioMixSnapshot.Traversal:
                    targetSea = ambience * 0.88f;
                    targetWind = ambience * 0.78f;
                    targetTraversalMusic = music * 0.72f;
                    targetBattleMusic = 0f;
                    break;
                case Level01AudioMixSnapshot.Assault:
                    targetSea = ambience * 0.58f;
                    targetWind = ambience * 0.46f;
                    targetTraversalMusic = 0f;
                    targetBattleMusic = music * 0.82f;
                    break;
                case Level01AudioMixSnapshot.Victory:
                    targetSea = ambience * 0.64f;
                    targetWind = ambience * 0.36f;
                    targetGate = 0f;
                    targetTraversalMusic = music * 0.22f;
                    targetBattleMusic = 0f;
                    break;
                case Level01AudioMixSnapshot.Failure:
                    targetSea = ambience * 0.30f;
                    targetWind = ambience * 0.30f;
                    targetGate = 0f;
                    targetTraversalMusic = 0f;
                    targetBattleMusic = music * 0.14f;
                    break;
                default:
                    targetSea = targetWind = targetGate = targetTraversalMusic = targetBattleMusic = 0f;
                    break;
            }
        }

        private void Awake()
        {
            EnsureInitialized();
        }

        private void Start()
        {
            if (playTraversalOnStart) EnterTraversal();
        }

        private void OnEnable()
        {
            if (!initialized || !wasDisabled) return;
            RefreshLoopClips();
            SetSnapshot(resumeSnapshot);
            wasDisabled = false;
        }

        private void Update()
        {
            if (!initialized) return;
            for (var i = activeOneShots.Count - 1; i >= 0; i--)
            {
                var source = activeOneShots[i];
                if (source != null && source.isPlaying) continue;
                activeOneShots.RemoveAt(i);
                if (!oneShots.Release(source))
                    Debug.LogWarning("Level 01 audio pool could not release a completed source.", this);
            }
            var step = Time.unscaledDeltaTime / Mathf.Max(0.05f, crossfadeSeconds);
            Fade(sea, targetSea, step);
            Fade(wind, targetWind, step);
            Fade(gate, targetGate, step);
            Fade(traversalMusic, targetTraversalMusic, step);
            var battleTarget = Time.unscaledTime < duckUntil ? targetBattleMusic * .5f : targetBattleMusic;
            Fade(battleMusic, battleTarget, step);
        }

        private void OnDisable()
        {
            resumeSnapshot = CurrentSnapshot;
            wasDisabled = true;
            targetSea = targetWind = targetGate = targetTraversalMusic = targetBattleMusic = 0f;
            StopAndReleaseOneShots();
            StopLoopSources();
            lastCueTimes.Clear();
        }

        private void OnDestroy()
        {
            battlePresentation?.Dispose();
            battlePresentation = null;
            StopAndReleaseOneShots();
            oneShots?.Dispose();
            oneShots = null;
            initialized = false;
        }

        private void EnsureInitialized()
        {
            if (initialized) return;
            oneShots = new AudioSourcePool<AudioSource>(
                oneShotCapacity, CreateOneShotSource, ResetOneShotSource, DestroyOneShotSource);
            oneShots.WarmUp(oneShotCapacity);
            sea = CreateLoopSource("AUDIO__SeaAmbience", Level01AudioCue.SeaAmbience, true);
            wind = CreateLoopSource("AUDIO__WindAmbience", Level01AudioCue.WindAmbience, true);
            gate = CreateLoopSource("AUDIO__GateEnergy", Level01AudioCue.GateEnergyLoop, true);
            traversalMusic = CreateLoopSource("AUDIO__TraversalMusic", Level01AudioCue.TraversalMusic, true);
            battleMusic = CreateLoopSource("AUDIO__GuardianBattleMusic", Level01AudioCue.GuardianBattleMusic, false);
            initialized = true;
            SetSnapshot(Level01AudioMixSnapshot.Muted);
        }

        private AudioSource CreateOneShotSource()
        {
            var source = CreateSource("AUDIO__PooledOneShot");
            source.loop = false;
            return source;
        }

        private AudioSource CreateLoopSource(string objectName, Level01AudioCue cue, bool loop)
        {
            var source = CreateSource(objectName);
            source.clip = library == null ? null : library.ClipFor(cue);
            source.loop = loop;
            source.volume = 0f;
            return source;
        }

        private void RefreshLoopClips()
        {
            RefreshLoopClip(sea, Level01AudioCue.SeaAmbience, true);
            RefreshLoopClip(wind, Level01AudioCue.WindAmbience, true);
            RefreshLoopClip(gate, Level01AudioCue.GateEnergyLoop, true);
            RefreshLoopClip(traversalMusic, Level01AudioCue.TraversalMusic, true);
            RefreshLoopClip(battleMusic, Level01AudioCue.GuardianBattleMusic, false);
        }

        private void RefreshLoopClip(AudioSource source, Level01AudioCue cue, bool playSilently)
        {
            if (source == null) return;
            source.Stop();
            source.clip = library == null ? null : library.ClipFor(cue);
        }

        private AudioSource CreateSource(string objectName)
        {
            var child = new GameObject(objectName);
            child.transform.SetParent(transform, false);
            var source = child.AddComponent<AudioSource>();
            source.playOnAwake = false;
            source.spatialBlend = 0f;
            source.dopplerLevel = 0f;
            source.priority = 96;
            return source;
        }

        private static void ResetOneShotSource(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
            source.clip = null;
            source.volume = 0f;
            source.pitch = 1f;
            source.priority = 96;
        }

        private static void DestroyOneShotSource(AudioSource source)
        {
            if (source != null) Destroy(source.gameObject);
        }

        private void StopAndReleaseOneShots()
        {
            if (oneShots == null) return;
            for (var i = activeOneShots.Count - 1; i >= 0; i--)
            {
                oneShots.Release(activeOneShots[i]);
                activeOneShots.RemoveAt(i);
            }
        }

        private void StopLoopSources()
        {
            StopLoopSource(sea);
            StopLoopSource(wind);
            StopLoopSource(gate);
            StopLoopSource(traversalMusic);
            StopLoopSource(battleMusic);
        }

        private static void StopLoopSource(AudioSource source)
        {
            if (source == null) return;
            source.Stop();
            source.volume = 0f;
        }

        private void PresentBattleEffect(BattlePresentationEffect effect)
        {
            var kind = effect.Kind;
            var eventType = effect.Event.Type;
            switch (kind)
            {
                case BattlePresentationKind.Gate:
                    if (effect.Event.Payload.Outcome == GateOutcome.Damage) PlayGateDamage();
                    else if (effect.Event.Payload.Outcome == GateOutcome.Multiply) PlayGateMultiply();
                    break;
                case BattlePresentationKind.Hit: break;
                case BattlePresentationKind.Loss: PlayCrewLoss(); break;
                case BattlePresentationKind.Landing:
                    if (eventType == BattleEventType.LandingCompleted) PlayLanding();
                    break;
                case BattlePresentationKind.Boss: EnterAssault(); break;
                case BattlePresentationKind.Destruction: PlayGuardianDefeat(); break;
                case BattlePresentationKind.Victory: PlayReward(); break;
                case BattlePresentationKind.Failure: PlayFailure(); break;
            }
        }

        private static void Fade(AudioSource source, float target, float step)
        {
            if (source == null) return;
            source.volume = Mathf.MoveTowards(source.volume, target, step);
            if (target > 0f && source.clip != null && !source.isPlaying) source.Play();
            else if (target <= .0001f && source.volume <= .0001f && source.isPlaying) source.Stop();
        }

        private void OnValidate()
        {
            oneShotCapacity = Mathf.Clamp(oneShotCapacity, 4, 12);
            crossfadeSeconds = Mathf.Max(0.05f, crossfadeSeconds);
        }
    }
}
