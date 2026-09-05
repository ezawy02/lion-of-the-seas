using System.Collections.Generic;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Core.Loadout;
using SeaLion.Core.Persistence;

namespace SeaLion.Tests.EditMode.Loadout
{
    public sealed class LoadoutServiceTests
    {
        private const string SavePath = "memory/loadout.json";
        private static readonly StableId DefaultFlagship = new StableId("default-flagship");
        private static readonly StableId DefaultCrew = new StableId("default-crew");
        private static readonly StableId DefaultAbility = new StableId("default-ability");
        private static readonly StableId FastFlagship = new StableId("fast-flagship");
        private static readonly StableId BomberCrew = new StableId("bomber-crew");
        private static readonly StableId PowderAbility = new StableId("powder-ability");

        [Test]
        public void DefaultsLoadAsAnImmutableSnapshot()
        {
            var files = new MemoryFiles();
            var service = CreateService(files);

            Assert.AreEqual(DefaultFlagship, service.CurrentSnapshot.FlagshipId);
            Assert.AreEqual(DefaultCrew, service.CurrentSnapshot.CrewRoleId);
            Assert.AreEqual(DefaultAbility, service.CurrentSnapshot.CaptainAbilityId);
        }

        [Test]
        public void OwnedSelectionPersistsAcrossServiceRestart()
        {
            var files = new MemoryFiles();
            var first = CreateService(files);
            string failure;
            Assert.IsTrue(first.TrySelect(LoadoutSlot.Flagship, FastFlagship, out failure), failure);
            Assert.IsTrue(first.TrySelect(LoadoutSlot.Crew, BomberCrew, out failure), failure);

            var second = CreateService(files, false);
            Assert.AreEqual(FastFlagship, second.CurrentSnapshot.FlagshipId);
            Assert.AreEqual(BomberCrew, second.CurrentSnapshot.CrewRoleId);
            Assert.AreEqual(DefaultAbility, second.CurrentSnapshot.CaptainAbilityId);
        }

        [Test]
        public void UnownedOrWrongSlotSelectionIsRejectedWithoutChangingState()
        {
            var files = new MemoryFiles();
            var service = CreateService(files);
            string failure;

            Assert.IsFalse(service.TrySelect(LoadoutSlot.Crew, FastFlagship, out failure));
            Assert.IsFalse(service.TrySelect(LoadoutSlot.CaptainAbility, PowderAbility, out failure));
            Assert.AreEqual(DefaultCrew, service.CurrentSnapshot.CrewRoleId);
            Assert.AreEqual(DefaultAbility, service.CurrentSnapshot.CaptainAbilityId);
        }

        [Test]
        public void InvalidSavedSelectionFallsBackToOwnedDefault()
        {
            var files = new MemoryFiles();
            var data = LocalSaveRepository.CreateDefault();
            data.ownedLoadoutIds.Add(FastFlagship.Value);
            data.selectedLoadout.flagshipId = FastFlagship.Value;
            string failure;
            Assert.IsTrue(new LocalSaveRepository(SavePath, files).Save(data, out failure), failure);

            var service = new LoadoutService(new LocalSaveRepository(SavePath, files),
                new[] { DefaultFlagship }, new[] { DefaultCrew }, new[] { DefaultAbility });

            Assert.AreEqual(DefaultFlagship, service.CurrentSnapshot.FlagshipId);
            Assert.AreEqual(DefaultCrew, service.CurrentSnapshot.CrewRoleId);
            Assert.AreEqual(DefaultAbility, service.CurrentSnapshot.CaptainAbilityId);
        }

        [Test]
        public void SetLoadoutRejectsAnyInvalidSlotAtomically()
        {
            var files = new MemoryFiles();
            var service = CreateService(files);
            string failure;
            var invalid = new LoadoutSnapshot(FastFlagship, BomberCrew, new StableId("not-defined"));

            Assert.IsFalse(service.TrySetLoadout(invalid, out failure));
            Assert.AreEqual(DefaultFlagship, service.CurrentSnapshot.FlagshipId);
            Assert.AreEqual(DefaultCrew, service.CurrentSnapshot.CrewRoleId);
            Assert.AreEqual(DefaultAbility, service.CurrentSnapshot.CaptainAbilityId);
        }

        private static LoadoutService CreateService(MemoryFiles files, bool seed = true)
        {
            var repository = new LocalSaveRepository(SavePath, files);
            if (seed)
            {
                var data = LocalSaveRepository.CreateDefault();
                data.ownedLoadoutIds.Add(FastFlagship.Value);
                data.ownedLoadoutIds.Add(BomberCrew.Value);
                string failure;
                Assert.IsTrue(repository.Save(data, out failure), failure);
            }
            return new LoadoutService(repository,
                new[] { DefaultFlagship, FastFlagship },
                new[] { DefaultCrew, BomberCrew },
                new[] { DefaultAbility, PowderAbility });
        }

        private sealed class MemoryFiles : ILocalSaveFileSystem
        {
            private readonly Dictionary<string, string> entries = new Dictionary<string, string>();
            public bool Exists(string path) { return entries.ContainsKey(path); }
            public string ReadAllText(string path) { return entries[path]; }
            public void WriteAllText(string path, string contents) { entries[path] = contents; }
            public void Delete(string path) { entries.Remove(path); }
            public void Replace(string temporaryPath, string destinationPath, string backupPath)
            {
                if (entries.ContainsKey(destinationPath)) entries[backupPath] = entries[destinationPath];
                entries[destinationPath] = entries[temporaryPath];
                entries.Remove(temporaryPath);
            }
        }
    }
}
