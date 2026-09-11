using SeaLion.Gameplay.Levels;
using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    public readonly struct Level01CameraPreset
    {
        public readonly Vector3 Position;
        public readonly Vector3 LookAt;
        public readonly float FieldOfView;

        public Level01CameraPreset(Vector3 position, Vector3 lookAt, float fieldOfView)
        {
            Position = position;
            LookAt = lookAt;
            FieldOfView = fieldOfView;
        }
    }

    /// <summary>Keeps the portrait camera close to the active Level 1 objective.</summary>
    [DisallowMultipleComponent]
    public sealed class Level01PhaseCameraPresenter : MonoBehaviour
    {
        private const float PositionSharpness = 1.85f;
        private const float RotationSharpness = 2.15f;
        private const float LensSharpness = 2.05f;

        private Level01TrialRuntime runtime;
        private Camera targetCamera;
        private Level01CameraPreset target;
        private GameObject openingRoot;
        private GameObject traversalRoot;
        private GameObject landingRoot;
        private GameObject assaultRoot;
        private GameObject victoryRoot;
        private Transform phaseFocus;
        private Transform assaultCommander;
        private Transform assaultGuardian;
        private Vector3 phaseFocusOrigin;
        private bool bound;

        public Level01TrialPhase CurrentPhase { get; private set; } = Level01TrialPhase.Loading;

        public void Bind(Level01TrialRuntime trialRuntime, Camera camera, GameObject opening,
            GameObject traversal, GameObject landing, GameObject assault, GameObject victory = null)
        {
            Unbind();
            runtime = trialRuntime;
            targetCamera = camera;
            openingRoot = opening;
            traversalRoot = traversal;
            landingRoot = landing;
            assaultRoot = assault;
            victoryRoot = victory;
            assaultCommander = Find(assault, "HOSTILE__EnemyCommander_REVIEW");
            assaultGuardian = Find(assault, "BOSS__HarborGuardian");
            if (runtime == null || targetCamera == null) return;
            runtime.PhaseChanged += SetPhase;
            bound = true;
            ApplyImmediate(runtime.Phase);
        }

        public void ApplyImmediate(Level01TrialPhase phase)
        {
            SetPhase(phase);
            if (targetCamera == null) return;
            targetCamera.transform.SetPositionAndRotation(target.Position, RotationFor(target));
            targetCamera.fieldOfView = target.FieldOfView;
        }

        public static Level01CameraPreset PresetFor(Level01TrialPhase phase)
        {
            switch (phase)
            {
                case Level01TrialPhase.Traversal:
                    return new Level01CameraPreset(new Vector3(-.35f, 8.8f, -3.5f),
                        new Vector3(0f, 2.2f, 28f), 33f);
                case Level01TrialPhase.Landing:
                    return new Level01CameraPreset(new Vector3(-.55f, 8.9f, 22f),
                        new Vector3(0f, 2.8f, 66f), 33f);
                case Level01TrialPhase.Assault:
                case Level01TrialPhase.Failure:
                    return new Level01CameraPreset(new Vector3(-.7f, 8.2f, 40f),
                        new Vector3(0f, 3.1f, 69f), 30f);
                case Level01TrialPhase.Victory:
                    return new Level01CameraPreset(new Vector3(-.35f, 7.6f, 52f),
                        new Vector3(.35f, 3.1f, 84f), 29f);
                default:
                    return new Level01CameraPreset(new Vector3(0f, 8.2f, -11f),
                        new Vector3(0f, 1.7f, 22f), 36f);
            }
        }

        public static float CorridorDepth(Level01CameraPreset preset)
        {
            return preset.LookAt.z - preset.Position.z;
        }

        private void SetPhase(Level01TrialPhase phase)
        {
            CurrentPhase = phase;
            target = PresetFor(phase);
            phaseFocus = FocusFor(phase);
            phaseFocusOrigin = phaseFocus == null ? Vector3.zero : phaseFocus.position;
        }

        private void LateUpdate()
        {
            if (targetCamera == null) return;
            var deltaTime = Time.unscaledDeltaTime;
            var positionBlend = 1f - Mathf.Exp(-PositionSharpness * deltaTime);
            var rotationBlend = 1f - Mathf.Exp(-RotationSharpness * deltaTime);
            var lensBlend = 1f - Mathf.Exp(-LensSharpness * deltaTime);
            var cameraTransform = targetCamera.transform;
            var frameTarget = FollowTarget();
            cameraTransform.position = Vector3.Lerp(cameraTransform.position, frameTarget.Position, positionBlend);
            cameraTransform.rotation = Quaternion.Slerp(cameraTransform.rotation,
                RotationFor(frameTarget), rotationBlend);
            targetCamera.fieldOfView = Mathf.Lerp(targetCamera.fieldOfView, frameTarget.FieldOfView, lensBlend);
        }

        public static float FollowFactor(Level01TrialPhase phase)
        {
            switch (phase)
            {
                case Level01TrialPhase.Opening: return .45f;
                case Level01TrialPhase.Traversal: return .78f;
                case Level01TrialPhase.Landing: return .82f;
                case Level01TrialPhase.Victory: return .28f;
                default: return 0f;
            }
        }

        public static float CombatPushIn(int hostileRemaining, float bossHealth01)
        {
            if (hostileRemaining > 0) return Mathf.Clamp01((8f - hostileRemaining) / 8f) * .45f;
            return .45f + (1f - Mathf.Clamp01(bossHealth01)) * .55f;
        }

        private Level01CameraPreset FollowTarget()
        {
            var position = target.Position;
            var lookAt = target.LookAt;
            if (phaseFocus != null)
            {
                var delta = phaseFocus.position - phaseFocusOrigin;
                var follow = FollowFactor(CurrentPhase);
                position += new Vector3(delta.x * .34f, delta.y * .12f, delta.z * follow);
                lookAt += new Vector3(delta.x * .18f, delta.y * .08f, delta.z * follow * .68f);
            }
            if (CurrentPhase == Level01TrialPhase.Opening && runtime != null)
            {
                var intro = Mathf.Clamp01(runtime.PhaseElapsed / 2f);
                var next = PresetFor(Level01TrialPhase.Traversal);
                position = Vector3.Lerp(position, next.Position, intro * .4f);
                lookAt = Vector3.Lerp(lookAt, next.LookAt, intro * .4f);
            }
            else if (CurrentPhase == Level01TrialPhase.Traversal && runtime != null)
            {
                var push = Level01SeaMotion.ShoreBlend01(runtime.RouteProgress, runtime.GateCommitted);
                var next = PresetFor(Level01TrialPhase.Landing);
                position = Vector3.Lerp(position, next.Position, push);
                lookAt = Vector3.Lerp(lookAt, next.LookAt, push);
            }
            var fov = target.FieldOfView;
            if (CurrentPhase == Level01TrialPhase.Assault && runtime != null)
            {
                var content = runtime.GuardianTelegraphing || runtime.HostileRemaining <= 0
                    ? assaultGuardian : assaultCommander;
                if (content != null)
                    lookAt = Vector3.Lerp(lookAt, content.position + Vector3.up * 2.1f, .58f);
                var push = CombatPushIn(runtime.HostileRemaining, runtime.BossHealth01);
                if (runtime.GuardianTelegraphing) push = Mathf.Max(push, .74f);
                else if (runtime.EliteGuardActive) push = Mathf.Max(push, .38f);
                position += new Vector3(0f, -.7f * push, 11f * push);
                lookAt += new Vector3(0f, .35f * push, 6.5f * push);
                if (runtime.GuardianTelegraphing) fov -= 2.2f;
                else if (runtime.EliteGuardActive) fov -= 1.1f;
            }
            return new Level01CameraPreset(position, lookAt, fov);
        }

        private Transform FocusFor(Level01TrialPhase phase)
        {
            switch (phase)
            {
                case Level01TrialPhase.Opening: return Find(openingRoot, "PLAYER__Flagship");
                case Level01TrialPhase.Traversal: return Find(traversalRoot, "PLAYER__Flagship");
                case Level01TrialPhase.Landing: return Find(landingRoot, "CRAFT__LandingFan_3");
                case Level01TrialPhase.Assault: return Find(assaultRoot, "PLAYER__BattleFlagship");
                case Level01TrialPhase.Victory: return Find(victoryRoot, "CHARACTER__Hayreddin_Victory");
                default: return null;
            }
        }

        private static Transform Find(GameObject root, string objectName)
        {
            if (root == null) return null;
            var values = root.GetComponentsInChildren<Transform>(true);
            for (var index = 0; index < values.Length; index++)
                if (values[index].name == objectName) return values[index];
            return null;
        }

        private static Quaternion RotationFor(Level01CameraPreset preset)
        {
            return Quaternion.LookRotation(preset.LookAt - preset.Position, Vector3.up);
        }

        private void Unbind()
        {
            if (bound && runtime != null) runtime.PhaseChanged -= SetPhase;
            bound = false;
        }

        private void OnDestroy()
        {
            Unbind();
        }
    }
}
