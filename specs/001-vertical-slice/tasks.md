# Tasks: First Playable Vertical Slice

**Input**: Design documents from `specs/001-vertical-slice/`  
**Prerequisites**: `plan.md`, `spec.md`, `research.md`, `data-model.md`, `contracts/`

**Tests and evidence**: Gameplay tests, performance captures, visual reviews, source-size
checks, license records, and store-promise validation are required by the constitution.

## Format

Every task uses `[ID] [P?] [Story?] Description with exact path`. `[P]` means the task can
run in parallel without touching files owned by an incomplete dependency.

## Phase 1: Local Project Setup

**Purpose**: Create the minimum Unity repository and local guardrails. Do not add gameplay.

- [x] T001 Install Unity Hub and the latest Unity 6.3 LTS editor with Android Build Support, SDK, NDK, and OpenJDK; record the revision in `ProjectSettings/ProjectVersion.txt`
- [x] T002 Create a Universal 3D Unity project named LionOfTheSeas in the repository and preserve `.specify/`, `specs/`, `.git/`, `README.md`, and `AGENTS.md`
- [x] T003 Configure portrait orientation, Android ARM64, Android 9/API 28 minimum, product name, and package placeholder in `ProjectSettings/ProjectSettings.asset`
- [x] T004 Add only URP, Input System, Cinemachine, Burst, Collections, Mathematics, Test Framework, and Performance Testing dependencies to `Packages/manifest.json`
- [x] T005 Create the planned `Assets/_Project/` directory tree and placeholder ownership notes in `Assets/_Project/README.md`
- [x] T006 [P] Create Core, Crowd, Gameplay, Presentation, UI, and test assembly definitions under `Assets/_Project/Scripts/` and `Assets/_Project/Tests/`
- [x] T007 [P] Create `ArtSource/Blender/Characters/`, `ArtSource/Blender/Environment/`, and `ArtSource/Blender/Ships/` with source/export conventions in `ArtSource/README.md`
- [x] T008 [P] Implement the non-blank authored C# size checker with 1,000-line warning and 1,500-line failure in `tools/check-source-size.sh` [SC-009]
- [x] T009 [P] Add EditMode tests for source-size categorization, generated/vendor exclusions, and threshold results in `Assets/_Project/Tests/EditMode/Maintainability/SourceSizePolicyTests.cs` [SC-009]
- [x] T010 [P] Define asset naming, import, atlas, LOD, pivot, scale, and FBX export rules in `Assets/_Project/Art/ART_PIPELINE.md`
- [x] T011 [P] Define evidence folder naming and local-only capture exclusions in `Artifacts/README.md`
- [x] T012 Define Git LFS patterns for future `.blend`, `.fbx`, texture, audio, and video assets in `.gitattributes`; verify the exact tracked-file boundary and private repository rule in `README.md`

**Checkpoint**: Unity opens without errors, Android tooling is available, and guardrail tests run.

## Phase 2: Foundational Systems

**Purpose**: Build shared contracts that block all playable user stories.

**Critical**: No level implementation begins until T013–T030 are complete.

