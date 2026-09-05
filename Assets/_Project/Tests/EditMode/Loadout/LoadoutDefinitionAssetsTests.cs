using System.Linq;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.UI.Loadout;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace SeaLion.Tests.EditMode.Loadout
{
    public sealed class LoadoutDefinitionAssetsTests
    {
        const string Root = "Assets/_Project/Data/Loadouts/VerticalSlice";
        const string RewardPath = "Assets/_Project/Data/Rewards/Level01Blueprint.asset";
        const string PrefabPath = "Assets/_Project/Prefabs/UI/Loadout/LoadoutScreen_R2_REVIEW.prefab";

        [Test]
        public void VerticalSliceHasTwoValidMeaningfullyDifferentOptionsPerSlot()
        {
            var flagships = Load<FlagshipDefinition>();
            var crew = Load<UnitRoleDefinition>();
            var abilities = Load<CaptainAbilityDefinition>();
            Assert.That(flagships, Has.Length.EqualTo(2));
            Assert.That(crew, Has.Length.EqualTo(2));
            Assert.That(abilities, Has.Length.EqualTo(2));
            foreach (var definition in flagships.Cast<DefinitionAsset>()
                .Concat(crew).Concat(abilities))
                Assert.That(definition.Validate(), Is.Empty, definition.name);
            Assert.That(flagships.Select(value => value.DeployPattern).Distinct().Count(), Is.EqualTo(2));
            Assert.That(crew.Select(value => value.Role).Distinct().Count(), Is.EqualTo(2));
            Assert.That(abilities.Select(value => value.GameplayEffect.Outcome).Distinct().Count(), Is.EqualTo(2));
        }

        [Test]
        public void LevelOneBlueprintUnlocksAuthoredSailmakersCrew()
        {
            var reward = AssetDatabase.LoadAssetAtPath<RewardDefinition>(RewardPath);
            var crew = Load<UnitRoleDefinition>();
            Assert.That(reward, Is.Not.Null);
            Assert.That(reward.Validate(), Is.Empty);
            Assert.That(reward.GrantType, Is.EqualTo(RewardGrantType.Ownership));
            Assert.That(crew.Select(value => value.Id), Does.Contain(reward.GrantTargetId));
        }

        [Test]
        public void LoadoutReviewPrefabHasSixButtonsAndNoMissingComponents()
        {
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(PrefabPath);
            Assert.That(prefab, Is.Not.Null);
            Assert.That(prefab.GetComponentsInChildren<Component>(true), Has.None.Null);
            Assert.That(prefab.GetComponentsInChildren<LoadoutOptionButton>(true), Has.Length.EqualTo(6));
            Assert.That(prefab.GetComponentsInChildren<Button>(true), Has.Length.EqualTo(7));
        }

        static T[] Load<T>() where T : DefinitionAsset
        {
            return AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { Root })
                .Select(AssetDatabase.GUIDToAssetPath)
                .Select(AssetDatabase.LoadAssetAtPath<T>)
                .Where(value => value != null).ToArray();
        }
    }
}
