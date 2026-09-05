using SeaLion.Gameplay.Levels;
using UnityEngine;
using UnityEngine.Rendering;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Reuses registered crew models in bounded instanced campaign formations.</summary>
    public sealed class CampaignCrowdPresenter : MonoBehaviour
    {
        private Mesh friendlyMesh, hostileMesh;
        private Material friendlyMaterial, hostileMaterial;
        private readonly Matrix4x4[] matrices = new Matrix4x4[500];
        private Level01TrialRuntime runtime;
        private Transform objective;
        private Quality.QualityProfileController quality;

        public void Bind(Level01TrialRuntime battle, Transform target, GameObject friendly,
            GameObject hostile, Material friendlySurface, Material hostileSurface)
        {
            runtime = battle; objective = target;
            Bake(friendly, friendlySurface, out friendlyMesh, out friendlyMaterial);
            Bake(hostile, hostileSurface, out hostileMesh, out hostileMaterial);
            quality = FindFirstObjectByType<Quality.QualityProfileController>();
        }

        public void SetObjective(Transform target) { objective = target; }

        private void LateUpdate()
        {
            if (runtime == null || objective == null) return;
            var land = runtime.Phase == Level01TrialPhase.Landing || runtime.Phase == Level01TrialPhase.Assault || runtime.CanRetry;
            if (!land) return;
            var reduced = quality != null && quality.ActiveProfile != null &&
                quality.ActiveProfile.ProfileKind == Core.Definitions.QualityProfileKind.Reduced;
            var friendly = Mathf.Min(500, runtime.DisplayedForceCount);
            var hostile = runtime.Phase == Level01TrialPhase.Assault ? runtime.HostileRemaining : runtime.InitialHostileCombatants;
            if (reduced) { friendly = Mathf.CeilToInt(friendly * .5f); hostile = Mathf.CeilToInt(hostile * .5f); }
            Draw(friendlyMesh, friendlyMaterial, friendly, true);
            Draw(hostileMesh, hostileMaterial, hostile, false);
        }

        private void Draw(Mesh mesh, Material material, int count, bool friendly)
        {
            if (mesh == null || material == null || count <= 0) return;
            count = Mathf.Min(count, matrices.Length);
            var anchor = objective.position;
            for (var i = 0; i < count; i++)
            {
                var row = i / 12; var column = i % 12;
                var position = anchor + new Vector3((column - 5.5f) * .55f,
                    Mathf.Abs(Mathf.Sin(runtime.TotalElapsed * 5f + i)) * .035f,
                    friendly ? -6f - row * .6f : -1f + row * .6f);
                matrices[i] = Matrix4x4.TRS(position, Quaternion.Euler(0f, friendly ? 0f : 180f, 0f), Vector3.one * .65f);
            }
            Graphics.DrawMeshInstanced(mesh, 0, material, matrices, count, null,
                ShadowCastingMode.Off, false);
        }

        private static void Bake(GameObject model, Material surface, out Mesh mesh, out Material material)
        {
            mesh = null; material = null;
            if (model == null) return;
            var source = Instantiate(model);
            source.SetActive(false);
            var renderer = source.GetComponentInChildren<SkinnedMeshRenderer>(true);
            if (renderer != null && (surface != null || renderer.sharedMaterial != null))
            {
                mesh = new Mesh(); renderer.BakeMesh(mesh);
                material = new Material(surface != null ? surface : renderer.sharedMaterial) { enableInstancing = true };
            }
            Destroy(source);
        }

        private void OnDestroy()
        {
            if (friendlyMesh != null) Destroy(friendlyMesh);
            if (hostileMesh != null) Destroy(hostileMesh);
            if (friendlyMaterial != null) Destroy(friendlyMaterial);
            if (hostileMaterial != null) Destroy(hostileMaterial);
        }
    }
}