- [x] T013 [P] Define stable-ID and definition validation primitives in `Assets/_Project/Scripts/Core/Definitions/StableId.cs` and `Assets/_Project/Scripts/Core/Definitions/DefinitionValidation.cs` [FR-020]
- [x] T014 [P] Create level, phase, gate, unit, flagship, captain ability, boss, reward, and quality ScriptableObject definitions under `Assets/_Project/Scripts/Core/Definitions/` [FR-003] [FR-014] [FR-018]
- [x] T015 [P] Add EditMode validation tests for missing IDs, broken references, illegal gate values, invalid loadouts, and phase cycles in `Assets/_Project/Tests/EditMode/Definitions/DefinitionValidationTests.cs`
- [x] T016 Implement deterministic battle lifecycle and strictly ordered domain events in `Assets/_Project/Scripts/Core/Battle/BattleSession.cs` and `Assets/_Project/Scripts/Core/Events/BattleEventStream.cs` [FR-012]
- [x] T017 [P] Add deterministic seed, fixed-step clock, and replay input record types in `Assets/_Project/Scripts/Core/Simulation/` [FR-004]
- [x] T018 [P] Implement versioned save schema, validation, atomic replacement, and idempotent reward transaction support in `Assets/_Project/Scripts/Persistence/LocalSaveRepository.cs` [FR-015] [FR-016]
- [x] T019 [P] Add EditMode tests for clean save, invalid IDs, interrupted writes, schema migration, and duplicate rewards in `Assets/_Project/Tests/EditMode/Persistence/LocalSaveRepositoryTests.cs` [FR-015] [FR-016]
- [x] T020 [P] Implement one-handed horizontal input adapter and lost-focus reset in `Assets/_Project/Scripts/Gameplay/Input/FlagshipInputAdapter.cs` [FR-001]
- [x] T021 [P] Implement typed reusable object pools for craft, projectiles, VFX, debris, UI numbers, and audio sources in `Assets/_Project/Scripts/Presentation/Pooling/` [SC-006]
- [x] T022 Implement structure-of-arrays crowd buffers and lifecycle ownership in `Assets/_Project/Scripts/Crowd/Simulation/CrowdBuffers.cs` [SC-006]
- [x] T023 Implement fixed-step Burst movement, formation target, and state-transition jobs in `Assets/_Project/Scripts/Crowd/Simulation/Jobs/` [SC-006]
- [x] T024 Implement uniform spatial-grid rebuild and query jobs in `Assets/_Project/Scripts/Crowd/Spatial/` [SC-006]
- [x] T025 Implement GPU-instanced ordinary-unit presentation with per-instance team, state, and animation phase in `Assets/_Project/Scripts/Crowd/Rendering/InstancedCrowdRenderer.cs` [FR-006] [SC-006]
- [x] T026 [P] Create Primary and Reduced URP/quality definitions under `Assets/_Project/Settings/Quality/` and runtime selection in `Assets/_Project/Scripts/Presentation/Quality/QualityProfileController.cs` [FR-018] [FR-024]
- [x] T027 [P] Create `Benchmark_Stress.unity` with deterministic 300-agent and 500-agent scenarios in `Assets/_Project/Scenes/Benchmark_Stress.unity` [SC-006]
- [x] T028 [P] Add Performance tests for 300-agent primary, 500-agent floor, allocation spikes, and quality-outcome parity in `Assets/_Project/Tests/Performance/CrowdPerformanceTests.cs` [SC-006]
- [ ] T029 Create bootstrap, frontend, direct-level-launch, save, quality, and scene-transition composition in `Assets/_Project/Scenes/Bootstrap.unity` and `Assets/_Project/Scripts/Core/Bootstrap/` [FR-012] [FR-019]
- [ ] T030 Run all foundational tests and record the first local evidence index in `Artifacts/Local/foundation/evidence.md`

**Checkpoint**: Definitions validate, sessions replay deterministically, persistence is safe,
and the empty stress scene can exercise scalable crowd state and rendering.

## Phase 3: User Story 1 — Command the First Landing (P1) — MVP

**Goal**: Complete Level 1 from control through reward with no written tutorial.

**Independent Test**: Launch Level 1 from a clean save and move, multiply, land, fight,
win/fail, claim a reward, and retry without any unfinished feature.

### Tests first

- [ ] T031 [P] [US1] Add EditMode tests for Add, Multiply, Convert, Damage, cap, rounding, and exactly-once gate resolution in `Assets/_Project/Tests/EditMode/Gates/GateResolverTests.cs` [FR-003] [FR-004]
- [ ] T032 [P] [US1] Add PlayMode tests for clean-save Level 1 control, gate, landing, guardian, victory, failure, and retry in `Assets/_Project/Tests/PlayMode/Levels/Level01JourneyTests.cs` [FR-001] [FR-009] [FR-012] [FR-013]
- [ ] T033 [P] [US1] Add PlayMode tests for release-outside-bounds, zero force, overlapping gates, cap compression, boss defeat during VFX, and pause/resume in `Assets/_Project/Tests/PlayMode/Levels/Level01EdgeCaseTests.cs`

