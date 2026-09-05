using SeaLion.Core.Definitions;
using UnityEngine;

namespace SeaLion.Presentation.Quality
{
    public enum QualityPreference { Auto, Primary, Reduced }

    /// <summary>Applies presentation settings only; simulation values remain authoritative elsewhere.</summary>
    public sealed class QualityProfileController : MonoBehaviour
    {
        [SerializeField] private QualityProfile primary;
        [SerializeField] private QualityProfile reduced;
        [SerializeField] private QualityPreference preference = QualityPreference.Auto;
        [SerializeField, Min(0.001f)] private float frameTimeEnter = 0.033f;
        [SerializeField, Min(0.001f)] private float frameTimeExit = 0.025f;
        [SerializeField, Min(0.1f)] private float smoothingSeconds = 0.75f;
        [SerializeField, Min(0.1f)] private float fallbackDwellSeconds = 0.75f;
        [SerializeField, Min(0.1f)] private float recoveryDwellSeconds = 2f;
        private const float MaxSampleSeconds = .1f;
        private QualityProfile active;
        private float smoothedFrameTime;
        private float pressureDuration;
        private float recoveryDuration;

        public QualityProfile ActiveProfile => active;
        public QualityPreference Preference => preference;
        public QualityProfile PrimaryProfile => primary;
        public QualityProfile ReducedProfile => reduced;

        private void OnEnable()
        {
            ResetSampling();
            Apply(preference == QualityPreference.Reduced ? reduced : primary);
        }

        private void Update()
        {
            if (preference == QualityPreference.Auto) SampleFrameTime(Time.unscaledDeltaTime);
        }

        public void SetPreference(QualityPreference value)
        {
            preference = value;
            ResetSampling();
            if (value == QualityPreference.Auto)
            {
                if (active == null) Apply(primary);
                return;
            }
            Apply(value == QualityPreference.Reduced ? reduced : primary);
        }

        /// <summary>Feeds a measured frame time to Auto; thresholds provide hysteresis.</summary>
        public void EvaluateFrameTime(float seconds)
        {
            if (preference != QualityPreference.Auto || !IsFinitePositive(seconds)) return;
            if (active == primary && seconds > frameTimeEnter) Apply(reduced);
            else if (active == reduced && seconds < frameTimeExit) Apply(primary);
            else if (active == null) Apply(seconds > frameTimeEnter ? reduced : primary);
        }

        /// <summary>Samples production frame time and switches only after sustained pressure.</summary>
        public void SampleFrameTime(float seconds)
        {
            if (preference != QualityPreference.Auto || !IsFinitePositive(seconds)) return;
            // A debugger break or app resume must not count as sustained frame pressure.
            seconds = Mathf.Min(seconds, MaxSampleSeconds);
            var blend = smoothedFrameTime <= 0f ? 1f :
                1f - Mathf.Exp(-seconds / Mathf.Max(.1f, smoothingSeconds));
            smoothedFrameTime = Mathf.Lerp(smoothedFrameTime <= 0f ? seconds : smoothedFrameTime,
                seconds, blend);
            if (active == null) Apply(primary);
            if (active == primary)
            {
                pressureDuration = smoothedFrameTime > frameTimeEnter ?
                    pressureDuration + seconds : Mathf.Max(0f, pressureDuration - seconds * 2f);
                recoveryDuration = 0f;
                if (pressureDuration >= fallbackDwellSeconds)
                {
                    Apply(reduced);
                    ResetSampling(false);
                }
            }
            else if (active == reduced)
            {
                recoveryDuration = smoothedFrameTime < frameTimeExit ?
                    recoveryDuration + seconds : Mathf.Max(0f, recoveryDuration - seconds * 2f);
                pressureDuration = 0f;
                if (recoveryDuration >= recoveryDwellSeconds)
                {
                    Apply(primary);
                    ResetSampling(false);
                }
            }
        }

        public void Apply(QualityProfile profile)
        {
            if (profile == null) return;
            var changed = active != profile;
            active = profile;
            QualitySettings.lodBias = profile.LodBias;
            QualitySettings.shadowDistance = profile.ProfileKind == QualityProfileKind.Reduced ? 0f : 35f;
            if (changed) Debug.Log($"[Quality] active={profile.ProfileKind}", this);
        }

        private void ResetSampling(bool clearAverage = true)
        {
            pressureDuration = recoveryDuration = 0f;
            if (clearAverage) smoothedFrameTime = 0f;
        }

        private static bool IsFinitePositive(float value) =>
            value > 0f && !float.IsNaN(value) && !float.IsInfinity(value);

        private void OnValidate()
        {
            frameTimeEnter = Mathf.Max(.001f, frameTimeEnter);
            frameTimeExit = Mathf.Clamp(frameTimeExit, .001f, frameTimeEnter - .0001f);
            smoothingSeconds = Mathf.Max(.1f, smoothingSeconds);
            fallbackDwellSeconds = Mathf.Max(.1f, fallbackDwellSeconds);
            recoveryDwellSeconds = Mathf.Max(.1f, recoveryDwellSeconds);
        }
    }
}
