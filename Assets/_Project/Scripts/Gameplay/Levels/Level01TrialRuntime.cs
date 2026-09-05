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
        private int FriendlyCombatants = 8;
        private int HostileCombatants => levelDefinition != null ? levelDefinition.EnemyCount : 8;
        [SerializeField] private LevelDefinition levelDefinition;
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
        [SerializeField, Min(1)] private int displayCap = 300;
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
            Mathf.Clamp01((float)landingIndex / Mathf.Max(1, fleet.Count));
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

        public void ConfigureLevel(LevelDefinition definition) { levelDefinition = definition; }
        public LevelDefinition Level => levelDefinition;
        public IReadOnlyList<FlagshipDefinition> Flagships => flagships;
        public IReadOnlyList<UnitRoleDefinition> CrewRoles => crewRoles;
        public IReadOnlyList<CaptainAbilityDefinition> CaptainAbilities => captainAbilities;
        public string SaveFileName => saveFileName;
        public CaptainAbilityDefinition ActiveAbility => loadout?.Ability.Definition;

        public bool Begin()
        {
            if (started || !DefinitionsReady()) return false;
            if (levelDefinition != null)
            {
                initialForce = levelDefinition.InitialForce;
                displayCap = levelDefinition.DisplayCap;
                guardianHealth = levelDefinition.BossHealth;
            }
            if (deployer == null || landing == null) Awake();
            repository = new LocalSaveRepository(Path.Combine(Application.persistentDataPath, saveFileName));
            rewardService = new RewardGrantService(repository);
            session = CreateSession();
            results = new BattleResultController(session, CreateSession);
            results.TerminalResultReceived += _ => StateChanged?.Invoke();
            started = BeginAttempt(session);
            return started;
        }
        private double pendingSeconds;
        private readonly SeaLion.Core.Simulation.FixedStepClock clock = new SeaLion.Core.Simulation.FixedStepClock();
        public long SimulationTick => clock.Tick;
        public void Step(float deltaSeconds)
        {
            if (!IsRunning || paused || !Finite(deltaSeconds) || deltaSeconds <= 0f) return;
            pendingSeconds += deltaSeconds;
            while (pendingSeconds + 1e-9 >= clock.FixedDeltaSeconds && IsRunning && !paused)
            {
                pendingSeconds -= clock.FixedDeltaSeconds;
                clock.AdvanceTicks(1);
                Simulate((float)clock.FixedDeltaSeconds);
            }
            StateChanged?.Invoke();
        }

        private void Simulate(float step)
        {
            if (Phase == Level01TrialPhase.Traversal || Phase == Level01TrialPhase.Assault)
                horizontalChoice = Mathf.Clamp(horizontalChoice + steeringIntent * step * 1.4f, -1f, 1f);
            phaseElapsed += step;
            totalElapsed += step;
            session.TryAdvance(step);
            loadout.TickAbility(step);
            TickPlayerInteraction(step);

            switch (Phase)
            {
                case Level01TrialPhase.Opening:
                    if (phaseElapsed >= (levelDefinition != null ? levelDefinition.OpeningThreatRevealSeconds : 2f)) SetPhase(Level01TrialPhase.Traversal);
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

        private float steeringIntent;
        public void SetSteeringIntent(float intent, bool engaged)
        {
            steeringIntent = Finite(intent) ? Mathf.Clamp(intent, -1f, 1f) : 0f;
            if (engaged && Mathf.Abs(steeringIntent) > .01f && Phase == Level01TrialPhase.Traversal)
                traversalPlayerSteered = true;
        }
        private void OnApplicationPause(bool value) { SetPaused(value); steeringIntent = 0f; }
        private void OnApplicationFocus(bool value) { if (!value) steeringIntent = 0f; }
        public void SetHorizontalChoice(float value)
        {
            if (Finite(value)) horizontalChoice = Mathf.Clamp(value, -1f, 1f);
        }

        public AbilityActivationResult TryActivateAbility()
        {
            if (!IsRunning || paused || loadout == null || (Phase != Level01TrialPhase.Traversal && Phase != Level01TrialPhase.Assault)) return AbilityActivationResult.Rejected;
            var result = loadout.TryActivateAbility();
            if (result != AbilityActivationResult.Activated) return result;
            var effect = loadout.Ability.Definition.GameplayEffect;
            var target = ActiveForce;
            if (effect.Outcome == GateOutcome.Add && target != null)
                ChangeForce(target, Mathf.Max(0, target.LogicalCount + Mathf.RoundToInt(effect.Value)));
            else if (effect.Outcome == GateOutcome.Multiply && target != null)
                ChangeForce(target, Mathf.Max(0, Mathf.RoundToInt(target.LogicalCount * effect.Value)));
            else if (effect.Outcome == GateOutcome.Damage && guardian != null)
                ApplyBossDamage(Mathf.Max(0f, effect.Value));
            if (target == seaForce && Phase == Level01TrialPhase.Traversal) ReconcileFleetCount();
            StateChanged?.Invoke();
            return result;
        }

        public bool Retry()
        {
            if (!CanRetry || !results.TryRetry(out var next)) return false;
            ClearAttempt();
            session = next;
            var began = BeginAttempt(session);
            if (began) SetPhase(Level01TrialPhase.Traversal);
            return began;
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
            LastGateBefore = LastGateAfter = 0;
            ChoseEasyGate = true;
            totalElapsed = horizontalChoice = 0f;
            pendingSeconds = 0d;
            clock.Reset();
            paused = false;
            steeringIntent = 0f;
            hostileRemaining = 0;
            ResetPlayerInteraction();
            ResetCampaign();
            ResetVoyage();
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

        private void BeginLanding()
        {
            landingIndex = 0;
            landing.Configure(session, landForce, new StableId("level01-beach"), fleet.Count);
        }

        private void StepLanding()
        {
            while (landingIndex < fleet.Count && phaseElapsed >=
                LandingDuration * (landingIndex + 1) / Mathf.Max(1, fleet.Count)) TransferNextCraft();
            if (landingIndex < fleet.Count) return;
            landing.Complete();
            SetPhase(Level01TrialPhase.Assault);
        }

        private void HandleDeployment(LandingCraftDeployment deployment)
        {
            AddCraft(deployment.Contribution);
            SyncSeaForce();
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
            { if (!AdvanceAssaultStage()) Finish(true, "guardian-defeated"); }
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
            if (victory) { RewardResult = reward; SaveCampaignProgress(); }
            if (victory && !reward.Succeeded) FailureReason = reward.Failure;
            if (!victory) FailureReason = reason;
            SetPhase(victory ? Level01TrialPhase.Victory : Level01TrialPhase.Failure);
        }
        private void ChangeForce(ForceRuntime target, int next)
        {
            if (target == null || next == target.LogicalCount) return;
            var before = target.LogicalCount;
            target.SetLogicalCount(next);
            if (target == landForce && loadout != null)
                target.SetRoleCounts(new[] { new KeyValuePair<UnitRole, int>(loadout.Crew.Role, next) });

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
                session.TrySetPhase(PhaseId("landing"));
                session.TryTransition(BattleState.Landing);
                BeginLanding();
            }
            else if (next == Level01TrialPhase.Assault)
            {
                session.TrySetPhase(PhaseId("assault"));
                session.TryTransition(BattleState.Assault);
                BeginAssault();
            }
            else if (next == Level01TrialPhase.Traversal)
                session.TrySetPhase(PhaseId("traversal"));
            else if (next == Level01TrialPhase.Opening)
                session.TrySetPhase(PhaseId("opening"));
            PhaseChanged?.Invoke(next);
            StateChanged?.Invoke();
        }

        private BattleSession CreateSession()
        {
            var loaded = repository.Load();
            var data = loaded.Data ?? LocalSaveRepository.CreateDefault();
            var snapshot = data.selectedLoadout.ToSnapshot();
            if (!HasDefinitions(snapshot)) snapshot = DefaultSnapshot();
            return new BattleSession(levelDefinition != null ? levelDefinition.Id : new StableId("level-01-hundred-sails"),
                PhaseId("opening"), snapshot);
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
            if (deployer != null) deployer.Deployed -= HandleDeployment;
            if (guardian != null) guardian.Event -= HandleGuardianEvent;
            if (combat != null) combat.Death -= HandleCombatDeath;
            deployer?.Dispose();
            landingTokens.Clear();
            fleet.Clear();
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