### Core implementation

- [ ] T034 [P] [US1] Author Level 1, phase, gate, rescue, guardian, and reward definitions under `Assets/_Project/Data/Levels/Level01/` [FR-007] [FR-009]
- [ ] T035 [P] [US1] Create the Level 1 greybox scene with flagship lane, gates, rescue, beach, defender field, and guardian anchors in `Assets/_Project/Scenes/Level_01_HundredSails.unity` [FR-009]
- [ ] T036 [US1] Implement clamped responsive flagship movement in `Assets/_Project/Scripts/Gameplay/Flagship/FlagshipController.cs` [FR-001]
- [ ] T037 [US1] Implement pooled continuous landing-craft deployment in `Assets/_Project/Scripts/Gameplay/Deployment/LandingCraftDeployer.cs` [FR-002]
- [ ] T038 [US1] Implement deterministic gate commitment and arithmetic in `Assets/_Project/Scripts/Gameplay/Gates/GateResolver.cs` [FR-003] [FR-004]
- [ ] T039 [US1] Implement logical-count versus displayed-agent compression in `Assets/_Project/Scripts/Crowd/Simulation/ForceRuntime.cs` [FR-004] [FR-018]
- [ ] T040 [US1] Implement landing-zone acceptance and one-time sea-to-land force transfer in `Assets/_Project/Scripts/Gameplay/Landing/LandingZoneController.cs` [FR-005]
- [ ] T041 [US1] Implement coarse land targeting, attack cadence, damage, death, and stable tie-breaks in `Assets/_Project/Scripts/Combat/OrdinaryCombatSystem.cs` [FR-005] [FR-008]
- [ ] T042 [US1] Implement Harbor Guardian entry, readable attacks, health, hit reaction events, victory, and failure pressure in `Assets/_Project/Scripts/Combat/Bosses/HarborGuardianController.cs` [FR-007] [FR-008] [FR-009]
- [ ] T043 [P] [US1] Implement force count, gate result, boss health, ability placeholder, and result UI in `Assets/_Project/Scripts/UI/Battle/` [FR-004] [FR-008]
- [ ] T044 [P] [US1] Implement pooled gate, hit, loss, landing, boss, destruction, victory, and failure presentation subscribers in `Assets/_Project/Scripts/Presentation/Battle/` [FR-008]
- [ ] T045 [US1] Implement terminal result isolation and under-three-second retry in `Assets/_Project/Scripts/Gameplay/Results/BattleResultController.cs` [FR-012] [FR-013] [SC-007]
- [ ] T046 [US1] Implement first-completion blueprint reward grant and presentation in `Assets/_Project/Scripts/Gameplay/Rewards/RewardGrantService.cs` and `Assets/_Project/Scripts/UI/Rewards/RewardRevealView.cs` [FR-016]

### MVP art and validation

- [ ] T047 [P] [US1] Create the final-quality Level 1 benchmark flagship, crew, enemy, gate, guardian, and environment slice under `ArtSource/Blender/` and `Assets/_Project/Art/` [FR-006] [SC-003] [SC-004]
- [ ] T048 [P] [US1] Create the benchmark stylized water, foam, wake, landing, hit, and boss-reaction pass in `Assets/_Project/Materials/Water/` and `Assets/_Project/VFX/` [SC-004]
- [ ] T049 [US1] Tune Level 1 opening threat, gate spacing, landing timing, guardian pressure, and 60–75 second target in `Assets/_Project/Data/Levels/Level01/Level01.asset` [FR-007] [SC-001] [SC-002]
- [ ] T050 [US1] Run the no-instruction comprehension and five-second readability tests in `Artifacts/Local/playtests/level01-player-gate.md` [SC-001] [SC-002] [SC-003]
- [ ] T051 [US1] Build `Assets/_Project/Scenes/Benchmark_Art.unity`, pass the Art Quality Contract, and pass the 300/500-agent M2 benchmark before approving later levels in `Artifacts/Local/m2-gate/evidence.md` [SC-004] [SC-006]

