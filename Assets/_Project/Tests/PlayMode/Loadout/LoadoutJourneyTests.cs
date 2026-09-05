using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using SeaLion.Core.Definitions;
using SeaLion.Core.Loadout;
using SeaLion.Core.Persistence;
using SeaLion.Gameplay.Flagship;
using SeaLion.Gameplay.Deployment;
using SeaLion.Gameplay.Loadout;
using SeaLion.Combat;
using SeaLion.Core.Battle;
using SeaLion.Core.Events;
using SeaLion.Gameplay.Abilities;
using Unity.Mathematics;
using SeaLion.Gameplay.Rewards;
using UnityEngine;
using UnityEngine.TestTools;

namespace SeaLion.Tests.PlayMode.Loadout
{
    public sealed class LoadoutJourneyTests
    {
        const string SavePath = "memory/loadout-journey.json";
        static readonly StableId DefaultFlagship = new StableId("default-flagship");
        static readonly StableId RaiderFlagship = new StableId("flagship-lateen-raider");
        static readonly StableId DefaultCrew = new StableId("default-crew");
        static readonly StableId SailmakersCrew = new StableId("loadout-crew-sailmakers");
        static readonly StableId DefaultAbility = new StableId("default-ability");
        static readonly StableId BarrageAbility = new StableId("ability-powder-barrage");

        [UnityTest]
        public IEnumerator RewardUnlockSelectionRestartAndReplayDifferenceStayDeterministic()
        {
            var files = new MemoryFiles();
            var repository = new LocalSaveRepository(SavePath, files);
            var data = LocalSaveRepository.CreateDefault();
            data.ownedLoadoutIds.Add(RaiderFlagship.Value);
            data.ownedLoadoutIds.Add(BarrageAbility.Value);
            Assert.That(repository.Save(data, out var failure), Is.True, failure);

            var reward = CreateReward();
            var grant = new RewardGrantService(repository);
            Assert.That(grant.TryGrant(true, reward, "level01-loadout-transaction", out var result), Is.True,
                result.Failure);
            Assert.That(result.Applied, Is.True);
            Assert.That(repository.Load().Data.ownedLoadoutIds, Does.Contain(SailmakersCrew.Value));

            var service = CreateService(repository);
            Assert.That(service.TrySelect(SeaLion.Core.Loadout.LoadoutSlot.Flagship, RaiderFlagship, out failure), Is.True, failure);
            Assert.That(service.TrySelect(SeaLion.Core.Loadout.LoadoutSlot.Crew, SailmakersCrew, out failure), Is.True, failure);
            Assert.That(service.TrySelect(SeaLion.Core.Loadout.LoadoutSlot.CaptainAbility, BarrageAbility, out failure), Is.True, failure);

            var restarted = CreateService(repository);
            Assert.That(restarted.CurrentSnapshot.FlagshipId, Is.EqualTo(RaiderFlagship));
            Assert.That(restarted.CurrentSnapshot.CrewRoleId, Is.EqualTo(SailmakersCrew));
            Assert.That(restarted.CurrentSnapshot.CaptainAbilityId, Is.EqualTo(BarrageAbility));

            var standard = CreateFlagship(DefaultFlagship, DeployPattern.Cadence, 1);
            var raider = CreateFlagship(RaiderFlagship, DeployPattern.Burst, 3);
            var adapter = new FlagshipLoadoutAdapter(new[] { standard, raider });
            Assert.That(adapter.TryResolve(new LoadoutSnapshot(DefaultFlagship, DefaultCrew, DefaultAbility), out var defaultReplay), Is.True);
            Assert.That(adapter.TryResolve(restarted.CurrentSnapshot, out var changedReplay), Is.True);
            Assert.That(changedReplay.DeployPattern, Is.Not.EqualTo(defaultReplay.DeployPattern));
            Assert.That(changedReplay.BurstSize, Is.GreaterThan(defaultReplay.BurstSize));

            Object.Destroy(reward); Object.Destroy(standard); Object.Destroy(raider);
            yield return null;
        }

