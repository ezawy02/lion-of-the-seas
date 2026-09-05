using SeaLion.Gameplay.Levels;
using Unity.Profiling;
using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Emits one development-build render snapshot during live Level 1 assault.</summary>
    [DisallowMultipleComponent]
    public sealed class Level01TrialRuntimeDiagnostics : MonoBehaviour
    {
        private Level01TrialRuntime runtime;
        private Level01TrialCrowdPresenter crowd;
        private ProfilerRecorder drawCalls;
        private ProfilerRecorder batches;
        private ProfilerRecorder setPass;
        private ProfilerRecorder triangles;
        private ProfilerRecorder vertices;
        private Level01TrialPhase observedPhase = Level01TrialPhase.Loading;
        private float sampleAt;
        private bool loggedAssault;

        public void Bind(Level01TrialRuntime trialRuntime, Level01TrialCrowdPresenter crowdPresenter)
        {
            runtime = trialRuntime;
            crowd = crowdPresenter;
        }

        private void OnEnable()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            drawCalls = StartRenderRecorder("Draw Calls Count");
            batches = StartRenderRecorder("Batches Count");
            setPass = StartRenderRecorder("SetPass Calls Count");
            triangles = StartRenderRecorder("Triangles Count");
            vertices = StartRenderRecorder("Vertices Count");
#endif
        }

        private void Update()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            if (runtime == null) return;
            if (runtime.Phase != observedPhase)
            {
                observedPhase = runtime.Phase;
                sampleAt = Time.unscaledTime + 2f;
            }
            if (loggedAssault || observedPhase != Level01TrialPhase.Assault ||
                Time.unscaledTime < sampleAt) return;
            loggedAssault = true;
            Debug.Log($"[Level01Diagnostics] phase=Assault drawCalls={Value(drawCalls)} " +
                $"batches={Value(batches)} setPass={Value(setPass)} triangles={Value(triangles)} " +
                $"vertices={Value(vertices)} gpuInstancing={crowd != null && crowd.UsesGpuInstancing} " +
                $"sources={crowd?.SourceRendererCount ?? 0} friendly={crowd?.FriendlyVisibleCount ?? 0} " +
                $"hostile={crowd?.HostileVisibleCount ?? 0}", this);
#endif
        }

        private void OnDisable()
        {
#if DEVELOPMENT_BUILD || UNITY_EDITOR
            drawCalls.Dispose();
            batches.Dispose();
            setPass.Dispose();
            triangles.Dispose();
            vertices.Dispose();
#endif
        }

#if DEVELOPMENT_BUILD || UNITY_EDITOR
        private static ProfilerRecorder StartRenderRecorder(string marker) =>
            ProfilerRecorder.StartNew(ProfilerCategory.Render, marker, 1);

        private static long Value(ProfilerRecorder recorder) => recorder.Valid ? recorder.LastValue : -1;
#endif
    }
}
