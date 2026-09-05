using System.Collections.Generic;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Gameplay.Levels;

namespace SeaLion.Tests.EditMode.Levels
{
    public sealed partial class Level01TrialRuntimeTests
    {
        [TestCase(2)]
        [TestCase(3)]
        public void CampaignEncounterCanWinAndRetryWithoutEarlierLevel(int number)
        {
            var runtime = CreateCampaign(number);
            Assert.That(runtime.Begin(), Is.True);
            var brokeChain = false;
            var sawSecondAssault = false;
            for (var i = 0; i < 3600 && runtime.IsRunning; i++)
            {
                runtime.SetTraversalControl(-1f, true);
                brokeChain |= runtime.BlockadeActive;
                sawSecondAssault |= runtime.AssaultStage == 2;
                if (runtime.CanPrimaryAttack) runtime.TryPrimaryAttack();
                if (runtime.AbilityReady) runtime.TryActivateAbility();
                runtime.Step(.05f);
            }
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Victory));
            Assert.That(runtime.RewardResult.Value.Succeeded, Is.True);
            if (number == 2) Assert.That(brokeChain, Is.True);
            if (number == 3) Assert.That(sawSecondAssault, Is.True);
            Assert.That(runtime.Retry(), Is.True);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Traversal));
            Assert.That(runtime.AssaultStage, Is.EqualTo(1));
            Assert.That(runtime.Powder, Is.Zero);
        }

        [TestCase(2)]
        [TestCase(3)]
        public void CampaignEncounterCanFailAndRetryIndependently(int number)
        {
            var runtime = CreateCampaign(number); runtime.Begin(); runtime.Step(3f);
            for (var sequence = 0; sequence < 6; sequence++) runtime.DestroyCraft(sequence);
            Assert.That(runtime.Phase, Is.EqualTo(Level01TrialPhase.Failure));
            Assert.That(runtime.RewardResult.HasValue, Is.False);
            Assert.That(runtime.Retry(), Is.True);
            Assert.That(runtime.ForceCount, Is.EqualTo(runtime.Level.InitialForce));
        }

        private Level01TrialRuntime CreateCampaign(int number)
        {
            var runtime = CreateRuntime();
            var prefix = "Assets/_Project/Data/Levels/Level0" + number + "/Level0" + number;
            runtime.Configure(new List<FlagshipDefinition>(runtime.Flagships).ToArray(),
                new List<UnitRoleDefinition>(runtime.CrewRoles).ToArray(),
                new List<CaptainAbilityDefinition>(runtime.CaptainAbilities).ToArray(),
                Load<GateDefinition>(prefix + "_Gate_Easy.asset"),
                Load<GateDefinition>(prefix + "_Gate_Risky.asset"),
                Load<RescueDefinition>("Assets/_Project/Data/Levels/Level01/Level01_Rescue.asset"),
                Load<BossDefinition>(prefix + "_Boss.asset"),
                Load<RewardDefinition>("Assets/_Project/Data/Rewards/Level0" + number + "Blueprint.asset"),
                runtime.SaveFileName);
            runtime.ConfigureLevel(Load<LevelDefinition>(prefix + ".asset"));
            runtime.ConfigureCenterGate(Load<GateDefinition>(prefix + "_Gate_Center.asset"));
            return runtime;
        }
    }
}
