using System.Collections.Generic;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Crowd.Simulation;

namespace SeaLion.Tests.EditMode.Crowd
{
    public sealed class ForceRuntimeTests
    {
        [Test]
        public void MultiplierKeepsLogicalCountAndCoversProjectionExtremes()
        {
            var force = new ForceRuntime(10, 3);
            force.ApplyMultiplier(2);
            Assert.That(force.LogicalCount, Is.EqualTo(20));
            Assert.That(force.DisplayedLogicalIndices, Is.EqualTo(new[] { 0, 9, 19 }));
        }

        [Test]
        public void CapChangesOnlyProjection()
        {
            var force = new ForceRuntime(17, 3);
            force.SetDisplayCap(7);
            Assert.That(force.LogicalCount, Is.EqualTo(17));
            Assert.That(force.DisplayedAgentCount, Is.EqualTo(7));
            force.SetDisplayCap(2);
            Assert.That(force.LogicalCount, Is.EqualTo(17));
            Assert.That(force.DisplayedLogicalIndices, Is.EqualTo(new[] { 0, 16 }));
        }

        [Test]
        public void ZeroAndInvalidValuesAreSafe()
        {
            var force = new ForceRuntime(0, 0);
            Assert.That(force.DisplayedAgentCount, Is.Zero);
            Assert.Throws<System.ArgumentOutOfRangeException>(() => force.SetLogicalCount(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => force.SetDisplayCap(-1));
            Assert.Throws<System.ArgumentOutOfRangeException>(() => force.ApplyMultiplier(-1));
        }

        [Test]
        public void MultiplierPreservesRoleTotals()
        {
            var force = new ForceRuntime(10, 10);
            force.SetRoleCounts(new[]
            {
                new KeyValuePair<UnitRole, int>(UnitRole.Sailor, 6),
                new KeyValuePair<UnitRole, int>(UnitRole.Musketeer, 4)
            });
            force.ApplyMultiplier(3);
            Assert.That(force.LogicalCount, Is.EqualTo(30));
            Assert.That(force.RoleCounts[UnitRole.Sailor], Is.EqualTo(18));
            Assert.That(force.RoleCounts[UnitRole.Musketeer], Is.EqualTo(12));
            Assert.That(force.DisplayedLogicalIndices[force.DisplayedAgentCount - 1], Is.EqualTo(29));
        }

        [Test]
        public void RoleContributionsUpdateLogicalAndRoleCountsAtomically()
        {
            var force = new ForceRuntime(0, 100);
            force.AddToRole(UnitRole.Sailor, 3);
            force.AddToRole(UnitRole.Sailor, 4);
            force.AddToRole(UnitRole.Musketeer, 2);

            Assert.That(force.LogicalCount, Is.EqualTo(9));
            Assert.That(force.RoleCounts[UnitRole.Sailor], Is.EqualTo(7));
            Assert.That(force.RoleCounts[UnitRole.Musketeer], Is.EqualTo(2));
        }
    }
}