        [UnityTest]
        public IEnumerator SessionSnapshotDrivesDeploymentCrewCombatAndCaptainAbility()
        {
            var standard = CreateFlagship(DefaultFlagship, DeployPattern.Cadence, 1, 0.9f, 1f);
            var raider = CreateFlagship(RaiderFlagship, DeployPattern.Burst, 3, 1.45f, 0.86f);
            var sailors = CreateCrew(DefaultCrew, UnitRole.Sailor, 1.8f, 1f, 1f);
            var sailmakers = CreateCrew(SailmakersCrew, UnitRole.Defender, 1.62f, 1.1f, 1.5f);
            var rally = CreateAbility(DefaultAbility, AbilityChargeRule.Time, 1f, 5f,
                GateOutcome.Add, 8f);
            var barrage = CreateAbility(BarrageAbility, AbilityChargeRule.Damage, 10f, 9f,
                GateOutcome.Damage, 18f);
            var flagships = new[] { standard, raider };
            var crew = new[] { sailors, sailmakers };
            var abilities = new[] { rally, barrage };

            var standardSession = ActiveSession(new LoadoutSnapshot(DefaultFlagship, DefaultCrew, DefaultAbility));
            var changedSession = ActiveSession(new LoadoutSnapshot(RaiderFlagship, SailmakersCrew, BarrageAbility));
            Assert.That(BattleLoadoutRuntime.TryCreate(standardSession, flagships, crew, abilities,
                out var standardRuntime, out var failure), Is.True, failure);
            Assert.That(BattleLoadoutRuntime.TryCreate(changedSession, flagships, crew, abilities,
                out var changedRuntime, out failure), Is.True, failure);

            Assert.That(changedRuntime.Deployment.Pattern, Is.EqualTo(DeployPattern.Burst));
            Assert.That(changedRuntime.Deployment.BurstSize, Is.GreaterThan(standardRuntime.Deployment.BurstSize));
            Assert.That(changedRuntime.Deployment.PresentationShipId,
                Is.Not.EqualTo(standardRuntime.Deployment.PresentationShipId));
            var baseUnit = new CombatUnit(CombatTeam.Friendly, float3.zero, 100f, 10f, 8f, 1f);
            var standardUnit = standardRuntime.ApplyCrewTo(baseUnit);
            var changedUnit = changedRuntime.ApplyCrewTo(baseUnit);
            Assert.That(changedRuntime.Crew.Role, Is.EqualTo(UnitRole.Defender));
            Assert.That(changedUnit.Damage, Is.LessThan(standardUnit.Damage));
            Assert.That(changedUnit.Health, Is.GreaterThan(standardUnit.Health));

            var root = new GameObject("LoadoutRuntimeJourney");
            var standardDeployer = root.AddComponent<LandingCraftDeployer>();
            var changedDeployer = root.AddComponent<LandingCraftDeployer>();
            var standardDeployments = 0;
            var changedDeployments = 0;
            standardDeployer.Deployed += _ => standardDeployments++;
            changedDeployer.Deployed += _ => changedDeployments++;
            standardRuntime.Configure(standardDeployer, () => new TestCraft(), 8, Vector3.zero);
            changedRuntime.Configure(changedDeployer, () => new TestCraft(), 8, Vector3.zero);
            standardDeployer.Tick(1.46f);
            changedDeployer.Tick(1.46f);
            Assert.That(standardDeployments, Is.EqualTo(1));
            Assert.That(changedDeployments, Is.EqualTo(3));

            BattleEvent activation = default;
            changedSession.Events.Subscribe(value =>
            {
                if (value.Type == BattleEventType.AbilityActivated) activation = value;
            });
            changedRuntime.ReportDamage(10f);
            Assert.That(changedRuntime.TryActivateAbility(), Is.EqualTo(AbilityActivationResult.Activated));
            Assert.That(activation.Type, Is.EqualTo(BattleEventType.AbilityActivated));
            Assert.That(activation.Payload.Value, Is.EqualTo(18));
            Assert.That(changedRuntime.Ability.CooldownRemaining, Is.EqualTo(9f));
            Assert.That(changedRuntime.TryActivateAbility(), Is.EqualTo(AbilityActivationResult.OnCooldown));

            Object.Destroy(root);
            Object.Destroy(standard); Object.Destroy(raider);
            Object.Destroy(sailors); Object.Destroy(sailmakers);
            Object.Destroy(rally); Object.Destroy(barrage);
            yield return null;
        }

        static LoadoutService CreateService(LocalSaveRepository repository)
        {
            return new LoadoutService(repository,
                new[] { DefaultFlagship, RaiderFlagship },
                new[] { DefaultCrew, SailmakersCrew },
                new[] { DefaultAbility, BarrageAbility });
        }

