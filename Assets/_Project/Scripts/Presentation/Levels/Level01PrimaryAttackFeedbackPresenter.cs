using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Small pooled muzzle-to-target projectile feedback for the primary attack.</summary>
    [DisallowMultipleComponent]
    public sealed class Level01PrimaryAttackFeedbackPresenter : MonoBehaviour
    {
        private const int PoolSize = 2;
        private const float Duration = 0.22f;
        private readonly GameObject[] pool = new GameObject[PoolSize];
        private readonly float[] age = new float[PoolSize];
        private Transform source;
        private Transform hostile;
        private Transform guardian;
        private bool ready;

        public void Bind(Scene scene)
        {
            source = Find(scene, "PLAYER__BattleFlagship")?.transform;
            hostile = Find(scene, "HOSTILE__EnemyCommander_REVIEW")?.transform;
            guardian = Find(scene, "BOSS__HarborGuardian")?.transform;
            var material = Find(scene, "VFX__CannonMuzzleFlash_Core")?
                .GetComponentInChildren<Renderer>(true)?.sharedMaterial;
            for (var index = 0; index < pool.Length; index++)
            {
                if (pool[index] == null) pool[index] = CreateProjectile(index, material);
                pool[index].SetActive(false);
                age[index] = Duration;
            }
            ready = source != null;
        }

        public void Play(bool hitGuardian)
        {
            if (!ready) return;
            var slot = 0;
            for (var index = 0; index < pool.Length; index++)
                if (!pool[index].activeSelf) { slot = index; break; }
            var target = hitGuardian && guardian != null ? guardian.position + Vector3.up * 1.2f :
                hostile != null ? hostile.position + Vector3.up * 0.8f : source.position + source.forward * 7f;
            pool[slot].transform.position = source.position + source.forward * 1.6f + Vector3.up * 1.1f;
            pool[slot].transform.forward = target - pool[slot].transform.position;
            pool[slot].GetComponent<TrailRenderer>()?.Clear();
            age[slot] = 0f;
            pool[slot].SetActive(true);
            StartCoroutine(Fly(slot, target));
        }

        private IEnumerator Fly(int slot, Vector3 target)
        {
            var start = pool[slot].transform.position;
            while (age[slot] < Duration && pool[slot] != null)
            {
                age[slot] += Time.unscaledDeltaTime;
                var t = Mathf.Clamp01(age[slot] / Duration);
                pool[slot].transform.position = Vector3.Lerp(start, target, t);
                yield return null;
            }
            if (pool[slot] != null) pool[slot].SetActive(false);
        }

        private static GameObject CreateProjectile(int index, Material material)
        {
            var value = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            value.name = "VFX__PrimaryAttackProjectile_" + index;
            value.transform.localScale = Vector3.one * 0.16f;
            var renderer = value.GetComponent<Renderer>();
            if (material != null) renderer.sharedMaterial = material;
            var trail = value.AddComponent<TrailRenderer>();
            trail.time = 0.16f;
            trail.startWidth = 0.13f;
            trail.endWidth = 0.01f;
            trail.minVertexDistance = 0.04f;
            if (material != null) trail.sharedMaterial = material;
            Object.Destroy(value.GetComponent<Collider>());
            return value;
        }

        private static GameObject Find(Scene scene, string name)
        {
            var roots = scene.GetRootGameObjects();
            for (var index = 0; index < roots.Length; index++)
            {
                var found = FindChild(roots[index].transform, name);
                if (found != null) return found;
            }
            return null;
        }

        private static GameObject FindChild(Transform root, string name)
        {
            if (root.name == name) return root.gameObject;
            for (var index = 0; index < root.childCount; index++)
            {
                var found = FindChild(root.GetChild(index), name);
                if (found != null) return found;
            }
            return null;
        }
    }
}
