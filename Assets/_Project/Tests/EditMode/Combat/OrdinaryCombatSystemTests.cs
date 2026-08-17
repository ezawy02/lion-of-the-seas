using NUnit.Framework;
using Unity.Mathematics;
using SeaLion.Combat;

namespace SeaLion.Tests.EditMode.Combat
{
    public sealed class OrdinaryCombatSystemTests
    {
        [Test] public void ChoosesLowestIndexAndAppliesQueuedDamage()
        {
            var units = new[] { new CombatUnit(CombatTeam.Friendly, float3.zero, 10, 3, 5, 1), new CombatUnit(CombatTeam.Hostile, new float3(2, 0, 0), 5, 1, 5, 1), new CombatUnit(CombatTeam.Hostile, new float3(-2, 0, 0), 5, 1, 5, 1) };
            var system = new OrdinaryCombatSystem(); var hit = -1; system.Hit += e => { if (e.Source == 0) hit = e.Target; };
            system.Step(units, 0f);
            Assert.AreEqual(1, hit); Assert.AreEqual(2, units[1].Health); Assert.AreEqual(5, units[2].Health);
        }

        [Test] public void CadenceAndDeathFireExactlyOnce()
        {
            var units = new[] { new CombatUnit(CombatTeam.Friendly, float3.zero, 10, 5, 2, 1), new CombatUnit(CombatTeam.Hostile, new float3(1, 0, 0), 5, 1, 2, 1) };
            var system = new OrdinaryCombatSystem(); var deaths = 0; system.Death += _ => deaths++;
            system.Step(units, .25f); Assert.IsTrue(units[1].Dead); Assert.AreEqual(1, deaths);
            system.Step(units, .5f); system.Step(units, .25f); Assert.AreEqual(1, deaths);
            system.Step(units, 2f); Assert.AreEqual(1, deaths);
        }

        [Test] public void InvalidTimeAndOutOfRangeTargetsAreIgnored()
        {
            var units = new[] { new CombatUnit(CombatTeam.Friendly, float3.zero, 1, 3, 1, 1), new CombatUnit(CombatTeam.Hostile, new float3(4, 0, 0), 5, 1, 1, 1) };
            var system = new OrdinaryCombatSystem(); system.Step(units, float.NaN); system.Step(units, 1f);
            Assert.AreEqual(5, units[1].Health);
        }

        [Test] public void SimultaneousQueuedAttacksResolveEvenWhenBothUnitsDie()
        {
            var units = new[]
            {
                new CombatUnit(CombatTeam.Friendly, float3.zero, 5, 5, 2, 1),
                new CombatUnit(CombatTeam.Hostile, new float3(1, 0, 0), 5, 5, 2, 1)
            };
            var system = new OrdinaryCombatSystem();
            system.Step(units, 0f);
            Assert.IsTrue(units[0].Dead);
            Assert.IsTrue(units[1].Dead);
        }
    }
}
