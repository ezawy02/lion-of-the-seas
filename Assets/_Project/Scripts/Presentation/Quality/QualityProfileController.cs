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
        private QualityProfile active;

        public QualityProfile ActiveProfile => active;
        public QualityPreference Preference => preference;
        public QualityProfile PrimaryProfile => primary;
        public QualityProfile ReducedProfile => reduced;

        private void OnEnable() => Apply(preference == QualityPreference.Reduced ? reduced : primary);

        public void SetPreference(QualityPreference value)
        {
            preference = value;
            if (value != QualityPreference.Auto) Apply(value == QualityPreference.Reduced ? reduced : primary);
        }

        /// <summary>Feeds a measured frame time to Auto; thresholds provide hysteresis.</summary>
        public void EvaluateFrameTime(float seconds)
        {
            if (preference != QualityPreference.Auto) return;
            if (active == primary && seconds > frameTimeEnter) Apply(reduced);
            else if (active == reduced && seconds < frameTimeExit) Apply(primary);
            else if (active == null) Apply(seconds > frameTimeEnter ? reduced : primary);
        }

        public void Apply(QualityProfile profile)
        {
            if (profile == null) return;
            active = profile;
            QualitySettings.lodBias = profile.LodBias;
            QualitySettings.shadowDistance = profile.ProfileKind == QualityProfileKind.Reduced ? 0f : 35f;
        }
    }
}
