using System;
using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    /// <summary>Recovers cloth detail on the close captain without changing shared art materials.</summary>
    [DisallowMultipleComponent]
    public sealed class Level01CharacterHighlightPresenter : MonoBehaviour
    {
        private static readonly int CompressionId = Shader.PropertyToID("_HighlightCompression");
        private static readonly int TintId = Shader.PropertyToID("_HighlightTint");
        private readonly List<Renderer> renderers = new List<Renderer>(8);
        private readonly List<MaterialPropertyBlock> originals = new List<MaterialPropertyBlock>(8);

        public int RendererCount => renderers.Count;

        public void Bind(params GameObject[] phaseRoots)
        {
            Restore();
            if (phaseRoots == null) return;
            for (var rootIndex = 0; rootIndex < phaseRoots.Length; rootIndex++)
            {
                var root = phaseRoots[rootIndex];
                if (root == null) continue;
                var candidates = root.GetComponentsInChildren<Renderer>(true);
                for (var index = 0; index < candidates.Length; index++)
                {
                    var candidate = candidates[index];
                    if (candidate.name.IndexOf("Hayreddin", StringComparison.OrdinalIgnoreCase) < 0 &&
                        candidate.transform.root.name.IndexOf("Hayreddin", StringComparison.OrdinalIgnoreCase) < 0 &&
                        !HasNamedAncestor(candidate.transform, "Hayreddin")) continue;
                    var original = new MaterialPropertyBlock();
                    candidate.GetPropertyBlock(original);
                    var revised = new MaterialPropertyBlock();
                    candidate.GetPropertyBlock(revised);
                    revised.SetFloat(CompressionId, .78f);
                    revised.SetColor(TintId, new Color(.9f, .76f, .56f, 1f));
                    candidate.SetPropertyBlock(revised);
                    renderers.Add(candidate);
                    originals.Add(original);
                }
            }
        }

        private static bool HasNamedAncestor(Transform value, string token)
        {
            while (value != null)
            {
                if (value.name.IndexOf(token, StringComparison.OrdinalIgnoreCase) >= 0) return true;
                value = value.parent;
            }
            return false;
        }

        private void OnDestroy() => Restore();

        private void Restore()
        {
            for (var index = 0; index < renderers.Count; index++)
                if (renderers[index] != null) renderers[index].SetPropertyBlock(originals[index]);
            renderers.Clear();
            originals.Clear();
        }
    }
}
