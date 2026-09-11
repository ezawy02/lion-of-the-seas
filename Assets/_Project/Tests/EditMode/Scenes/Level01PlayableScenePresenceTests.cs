using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Scenes
{
    public sealed class Level01PlayableScenePresenceTests
    {
        private const string Path = "Assets/_Project/Scenes/Level_01_HundredSails.unity";

        private static readonly string[] RequiredReuseAssets =
        {
            "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2_Optimized_REVIEW.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-010_Left_Coastal_Cliff_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-011_Right_Artillery_Cliff_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-012_Mediterranean_Mountain_City_Backdrop_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-006_Palm_Tree_Cluster_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-GAT-001_Multiplier_Gate_Arch_Buoy_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-001_Fortress_Wall_Module_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-003_Fortress_Main_Gate_Module_Optimized.fbx",
            "Assets/_Project/Art/Characters/L01-CHR-001_Hayreddin_Barbarossa_Rigged_Optimized_R2_LeadershipPose_REVIEW.fbx",
            "Assets/_Project/Art/Characters/L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx",
            "Assets/_Project/Art/Characters/L01-CHR-003_Hostile_Infantry_Rigged_Optimized.fbx",
            "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized.fbx",
            "Assets/_Project/Art/Ships/L01-SHP-002_Landing_Craft_Optimized.fbx",
            "Assets/_Project/Art/Ships/L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-004_Mediterranean_Harbor_Dock_Module_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-005_Mediterranean_Coastal_House_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-009_Shoreline_Rock_Sand_Cluster_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-013_Wooden_Landing_Gangway_REVIEW.fbx",
            "Assets/_Project/Art/Environment/L01-ENV-015_Fortress_R6_Modular_R5_VISIBLE_REVIEW.fbx",
            "Assets/_Project/Art/Environment/L01-PRP-001_Shore_Cannon_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-PRP-002_Lion_Wave_Banner_Optimized.fbx",
            "Assets/_Project/Art/Environment/L01-PRP-004_Captive_Sailmakers_Rescue_Raft_Cage_Optimized.fbx"
        };

        private static readonly string[] Required =
        {
            "PHASE__Opening_ReferenceMatch",
            "PHASE__Traversal_GateRescue_ReferenceMatch",
            "PHASE__BeachLanding_ReferenceMatch",
            "PHASE__BossBattle_Prototype_NoExecutionReference",
            "PHASE__VictoryReward_Prototype_NoExecutionReference",
            "PLAYER__Flagship",
            "GATE__Multiplier_x4",
            "UI3D__GateValue_X4",
            "RESCUE__CaptiveSailmakers",
            "ENV__LeftCoastalCliff",
            "ENV__RightArtilleryCliff",
            "CITY__MountainBackdrop",
            "ENV__PalmCluster_Left",
            "GROUP__Authored3DCoastline",
            "GROUP__LandingFortress_Right",
            "GROUP__BattleFortress_Approved",
            "FRIENDLY__LandingForce",
            "FRIENDLY__LandingForce_Front",
            "HOSTILE__Defenders_Front",
            "BOSS__HarborGuardian",
            "CHARACTER__Hayreddin_OnDeck",
            "CHARACTER__Hayreddin_Victory",
            "CRAFT__LandingFan_3",
            "FRIENDLY__GateCraft_0",
            "ESCORT__Port",
            "ESCORT__Starboard",
            "CITY__FortressWall",
            "CITY__TerraceHouse_00",
            "FORTRESS__RightCliffTower",
            "GROUP__BeachCityExtension_Approved",
            "ENV__WoodenLandingGangway_APPROVED"
        };

        [Test]
        public void PlayableArtSceneWiresTheReadableLevel01Loop()
        {
            var scene = EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive);
            try
            {
                var names = scene.GetRootGameObjects()
                    .SelectMany(root => root.GetComponentsInChildren<Transform>(true))
                    .Select(node => node.name)
                    .ToHashSet();
                foreach (var name in Required)
                    Assert.That(names.Contains(name), Is.True, name);
            }
            finally
            {
                EditorSceneManager.CloseScene(scene, true);
            }
        }

        [Test]
        public void PlayableArtSceneInstancesTheLockedLevel01PreparedFbx()
        {
            var yaml = File.ReadAllText(Path);
            foreach (var assetPath in RequiredReuseAssets)
            {
                var guid = AssetDatabase.AssetPathToGUID(assetPath);
                Assert.That(guid, Is.Not.Null.And.Not.Empty, assetPath);
                Assert.That(yaml.Contains(guid), Is.True, assetPath);
            }
        }
    }
}
