using System;
using System.Collections.Generic;
using NUnit.Framework;
using SeaLion.Core.Persistence;

namespace SeaLion.Tests.EditMode.Persistence
{
    public sealed class LocalSaveRepositoryTests
    {
        private const string Path = "memory/save.json";

        [Test]
        public void CleanSaveThenFreshLoadRoundTripsData()
        {
            var files = new MemorySaveFileSystem();
            var repository = new LocalSaveRepository(Path, files);
            var data = LocalSaveRepository.CreateDefault();
            data.highestUnlockedLevel = 2;

            string failure;
            Assert.IsTrue(repository.Save(data, out failure), failure);
            var loaded = repository.Load();

            Assert.IsTrue(loaded.Succeeded, loaded.Failure);
            Assert.IsFalse(loaded.UsedDefault);
            Assert.AreEqual(2, loaded.Data.highestUnlockedLevel);
            Assert.IsTrue(files.Contains(Path));
            Assert.IsFalse(files.Contains(Path + ".tmp"));
        }

        [Test]
        public void MissingSaveReturnsFreshDefault()
        {
            var result = new LocalSaveRepository(Path, new MemorySaveFileSystem()).Load();
            Assert.IsTrue(result.Succeeded);
            Assert.IsTrue(result.UsedDefault);
            Assert.AreEqual(1, result.Data.highestUnlockedLevel);
            Assert.AreEqual("English", result.Data.settings.languagePreference);
        }

        [Test]
        public void LanguagePreferenceRoundTripsAndLegacyEmptyValueFallsBackToEnglish()
        {
            var files = new MemorySaveFileSystem();
            var repository = new LocalSaveRepository(Path, files);
            var data = LocalSaveRepository.CreateDefault();
            data.settings.languagePreference = "Arabic";
            string failure;

            Assert.IsTrue(repository.Save(data, out failure), failure);
            Assert.AreEqual("Arabic", repository.Load().Data.settings.languagePreference);

            data.settings.languagePreference = string.Empty;
            files.WriteAllText(Path, UnityEngine.JsonUtility.ToJson(data));
            var legacy = repository.Load();
            Assert.IsTrue(legacy.Succeeded, legacy.Failure);
            Assert.AreEqual("English", legacy.Data.settings.languagePreference);
        }

        [Test]
        public void SaveRejectsUnknownLanguagePreference()
        {
            var data = LocalSaveRepository.CreateDefault();
            data.settings.languagePreference = "Pirate";
            Assert.IsFalse(new LocalSaveRepository(Path, new MemorySaveFileSystem())
                .Save(data, out var failure));
            StringAssert.Contains("Language preference", failure);
        }

        [TestCase("bad id")]
        [TestCase("default-flagship")]
        public void SaveRejectsInvalidOrDuplicateOwnedIds(string invalidId)
        {
            var data = LocalSaveRepository.CreateDefault();
            data.ownedLoadoutIds.Add(invalidId);
            string failure;
            Assert.IsFalse(new LocalSaveRepository(Path, new MemorySaveFileSystem()).Save(data, out failure));
            Assert.IsTrue(!string.IsNullOrEmpty(failure));
        }

        [Test]
        public void SaveRejectsUnownedSelectionAndNonFiniteSettings()
        {
            var repository = new LocalSaveRepository(Path, new MemorySaveFileSystem());
            var unowned = LocalSaveRepository.CreateDefault();
            unowned.selectedLoadout.flagshipId = "not-owned";
            var nan = LocalSaveRepository.CreateDefault();
            nan.settings.musicVolume = float.NaN;
            string failure;

            Assert.IsFalse(repository.Save(unowned, out failure));
            Assert.IsFalse(repository.Save(nan, out failure));
        }

        [Test]
        public void InterruptedTempWriteAndReplacePreserveValidSave()
        {
            var files = new MemorySaveFileSystem();
            var repository = new LocalSaveRepository(Path, files);
            var original = LocalSaveRepository.CreateDefault();
            string failure;
            Assert.IsTrue(repository.Save(original, out failure), failure);
            files.WriteAllText(Path + ".tmp", "interrupted");
            files.ThrowOnReplace = true;
            var changed = LocalSaveRepository.CreateDefault();
            changed.highestUnlockedLevel = 3;

            Assert.IsFalse(repository.Save(changed, out failure));
            var loaded = repository.Load();
            Assert.IsTrue(loaded.Succeeded, loaded.Failure);
            Assert.AreEqual(1, loaded.Data.highestUnlockedLevel);
            Assert.IsFalse(files.Contains(Path + ".tmp"));
        }

        [Test]
        public void CorruptPrimaryRecoversValidBackup()
        {
            var files = new MemorySaveFileSystem();
            var repository = new LocalSaveRepository(Path, files);
            string failure;
            Assert.IsTrue(repository.Save(LocalSaveRepository.CreateDefault(), out failure), failure);
            var newer = LocalSaveRepository.CreateDefault();
            newer.highestUnlockedLevel = 2;
            Assert.IsTrue(repository.Save(newer, out failure), failure);
            files.WriteAllText(Path, "corrupt json");

            var loaded = repository.Load();
            Assert.IsTrue(loaded.Succeeded, loaded.Failure);
            Assert.AreEqual(1, loaded.Data.highestUnlockedLevel);
        }

