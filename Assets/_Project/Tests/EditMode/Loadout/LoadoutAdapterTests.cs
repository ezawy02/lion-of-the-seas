using NUnit.Framework;
using SeaLion.Combat;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Flagship;

namespace SeaLion.Tests.EditMode.Loadout
{
    public sealed class LoadoutAdapterTests
    {
        [Test]
        public void FlagshipAdapterResolvesOwnedSnapshotOnly()
        {
            var flagship = UnityEngine.ScriptableObject.CreateInstance<FlagshipDefinition>(); SetId(flagship, "ship-a");
            var adapter = new FlagshipLoadoutAdapter(new[] { flagship }, new[] { new StableId("ship-a") });
            Assert.That(adapter.TryResolve(new LoadoutSnapshot(new StableId("ship-a"), default, default), out var resolved), Is.True);
            Assert.That(resolved, Is.SameAs(flagship));
            Assert.That(adapter.TryResolveDeployment(new LoadoutSnapshot(new StableId("ship-a"), default, default), out var profile), Is.True);
            Assert.That(profile.Id, Is.EqualTo(new StableId("ship-a")));
            Assert.That(adapter.TryResolve(new StableId("ship-b"), out _), Is.False);
            UnityEngine.Object.DestroyImmediate(flagship);
        }

        [Test]
        public void CrewAdapterRejectsUnownedRole()
        {
            var crew = new CrewRoleProfile(new StableId("crew-a"), UnitRole.Musketeer, 1.15f, 0.9f, 2);
            var adapter = new CrewRoleLoadoutAdapter(new[] { crew }, new[] { new StableId("crew-b") });
            Assert.That(adapter.TryResolve(new LoadoutSnapshot(default, new StableId("crew-a"), default), out _), Is.False);
        }

        private static void SetId(UnityEngine.ScriptableObject asset, string id)
        {
            var serialized = new UnityEditor.SerializedObject(asset);
            serialized.FindProperty("id").FindPropertyRelative("value").stringValue = id;
            serialized.ApplyModifiedPropertiesWithoutUndo();
        }
    }
}
