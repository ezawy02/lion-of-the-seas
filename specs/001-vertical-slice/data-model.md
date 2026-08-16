# Data Model: First Playable Vertical Slice

The model separates immutable authoring definitions, mutable runtime state, persisted player
state, and generated quality evidence. Stable string identifiers connect definitions without
embedding scene-object references in save data.

## Authoring definitions

### LevelDefinition

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique; never reused after release |
| displayName | Localized text key | Required |
| sceneId | Stable scene ID | Must resolve to one direct-launch scene |
| order | Integer | 1–3 in the slice |
| phases | Ordered PhaseDefinition IDs | At least opening, traversal, assault, result |
| gateSets | GateSetDefinition IDs | Every decision appears before commitment |
| encounters | EncounterDefinition IDs | At least one boss or final objective |
| rewardId | RewardDefinition ID | Required for first completion |
| qualityProfileId | QualityProfile ID | Required |
| storeMoments | StoreMoment IDs | Optional, but all entries must be traceable |

**Validation**:

- All referenced IDs exist.
- A direct launch can enter and complete the level without campaign state.
- Every phase has one entry condition and one exit condition.
- Level 1 is reachable from a clean save; Levels 2 and 3 follow sequential unlocks.

### PhaseDefinition

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique within the project |
| kind | Opening, Traversal, Landing, Assault, Boss, Result | Required |
| durationBudget | Seconds | Soft design target, not an automatic timeout |
| cameraProfileId | Stable ID | Required |
| spawnGroups | SpawnGroup IDs | May be empty for Result |
| completionRule | Rule definition | Deterministic and testable |
| nextPhaseId | Stable ID | Empty only for terminal result |

### GateDefinition

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique |
| outcome | Multiply, Add, Convert, Damage, Reward | Required |
| value | Number or conversion ID | Must be valid for outcome |
| eligibleForce | Landing craft, crew, or both | Required |
| movementProfile | Static, horizontal, vertical, timed | Required |
| commitmentLine | World marker | Must precede outcome collision |
| visualStyle | Friendly, risky, hostile, special | Must match consequence |
| feedbackProfileId | Stable ID | Required |

**Validation**:

- Positive gates cannot use hostile color treatment.
- Conversion gates identify source and destination roles.
- Gate evaluation is idempotent per force member.
- Result count respects the configured simulation cap and records any display compression.

### UnitRoleDefinition

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique |
| allegiance | Friendly or hostile | Required |
| role | Sailor, musketeer, bomber, defender, boss support | Required |
| movement | Speed and steering profile | Positive values |
| combat | Damage, cadence, range, target rules | Non-negative |
| durability | Health or hit count | Positive |
| presentation | Mesh/material/pose/VFX/audio IDs | Must pass asset validation |
| simulationCostTier | Ordinary, hero, boss | Controls presentation path |

### FlagshipDefinition

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique |
| controlBounds | Normalized lane limits | Left must be less than right |
| deployPattern | Cadence, burst, spread | Must create at least one craft |
| baseStats | Deployment and ability modifiers | Bounded by tuning limits |
| presentation | Ship, wake, recoil, audio IDs | Required |
| unlockRule | Default or reward ID | One slice option is default |

### CaptainAbilityDefinition

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique |
| chargeRule | Time, damage, gates, or hybrid | Required |
| activation | Player tap or automatic | Slice uses one consistent input rule |
| gameplayEffect | Typed effect definition | Deterministic |
| duration | Seconds | Non-negative |
| cooldown | Seconds | Non-negative |
| presentation | Hero/VFX/audio/camera IDs | Must not delay gameplay result |

### BossDefinition

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique |
| phases | Ordered BossPhaseDefinition list | At least one |
| healthModel | Health, armor+health, or staged objectives | Required |
| attacks | AttackDefinition IDs | Each has a readable warning |
| targetRules | Force, flagship, or objective | Required |
| transitions | Health/event thresholds | Deterministic |
| victoryRule | Rule definition | Required |
| failurePressure | Timer, breakthrough, or force depletion | Required |

### RewardDefinition

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique |
| grantType | Ownership, upgrade, currency placeholder | Slice favors ownership |
| grantTargetId | Loadout option ID | Required for ownership |
| amount | Integer | Positive |
| firstCompletionOnly | Boolean | Required decision |
| presentation | Icon, reveal, audio, description IDs | Required |

### QualityProfile

| Field | Type | Rules |
|------|------|-------|
| id | Primary or Reduced | Exactly two in the slice |
| crowdPresentationCap | Integer | At least 300 primary |
| shadowProfile | Preset ID | Reduced is no more expensive |
| vfxDensity | Normalized value | 0–1 |
| waterProfile | Preset ID | Outcome-neutral |
| lodBias | Positive number | Bounded |
| fallbackTriggers | Frame-time rules | Hysteresis required to avoid flicker |

## Runtime state

### BattleSession

