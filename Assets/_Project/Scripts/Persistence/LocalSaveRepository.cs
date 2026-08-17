using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using SeaLion.Core.Definitions;

namespace SeaLion.Core.Persistence
{
    /// <summary>Small file seam so persistence tests never touch a player's files.</summary>
    public interface ILocalSaveFileSystem
    {
        bool Exists(string path);
        string ReadAllText(string path);
        void WriteAllText(string path, string contents);
        void Replace(string temporaryPath, string destinationPath, string backupPath);
        void Delete(string path);
    }

    public sealed class LocalSaveFileSystem : ILocalSaveFileSystem
    {
        public bool Exists(string path) { return File.Exists(path); }
        public string ReadAllText(string path) { return File.ReadAllText(path); }
        public void WriteAllText(string path, string contents) { File.WriteAllText(path, contents); }
        public void Delete(string path) { if (File.Exists(path)) File.Delete(path); }

        public void Replace(string temporaryPath, string destinationPath, string backupPath)
        {
            if (File.Exists(destinationPath))
                File.Replace(temporaryPath, destinationPath, backupPath, true);
            else
                File.Move(temporaryPath, destinationPath);
        }
    }

    [Serializable]
    public sealed class SaveLoadout
    {
        public string flagshipId;
        public string crewRoleId;
        public string captainAbilityId;

        public LoadoutSnapshot ToSnapshot()
        {
            return new LoadoutSnapshot(new StableId(flagshipId), new StableId(crewRoleId), new StableId(captainAbilityId));
        }
    }

    [Serializable]
    public sealed class SaveSettings
    {
        public string qualityPreference = "Auto";
        public bool haptics = true;
        public float musicVolume = 1f;
        public float effectsVolume = 1f;
    }

    [Serializable]
    public sealed class RewardTransaction
    {
        public string transactionId;
        public string rewardId;
    }

    [Serializable]
    public sealed class PlayerSaveData
    {
        public int schemaVersion = LocalSaveRepository.CurrentSchemaVersion;
        public int highestUnlockedLevel = 1;
        public List<string> ownedLoadoutIds = new List<string>();
        public SaveLoadout selectedLoadout = new SaveLoadout();
        public List<string> claimedRewardIds = new List<string>();
        public List<RewardTransaction> rewardTransactions = new List<RewardTransaction>();
        public SaveSettings settings = new SaveSettings();
        public string lastWriteId;
    }

    public interface ISaveMigration
    {
        int FromVersion { get; }
        bool TryMigrate(PlayerSaveData source, out PlayerSaveData migrated, out string error);
    }

    public sealed class SaveLoadResult
    {
        public PlayerSaveData Data { get; private set; }
        public bool Succeeded { get; private set; }
        public bool UsedDefault { get; private set; }
        public string Failure { get; private set; }

        private SaveLoadResult(PlayerSaveData data, bool succeeded, bool usedDefault, string failure)
        { Data = data; Succeeded = succeeded; UsedDefault = usedDefault; Failure = failure; }

        public static SaveLoadResult Success(PlayerSaveData data) { return new SaveLoadResult(data, true, false, string.Empty); }
        public static SaveLoadResult FreshDefault()
        { return new SaveLoadResult(LocalSaveRepository.CreateDefault(), true, true, string.Empty); }
        public static SaveLoadResult FailureWithDefault(string reason)
        { return new SaveLoadResult(LocalSaveRepository.CreateDefault(), false, true, reason); }
    }

    /// <summary>Versioned, scene-independent local JSON persistence for player progression.</summary>
    public sealed class LocalSaveRepository
    {
        public const int CurrentSchemaVersion = 1;
        private const string TemporarySuffix = ".tmp";
        private const string BackupSuffix = ".bak";
        private readonly string path;
        private readonly ILocalSaveFileSystem files;
        private readonly IReadOnlyList<ISaveMigration> migrations;

        public LocalSaveRepository(string path, ILocalSaveFileSystem files = null, IReadOnlyList<ISaveMigration> migrations = null)
        {
            if (string.IsNullOrEmpty(path)) throw new ArgumentException("Save path is required.", "path");
            this.path = path;
            this.files = files ?? new LocalSaveFileSystem();
            this.migrations = migrations ?? new List<ISaveMigration>();
        }

