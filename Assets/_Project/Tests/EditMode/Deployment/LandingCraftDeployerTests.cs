using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Deployment;
using UnityEngine;

namespace SeaLion.Tests.EditMode.Deployment
{
    public sealed class LandingCraftDeployerTests
    {
        [Test]
        public void CadenceIsSafeForZeroAndInvalidValues()
        {
            Assert.AreEqual(2f, LandingCraftDeployer.ComputeInterval(2f));
            Assert.IsTrue(float.IsPositiveInfinity(LandingCraftDeployer.ComputeInterval(0f)));
            Assert.IsTrue(float.IsPositiveInfinity(LandingCraftDeployer.ComputeInterval(float.NaN)));
        }

        [Test]
        public void BurstAndSpreadUseConfiguredCountWhileCadenceUsesOne()
        {
            Assert.AreEqual(4, LandingCraftDeployer.ComputeBurstSize(DeployPattern.Burst, 4));
            Assert.AreEqual(4, LandingCraftDeployer.ComputeBurstSize(DeployPattern.Spread, 4));
            Assert.AreEqual(1, LandingCraftDeployer.ComputeBurstSize(DeployPattern.Cadence, 4));
            Assert.AreEqual(1, LandingCraftDeployer.ComputeBurstSize(DeployPattern.Burst, 0));
        }

        [Test]
        public void SpreadIsSymmetricAndDeterministic()
        {
            Assert.AreEqual(-1f, LandingCraftDeployer.ComputeSpreadOffset(DeployPattern.Spread, 0, 3, 2f));
            Assert.AreEqual(0f, LandingCraftDeployer.ComputeSpreadOffset(DeployPattern.Spread, 1, 3, 2f));
            Assert.AreEqual(1f, LandingCraftDeployer.ComputeSpreadOffset(DeployPattern.Spread, 2, 3, 2f));
            Assert.AreEqual(0f, LandingCraftDeployer.ComputeSpreadOffset(DeployPattern.Cadence, 0, 3, 2f));
        }

        [Test]
        public void ContributionUsesExplicitAwayFromZeroRounding()
        {
            Assert.AreEqual(3, LandingCraftDeployer.ComputeContribution(2.5f));
            Assert.AreEqual(1, LandingCraftDeployer.ComputeContribution(float.NaN));
        }

        [Test]
        public void TickUsesBoundedPoolAndReleasedCraftCanBeReused()
        {
            var host = new GameObject("deployer-test");
            var definition = ScriptableObject.CreateInstance<FlagshipDefinition>();
            var created = new List<Craft>();
            try
            {
                Set(definition, "deployPattern", DeployPattern.Burst);
                Set(definition, "deploymentCadence", .5f);
                Set(definition, "burstSize", 3);
                Set(definition, "baseDeployment", 2.5f);
                var deployer = host.AddComponent<LandingCraftDeployer>();
                deployer.Configure(definition, null, () =>
                {
                    var craft = new Craft();
                    created.Add(craft);
                    return craft;
                }, 4, Vector3.zero);
                deployer.Tick(.5f);
                deployer.Tick(.5f);
                Assert.AreEqual(4, deployer.InUseCount);
                Assert.AreEqual(4, created.Count);
                Assert.AreEqual(3, created[0].Contribution);
                Assert.IsTrue(deployer.Release(created[0]));
                deployer.Tick(.5f);
                Assert.AreEqual(4, deployer.InUseCount);
                Assert.AreEqual(2, created[0].Activations);
            }
            finally
            {
                Object.DestroyImmediate(host);
                Object.DestroyImmediate(definition);
            }
        }

        private sealed class Craft : ILandingCraft
        {
            public int Activations, Contribution;
            public void Activate(Vector3 position, int contribution, int sequence)
            {
                Activations++;
                Contribution = contribution;
            }
            public void Deactivate() { }
        }

        private static void Set(Object target, string name, object value)
        {
            target.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(target, value);
        }
    }
}
