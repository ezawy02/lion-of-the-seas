using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Core.Persistence;
using SeaLion.Gameplay.Rewards;
using SeaLion.UI.Rewards;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Rewards
{
    public sealed class RewardGrantServiceTests
    {
        [Test]
        public void VictoryGrantsBlueprintAndMarksFutureAttemptEffect()
        {
            var files = new MemoryFiles();
            var repository = new LocalSaveRepository("memory/reward.json", files);
            string failure;
            Assert.IsTrue(repository.Save(LocalSaveRepository.CreateDefault(), out failure), failure);
            var reward = ScriptableObject.CreateInstance<RewardDefinition>();
            Set(reward, "reward-level01", "blueprint-sailmakers");

            RewardGrantResult result;
            Assert.IsTrue(new RewardGrantService(repository).TryGrant(true, reward, "tx-level01", out result), result.Failure);
            Assert.IsTrue(result.Applied);
            Assert.AreEqual(FutureAttemptEffect.BlueprintUnlocked, result.FutureAttemptEffect);
            Assert.Contains("blueprint-sailmakers", repository.Load().Data.ownedLoadoutIds);
        }

        [Test]
        public void ReplayedCompletionIsIdempotentAndRevealIdentifiesExistingBlueprint()
        {
            var files = new MemoryFiles();
            var repository = new LocalSaveRepository("memory/reward.json", files);
            string failure;
            Assert.IsTrue(repository.Save(LocalSaveRepository.CreateDefault(), out failure), failure);
            var reward = ScriptableObject.CreateInstance<RewardDefinition>();
            Set(reward, "reward-level01", "blueprint-sailmakers");
            var service = new RewardGrantService(repository);
            RewardGrantResult first, second;
            Assert.IsTrue(service.TryGrant(true, reward, "tx-level01", out first), first.Failure);
            Assert.IsTrue(service.TryGrant(true, reward, "tx-retry", out second), second.Failure);
            Assert.IsFalse(second.Applied);
            Assert.AreEqual(FutureAttemptEffect.BlueprintAlreadyUnlocked, second.FutureAttemptEffect);
            var view = new RewardRevealView();
            view.Present(second);
            Assert.IsTrue(view.State.Visible);
            Assert.IsFalse(view.State.NewlyEarned);
            StringAssert.Contains("next attempt", view.State.Message);
        }

        [Test]
        public void FailureDoesNotRevealOrGrant()
        {
            var repository = new LocalSaveRepository("memory/reward.json", new MemoryFiles());
            var reward = ScriptableObject.CreateInstance<RewardDefinition>();
            Set(reward, "reward-level01", "blueprint-sailmakers");
            RewardGrantResult result;
            Assert.IsFalse(new RewardGrantService(repository).TryGrant(false, reward, "tx-level01", out result));
            Assert.AreEqual(FutureAttemptEffect.None, result.FutureAttemptEffect);
            var view = new RewardRevealView();
            view.Present(result);
            Assert.IsFalse(view.State.Visible);
        }

        private static void Set(RewardDefinition reward, string rewardId, string targetId)
        {
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            typeof(DefinitionAsset).GetField("id", flags).SetValue(reward, new StableId(rewardId));
            typeof(RewardDefinition).GetField("grantTargetId", flags).SetValue(reward, new StableId(targetId));
        }

        private sealed class MemoryFiles : ILocalSaveFileSystem
        {
            private readonly System.Collections.Generic.Dictionary<string, string> data = new System.Collections.Generic.Dictionary<string, string>();
            public bool Exists(string path) { return data.ContainsKey(path); }
            public string ReadAllText(string path) { return data[path]; }
            public void WriteAllText(string path, string contents) { data[path] = contents; }
            public void Delete(string path) { data.Remove(path); }
            public void Replace(string temporaryPath, string destinationPath, string backupPath)
            { if (data.ContainsKey(destinationPath)) data[backupPath] = data[destinationPath]; data[destinationPath] = data[temporaryPath]; data.Remove(temporaryPath); }
        }
    }
}
