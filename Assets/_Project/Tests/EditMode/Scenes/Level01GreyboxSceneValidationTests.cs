using System.Linq;
using NUnit.Framework;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Scenes
{
    public sealed class Level01GreyboxSceneValidationTests
    {
        const string Path = "Assets/_Project/Scenes/Level_01_HundredSails.unity";
        static readonly string[] Required = { "ANCHOR_01_FlagshipLane_Start", "ANCHOR_02_GateChoice_Easy_x4", "ANCHOR_03_GateChoice_Risky_Damage1", "ANCHOR_04_PrisonerRescue_Sailmakers", "ANCHOR_05_BeachLanding_Transfer", "GREYBOX_FIELD__DefenderField", "ANCHOR_06_HarborGuardian_Entry", "PORTRAIT_CAMERA__Level01Opening", "KEY_LIGHT__Level01Greybox" };

        [Test]
        public void OpensWithReadableLevel01GreyboxAnchors()
        {
            var scene = EditorSceneManager.OpenScene(Path, OpenSceneMode.Additive);
            try
            {
                foreach (var name in Required) Assert.That(scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Any(t => t.name == name), Is.True, name);
                var anchors = Required.Where(n => n.StartsWith("ANCHOR_")).Select(n => scene.GetRootGameObjects().SelectMany(r => r.GetComponentsInChildren<Transform>(true)).Single(t => t.name == n)).ToArray();
                Assert.That(anchors[0].position.z, Is.LessThan(anchors[1].position.z));
                Assert.That(anchors[1].position.z, Is.EqualTo(anchors[2].position.z).Within(.01f));
                Assert.That(anchors[2].position.z, Is.LessThan(anchors[3].position.z));
                Assert.That(anchors[3].position.z, Is.LessThan(anchors[4].position.z));
                Assert.That(anchors[4].position.z, Is.LessThan(anchors[5].position.z));
                Assert.That(Vector3.Distance(anchors[1].position, anchors[2].position), Is.GreaterThan(8f));
                Assert.That(anchors[4].position.z - anchors[3].position.z, Is.GreaterThan(20f));
                Assert.That(anchors[5].position.z - anchors[4].position.z, Is.GreaterThan(15f));
            }
            finally { EditorSceneManager.CloseScene(scene, true); }
        }
    }
}