        [Test]
        public void SchemaMigrationSuccessLoadsCurrentVersion()
        {
            var files = new MemorySaveFileSystem();
            var old = LocalSaveRepository.CreateDefault();
            old.schemaVersion = 0;
            files.WriteAllText(Path, UnityEngine.JsonUtility.ToJson(old));
            var migrations = new List<ISaveMigration> { new Migration(0, true) };

            var result = new LocalSaveRepository(Path, files, migrations).Load();

            Assert.IsTrue(result.Succeeded, result.Failure);
            Assert.AreEqual(LocalSaveRepository.CurrentSchemaVersion, result.Data.schemaVersion);
            Assert.AreEqual(3, result.Data.highestUnlockedLevel);
        }

        [Test]
        public void FailedMigrationFallsBackWithoutReplacingPreviousValidBackup()
        {
            var files = new MemorySaveFileSystem();
            var valid = LocalSaveRepository.CreateDefault();
            valid.highestUnlockedLevel = 2;
            files.WriteAllText(Path + ".bak", UnityEngine.JsonUtility.ToJson(valid));
            var old = LocalSaveRepository.CreateDefault();
            old.schemaVersion = 0;
            files.WriteAllText(Path, UnityEngine.JsonUtility.ToJson(old));

            var result = new LocalSaveRepository(Path, files, new List<ISaveMigration> { new Migration(0, false) }).Load();

            Assert.IsFalse(result.Succeeded);
            Assert.IsTrue(result.UsedDefault);
            Assert.AreEqual(2, UnityEngine.JsonUtility.FromJson<PlayerSaveData>(files.ReadAllText(Path + ".bak")).highestUnlockedLevel);
        }

        [Test]
        public void ExactDuplicateRewardRequestIsIdempotent()
        {
            var files = new MemorySaveFileSystem();
            var repository = new LocalSaveRepository(Path, files);
            string failure;
            Assert.IsTrue(repository.Save(LocalSaveRepository.CreateDefault(), out failure), failure);
            bool applied;
            Assert.IsTrue(repository.TryGrantReward("tx-1", "reward-1", out applied, out failure), failure);
            Assert.IsTrue(applied);
            Assert.IsTrue(repository.TryGrantReward("tx-1", "reward-1", out applied, out failure), failure);
            Assert.IsFalse(applied);
            Assert.AreEqual(1, repository.Load().Data.rewardTransactions.Count);
        }

        [Test]
        public void SameTransactionDifferentRewardIsRejected()
        {
            var repository = RepositoryWithDefault();
            bool applied;
            string failure;
            Assert.IsTrue(repository.TryGrantReward("tx-1", "reward-1", out applied, out failure), failure);
            Assert.IsFalse(repository.TryGrantReward("tx-1", "reward-2", out applied, out failure));
            StringAssert.Contains("already bound", failure);
        }

        [Test]
        public void SameRewardWithAnotherTransactionIsIdempotent()
        {
            var repository = RepositoryWithDefault();
            bool applied;
            string failure;
            Assert.IsTrue(repository.TryGrantReward("tx-1", "reward-1", out applied, out failure), failure);
            Assert.IsTrue(repository.TryGrantReward("tx-2", "reward-1", out applied, out failure), failure);
            Assert.IsFalse(applied);
            Assert.AreEqual(1, repository.Load().Data.rewardTransactions.Count);
        }

        private static LocalSaveRepository RepositoryWithDefault()
        {
            var files = new MemorySaveFileSystem();
            var repository = new LocalSaveRepository(Path, files);
            string failure;
            Assert.IsTrue(repository.Save(LocalSaveRepository.CreateDefault(), out failure), failure);
            return repository;
        }

        private sealed class Migration : ISaveMigration
        {
            private readonly bool succeeds;
            public Migration(int fromVersion, bool succeeds) { FromVersion = fromVersion; this.succeeds = succeeds; }
            public int FromVersion { get; private set; }
            public bool TryMigrate(PlayerSaveData source, out PlayerSaveData migrated, out string error)
            {
                migrated = source;
                if (!succeeds) { error = "fixture migration failure"; return false; }
                migrated.schemaVersion = LocalSaveRepository.CurrentSchemaVersion;
                migrated.highestUnlockedLevel = 3;
                error = string.Empty;
                return true;
            }
        }

        private sealed class MemorySaveFileSystem : ILocalSaveFileSystem
        {
            private readonly Dictionary<string, string> entries = new Dictionary<string, string>();
            public bool ThrowOnReplace { get; set; }
            public bool Contains(string path) { return entries.ContainsKey(path); }
            public bool Exists(string path) { return entries.ContainsKey(path); }
            public string ReadAllText(string path) { return entries[path]; }
            public void WriteAllText(string path, string contents) { entries[path] = contents; }
            public void Delete(string path) { entries.Remove(path); }
            public void Replace(string temporaryPath, string destinationPath, string backupPath)
            {
                if (ThrowOnReplace) throw new InvalidOperationException("fixture replace interruption");
                if (entries.ContainsKey(destinationPath)) entries[backupPath] = entries[destinationPath];
                entries[destinationPath] = entries[temporaryPath];
                entries.Remove(temporaryPath);
            }
        }
    }
}
