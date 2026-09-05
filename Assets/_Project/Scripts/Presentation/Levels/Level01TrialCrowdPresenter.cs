using System;
using System.Collections.Generic;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Levels;
using SeaLion.Presentation.Quality;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Renders Level 1 ordinary combatants as count-driven, baked-pose GPU instances.</summary>
    [DisallowMultipleComponent]
    public sealed class Level01TrialCrowdPresenter : MonoBehaviour
    {
        private sealed class CrowdBatch
        {
            public Mesh Mesh;
            public Material Material;
            public Renderer[] Sources;
            public Matrix4x4[] BaseMatrices;
            public Matrix4x4[] DrawMatrices;
            public int Layer;
            public int VisibleCount;
        }

        private static readonly string[] FriendlyGroups =
        {
            "FRIENDLY__LandingForce_Front", "FRIENDLY__LandingForce_Rear"
        };

        private static readonly string[] HostileGroups =
        {
            "HOSTILE__Defenders_Front", "HOSTILE__Defenders_Rear"
        };

        private Level01TrialRuntime runtime;
        private QualityProfileController quality;
        private GameObject landingRoot;
        private GameObject assaultRoot;
        private CrowdBatch landingFriendly;
        private CrowdBatch friendly;
        private CrowdBatch hostile;
        private float friendlyPulse;
        private float hostilePulse;
        private bool bound;

        public int FriendlyVisibleCount => friendly == null ? 0 : friendly.VisibleCount;
        public int HostileVisibleCount => hostile == null ? 0 : hostile.VisibleCount;
        public int SourceRendererCount => CountSources(landingFriendly) + CountSources(friendly) +
            CountSources(hostile);
        public bool UsesGpuInstancing => bound && landingFriendly != null && friendly != null && hostile != null;

        public bool Bind(Level01TrialRuntime trialRuntime, GameObject landing, GameObject assault,
            QualityProfileController qualityController)
        {
            Release();
            runtime = trialRuntime;
            quality = qualityController;
            landingRoot = landing;
            assaultRoot = assault;
            landingFriendly = CreateBatch(landing, new[] { "FRIENDLY__LandingForce" }, "LandingFriendly");
            friendly = CreateBatch(assault, FriendlyGroups, "Friendly");
            hostile = CreateBatch(assault, HostileGroups, "Hostile");
            bound = runtime != null && landingRoot != null && assaultRoot != null &&
                landingFriendly != null && friendly != null && hostile != null;
            return bound;
        }

        private void LateUpdate()
        {
            if (!bound || runtime == null) return;
            var scale = QualityScale();
            if (runtime.Phase == Level01TrialPhase.Landing && landingRoot.activeInHierarchy)
            {
                landingFriendly.VisibleCount = Level01CrowdPresentationBudget.FriendlyVisibleCount(
                    runtime.DisplayedForceCount, runtime.DisplayCap, landingFriendly.BaseMatrices.Length, scale);
                FillAndDraw(landingFriendly, true, 0f, 2.4f, 9f);
                return;
            }
            if (assaultRoot == null || !assaultRoot.activeInHierarchy) return;
            var friendlyCount = Level01CrowdPresentationBudget.FriendlyVisibleCount(
                runtime.DisplayedForceCount, runtime.DisplayCap, friendly.BaseMatrices.Length, scale);
            var hostileCount = Level01CrowdPresentationBudget.HostileVisibleCount(
                runtime.HostileRemaining, runtime.InitialHostileCombatants, hostile.BaseMatrices.Length, scale);
            if (friendly.VisibleCount > 0 && friendlyCount < friendly.VisibleCount) friendlyPulse = .42f;
            if (hostile.VisibleCount > 0 && hostileCount < hostile.VisibleCount) hostilePulse = .42f;
            friendly.VisibleCount = friendlyCount;
            hostile.VisibleCount = hostileCount;
            FillAndDraw(friendly, true, friendlyPulse, 4.2f, 12f);
            FillAndDraw(hostile, false, hostilePulse, 2.3f, 12f);
            friendlyPulse = Mathf.Max(0f, friendlyPulse - Time.unscaledDeltaTime);
            hostilePulse = Mathf.Max(0f, hostilePulse - Time.unscaledDeltaTime);
        }

        private void FillAndDraw(CrowdBatch batch, bool isFriendly, float pulse,
            float travelDistance, float travelSeconds)
        {
            var count = batch == null ? 0 : batch.VisibleCount;
            if (count <= 0 || batch.Mesh == null || batch.Material == null) return;
            var time = runtime.TotalElapsed;
            var progress = Mathf.Clamp01(runtime.PhaseElapsed / Mathf.Max(.1f, travelSeconds));
            var pulse01 = pulse <= 0f ? 0f : Mathf.Sin(Mathf.Clamp01(pulse / .42f) * Mathf.PI);
            for (var index = 0; index < count; index++)
            {
                var sourceIndex = Level01CrowdPresentationBudget.SourceIndex(index, count,
                    batch.BaseMatrices.Length);
                var source = batch.BaseMatrices[sourceIndex];
                var position = (Vector3)source.GetColumn(3);
                var phase = sourceIndex * .71f;
                var stride = Mathf.Sin(time * 4.8f + phase);
                position.y += Mathf.Abs(stride) * .045f;
                position.z += progress * (isFriendly ? travelDistance : -travelDistance);
                var rotation = source.rotation * Quaternion.Euler(Mathf.Abs(stride) * 1.4f,
                    Mathf.Sin(time * 2.4f + phase) * .65f, 0f);
                var scale = source.lossyScale * (1f + pulse01 * .12f);
                batch.DrawMatrices[index] = Matrix4x4.TRS(position, rotation, scale);
            }

            Graphics.DrawMeshInstanced(batch.Mesh, 0, batch.Material, batch.DrawMatrices, count,
                null, ShadowCastingMode.Off, false, batch.Layer, null, LightProbeUsage.Off);
        }

        private static CrowdBatch CreateBatch(GameObject root, IReadOnlyList<string> groupNames,
            string label)
        {
            if (root == null) return null;
            var renderers = new List<SkinnedMeshRenderer>(128);
            for (var index = 0; index < groupNames.Count; index++)
            {
                var group = Find(root.transform, groupNames[index]);
                if (group != null) renderers.AddRange(group.GetComponentsInChildren<SkinnedMeshRenderer>(true));
            }
            if (renderers.Count == 0 || renderers[0].sharedMesh == null ||
                renderers[0].sharedMaterial == null) return null;

            var mesh = new Mesh { name = "Level01_" + label + "_BakedPose_Runtime" };
            renderers[0].BakeMesh(mesh, false);
            var material = new Material(renderers[0].sharedMaterial)
            {
                name = "Level01_" + label + "_Instanced_Runtime",
                enableInstancing = true
            };
            var sources = new Renderer[renderers.Count];
            var matrices = new Matrix4x4[renderers.Count];
            for (var index = 0; index < renderers.Count; index++)
            {
                sources[index] = renderers[index];
                matrices[index] = renderers[index].localToWorldMatrix;
                renderers[index].enabled = false;
            }
            return new CrowdBatch
            {
                Mesh = mesh,
                Material = material,
                Sources = sources,
                BaseMatrices = matrices,
                DrawMatrices = new Matrix4x4[renderers.Count],
                Layer = renderers[0].gameObject.layer
            };
        }

        private float QualityScale()
        {
            return quality != null && quality.ActiveProfile != null &&
                quality.ActiveProfile.ProfileKind == QualityProfileKind.Reduced ? .5f : 1f;
        }

        private void OnDestroy() => Release();

        private void Release()
        {
            RestoreAndDestroy(friendly);
            RestoreAndDestroy(hostile);
            RestoreAndDestroy(landingFriendly);
            friendly = hostile = landingFriendly = null;
            bound = false;
        }

        private static void RestoreAndDestroy(CrowdBatch batch)
        {
            if (batch == null) return;
            if (batch.Sources != null)
                for (var index = 0; index < batch.Sources.Length; index++)
                    if (batch.Sources[index] != null) batch.Sources[index].enabled = true;
            if (batch.Mesh != null) Destroy(batch.Mesh);
            if (batch.Material != null) Destroy(batch.Material);
        }

        private static int CountSources(CrowdBatch batch) =>
            batch == null || batch.Sources == null ? 0 : batch.Sources.Length;

        private static Transform Find(Transform root, string objectName)
        {
            if (root == null) return null;
            if (root.name == objectName) return root;
            for (var index = 0; index < root.childCount; index++)
            {
                var found = Find(root.GetChild(index), objectName);
                if (found != null) return found;
            }
            return null;
        }
    }
}
