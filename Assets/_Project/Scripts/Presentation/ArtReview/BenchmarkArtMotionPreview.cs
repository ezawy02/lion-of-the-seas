using System.Collections.Generic;
using UnityEngine;

namespace SeaLion.Presentation.ArtReview
{
    /// <summary>Deterministic lightweight motion for the playable Benchmark_Art review scene.</summary>
    public sealed class BenchmarkArtMotionPreview : MonoBehaviour
    {
        [SerializeField] private uint seed = 2701;
        [SerializeField] private float unitBob = 0.055f;
        [SerializeField] private float unitStride = 0.085f;
        [SerializeField] private float speed = 2.4f;

        private readonly List<Transform> units = new List<Transform>();
        private Vector3[] basePositions;
        private Quaternion[] baseRotations;

        public uint Seed => seed;
        public int ActiveUnitCount => units.Count;

        private void OnEnable()
        {
            if (!Application.isPlaying) return;
            CollectUnits();
        }

        private void Update()
        {
            if (!Application.isPlaying || units.Count == 0) return;
            var time = Time.time * speed;
            for (var index = 0; index < units.Count; index++)
            {
                var phase = (seed * 0.0001f + index * 0.6180339f) * Mathf.PI * 2f;
                var wave = Mathf.Sin(time + phase);
                var position = basePositions[index];
                position.y += Mathf.Abs(wave) * unitBob;
                position.z += wave * unitStride;
                units[index].localPosition = position;
                units[index].localRotation = baseRotations[index] * Quaternion.Euler(0f, wave * 2.2f, 0f);
            }
        }

        private void OnDisable()
        {
            if (basePositions == null) return;
            for (var index = 0; index < units.Count; index++)
            {
                if (units[index] == null) continue;
                units[index].localPosition = basePositions[index];
                units[index].localRotation = baseRotations[index];
            }
        }

        private void CollectUnits()
        {
            units.Clear();
            foreach (var groupName in new[]
            {
                "FRIENDLY__LandingForce_Front", "FRIENDLY__LandingForce_Rear",
                "HOSTILE__Defenders_Front", "HOSTILE__Defenders_Rear"
            })
            {
                var group = GameObject.Find(groupName);
                if (group == null) continue;
                for (var index = 0; index < group.transform.childCount; index++)
                    units.Add(group.transform.GetChild(index));
            }

            basePositions = new Vector3[units.Count];
            baseRotations = new Quaternion[units.Count];
            for (var index = 0; index < units.Count; index++)
            {
                basePositions[index] = units[index].localPosition;
                baseRotations[index] = units[index].localRotation;
            }
        }
    }
}
