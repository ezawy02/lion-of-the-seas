#if UNITY_EDITOR
using System.Collections.Generic;
using SeaLion.Presentation.Vfx;
using UnityEditor;
using UnityEngine;

namespace SeaLion.Editor
{
    /// <summary>Editor-only smoke validation for the T048 water benchmark family.</summary>
    public static class WaterVfxAssetValidator
    {
        private static readonly string[] RequiredMaterials =
        {
            "Assets/_Project/Materials/Water/SeaLion_Water_Primary.mat",
            "Assets/_Project/Materials/Water/SeaLion_Water_Reduced.mat",
            "Assets/_Project/Materials/Water/SeaLion_Foam_Primary.mat",
            "Assets/_Project/Materials/Water/SeaLion_Foam_Reduced.mat"
        };

        private static readonly string[] RequiredPrefabs =
        {
            "Assets/_Project/VFX/WaterSurface.prefab",
            "Assets/_Project/VFX/Wake.prefab",
            "Assets/_Project/VFX/FoamPatch.prefab",
            "Assets/_Project/VFX/LandingSplash.prefab",
            "Assets/_Project/VFX/HitSplash.prefab",
            "Assets/_Project/VFX/BossReaction.prefab"
        };

        [MenuItem("Sea Lion/Validation/T048 Water Benchmark")]
        public static void ValidateFromMenu()
        {
            var errors = Validate();
            if (errors.Count == 0) Debug.Log("T048 water benchmark validation passed.");
            else foreach (var error in errors) Debug.LogError(error);
        }

        public static IReadOnlyList<string> Validate()
        {
            var errors = new List<string>();
            AssetDatabase.ImportAsset("Assets/_Project/VFX/WaterSurface.prefab", ImportAssetOptions.ForceSynchronousImport);
            var shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/_Project/Materials/Water/SeaLionWater.shader");
            if (shader == null) errors.Add("T048: SeaLionWater.shader is missing or failed to import.");
            for (var i = 0; i < RequiredMaterials.Length; i++)
            {
                var material = AssetDatabase.LoadAssetAtPath<Material>(RequiredMaterials[i]);
                if (material == null) { errors.Add("T048: missing material " + RequiredMaterials[i]); continue; }
                if (material.shader != shader) errors.Add("T048: material does not use SeaLionWater.shader: " + RequiredMaterials[i]);
            }

            for (var i = 0; i < RequiredPrefabs.Length; i++)
            {
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(RequiredPrefabs[i]);
                if (prefab == null) { errors.Add("T048: missing prefab " + RequiredPrefabs[i]); continue; }
                if (prefab.GetComponent<MeshRenderer>() == null) errors.Add("T048: prefab has no renderer: " + RequiredPrefabs[i]);
                var preview = PrefabUtility.InstantiatePrefab(prefab) as GameObject;
                if (preview == null) { errors.Add("T048: prefab could not instantiate: " + RequiredPrefabs[i]); continue; }
                try
                {
                    var meshFilter = preview.GetComponent<MeshFilter>();
                    if (meshFilter == null || meshFilter.sharedMesh == null || meshFilter.sharedMesh.vertexCount < 4)
                        errors.Add("T048: prefab did not build a readable mesh: " + RequiredPrefabs[i]);
                }
                finally { Object.DestroyImmediate(preview); }
                if (RequiredPrefabs[i].EndsWith("WaterSurface.prefab"))
                {
                    if (prefab.GetComponent<WaterSurface>() == null) errors.Add("T048: WaterSurface component missing.");
                }
                else if (prefab.GetComponent<WaterVfxEffect>() == null) errors.Add("T048: pooled WaterVfxEffect missing: " + RequiredPrefabs[i]);
            }
            return errors;
        }
    }
}
#endif
