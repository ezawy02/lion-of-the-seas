using System.Collections.Generic;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Core.Persistence;
using SeaLion.UI.Loadout;
using UnityEngine;

namespace SeaLion.Tests.EditMode.UI.Loadout
{
    public sealed class LoadoutScreenControllerTests
    {
        [Test]
        public void InitializesThreeSlotsAndPersistsOwnedSelection()
        {
            var files = new MemoryFiles();
            var repository = new LocalSaveRepository("memory/controller.json", files);
            var data = LocalSaveRepository.CreateDefault(); data.ownedLoadoutIds.Add("fast-ship");
            string failure; Assert.IsTrue(repository.Save(data, out failure), failure);
            var ship = Definition<FlagshipDefinition>("fast-ship");
            var crew = Definition<UnitRoleDefinition>("default-crew");
            var ability = Definition<CaptainAbilityDefinition>("default-ability");
            var go = new GameObject("LoadoutControllerTest"); var controller = go.AddComponent<LoadoutScreenController>();
            controller.Initialize(repository, new[] { ship }, new[] { crew }, new[] { ability });
            Assert.AreEqual(1, controller.View.GetOptions(LoadoutSlot.Flagship).Count);
            Assert.AreEqual(1, controller.View.GetOptions(LoadoutSlot.CrewRole).Count);
            Assert.AreEqual(1, controller.View.GetOptions(LoadoutSlot.CaptainAbility).Count);
            Assert.IsTrue(controller.TrySelect(LoadoutSlot.Flagship, new StableId("fast-ship")));
            Assert.AreEqual("fast-ship", repository.Load().Data.selectedLoadout.flagshipId);
            Object.DestroyImmediate(go);
            Object.DestroyImmediate(ship); Object.DestroyImmediate(crew); Object.DestroyImmediate(ability);
        }

        private static T Definition<T>(string id) where T : ScriptableObject
        {
            var asset = ScriptableObject.CreateInstance<T>(); var serialized = new UnityEditor.SerializedObject(asset);
            serialized.FindProperty("id").FindPropertyRelative("value").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo(); return asset;
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
                entries[destinationPath] = entries[temporaryPath]; entries.Remove(temporaryPath);
            }
        }
    }
}
