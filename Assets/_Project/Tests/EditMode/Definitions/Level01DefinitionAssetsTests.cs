using System.Linq;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using UnityEditor;

namespace SeaLion.Tests.EditMode.Definitions
{
    public sealed class Level01DefinitionAssetsTests
    {
        private const string Root = "Assets/_Project/Data/Levels/Level01";

        [Test]
        public void EveryLevel01DefinitionLoadsAndValidates()
        {
            var paths = AssetDatabase.FindAssets("t:DefinitionAsset", new[] { Root })
                .Select(AssetDatabase.GUIDToAssetPath).ToArray();
            Assert.That(paths, Has.Length.EqualTo(12));
            foreach (var path in paths)
            {
                var definition = AssetDatabase.LoadAssetAtPath<DefinitionAsset>(path);
                Assert.That(definition, Is.Not.Null, path);
                Assert.That(definition.Validate(), Is.Empty, path);
            }
        }

        [Test]
        public void LevelContainsLandingGuardianAndMultiplierMoment()
        {
            var level = AssetDatabase.LoadAssetAtPath<LevelDefinition>(Root + "/Level01.asset");
            var gate = AssetDatabase.LoadAssetAtPath<GateDefinition>(Root + "/Level01_Gate_Easy.asset");
            Assert.That(level.Phases, Does.Contain(new StableId("level01-landing")));
            Assert.That(level.Encounters, Does.Contain(new StableId("boss-level01-harbor-guardian")));
            Assert.That(gate.Outcome, Is.EqualTo(GateOutcome.Multiply));
            Assert.That(gate.Value, Is.EqualTo(4f));
        }
    }
}