**Checkpoint**: User Story 1 is a complete playable MVP and all Level 1 tests pass.

## Phase 4: User Story 2 — Make a Tactical Loadout Choice (P2)

**Goal**: Let the player change flagship, crew role, and captain ability, persist the choice,
and experience a measurable replay difference.

**Independent Test**: Complete Level 1, change one slot, restart the app, and replay Level 1
with the selected option present and behaviorally different.

### Tests first

- [ ] T052 [P] [US2] Add EditMode tests for owned-option validation, defaults, persistence, and invalid selection fallback in `Assets/_Project/Tests/EditMode/Loadout/LoadoutServiceTests.cs` [FR-014] [FR-015]
- [ ] T053 [P] [US2] Add PlayMode tests for reward unlock, three-slot selection, restart persistence, and replay difference in `Assets/_Project/Tests/PlayMode/Loadout/LoadoutJourneyTests.cs` [FR-014] [FR-015] [FR-016]

### Implementation

- [ ] T054 [P] [US2] Author two flagship, two crew, and two captain ability definitions under `Assets/_Project/Data/Loadouts/VerticalSlice/` [FR-014]
- [ ] T055 [US2] Implement loadout ownership, selection, validation, and immutable battle snapshot in `Assets/_Project/Scripts/Loadout/LoadoutService.cs` [FR-014] [FR-015]
- [ ] T056 [P] [US2] Build the three-slot loadout screen with role, trade-off, lock, and active state in `Assets/_Project/Scripts/UI/Loadout/` and `Assets/_Project/Prefabs/UI/Loadout/` [FR-014]
- [ ] T057 [US2] Connect flagship selection to deployment pattern and presentation in `Assets/_Project/Scripts/Gameplay/Flagship/FlagshipLoadoutAdapter.cs` [FR-014]
- [ ] T058 [US2] Connect crew selection to role composition and combat contribution in `Assets/_Project/Scripts/Combat/CrewRoleLoadoutAdapter.cs` [FR-014]
- [ ] T059 [US2] Implement charge, accepted activation, outcome event, and cooldown for captain abilities in `Assets/_Project/Scripts/Gameplay/Abilities/CaptainAbilitySystem.cs` [FR-014]
- [ ] T060 [P] [US2] Add ability-ready, rejected, active, and cooldown feedback in `Assets/_Project/Scripts/UI/Battle/CaptainAbilityView.cs` [FR-008]
- [ ] T061 [US2] Connect Level 1 reward ownership to the relevant loadout option in `Assets/_Project/Data/Rewards/Level01Blueprint.asset` [FR-016]
- [ ] T062 [US2] Capture default-versus-changed replay evidence and review whether the trade-off is visible in `Artifacts/Local/playtests/loadout-replay.md` [SC-004]

**Checkpoint**: User Stories 1 and 2 work independently and persist through restart.

## Phase 5: User Story 3 — Conquer Three Distinct Encounters (P3)

**Goal**: Deliver Levels 2 and 3 as data-driven extensions with new readable decisions.

**Independent Test**: Direct launch, complete, fail, and retry each level without campaign,
store-preview, or unfinished later-content dependencies.

### Tests first

