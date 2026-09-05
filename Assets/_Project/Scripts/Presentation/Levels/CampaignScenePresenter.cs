using System.Collections;
using System.Collections.Generic;
using SeaLion.Gameplay.Levels;
using SeaLion.Gameplay.Input;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Runtime binding for the existing Chain Strait and Storm Fortress art scenes.
    /// Visual placement remains pending in-engine review.</summary>
    public sealed class CampaignScenePresenter : MonoBehaviour
    {
        [SerializeField] private string artSceneName;
        [SerializeField] private GameObject friendlyModel, hostileModel;
        [SerializeField] private Material friendlyMaterial, hostileMaterial;
        [SerializeField] private Audio.Level01AudioLibrary audioLibrary;
        private Audio.Level01AudioDirector audioDirector;
        private CampaignCrowdPresenter crowd;
        private Haptics.HapticsController haptics;
        public bool IsReady => ready;
        private Level01TrialRuntime runtime;
        private FlagshipInputAdapter input;
        private Transform ship;
        private Transform boss;
        private Transform outerGate;
        private Camera gameplayCamera;
        private Vector3 shipStart, cameraStart;
        private Quaternion cameraRotation;
        private readonly List<Transform> chain = new List<Transform>();
        private readonly List<Transform> mines = new List<Transform>();
        private readonly List<Vector3> mineStarts = new List<Vector3>();
        private bool ready;

        private IEnumerator Start()
        {
            runtime = GetComponent<Level01TrialRuntime>();
            input = GetComponent<FlagshipInputAdapter>();
            if (input == null) input = gameObject.AddComponent<FlagshipInputAdapter>();
            var scene = SceneManager.GetSceneByName(artSceneName);
            if (!scene.IsValid() || !scene.isLoaded)
            {
                var load = SceneManager.LoadSceneAsync(artSceneName, LoadSceneMode.Additive);
                if (load == null) { Debug.LogError("Campaign art scene is absent from the build.", this); yield break; }
                yield return load;
                scene = SceneManager.GetSceneByName(artSceneName);
            }
            foreach (var root in scene.GetRootGameObjects())
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                {
                    if (node.name == "PLAYER__Flagship") ship = node;
                    if (node.name == "BOSS__ArmoredWarship" || node.name == "BOSS__StormCommander") boss = node;
                    if (node.name == "OBJECTIVE__OuterGate") outerGate = node;
                    if (node.name.StartsWith("OBJECTIVE__ChainUnit")) chain.Add(node);
                    if (node.name.StartsWith("HAZARD__Mine")) { mines.Add(node); mineStarts.Add(node.position); }
                    if (node.TryGetComponent<Camera>(out var camera)) gameplayCamera = camera;
                }
            if (runtime == null || ship == null || boss == null || gameplayCamera == null)
            { Debug.LogError("Campaign scene requires flagship, boss and camera anchors.", this); yield break; }
            shipStart = ship.position; cameraStart = gameplayCamera.transform.position;
            cameraRotation = gameplayCamera.transform.rotation;
            crowd = gameObject.AddComponent<CampaignCrowdPresenter>();
            crowd.Bind(runtime, boss, friendlyModel, hostileModel, friendlyMaterial, hostileMaterial);
            // Replace the authored static assault group with the count-driven formation.
            foreach (var root in scene.GetRootGameObjects())
                foreach (var node in root.GetComponentsInChildren<Transform>(true))
                    if (node.name == "FRIENDLY__Assault" || node.name == "HOSTILE__FortressGuard")
                        foreach (var renderer in node.GetComponentsInChildren<Renderer>(true)) renderer.enabled = false;
            audioDirector = gameObject.AddComponent<Audio.Level01AudioDirector>();
            audioDirector.Configure(audioLibrary, false);
            haptics = gameObject.AddComponent<Haptics.HapticsController>();
            runtime.PrimaryAttackFired += OnPrimaryAttack;
            runtime.PhaseChanged += OnPhaseChanged;
            runtime.AttemptStarted += ResetPresentation;
            ready = runtime.Begin();
        }

        private void Update()
        {
            if (ready) runtime.SetSteeringIntent(input.HorizontalIntent, input.HasSteered);
        }

        private void LateUpdate()
        {
            if (!ready) return;
            ship.position = shipStart + new Vector3(runtime.HorizontalChoice * 7f,
                Mathf.Sin(runtime.TotalElapsed * 2f) * .05f, runtime.RouteProgress * 18f);
            foreach (var item in chain) if (item != null) item.gameObject.SetActive(runtime.BlockadeHealth01 > 0f);
            for (var i = 0; i < mines.Count; i++)
                mines[i].position = mineStarts[i] + Vector3.right *
                    Mathf.Sin(runtime.TotalElapsed + i * .7f) * 1.2f;
            if (outerGate != null) outerGate.gameObject.SetActive(runtime.AssaultStage == 1);
            var assault = runtime.Phase == Level01TrialPhase.Assault || runtime.CanRetry;
            var target = runtime.LevelNumber == 3 && runtime.AssaultStage == 1 && outerGate != null ? outerGate : boss;
            crowd?.SetObjective(target);
            if (assault)
            {
                var desired = target.position + new Vector3(0f, 16f, -22f);
                gameplayCamera.transform.position = Vector3.Lerp(gameplayCamera.transform.position, desired,
                    1f - Mathf.Exp(-Time.deltaTime * 3f));
                gameplayCamera.transform.LookAt(target.position + Vector3.up * 2f);
            }
        }

        private void ResetPresentation(SeaLion.Core.Battle.BattleSession session)
        {
            input.Reset();
            audioDirector.Bind(session.Events);
            haptics.Bind(session.Events);
            var saved = new SeaLion.Core.Persistence.LocalSaveRepository(System.IO.Path.Combine(Application.persistentDataPath,
                runtime.SaveFileName)).Load();
            if (saved.Succeeded && saved.Data != null)
            {
                audioDirector.ApplyPreferences(saved.Data.settings.musicVolume, saved.Data.settings.effectsVolume);
                haptics.EnabledBySetting = saved.Data.settings.haptics;
            }
            ship.position = shipStart;
            gameplayCamera.transform.SetPositionAndRotation(cameraStart, cameraRotation);
        }

        private void OnPrimaryAttack(Level01PrimaryAttackEvent item)
        { audioDirector?.PlayBroadside(); haptics?.TryPulse(Haptics.HapticCue.Broadside); }
        private void OnPhaseChanged(Level01TrialPhase phase)
        {
            if (phase == Level01TrialPhase.Assault) audioDirector?.EnterAssault();
            else if (phase == Level01TrialPhase.Victory) audioDirector?.EnterVictory();
            else if (phase == Level01TrialPhase.Failure) audioDirector?.EnterFailure();
            else audioDirector?.EnterTraversal();
        }
        private void OnDestroy()
        {
            if (runtime != null)
            {
                runtime.AttemptStarted -= ResetPresentation;
                runtime.PrimaryAttackFired -= OnPrimaryAttack;
                runtime.PhaseChanged -= OnPhaseChanged;
            }
        }
    }
}
