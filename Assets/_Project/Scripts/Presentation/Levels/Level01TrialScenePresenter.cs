using System.Collections;
using SeaLion.Combat.Bosses;
using SeaLion.Core.Battle;
using SeaLion.Gameplay.Flagship;
using SeaLion.Gameplay.Input;
using SeaLion.Gameplay.Levels;
using SeaLion.Presentation.Audio;
using SeaLion.Presentation.Haptics;
using SeaLion.Presentation.Quality;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Connects the playable trial to the separately approved Level 1 art scene.</summary>
    [DisallowMultipleComponent]
    public sealed class Level01TrialScenePresenter : MonoBehaviour
    {
        private const float TrialCameraFieldOfView = 39f;

        [SerializeField] private Level01TrialRuntime runtime;
        [SerializeField] private string artSceneName = "Level_01_HundredSails";

        private GameObject opening;
        private GameObject traversal;
        private GameObject landing;
        private GameObject assault;
        private GameObject victory;
        private FlagshipInputAdapter input;
        private FlagshipController flagship;
        private Vector3 flagshipStart;
        private Level01AudioDirector audioDirector;
        private Level01TrialMotionPresenter motion;
        private Level01TrialCrowdPresenter crowd;
        private Level01CharacterHighlightPresenter characterHighlights;
        private Level01PrimaryAttackFeedbackPresenter attackFeedback;
        private Level01PhaseCameraPresenter phaseCamera;
        private Level01PhaseTransitionPresenter phaseTransition;
        private HapticsController haptics;
        private Camera gameplayCamera;
        private bool bound;

        public bool IsReady { get; private set; }
        public string BindingFailure { get; private set; } = string.Empty;

        public void Configure(Level01TrialRuntime trialRuntime, string sceneName)
        {
            runtime = trialRuntime;
            if (!string.IsNullOrWhiteSpace(sceneName)) artSceneName = sceneName;
        }

        private IEnumerator Start()
        {
            if (runtime == null) runtime = GetComponent<Level01TrialRuntime>();
            if (runtime == null)
            {
                BindingFailure = "Level 1 trial runtime is missing.";
                Debug.LogError(BindingFailure, this);
                yield break;
            }

            var scene = SceneManager.GetSceneByName(artSceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                var operation = SceneManager.LoadSceneAsync(artSceneName, LoadSceneMode.Additive);
                if (operation == null)
                {
                    BindingFailure = "The Level 1 art scene is not available in this build.";
                    Debug.LogError(BindingFailure, this);
                    yield break;
                }
                while (!operation.isDone)
                {
                    scene = SceneManager.GetSceneByName(artSceneName);
                    if (scene.IsValid() && scene.isLoaded) break;
                    yield return null;
                }
                scene = SceneManager.GetSceneByName(artSceneName);
            }

            if (!BindScene(scene)) yield break;
            BindRuntime();
            IsReady = runtime.Begin();
            if (!IsReady)
            {
                BindingFailure = "The Level 1 trial could not initialize its definitions.";
                Debug.LogError(BindingFailure, this);
            }
        }

        private void Update()
        {
            if (!IsReady || flagship == null) return;
            runtime.SetSteeringIntent(input != null ? input.HorizontalIntent : 0f,
                input != null && input.HasSteered);
        }

        private void LateUpdate()
        {
            if (!IsReady || flagship == null) return;
            var position = flagship.transform.position;
            position.x = Mathf.Lerp(flagship.LeftBound, flagship.RightBound,
                (runtime.HorizontalChoice + 1f) * .5f);
            flagship.transform.position = position;
        }

        private bool BindScene(Scene scene)
        {
            opening = Find(scene, "PHASE__Opening_ReferenceMatch");
            traversal = Find(scene, "PHASE__Traversal_GateRescue_ReferenceMatch");
            landing = Find(scene, "PHASE__BeachLanding_ReferenceMatch");
            assault = Find(scene, "PHASE__BossBattle_Prototype_NoExecutionReference");
            victory = Find(scene, "PHASE__VictoryReward_Prototype_NoExecutionReference");
            if (opening == null || traversal == null || landing == null || assault == null || victory == null)
            {
                BindingFailure = "One or more approved Level 1 phase roots are missing.";
                Debug.LogError(BindingFailure, this);
                return false;
            }

            var flagshipObject = FindChild(traversal.transform, "PLAYER__Flagship");
            if (flagshipObject == null)
            {
                BindingFailure = "The traversal flagship binding is missing.";
                Debug.LogError(BindingFailure, this);
                return false;
            }

            input = GetComponent<FlagshipInputAdapter>();
            if (input == null) input = gameObject.AddComponent<FlagshipInputAdapter>();
            flagship = flagshipObject.GetComponent<FlagshipController>();
            if (flagship == null) flagship = flagshipObject.AddComponent<FlagshipController>();
            audioDirector = FindComponent<Level01AudioDirector>(scene);
            gameplayCamera = FindComponent<Camera>(scene);
            if (gameplayCamera != null && !gameplayCamera.orthographic)
            {
                gameplayCamera.fieldOfView = TrialCameraFieldOfView;
                gameplayCamera.aspect = 720f / 1280f;
            }
            phaseCamera = GetComponent<Level01PhaseCameraPresenter>();
            if (phaseCamera == null) phaseCamera = gameObject.AddComponent<Level01PhaseCameraPresenter>();
            phaseCamera.Bind(runtime, gameplayCamera, opening, traversal, landing, assault);
            phaseTransition = GetComponent<Level01PhaseTransitionPresenter>();
            if (phaseTransition == null) phaseTransition = gameObject.AddComponent<Level01PhaseTransitionPresenter>();
            phaseTransition.Bind(opening, traversal, landing, assault, victory);
            motion = GetComponent<Level01TrialMotionPresenter>();
            if (motion == null) motion = gameObject.AddComponent<Level01TrialMotionPresenter>();
            motion.Bind(runtime, opening, traversal, landing, assault, victory);
            phaseCamera.ApplyImmediate(Level01TrialPhase.Traversal);
            var presentationBounds = CalculatePresentationBounds(traversal, flagshipObject);
            var travelRange = Level01TraversalBounds.Calculate(gameplayCamera, presentationBounds,
                flagship.transform.position.x);
            phaseCamera.ApplyImmediate(Level01TrialPhase.Loading);
            flagship.Configure(input, travelRange.Left, travelRange.Right, 7.5f, 16f);
            flagship.enabled = false; // Authoritative movement is advanced by the fixed-step runtime.
            flagshipStart = flagship.transform.position;
            var quality = FindFirstObjectByType<QualityProfileController>(FindObjectsInactive.Include);
            crowd = GetComponent<Level01TrialCrowdPresenter>();
            if (crowd == null) crowd = gameObject.AddComponent<Level01TrialCrowdPresenter>();
            if (!crowd.Bind(runtime, landing, assault, quality))
            {
                BindingFailure = "The Level 1 instanced crowd presentation could not bind.";
                Debug.LogError(BindingFailure, this);
                return false;
            }
            var diagnostics = GetComponent<Level01TrialRuntimeDiagnostics>();
            if (diagnostics == null) diagnostics = gameObject.AddComponent<Level01TrialRuntimeDiagnostics>();
            diagnostics.Bind(runtime, crowd);
            characterHighlights = GetComponent<Level01CharacterHighlightPresenter>();
            if (characterHighlights == null)
                characterHighlights = gameObject.AddComponent<Level01CharacterHighlightPresenter>();
            characterHighlights.Bind(opening, traversal, landing, assault, victory);
            attackFeedback = GetComponent<Level01PrimaryAttackFeedbackPresenter>();
            if (attackFeedback == null) attackFeedback = gameObject.AddComponent<Level01PrimaryAttackFeedbackPresenter>();
            attackFeedback.Bind(scene);
            haptics = GetComponent<HapticsController>();
            if (haptics == null) haptics = gameObject.AddComponent<HapticsController>();
            ApplyPhase(Level01TrialPhase.Loading);
            return true;
        }

        private void BindRuntime()
        {
            if (bound) return;
            runtime.PhaseChanged += ApplyPhase;
            runtime.StateChanged += ApplyState;
            runtime.AttemptStarted += HandleAttemptStarted;
            runtime.GuardianEvent += HandleGuardianEvent;
            runtime.PrimaryAttackFired += HandlePrimaryAttackFired;
            bound = true;
        }

        private void HandlePrimaryAttackFired(Level01PrimaryAttackEvent item)
        {
            audioDirector?.PlayBroadside();
            haptics?.TryPulse(HapticCue.Broadside);
            attackFeedback?.Play(item.HitGuardian);
        }

        private void HandleAttemptStarted(BattleSession session)
        {
            if (audioDirector != null) audioDirector.Bind(session.Events);
            haptics?.Bind(session.Events);
            var saved = new SeaLion.Core.Persistence.LocalSaveRepository(System.IO.Path.Combine(
                Application.persistentDataPath, runtime.SaveFileName)).Load();
            if (saved.Succeeded && saved.Data != null)
            {
                var settings = saved.Data.settings;
                audioDirector?.ApplyPreferences(settings.musicVolume, settings.effectsVolume);
                if (haptics != null) haptics.EnabledBySetting = settings.haptics;
            }
        }

        private void ApplyState()
        {
            if (audioDirector != null)
                audioDirector.SetGateEnergyActive(runtime.Phase == Level01TrialPhase.Traversal && !runtime.GateCommitted);
        }

        private void ApplyPhase(Level01TrialPhase phase)
        {
            if (phaseTransition != null) phaseTransition.Present(phase);
            else
            {
                SetActive(opening, phase == Level01TrialPhase.Opening);
                SetActive(traversal, phase == Level01TrialPhase.Traversal);
                SetActive(landing, phase == Level01TrialPhase.Landing);
                SetActive(assault, phase == Level01TrialPhase.Assault || phase == Level01TrialPhase.Failure);
                SetActive(victory, phase == Level01TrialPhase.Victory);
            }
            if (flagship != null && phase == Level01TrialPhase.Traversal)
            {
                flagship.transform.position = flagshipStart;
                input.Reset();
            }

            if (audioDirector == null) return;
            audioDirector.SetGateEnergyActive(phase == Level01TrialPhase.Traversal && !runtime.GateCommitted);
            if (phase == Level01TrialPhase.Opening || phase == Level01TrialPhase.Traversal ||
                phase == Level01TrialPhase.Landing) audioDirector.EnterTraversal();
            else if (phase == Level01TrialPhase.Assault) audioDirector.EnterAssault();
        }

        private void HandleGuardianEvent(HarborGuardianEvent item)
        {
            if (item.Type == HarborGuardianEventType.AttackFired)
            {
                audioDirector?.PlayBroadside();
                haptics?.TryPulse(HapticCue.Broadside);
            }
            else if (item.Type == HarborGuardianEventType.HitReaction) audioDirector?.PlayGuardianHit();
        }

        private void OnDestroy()
        {
            if (!bound || runtime == null) return;
            runtime.PhaseChanged -= ApplyPhase;
            runtime.StateChanged -= ApplyState;
            runtime.AttemptStarted -= HandleAttemptStarted;
            runtime.GuardianEvent -= HandleGuardianEvent;
            runtime.PrimaryAttackFired -= HandlePrimaryAttackFired;
        }

        private static GameObject Find(Scene scene, string objectName)
        {
            var roots = scene.GetRootGameObjects();
            for (var index = 0; index < roots.Length; index++)
            {
                var found = FindChild(roots[index].transform, objectName);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject FindChild(Transform root, string objectName)
        {
            if (root.name == objectName) return root.gameObject;
            for (var index = 0; index < root.childCount; index++)
            {
                var found = FindChild(root.GetChild(index), objectName);
                if (found != null) return found;
            }
            return null;
        }

        private static T FindComponent<T>(Scene scene) where T : Component
        {
            var roots = scene.GetRootGameObjects();
            for (var index = 0; index < roots.Length; index++)
            {
                var found = roots[index].GetComponentInChildren<T>(true);
                if (found != null) return found;
            }
            return null;
        }

        private static Bounds CalculatePresentationBounds(GameObject phase, GameObject flagshipObject)
        {
            var renderers = new System.Collections.Generic.List<Renderer>();
            AddRenderers(flagshipObject, renderers);
            AddRenderers(FindChild(phase.transform, "PLAYER__SecondLateenAndHelm"), renderers);
            AddRenderers(FindChild(phase.transform, "CHARACTER__Hayreddin_OnDeck"), renderers);
            AddRenderers(FindChild(phase.transform, "PROP__FlagshipLionWaveBanner"), renderers);
            if (renderers.Count == 0) return new Bounds(flagshipObject.transform.position,
                new Vector3(5f, 4f, 8f));
            var bounds = renderers[0].bounds;
            for (var index = 1; index < renderers.Count; index++) bounds.Encapsulate(renderers[index].bounds);
            return bounds;
        }

        private static void AddRenderers(GameObject target,
            System.Collections.Generic.List<Renderer> renderers)
        {
            if (target != null) renderers.AddRange(target.GetComponentsInChildren<Renderer>(true));
        }

        private static void SetActive(GameObject target, bool active)
        {
            if (target != null && target.activeSelf != active) target.SetActive(active);
        }
    }
}