- [ ] T063 [P] [US3] Add PlayMode tests for Level 2 three-lane commitment, reward/hazard readability, chain blockade, and armored boss phases in `Assets/_Project/Tests/PlayMode/Levels/Level02JourneyTests.cs` [FR-010]
- [ ] T064 [P] [US3] Add PlayMode tests for Level 3 storm movement, force-versus-powder choice, fortress landing, and two assault phases in `Assets/_Project/Tests/PlayMode/Levels/Level03JourneyTests.cs` [FR-011]
- [ ] T065 [P] [US3] Add direct-launch, completion, failure, and retry matrix tests for all levels in `Assets/_Project/Tests/PlayMode/Levels/LevelIndependenceTests.cs` [FR-012] [SC-005]

### Level 2

- [ ] T066 [P] [US3] Author Level 2 phases, three lanes, moving gates, cannons, chain, warship, and reward definitions under `Assets/_Project/Data/Levels/Level02/` [FR-010]
- [ ] T067 [P] [US3] Create the Level 2 greybox scene and readable pre-commit lane vistas in `Assets/_Project/Scenes/Level_02_ChainStrait.unity` [FR-010]
- [ ] T068 [US3] Implement generic lane commitment and consequence preview in `Assets/_Project/Scripts/Gameplay/Lanes/LaneChoiceController.cs` [FR-010] [FR-021]
- [ ] T069 [US3] Implement telegraphed shore cannon and mine hazards in `Assets/_Project/Scripts/Gameplay/Hazards/` [FR-008] [FR-010]
- [ ] T070 [US3] Implement chain blockade state, damage, breach, and presentation events in `Assets/_Project/Scripts/Gameplay/Objectives/ChainBlockadeController.cs` [FR-010]
- [ ] T071 [US3] Implement armored-warship armor break, health phase, attacks, and defeat in `Assets/_Project/Scripts/Combat/Bosses/ArmoredWarshipController.cs` [FR-010]
- [ ] T072 [US3] Tune Level 2 risk/reward and 80–90 second target in `Assets/_Project/Data/Levels/Level02/Level02.asset` [SC-004]

### Level 3

- [ ] T073 [P] [US3] Author Level 3 storm, moving gates, powder conversion, fortress, phases, and reward definitions under `Assets/_Project/Data/Levels/Level03/` [FR-011]
- [ ] T074 [P] [US3] Create the Level 3 greybox scene with storm visibility and fortress assault anchors in `Assets/_Project/Scenes/Level_03_StormFortress.unity` [FR-011]
- [ ] T075 [US3] Implement deterministic storm lane movement that preserves input clarity in `Assets/_Project/Scripts/Gameplay/Environment/StormLaneController.cs` [FR-011]
- [ ] T076 [US3] Implement force-versus-powder conversion and fortress damage contribution in `Assets/_Project/Scripts/Gameplay/Objectives/PowderChoiceController.cs` [FR-003] [FR-011]
- [ ] T077 [US3] Implement fortress landing and phase-one objective in `Assets/_Project/Scripts/Gameplay/Objectives/FortressBreachController.cs` [FR-011]
- [ ] T078 [US3] Implement final commander phase, ability pressure, victory, and failure in `Assets/_Project/Scripts/Combat/Bosses/StormFortressCommander.cs` [FR-011]
- [ ] T079 [US3] Tune Level 3 storm readability, choice impact, and 100–120 second target in `Assets/_Project/Data/Levels/Level03/Level03.asset` [SC-004]

### Campaign slice integration

- [ ] T080 [US3] Implement sequential unlock and direct-launch override in `Assets/_Project/Scripts/Levels/LevelAccessService.cs` [FR-012]
- [ ] T081 [P] [US3] Add peak force, decisive gate, losses, and boss time summary in `Assets/_Project/Scripts/UI/Results/BattleSummaryView.cs` [FR-022]
- [ ] T082 [US3] Run independent completion and readability checks for all levels and record results in `Artifacts/Local/playtests/three-level-review.md` [SC-003] [SC-005]

**Checkpoint**: All three levels work independently and form a rising campaign.

## Phase 6: User Story 4 — See an Honest Store Promise (P4)

**Goal**: Produce and validate a 30-second store preview from production gameplay only.

