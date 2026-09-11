using SeaLion.Gameplay.Levels;
using UnityEngine;

namespace SeaLion.Presentation.Levels
{
    /// <summary>
    /// Keeps the authored opening coast and city readable during sea travel.
    /// Phase roots otherwise hide that horizon when Opening deactivates.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class Level01SharedHorizonPresenter : MonoBehaviour
    {
        public static readonly string[] SeaHorizonNames =
        {
            "ENV__LeftCoastalCliff", "ENV__RightArtilleryCliff",
            "ENV__LeftShoreFoot", "ENV__RightShoreFoot",
            "ENV__LeftCrownVegetation", "ENV__RightCrownVegetation",
            "CITY__MountainBackdrop", "CITY__FortressWall", "CITY__Gate",
            "CITY__Tower_Left", "CITY__Tower_Right",
            "CITY__TerraceHouse_00", "CITY__TerraceHouse_01",
            "CITY__TerraceHouse_02", "CITY__TerraceHouse_03",
            "FORTRESS__RightCliffTower", "FORTRESS__RightCliffCannon",
            "VFX__CannonMuzzleFlash_Core", "VFX__CannonMuzzleFlash_Plume",
            "VFX__CannonMuzzleLight"
        };

        private GameObject sharedRoot;

        public int SharedCount { get; private set; }
        public bool IsSeaHorizonVisible => sharedRoot != null && sharedRoot.activeSelf;

        public static bool ShowsSeaHorizon(Level01TrialPhase phase)
        {
            return phase == Level01TrialPhase.Opening || phase == Level01TrialPhase.Traversal;
        }

        public void Bind(GameObject opening, GameObject traversal)
        {
            if (sharedRoot == null)
            {
                sharedRoot = new GameObject("GROUP__SharedSeaHorizon");
                sharedRoot.transform.SetParent(transform, false);
            }

            SharedCount = 0;
            for (var index = 0; index < SeaHorizonNames.Length; index++)
            {
                var piece = FindChild(opening, SeaHorizonNames[index]);
                if (piece == null) continue;
                piece.transform.SetParent(sharedRoot.transform, true);
                SharedCount++;
            }

            HideDuplicates(traversal);
            sharedRoot.SetActive(false);
        }

        public void Present(Level01TrialPhase phase)
        {
            if (sharedRoot != null) sharedRoot.SetActive(ShowsSeaHorizon(phase));
        }

        private static void HideDuplicates(GameObject traversal)
        {
            if (traversal == null) return;
            for (var index = 0; index < SeaHorizonNames.Length; index++)
            {
                var duplicate = FindChild(traversal, SeaHorizonNames[index]);
                if (duplicate != null) duplicate.SetActive(false);
            }
        }

        private static GameObject FindChild(GameObject root, string objectName)
        {
            if (root == null) return null;
            var values = root.GetComponentsInChildren<Transform>(true);
            for (var index = 0; index < values.Length; index++)
                if (values[index].name == objectName) return values[index].gameObject;
            return null;
        }
    }
}
