using System.Collections;
using UnityEngine;

namespace SeaLion.Presentation.Audio
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Level01AudioDirector))]
    public sealed class Level01AudioReviewSequence : MonoBehaviour
    {
        [SerializeField] private Level01AudioDirector director;
        [SerializeField] private bool loopReview = true;
        private bool runtimeValidated;
        private bool previousRunInBackground;

        public void Configure(Level01AudioDirector value, bool shouldLoop = true)
        {
            director = value;
            loopReview = shouldLoop;
        }

        private void Awake()
        {
            if (director == null) director = GetComponent<Level01AudioDirector>();
        }

        private void OnEnable()
        {
            previousRunInBackground = Application.runInBackground;
            Application.runInBackground = true;
        }

        private void OnDisable()
        {
            Application.runInBackground = previousRunInBackground;
        }

        private IEnumerator Start()
        {
            do
            {
                director.EnterTraversal();
                director.SetGateEnergyActive(true);
                yield return new WaitForSecondsRealtime(0.5f);
                ValidateRuntimePlayback();
                yield return new WaitForSecondsRealtime(1.5f);
                director.PlayBroadside();
                yield return new WaitForSecondsRealtime(2.4f);
                director.PlayGateMultiply();
                yield return new WaitForSecondsRealtime(2.2f);
                director.SetGateEnergyActive(false);
                director.PlayLanding();
                yield return new WaitForSecondsRealtime(2.0f);
                director.EnterAssault();
                yield return new WaitForSecondsRealtime(1.6f);
                director.PlayGuardianHit();
                yield return new WaitForSecondsRealtime(0.85f);
                director.PlayGuardianHit();
                yield return new WaitForSecondsRealtime(1.45f);
                director.PlayGuardianDefeat();
                yield return new WaitForSecondsRealtime(2.6f);
                director.PlayReward();
                yield return new WaitForSecondsRealtime(5.4f);
                director.PlayFailure();
                yield return new WaitForSecondsRealtime(7.0f);
                director.Mute();
                yield return new WaitForSecondsRealtime(1.0f);
            }
            while (loopReview);
        }

        private void ValidateRuntimePlayback()
        {
            if (runtimeValidated) return;
            runtimeValidated = true;
            var playing = 0;
            var sources = GetComponentsInChildren<AudioSource>(true);
            foreach (var source in sources)
                if (source.isPlaying) playing++;
            var expectedSources = 5 + director.OneShotCapacity;
            if (sources.Length != expectedSources || playing < 3)
            {
                Debug.LogError(
                    $"Level 01 audio REVIEW failed runtime validation: sources={sources.Length}, " +
                    $"expected={expectedSources}, playing={playing}.",
                    this);
                return;
            }
            Debug.Log(
                $"Level 01 audio REVIEW runtime passed: sources={sources.Length}, playing={playing}.", this);
        }
    }
}