**Independent Test**: Reproduce every preview shot from a clean save within ten minutes and
verify the same choices, arithmetic, units, boss phases, rewards, and outcome.

### Validation first

- [ ] T083 [P] [US4] Create automated StoreMoment definition validation for level, reach-time, build, capture, and verified state in `Assets/_Project/Tests/EditMode/Marketing/StoreMomentValidationTests.cs` [FR-017] [SC-008]
- [ ] T084 [P] [US4] Add a clean-save PlayMode timing path for all promised moments in `Assets/_Project/Tests/PlayMode/Marketing/StoreReachabilityTests.cs` [FR-017] [SC-008]

### Capture and traceability

- [ ] T085 [P] [US4] Author StoreMoment definitions for opening threat, ×4 gate, landing guardian, three lanes, fortress breach, and reward in `Assets/_Project/Data/Marketing/StoreMoments/` [FR-017]
- [ ] T086 [US4] Implement a development-only deterministic capture launcher that cannot alter battle outcomes in `Assets/_Project/Scripts/Presentation/Capture/StoreCaptureLauncher.cs` [FR-017]
- [ ] T087 [P] [US4] Create the 0–30 second shot and audio plan with exact level/build mappings in `Artifacts/StorePreview/storyboard.md` [SC-008]
- [ ] T088 [US4] Capture uninterrupted portrait production footage for every approved moment into `Artifacts/StorePreview/Source/` [SC-008]
- [ ] T089 [US4] Edit the 30-second preview without changing outcomes and save the master to `Artifacts/StorePreview/Exports/LionOfTheSeas_AppPreview_9x16.mp4` [SC-008]
- [ ] T090 [US4] Complete the frame-by-frame store-truth review in `Artifacts/StorePreview/traceability.md` [FR-017] [SC-008]
- [ ] T091 [US4] Verify a clean-save player reaches every advertised moment within ten minutes and record the signed result in `Artifacts/StorePreview/reachability-review.md` [SC-008]

**Checkpoint**: The preview and downloadable slice make the same promise.

## Phase 7: Art Lock, Performance, and Release Polish

**Purpose**: Replace approved placeholders, meet the visual bar, and pass all cross-cutting gates.

- [ ] T092 [P] Create the bespoke flagship source and export in `ArtSource/Blender/Ships/Flagship.blend` and `Assets/_Project/Art/Ships/Flagship/Flagship.fbx`
- [ ] T093 [P] Create friendly and hostile ordinary-unit sources, shared atlas, pose data, and exports under `ArtSource/Blender/Characters/` and `Assets/_Project/Art/Characters/`
- [ ] T094 [P] Create Harbor Guardian, Armored Warship, and Storm Commander hero sources and exports under `ArtSource/Blender/Characters/Bosses/`, `ArtSource/Blender/Ships/Bosses/`, and `Assets/_Project/Art/Bosses/`
- [ ] T095 [P] Create bespoke gates, fortress kit, shoreline, cannons, chain, and store-facing environment assets under `ArtSource/Blender/Environment/` and `Assets/_Project/Art/Environment/`
- [ ] T096 Create final stylized water, wakes, foam, storm, smoke, impacts, destruction, and reward presentation under `Assets/_Project/Materials/Water/` and `Assets/_Project/VFX/` [SC-004]
- [ ] T097 Refresh and revalidate `Assets/_Project/Scenes/Benchmark_Art.unity` after final slice assets, then pass every item in `specs/001-vertical-slice/contracts/art-quality-contract.md` [SC-003] [SC-004]
- [ ] T098 [P] Add pooled battle audio, music transitions, and mix snapshots under `Assets/_Project/Audio/` and `Assets/_Project/Scripts/Presentation/Audio/` [FR-008]
- [ ] T099 [P] Add optional gate, broadside, armor-break, and victory haptics in `Assets/_Project/Scripts/Presentation/Haptics/HapticsController.cs` [FR-023]
- [ ] T100 Profile multiplier, landing, peak combat, boss break, destruction, reward, memory, and allocations on the primary physical device and record `Artifacts/Performance/primary/evidence.md` [SC-006]
- [ ] T101 Profile the 500-agent stress sequence and 10-minute thermal loop on the floor physical device and record `Artifacts/Performance/floor/evidence.md` [SC-006]
- [ ] T102 Verify Primary and Reduced profiles produce identical arithmetic, combat, reward, and terminal outcomes in `Artifacts/Performance/outcome-parity.md` [FR-018]
- [ ] T103 Run the source-size checker and resolve every failure/warning into `Artifacts/Quality/source-size-report.md` [SC-009]
- [ ] T104 Audit all third-party code and assets and complete `THIRD_PARTY_NOTICES.md` with zero unresolved licenses [FR-020] [SC-010]
- [ ] T105 Run the full EditMode, PlayMode, and Performance suites and record the release test matrix in `Artifacts/Quality/test-matrix.md`
- [ ] T106 Run a target-player satisfaction test for growth, landing, and boss payoff and record ratings in `Artifacts/Local/playtests/final-satisfaction.md` [SC-004]
- [ ] T107 Validate offline installation, restart persistence, pause/resume, and all three direct launches in `Artifacts/Quality/release-checklist.md` [FR-015] [FR-019]
- [ ] T108 Update `README.md`, `specs/001-vertical-slice/quickstart.md`, and build/capture instructions to match the tested release candidate
- [ ] T109 Verify repository privacy, tracked-file scope, secret scan, ignored build output, and exact remote destination in `Artifacts/Quality/private-repository-audit.md`