| Field | Type | Notes |
|------|------|-------|
| sessionId | Runtime GUID | Not persisted as progression |
| levelId | Stable ID | Direct launch or campaign |
| phaseId | Stable ID | Current phase |
| battleState | Loading, Ready, Active, Landing, Assault, Victory, Failure, Exiting | State machine |
| elapsedTime | Seconds | Monotonic while active |
| selectedLoadout | LoadoutSnapshot | Immutable during battle |
| friendlyForce | ForceRuntime | Mutable |
| hostileForce | ForceRuntime | Mutable |
| bossState | BossRuntime | Optional before boss phase |
| qualityProfile | Active profile | May reduce presentation only |
| eventSequence | Integer | Strictly increasing |
| result | BattleResult | Empty until terminal |

### ForceRuntime

| Field | Type | Notes |
|------|------|-------|
| logicalCount | Integer | Authoritative arithmetic count |
| displayedAgentCount | Integer | May compress at cap |
| roleCounts | Map role ID → count | Sum equals logical count |
| state | Deploying, Traversing, Landing, Fighting, Routed, Complete | Required |
| positions | Native buffers | Runtime only |
| velocities | Native buffers | Runtime only |
| healthOrHits | Native buffers | Runtime only |
| flags | Native buffers | Gate and combat processing |

**Invariant**: Display compression cannot alter damage, gate arithmetic, reward, or outcome.

### BossRuntime

| Field | Type | Notes |
|------|------|-------|
| bossId | Stable ID | Required |
| phaseIndex | Integer | Valid definition index |
| armor | Number | Optional by model |
| health | Number | Never below zero |
| activeAttack | Attack ID | Optional |
| state | Dormant, Entering, Active, Transitioning, Defeated | State machine |

### BattleResult

| Field | Type | Notes |
|------|------|-------|
| outcome | Victory or Failure | Required |
| completionTime | Seconds | Required |
| peakLogicalForce | Integer | Required |
| decisiveGateId | Stable ID | Optional if no gate processed |
| remainingForce | Integer | Non-negative |
| bossCompletion | Number | 0–1 |
| rewardGrantId | Grant transaction ID | Victory only |

## Persisted player state

### PlayerSave

| Field | Type | Rules |
|------|------|-------|
| schemaVersion | Integer | Required; migration path for older values |
| highestUnlockedLevel | Integer | 1–3 |
| ownedLoadoutIds | Set of stable IDs | Includes defaults |
| selectedLoadout | LoadoutSnapshot | All IDs must be owned |
| claimedRewardIds | Set of reward IDs | Prevents duplicate first rewards |
| settings | PlayerSettings | Validated on load |
| lastWriteId | GUID | Detects interrupted replacement |

### LoadoutSnapshot

| Field | Type | Rules |
|------|------|-------|
| flagshipId | Stable ID | Owned and valid |
| crewRoleId | Stable ID | Owned and valid |
| captainAbilityId | Stable ID | Owned and valid |

### PlayerSettings

| Field | Type | Rules |
|------|------|-------|
| qualityPreference | Auto, Primary, Reduced | Auto by default |
| haptics | Boolean | Default on when supported |
| musicVolume | 0–1 | Bounded |
| effectsVolume | 0–1 | Bounded |

## Quality and compliance evidence

### QualityEvidenceRecord

| Field | Type | Rules |
|------|------|-------|
| buildId | String | Exact tested build |
| commitId | String | Required for release candidate |
| deviceModel | String | Physical model, no personal device identifier |
| osVersion | String | Required |
| qualityProfile | Stable ID | Required |
| scenario | Multiplier, landing, combat, boss, reward, stress | Required |
| agentCount | Integer | Required for gameplay scenarios |
| fpsSummary | Median, p95 frame time, minimum | Required |
| memoryPeak | Bytes | Required |
| capturePaths | Repository-relative artifact references | No private paths |
| reviewer | Team role | Not personal email |
| result | Pass or Fail | Required |

### StoreMoment

| Field | Type | Rules |
|------|------|-------|
| id | Stable ID | Unique |
| description | Text | Required |
| levelId | Stable ID | Required |
| cleanSaveReachTime | Seconds | At most 600 |
| buildId | String | Required for final capture |
| captureReference | Artifact path or timecode | Required for approval |
| verified | Boolean | False blocks store use |

### AssetLicenseRecord

| Field | Type | Rules |
|------|------|-------|
| assetId | Stable ID | Unique |
| assetName | Text | Required |
| sourceUrl | URL | Required for third-party assets |
| author | Text | Required |
| license | SPDX-like name or full license reference | Required |
| commercialUseAllowed | Boolean | Must be true for release |
| modificationAllowed | Boolean | Must match usage |
| changes | Text | Required when modified |
| projectPaths | List of repository-relative paths | Required |

## State transitions

### BattleSession

```text
Loading → Ready → Active → Landing → Assault → Victory → Exiting
                         ↘ Failure ────────────────→ Exiting
Ready → Active
Victory/Failure → Loading  (retry creates a fresh session)
```

- No terminal state can return to Active.
- Retry creates fresh runtime buffers and reuses immutable definitions.
- Presentation completion cannot block the authoritative terminal result.

### BossRuntime

```text
Dormant → Entering → Active ↔ Transitioning → Defeated
```

- Phase transitions occur once at deterministic thresholds.
- Defeated is terminal even if queued hit presentation remains.

### Reward grant

```text
Pending → Validated → Persisting → Granted
                   ↘ Rejected
```

- A first-completion reward ID can reach Granted once per save.
- Interrupted persistence replays the same transaction idempotently.
