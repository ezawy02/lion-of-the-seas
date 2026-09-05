using System;
using System.Collections.Generic;
using System.IO;
using SeaLion.Combat;
using SeaLion.Combat.Bosses;
using SeaLion.Core.Battle;
using SeaLion.Core.Definitions;
using SeaLion.Core.Events;
using SeaLion.Core.Persistence;
using SeaLion.Crowd.Simulation;
using SeaLion.Gameplay.Abilities;
using SeaLion.Gameplay.Deployment;
using SeaLion.Gameplay.Gates;
using SeaLion.Gameplay.Landing;
using SeaLion.Gameplay.Loadout;
using SeaLion.Gameplay.Results;
using SeaLion.Gameplay.Rewards;
using Unity.Mathematics;
using UnityEngine;

namespace SeaLion.Gameplay.Levels
{
    public enum Level01TrialPhase
    {
        Loading,
        Opening,
        Traversal,
        Landing,
        Assault,
        Victory,
        Failure
    }

    /// <summary>
    /// Playable Level 1 trial coordinator. It joins the tested domain systems while the
    /// separate scene presenter owns approved art, cameras, audio, and visible feedback.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed partial class Level01TrialRuntime : MonoBehaviour
    {
        private const int LandingCraftCount = 6;
        private const int FriendlyCombatants = 8;
        private const int HostileCombatants = 8;
        [Header("Definitions")]
        [SerializeField] private FlagshipDefinition[] flagships = Array.Empty<FlagshipDefinition>();
        [SerializeField] private UnitRoleDefinition[] crewRoles = Array.Empty<UnitRoleDefinition>();
        [SerializeField] private CaptainAbilityDefinition[] captainAbilities = Array.Empty<CaptainAbilityDefinition>();
        [SerializeField] private GateDefinition easyGate;
        [SerializeField] private GateDefinition riskyGate;
        [SerializeField] private RescueDefinition rescue;
        [SerializeField] private BossDefinition guardianDefinition;
        [SerializeField] private RewardDefinition rewardDefinition;

        [Header("Trial tuning")]
        [SerializeField] private string saveFileName = LocalSaveRepository.DefaultFileName;
        [SerializeField, Min(1)] private int initialForce = 8;
        [SerializeField, Min(1)] private int displayCap = 120;
        [SerializeField, Min(10f)] private float guardianHealth = 140f;
        private readonly List<LandingToken> landingTokens = new List<LandingToken>(LandingCraftCount);
        private LandingCraftDeployer deployer;
        private LandingZoneController landing;
        private LocalSaveRepository repository;
        private RewardGrantService rewardService;
        private BattleSession session;
        private BattleResultController results;
        private BattleLoadoutRuntime loadout;
        private ForceRuntime seaForce;
        private ForceRuntime landForce;
        private GateResolver gateResolver;
        private HarborGuardianController guardian;
        private OrdinaryCombatSystem combat;
        private CombatUnit[] combatants;
        private int hostileRemaining;
        private int landingIndex;
        private int landingContribution;
        private int landingRemainder;
        private int lossPerFriendly;
        private float phaseElapsed;
        private float totalElapsed;
        private float horizontalChoice;
        private float combatAccumulator;
        private float guardianAttackAccumulator;
        private bool gateCommitted;
        private bool rescueApplied;
        private bool started;
        private bool paused;

        public event Action<Level01TrialPhase> PhaseChanged;
        public event Action<BattleSession> AttemptStarted;
        public event Action<HarborGuardianEvent> GuardianEvent;
        public event Action StateChanged;
        public Level01TrialPhase Phase { get; private set; } = Level01TrialPhase.Loading;
        public BattleSession Session => session;
        public float PhaseElapsed => phaseElapsed;
        public float TotalElapsed => totalElapsed;
        public int ForceCount => ActiveForce == null ? 0 : ActiveForce.LogicalCount;
        public int DisplayedForceCount => ActiveForce == null ? 0 : ActiveForce.DisplayedAgentCount;
        public int DisplayCap => displayCap;
        public float BossHealth01 => guardian == null ? 1f : guardian.Health01;
        public bool GateCommitted => gateCommitted;
        public bool ChoseEasyGate { get; private set; }
        public bool AbilityReady => loadout != null && loadout.Ability.IsReady;
        public float AbilityCharge01 => loadout == null ? 0f : loadout.Ability.Charge;
        public float AbilityCooldown => loadout == null ? 0f : loadout.Ability.CooldownRemaining;
        public bool CanRetry => results != null && results.HasTerminalResult;
        public RewardGrantResult? RewardResult { get; private set; }
        public string FailureReason { get; private set; } = string.Empty;
        public float HorizontalChoice => horizontalChoice;
        public int LastGateBefore { get; private set; }
        public int LastGateAfter { get; private set; }
        public GateDefinition EasyGate => easyGate;
        public GateDefinition RiskyGate => riskyGate;
        public float LandingProgress01 => Phase != Level01TrialPhase.Landing ? 0f :
            Mathf.Clamp01((float)landingIndex / Mathf.Max(1, LandingCraftCount));
        public bool IsRunning => started && Phase != Level01TrialPhase.Victory && Phase != Level01TrialPhase.Failure;
        public FlagshipDefinition ActiveFlagship => loadout == null ? null : FindFlagship(session.SelectedLoadout.FlagshipId);

        private ForceRuntime ActiveForce => Phase == Level01TrialPhase.Landing ||
            Phase == Level01TrialPhase.Assault || Phase == Level01TrialPhase.Victory ||
            Phase == Level01TrialPhase.Failure ? landForce : seaForce;

        private void Awake()
        {
            deployer = GetComponent<LandingCraftDeployer>();
            if (deployer == null) deployer = gameObject.AddComponent<LandingCraftDeployer>();
            landing = GetComponent<LandingZoneController>();
            if (landing == null) landing = gameObject.AddComponent<LandingZoneController>();
        }

        private void Update()
        {
            if (started && !paused) Step(Time.deltaTime);
        }

        public void Configure(FlagshipDefinition[] ships, UnitRoleDefinition[] crews,
            CaptainAbilityDefinition[] abilities, GateDefinition easy, GateDefinition risky,
            RescueDefinition rescueDefinition, BossDefinition boss, RewardDefinition reward,
            string localSaveFileName = LocalSaveRepository.DefaultFileName)
        {
            flagships = ships ?? Array.Empty<FlagshipDefinition>();
            crewRoles = crews ?? Array.Empty<UnitRoleDefinition>();
            captainAbilities = abilities ?? Array.Empty<CaptainAbilityDefinition>();
            easyGate = easy;
            riskyGate = risky;
            rescue = rescueDefinition;
            guardianDefinition = boss;
            rewardDefinition = reward;
            saveFileName = string.IsNullOrWhiteSpace(localSaveFileName) ?
                LocalSaveRepository.DefaultFileName : localSaveFileName;
        }

        public bool Begin()
        {
            if (started || !DefinitionsReady()) return false;
            if (deployer == null || landing == null) Awake();
            repository = new LocalSaveRepository(Path.Combine(Application.persistentDataPath, saveFileName));
            rewardService = new RewardGrantService(repository);
            session = CreateSession();
            results = new BattleResultController(session, CreateSession);
            results.TerminalResultReceived += _ => StateChanged?.Invoke();
            started = BeginAttempt(session);
            return started;
        }
        public void Step(float deltaSeconds)
        {
            if (!IsRunning || !Finite(deltaSeconds) || deltaSeconds <= 0f) return;
            var step = Mathf.Min(deltaSeconds, 0.1f);
            phaseElapsed += step;
            totalElapsed += step;
            session.TryAdvance(step);
            loadout.TickAbility(step);
            TickPlayerInteraction(step);

            switch (Phase)
            {
                case Level01TrialPhase.Opening:
                    if (phaseElapsed >= 3f) SetPhase(Level01TrialPhase.Traversal);
                    break;
                case Level01TrialPhase.Traversal:
                    StepTraversal(step);
                    break;
                case Level01TrialPhase.Landing:
                    StepLanding();
                    break;
                case Level01TrialPhase.Assault:
                    StepAssault(step);
                    break;
            }
            StateChanged?.Invoke();
        }

        public void SetHorizontalChoice(float value)
        {
            if (Finite(value)) horizontalChoice = Mathf.Clamp(value, -1f, 1f);
        }

        public AbilityActivationResult TryActivateAbility()
        {
            if (!IsRunning || loadout == null) return AbilityActivationResult.Rejected;
            var result = loadout.TryActivateAbility();
            if (result != AbilityActivationResult.Activated) return result;
            var effect = loadout.Ability.Definition.GameplayEffect;
            var target = ActiveForce;
            if (effect.Outcome == GateOutcome.Add && target != null)
                ChangeForce(target, Mathf.Max(0, target.LogicalCount + Mathf.RoundToInt(effect.Value)));
            else if (effect.Outcome == GateOutcome.Multiply && target != null)
                ChangeForce(target, Mathf.Max(0, Mathf.RoundToInt(target.LogicalCount * effect.Value)));
            else if (effect.Outcome == GateOutcome.Damage && guardian != null)
                guardian.ApplyDamage(Mathf.Max(0f, effect.Value), Mathf.RoundToInt(totalElapsed * 10f));
            StateChanged?.Invoke();
            return result;
        }

        public bool Retry()
        {
            if (!CanRetry || !results.TryRetry(out var next)) return false;
            ClearAttempt();
            session = next;
            return BeginAttempt(session);
        }

        public void SetPaused(bool value)
        {
            paused = value;
            deployer?.SetPaused(value || Phase != Level01TrialPhase.Traversal);
            landing?.SetPaused(value);
        }

        private bool BeginAttempt(BattleSession attempt)
        {
            RewardResult = null;
            FailureReason = string.Empty;
            gateCommitted = rescueApplied = false;
            ChoseEasyGate = true;
            totalElapsed = horizontalChoice = 0f;
            hostileRemaining = 0;
            ResetPlayerInteraction();
            seaForce = new ForceRuntime(initialForce, displayCap);
            landForce = new ForceRuntime(0, displayCap);
            gateResolver = new GateResolver(displayCap, attempt);
            if (!TryCreateLoadout(attempt, out loadout)) return false;
            loadout.Configure(deployer, () => new DeploymentToken(), 48, Vector3.zero, 2.5f, 12);
            deployer.Deployed += HandleDeployment;
            deployer.SetPaused(true);
            if (!attempt.TryTransition(BattleState.Ready) || !attempt.TryTransition(BattleState.Active)) return false;
            SetPhase(Level01TrialPhase.Opening);
            AttemptStarted?.Invoke(attempt);
            return true;
        }

        private void CommitGate(GateDefinition gate)
        {
            var before = seaForce.LogicalCount;
            var result = gateResolver.Resolve(gate, before, new StableId("trial-fleet"));
            if (!result.Applied) return;
            ChoseEasyGate = gate == easyGate;
            gateCommitted = true;
            LastGateBefore = before;
            LastGateAfter = result.After;
            seaForce.SetLogicalCount(result.After);
            loadout.ReportGateResolved();
        }

        private void ApplyRescue()
        {
            rescueApplied = true;
            ChangeForce(seaForce, checked(seaForce.LogicalCount + rescue.SurvivorCount));
        }

        private void BeginLanding()
        {
            landingTokens.Clear();
            for (var index = 0; index < LandingCraftCount; index++) landingTokens.Add(new LandingToken());
            landingIndex = 0;
            landingContribution = seaForce.LogicalCount / LandingCraftCount;
            landingRemainder = seaForce.LogicalCount % LandingCraftCount;
            landing.Configure(session, landForce, new StableId("level01-beach"), LandingCraftCount);
        }

        private void StepLanding()
        {
            while (landingIndex < LandingCraftCount && phaseElapsed >= 0.8f + landingIndex * 1.15f)
            {
                var contribution = landingContribution + (landingIndex < landingRemainder ? 1 : 0);
                landing.TryAccept(landingTokens[landingIndex], landingIndex, contribution, contribution > 0);
                landingIndex++;
            }
            if (phaseElapsed < 9f) return;
            landing.Complete();
            SetPhase(Level01TrialPhase.Assault);
        }

        private void BeginAssault()
        {
            guardian = new HarborGuardianController(guardianDefinition.Id, guardianHealth,
                guardianDefinition.Phases, guardianDefinition.Attacks,
                guardianDefinition.FailurePressure, 1f);
            guardian.Event += HandleGuardianEvent;
            guardian.Enter();
            combat = new OrdinaryCombatSystem();
            combat.Death += HandleCombatDeath;
            BuildCombatants();
            hostileRemaining = HostileCombatants;
            lossPerFriendly = Mathf.Max(1, landForce.LogicalCount / 32);
            combatAccumulator = guardianAttackAccumulator = 0f;
        }

        private void StepAssault(float step)
        {
            combatAccumulator += step;
            while (combatAccumulator >= 0.25f && hostileRemaining > 0)
            {
                combatAccumulator -= 0.25f;
                combat.StepHostileAttacks(combatants, 0.25f);
            }

            guardianAttackAccumulator += step;
            if (guardianAttackAccumulator >= 6f && guardian.State == HarborGuardianState.Active)
            {
                guardianAttackAccumulator -= 6f;
                var attack = FirstAttack();
                guardian.TryFireAttack(attack, Mathf.RoundToInt(totalElapsed * 10f));
                var baseLoss = Mathf.Max(3, Mathf.CeilToInt(landForce.LogicalCount * 0.12f));
                var loss = ComputeGuardianLoss(baseLoss);
                ChangeForce(landForce, Mathf.Max(0, landForce.LogicalCount - loss));
                guardian.NotifyForceRemaining(landForce.LogicalCount, Mathf.RoundToInt(totalElapsed * 10f));
            }
            if (AssaultTimedOut(phaseElapsed) && Phase == Level01TrialPhase.Assault)
                Finish(false, "guardian-timeout");
        }

        private void BuildCombatants()
        {
            combatants = new CombatUnit[FriendlyCombatants + HostileCombatants];
            for (var index = 0; index < FriendlyCombatants; index++)
            {
                var unit = new CombatUnit(CombatTeam.Friendly, new float3(index % 4, 0f, index / 4),
                    6f, 2f, 12f, 0.75f);
                combatants[index] = loadout.ApplyCrewTo(unit);
            }
            for (var index = 0; index < HostileCombatants; index++)
                combatants[FriendlyCombatants + index] = new CombatUnit(CombatTeam.Hostile,
                    new float3(index % 4, 0f, 1f + index / 4), 5f, 1.1f, 12f, 1.15f);
        }

        private void HandleCombatDeath(CombatDeath death)
        {
            if (death.Unit < FriendlyCombatants)
                ChangeForce(landForce, Mathf.Max(0, landForce.LogicalCount - lossPerFriendly));
            else
                hostileRemaining = Mathf.Max(0, hostileRemaining - 1);
        }

        private void HandleDeployment(LandingCraftDeployment deployment)
        {
            ChangeForce(seaForce, checked(seaForce.LogicalCount + deployment.Contribution));
        }

        private void HandleGuardianEvent(HarborGuardianEvent item)
        {
            GuardianEvent?.Invoke(item);
            if (item.Type == HarborGuardianEventType.Entered || item.Type == HarborGuardianEventType.PhaseChanged)
                session.TryPublishGameplayEvent(BattleEventType.BossPhaseChanged,
                    new BattleEventPayload(session.SessionId, item.BossId, item.AttackId,
                        Allegiance.Hostile, Mathf.RoundToInt(item.Before), Mathf.RoundToInt(item.After),
                        item.Phase, default, default));
            else if (item.Type == HarborGuardianEventType.Victory)
                Finish(true, "guardian-defeated");
            else if (item.Type == HarborGuardianEventType.Failure)
                Finish(false, "force-depleted");
        }
        private void Finish(bool victory, string reason)
        {
            if (Phase == Level01TrialPhase.Victory || Phase == Level01TrialPhase.Failure) return;
            deployer.SetTerminal(true);
            landing.SetTerminal(true);
            if (!session.End(victory, reason)) return;
            var reward = default(RewardGrantResult);
            if (victory) rewardService.TryGrant(session, rewardDefinition, out reward);
            if (victory) RewardResult = reward;
            if (victory && !reward.Succeeded) FailureReason = reward.Failure;
            if (!victory) FailureReason = reason;
            SetPhase(victory ? Level01TrialPhase.Victory : Level01TrialPhase.Failure);
        }
        private void ChangeForce(ForceRuntime target, int next)
        {
            if (target == null || next == target.LogicalCount) return;
            var before = target.LogicalCount;
            target.SetLogicalCount(next);
            session.TryPublishGameplayEvent(BattleEventType.ForceChanged,
                new BattleEventPayload(session.SessionId, new StableId("trial-fleet"), default,
                    Allegiance.Friendly, before, next, next - before, default, default));
        }
        private void SetPhase(Level01TrialPhase next)
        {
            Phase = next;
            phaseElapsed = 0f;
            deployer.SetPaused(paused || next != Level01TrialPhase.Traversal);
            if (next == Level01TrialPhase.Landing)
            {
                session.TrySetPhase(new StableId("level01-landing"));
                session.TryTransition(BattleState.Landing);
                BeginLanding();
            }
            else if (next == Level01TrialPhase.Assault)
            {
                session.TrySetPhase(new StableId("level01-assault"));
                session.TryTransition(BattleState.Assault);
                BeginAssault();
            }
            else if (next == Level01TrialPhase.Traversal)
                session.TrySetPhase(new StableId("level01-traversal"));
            else if (next == Level01TrialPhase.Opening)
                session.TrySetPhase(new StableId("level01-opening"));
            PhaseChanged?.Invoke(next);
            StateChanged?.Invoke();
        }

        private BattleSession CreateSession()
        {
            var loaded = repository.Load();
            var data = loaded.Data ?? LocalSaveRepository.CreateDefault();
            var snapshot = data.selectedLoadout.ToSnapshot();
            if (!HasDefinitions(snapshot)) snapshot = DefaultSnapshot();
            return new BattleSession(new StableId("level-01-hundred-sails"),
                new StableId("level01-opening"), snapshot);
        }

        private bool TryCreateLoadout(BattleSession attempt, out BattleLoadoutRuntime value)
        {
            if (BattleLoadoutRuntime.TryCreate(attempt, flagships, crewRoles, captainAbilities,
                out value, out var failure)) return true;
            FailureReason = failure;
            Debug.LogError("Level 1 trial loadout could not be created: " + failure, this);
            return false;
        }

        private bool HasDefinitions(LoadoutSnapshot snapshot)
        {
            return FindFlagship(snapshot.FlagshipId) != null &&
                FindDefinition(crewRoles, snapshot.CrewRoleId) != null &&
                FindDefinition(captainAbilities, snapshot.CaptainAbilityId) != null;
        }

        private LoadoutSnapshot DefaultSnapshot()
        {
            return new LoadoutSnapshot(new StableId("default-flagship"),
                new StableId("default-crew"), new StableId("default-ability"));
        }

        private FlagshipDefinition FindFlagship(StableId id) => FindDefinition(flagships, id);

        private static T FindDefinition<T>(IEnumerable<T> values, StableId id) where T : DefinitionAsset
        {
            if (values == null) return null;
            foreach (var value in values) if (value != null && value.Id == id) return value;
            return null;
        }

        private StableId FirstAttack()
        {
            foreach (var attack in guardianDefinition.Attacks) return attack;
            return new StableId("attack-harbor-broadside");
        }

        private bool DefinitionsReady()
        {
            return flagships.Length > 0 && crewRoles.Length > 0 && captainAbilities.Length > 0 &&
                easyGate != null && riskyGate != null && rescue != null &&
                guardianDefinition != null && rewardDefinition != null;
        }

        private void ClearAttempt()
        {
            deployer.Deployed -= HandleDeployment;
            if (guardian != null) guardian.Event -= HandleGuardianEvent;
            if (combat != null) combat.Death -= HandleCombatDeath;
            deployer.Dispose();
            landingTokens.Clear();
            guardian = null;
            combat = null;
            combatants = null;
        }
        private void OnDestroy()
        {
            ClearAttempt();
            results?.Dispose();
        }

        private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);
    }
}