        public SaveLoadResult Load()
        {
            var backupPath = path + BackupSuffix;
            if (!files.Exists(path) && !files.Exists(backupPath)) return SaveLoadResult.FreshDefault();

            string failure;
            PlayerSaveData data;
            if (!TryRead(path, out data, out failure))
            {
                if (!TryRead(backupPath, out data, out failure))
                    return SaveLoadResult.FailureWithDefault("Save recovery failed: " + failure);
            }

            if (!TryMigrate(data, out data, out failure) || !Validate(data, out failure))
                return SaveLoadResult.FailureWithDefault("Save migration/validation failed: " + failure);
            return SaveLoadResult.Success(data);
        }

        public bool Save(PlayerSaveData data, out string failure)
        {
            if (!Validate(data, out failure)) return false;
            data.schemaVersion = CurrentSchemaVersion;
            data.lastWriteId = Guid.NewGuid().ToString("N");
            var json = JsonUtility.ToJson(data, true);
            var temporaryPath = path + TemporarySuffix;
            try
            {
                files.WriteAllText(temporaryPath, json);
                PlayerSaveData roundTrip;
                if (!TryRead(temporaryPath, out roundTrip, out failure) || !Validate(roundTrip, out failure))
                { files.Delete(temporaryPath); return false; }
                files.Replace(temporaryPath, path, path + BackupSuffix);
                return true;
            }
            catch (Exception exception)
            {
                failure = "Atomic save failed: " + exception.Message;
                try { files.Delete(temporaryPath); } catch (Exception cleanup) { failure += " Cleanup failed: " + cleanup.Message; }
                return false;
            }
        }

        public bool TryGrantReward(string transactionId, string rewardId, out bool applied, out string failure)
        {
            applied = false;
            if (!StableId.IsValid(transactionId) || !StableId.IsValid(rewardId))
            { failure = "Reward transaction and reward IDs must be valid stable IDs."; return false; }
            var result = Load();
            if (!result.Succeeded)
            { failure = result.Failure; return false; }
            var data = result.Data;
            if (data.claimedRewardIds.Contains(rewardId))
            { failure = string.Empty; return true; }
            for (var i = 0; i < data.rewardTransactions.Count; i++)
            {
                var transaction = data.rewardTransactions[i];
                if (transaction == null) continue;
                if (transaction.transactionId == transactionId && transaction.rewardId == rewardId)
                { failure = string.Empty; return true; }
                if (transaction.transactionId == transactionId)
                { failure = "Transaction ID is already bound to another reward."; return false; }
                if (transaction.rewardId == rewardId)
                { failure = string.Empty; return true; }
            }
            data.claimedRewardIds.Add(rewardId);
            data.rewardTransactions.Add(new RewardTransaction { transactionId = transactionId, rewardId = rewardId });
            if (!Save(data, out failure)) return false;
            applied = true;
            return true;
        }

        /// <summary>Atomically records a first-completion reward and its unlocked loadout blueprint.</summary>
        public bool TryGrantRewardWithOwnership(string transactionId, string rewardId, string ownedId,
            out bool applied, out string failure)
        {
            applied = false;
            if (!StableId.IsValid(transactionId) || !StableId.IsValid(rewardId) || !StableId.IsValid(ownedId))
            { failure = "Reward transaction, reward, and ownership IDs must be valid stable IDs."; return false; }
            var result = Load();
            if (!result.Succeeded) { failure = result.Failure; return false; }
            var data = result.Data;
            var rewardClaimed = data.claimedRewardIds.Contains(rewardId);
            for (var i = 0; i < data.rewardTransactions.Count; i++)
            {
                var transaction = data.rewardTransactions[i];
                if (transaction == null) continue;
                if (transaction.transactionId == transactionId && transaction.rewardId != rewardId)
                { failure = "Transaction ID is already bound to another reward."; return false; }
                if (transaction.rewardId == rewardId) rewardClaimed = true;
            }
            if (rewardClaimed)
            {
                if (!data.ownedLoadoutIds.Contains(ownedId))
                {
                    data.ownedLoadoutIds.Add(ownedId);
                    if (!Save(data, out failure)) return false;
                }
                failure = string.Empty;
                return true;
            }
            data.claimedRewardIds.Add(rewardId);
            data.rewardTransactions.Add(new RewardTransaction { transactionId = transactionId, rewardId = rewardId });
            if (!data.ownedLoadoutIds.Contains(ownedId)) data.ownedLoadoutIds.Add(ownedId);
            if (!Save(data, out failure)) return false;
            applied = true;
            return true;
        }