        static RewardDefinition CreateReward()
        {
            var reward = ScriptableObject.CreateInstance<RewardDefinition>();
            Set(typeof(DefinitionAsset), reward, "id", new StableId("reward-level01-loadout-blueprint"));
            Set(typeof(RewardDefinition), reward, "grantTargetId", SailmakersCrew);
            Set(typeof(RewardDefinition), reward, "grantType", RewardGrantType.Ownership);
            Set(typeof(RewardDefinition), reward, "amount", 1);
            Set(typeof(RewardDefinition), reward, "firstCompletionOnly", true);
            return reward;
        }

        static FlagshipDefinition CreateFlagship(StableId id, DeployPattern pattern, int burstSize,
            float cadence = 1f, float baseDeployment = 1f)
        {
            var flagship = ScriptableObject.CreateInstance<FlagshipDefinition>();
            Set(typeof(DefinitionAsset), flagship, "id", id);
            Set(typeof(FlagshipDefinition), flagship, "deployPattern", pattern);
            Set(typeof(FlagshipDefinition), flagship, "burstSize", burstSize);
            Set(typeof(FlagshipDefinition), flagship, "deploymentCadence", cadence);
            Set(typeof(FlagshipDefinition), flagship, "baseDeployment", baseDeployment);
            Set(typeof(FlagshipDefinition), flagship, "presentationShipId",
                new StableId(id.Value + "-presentation"));
            Set(typeof(FlagshipDefinition), flagship, "wakeId", new StableId(id.Value + "-wake"));
            Set(typeof(FlagshipDefinition), flagship, "recoilId", new StableId(id.Value + "-recoil"));
            Set(typeof(FlagshipDefinition), flagship, "audioId", new StableId(id.Value + "-audio"));
            return flagship;
        }

        static UnitRoleDefinition CreateCrew(StableId id, UnitRole role, float damage,
            float cadence, float durability)
        {
            var crew = ScriptableObject.CreateInstance<UnitRoleDefinition>();
            Set(typeof(DefinitionAsset), crew, "id", id);
            Set(typeof(UnitRoleDefinition), crew, "role", role);
            Set(typeof(UnitRoleDefinition), crew, "combat", new CombatStats(damage, cadence, 8f));
            Set(typeof(UnitRoleDefinition), crew, "durability", durability);
            return crew;
        }

        static CaptainAbilityDefinition CreateAbility(StableId id, AbilityChargeRule chargeRule,
            float duration, float cooldown, GateOutcome outcome, float value)
        {
            var ability = ScriptableObject.CreateInstance<CaptainAbilityDefinition>();
            Set(typeof(DefinitionAsset), ability, "id", id);
            Set(typeof(CaptainAbilityDefinition), ability, "chargeRule", chargeRule);
            Set(typeof(CaptainAbilityDefinition), ability, "activation", AbilityActivation.PlayerTap);
            Set(typeof(CaptainAbilityDefinition), ability, "gameplayEffect",
                new TypedEffect(outcome, value, StableId.Empty));
            Set(typeof(CaptainAbilityDefinition), ability, "duration", duration);
            Set(typeof(CaptainAbilityDefinition), ability, "cooldown", cooldown);
            return ability;
        }

        static BattleSession ActiveSession(LoadoutSnapshot loadout)
        {
            var session = new BattleSession(new StableId("level-01"), new StableId("opening"), loadout);
            Assert.That(session.TryTransition(BattleState.Ready), Is.True);
            Assert.That(session.TryTransition(BattleState.Active), Is.True);
            return session;
        }

        static void Set(System.Type owner, object target, string name, object value)
        {
            var field = owner.GetField(name, BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, name); field.SetValue(target, value);
        }

        sealed class TestCraft : ILandingCraft
        {
            public void Activate(Vector3 position, int contribution, int sequence) { }
            public void Deactivate() { }
        }

        sealed class MemoryFiles : ILocalSaveFileSystem
        {
            readonly Dictionary<string, string> entries = new Dictionary<string, string>();
            public bool Exists(string path) => entries.ContainsKey(path);
            public string ReadAllText(string path) => entries[path];
            public void WriteAllText(string path, string contents) => entries[path] = contents;
            public void Delete(string path) => entries.Remove(path);
            public void Replace(string temporaryPath, string destinationPath, string backupPath)
            {
                if (entries.ContainsKey(destinationPath)) entries[backupPath] = entries[destinationPath];
                entries[destinationPath] = entries[temporaryPath]; entries.Remove(temporaryPath);
            }
        }
    }
}