## Dependencies and Execution Order

### Phase dependencies

```text
Phase 1 Setup
    ↓
Phase 2 Foundation
    ↓
US1 Level 1 MVP
    ↓
US2 Loadout ─────┐
    ↓             │
US3 Levels 2–3 ──┤
    ↓             │
US4 Store Truth ←┘
    ↓
Art Lock / Performance / Release
```

- Setup has no feature dependency.
- Foundation blocks all user stories.
- US1 is the MVP and proves the core loop.
- US2 depends on US1 reward and battle snapshot but remains independently replay-testable.
- US3 depends on proven shared systems, not on US2 UI; its levels use a valid default loadout.
- US4 depends on production encounters from US1 and US3.
- Final art may begin as parallel source work after Art Lock direction is approved, but no
  final integration occurs before the relevant playable phase exists.

## Parallel Opportunities

- Setup: T006–T011 can proceed in parallel after T002.
- Foundation: definition/tests, persistence/tests, pools, quality assets, and stress-scene
  authoring can proceed in parallel around the shared session/crowd core.
- US1: tests, data, greybox, UI, presentation, and temporary water can begin in parallel;
  integration remains ordered T036 → T045.
- US2: definitions, UI shell, and tests can proceed before service integration.
- US3: Level 2 and Level 3 data, scenes, and tests can proceed in parallel after Foundation,
  while shared lane/objective code is coordinated.
- Final art Tier A assets T092–T095 are independent Blender workstreams.

## Implementation Strategy

### MVP first

1. Finish Setup and Foundation.
2. Deliver only US1 through T051.
3. Stop and validate comprehension, readability, fun, performance direction, retry, and code
   size before approving loadout, additional levels, or final art production.

### Incremental delivery

1. US1 proves the complete loop.
2. US2 proves one small progression/replay layer.
3. US3 proves the loop can create three distinct encounters.
4. US4 packages the real product promise.
5. The final phase raises the approved experience to store quality and verifies all budgets.

## Task Count Summary

- Setup: 12 tasks
- Foundational: 18 tasks
- US1: 21 tasks
- US2: 11 tasks
- US3: 20 tasks
- US4: 9 tasks
- Final quality and release: 18 tasks
- **Total: 109 tasks**