        public static PlayerSaveData CreateDefault()
        {
            var data = new PlayerSaveData { schemaVersion = CurrentSchemaVersion, highestUnlockedLevel = 1, settings = new SaveSettings() };
            data.ownedLoadoutIds.Add("default-flagship");
            data.ownedLoadoutIds.Add("default-crew");
            data.ownedLoadoutIds.Add("default-ability");
            data.selectedLoadout = new SaveLoadout { flagshipId = "default-flagship", crewRoleId = "default-crew", captainAbilityId = "default-ability" };
            return data;
        }

        public static bool Validate(PlayerSaveData data, out string failure)
        {
            failure = string.Empty;
            if (data == null) { failure = "Save is null."; return false; }
            if (data.schemaVersion < 0 || data.schemaVersion > CurrentSchemaVersion) { failure = "Unsupported schema version."; return false; }
            if (data.highestUnlockedLevel < 1 || data.highestUnlockedLevel > 3) { failure = "Unlocked level is out of range."; return false; }
            if (data.ownedLoadoutIds == null || data.claimedRewardIds == null || data.rewardTransactions == null || data.selectedLoadout == null || data.settings == null)
            { failure = "Save contains missing collections or state."; return false; }
            if (!ValidateIds(data.ownedLoadoutIds) || !ValidateIds(data.claimedRewardIds)) { failure = "Save contains an invalid or duplicate stable ID."; return false; }
            if (!StableId.IsValid(data.selectedLoadout.flagshipId) || !StableId.IsValid(data.selectedLoadout.crewRoleId) || !StableId.IsValid(data.selectedLoadout.captainAbilityId))
            { failure = "Selected loadout contains an invalid stable ID."; return false; }
            if (!data.ownedLoadoutIds.Contains(data.selectedLoadout.flagshipId) || !data.ownedLoadoutIds.Contains(data.selectedLoadout.crewRoleId) || !data.ownedLoadoutIds.Contains(data.selectedLoadout.captainAbilityId))
            { failure = "Selected loadout contains an unowned stable ID."; return false; }
            if (float.IsNaN(data.settings.musicVolume) || float.IsInfinity(data.settings.musicVolume) ||
                float.IsNaN(data.settings.effectsVolume) || float.IsInfinity(data.settings.effectsVolume) ||
                data.settings.musicVolume < 0f || data.settings.musicVolume > 1f ||
                data.settings.effectsVolume < 0f || data.settings.effectsVolume > 1f)
            { failure = "Volume is out of range."; return false; }
            if (data.settings.qualityPreference != "Auto" && data.settings.qualityPreference != "Primary" &&
                data.settings.qualityPreference != "Reduced")
            { failure = "Quality preference is invalid."; return false; }
            var transactions = new HashSet<string>();
            var rewards = new HashSet<string>();
            for (var i = 0; i < data.rewardTransactions.Count; i++)
            {
                var transaction = data.rewardTransactions[i];
                if (transaction == null || !StableId.IsValid(transaction.transactionId) || !StableId.IsValid(transaction.rewardId) || !transactions.Add(transaction.transactionId) || !rewards.Add(transaction.rewardId))
                { failure = "Reward transactions must have unique stable transaction and reward IDs."; return false; }
            }
            return true;
        }

        private bool TryRead(string candidatePath, out PlayerSaveData data, out string failure)
        {
            data = null;
            if (!files.Exists(candidatePath)) { failure = "File not found."; return false; }
            try { data = JsonUtility.FromJson<PlayerSaveData>(files.ReadAllText(candidatePath)); }
            catch (Exception exception) { failure = "JSON read failed: " + exception.Message; return false; }
            if (data == null) { failure = "JSON produced no save data."; return false; }
            failure = string.Empty;
            return true;
        }

        private bool TryMigrate(PlayerSaveData source, out PlayerSaveData migrated, out string failure)
        {
            migrated = source; failure = string.Empty;
            while (migrated.schemaVersion < CurrentSchemaVersion)
            {
                ISaveMigration migration = null;
                for (var i = 0; i < migrations.Count; i++) if (migrations[i].FromVersion == migrated.schemaVersion) { migration = migrations[i]; break; }
                if (migration == null || !migration.TryMigrate(migrated, out migrated, out failure)) return false;
            }
            return migrated.schemaVersion == CurrentSchemaVersion;
        }

        private static bool ValidateIds(List<string> ids)
        {
            var seen = new HashSet<string>();
            for (var i = 0; i < ids.Count; i++) if (!StableId.IsValid(ids[i]) || !seen.Add(ids[i])) return false;
            return true;
        }
    }
}
