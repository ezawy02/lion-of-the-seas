using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Keeps the local Level 1 review build in its authored portrait presentation.</summary>
    [DisallowMultipleComponent]
    public sealed class Level01TrialDisplay : MonoBehaviour
    {
#if UNITY_STANDALONE_OSX
        [SerializeField] private int reviewWidth = 720;
        [SerializeField] private int reviewHeight = 1280;
#endif

        private void Awake()
        {
            Application.targetFrameRate = 60;
#if UNITY_ANDROID || UNITY_IOS
            Screen.orientation = ScreenOrientation.Portrait;
            Screen.autorotateToLandscapeLeft = false;
            Screen.autorotateToLandscapeRight = false;
#elif UNITY_STANDALONE_OSX
            if (!Application.isEditor)
                Screen.SetResolution(reviewWidth, reviewHeight, FullScreenMode.Windowed);
#endif
        }
    }
}
