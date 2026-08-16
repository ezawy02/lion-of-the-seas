# Feature Specification: First Playable Vertical Slice

**Feature Branch**: `001-vertical-slice`  
**Created**: 2026-08-16  
**Status**: Ready for Planning  
**Input**: Create the first playable vertical slice for "أسد البحار: فتوحات المتوسط"
with three sea-to-land levels, premium stylized graphics, mobile performance budgets,
truthful store media, modular code below 1,500 lines per authored file, and step-by-step
development acceptance criteria.

## Problem Statement

Crowd-control mobile games attract players by showing an impossible threat, a simple
numeric decision, explosive growth, and a large payoff. The project needs to prove that
this loop remains satisfying when transformed into an original Mediterranean pirate
fantasy, and that the high visual promise can run smoothly on ordinary mobile devices.
Without a tightly scoped vertical slice, the team risks spending months on ships, maps,
and progression before proving the minute-to-minute game is fun.

## Goals

- Deliver one complete sea-to-land loop that a first-time player understands without a
  written tutorial within 30 seconds.
- Deliver three independently playable levels that add one meaningful mechanic each.
- Establish an in-game visual benchmark equal in consistency and impact to the target
  crowd-game references without copying their protected assets or identity.
- Demonstrate stable crowd growth and boss combat on the agreed reference devices.
- Produce a truthful 30-second store preview using encounters reachable in the first
  ten minutes of the playable build.

## Non-Goals

- Open-world sailing or free-roaming exploration; it does not help validate the core loop.
- Online multiplayer, clans, leaderboards, or a production backend; these require separate
  retention and operations specifications.
- A large economy, season pass, advertising SDK, or real-money purchases; monetization is
  premature before player enjoyment and retention are tested.
- Historically exact nations, flags, or political conflicts; the slice uses fictional
  Mediterranean factions to keep the experience accessible and avoid premature research.
- More than three levels or more than one polished loadout screen; additional content is
  deferred until the vertical slice meets its quality gates.

## Target Players

- Mobile players who enjoy short, visually satisfying battles and clear numeric choices.
- Strategy-curious casual players who want meaningful decisions without complex controls.
- Prospective players deciding whether to install after seeing a short store preview.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Command the First Landing (Priority: P1)

As a first-time captain, I want to steer my flagship, deploy landing craft through a
multiplier gate, and defeat a visible harbor guardian so that I feel powerful within my
first minute of play.

**Why this priority**: This is the smallest complete experience that proves the game's
main promise and can stand alone as an MVP.

**Independent Test**: Launch Level 1 from a clean save, provide no written instructions,
and observe whether the player can move, choose a gate, land, fight, win, and claim a
reward in one uninterrupted attempt.

**Acceptance Scenarios**:

1. **Given** a new player enters Level 1, **When** they drag horizontally, **Then** the
   flagship follows within its safe combat lane and immediately communicates control.
2. **Given** landing craft enter a positive gate, **When** they clear it, **Then** the
   displayed force and visible formation increase by the stated value without ambiguity.
3. **Given** the force reaches the beach, **When** landing begins, **Then** crews disembark,
   engage defenders, and preserve the player's accumulated strength in readable form.
4. **Given** the harbor guardian is defeated, **When** the encounter ends, **Then** the
   player receives a visible reward and can continue or replay within three seconds.

---

### User Story 2 - Make a Tactical Loadout Choice (Priority: P2)

As a returning captain, I want to choose a flagship, crew role, and captain ability so
that my next attempt has a clear strategic identity rather than being a cosmetic repeat.

**Why this priority**: A small loadout layer proves the core battle can support progression
without requiring a full economy.

**Independent Test**: Complete Level 1, change one loadout slot, replay the battle, and
verify that the selected option produces a visible and measurable gameplay difference.

**Acceptance Scenarios**:

1. **Given** the player owns at least two options for a slot, **When** they select one,
   **Then** the loadout screen shows its role, trade-off, and active state clearly.
2. **Given** a loadout is confirmed, **When** the next level begins, **Then** the selected
   ship, crew behavior, and captain ability are present without another confirmation.
3. **Given** the player closes and reopens the slice, **When** the loadout screen appears,
   **Then** the last valid selection remains active.

---

### User Story 3 - Conquer Three Distinct Encounters (Priority: P3)

As a captain who understands the basics, I want each new level to introduce a readable
risk-and-reward decision so that the three-level slice feels like a rising campaign.

**Why this priority**: Three encounters demonstrate that the loop can create variety
without an open world or excessive systems.

**Independent Test**: Launch each level directly and confirm it can be understood,
completed, failed, and retried without depending on unfinished future content.

**Acceptance Scenarios**:

1. **Given** Level 2 presents three sea lanes, **When** the player commits to a lane,
   **Then** its reward and hazard are readable before the commitment becomes irreversible.
2. **Given** Level 3 introduces storm movement and two assault phases, **When** the player
   reaches the fortress, **Then** prior gate and loadout choices materially affect the
   remaining force and boss outcome.
3. **Given** any level is failed, **When** the failure state appears, **Then** the player
   understands the decisive loss and can retry in under three seconds.

---

### User Story 4 - See an Honest Store Promise (Priority: P4)

As a prospective player, I want the store preview to show battles I can actually reach
soon after installing so that the downloaded experience matches my expectation.

**Why this priority**: Trust is a product requirement, but store capture follows a proven
and polished playable slice.

**Independent Test**: Review every shot in the final 30-second preview and reproduce it
from a clean save within the first ten minutes of play.

**Acceptance Scenarios**:

1. **Given** a store-preview shot shows a gate, boss, environment, ability, or reward,
   **When** its traceability record is checked, **Then** it maps to a reachable build and
   level with matching consequences.
2. **Given** the preview uses a dramatic camera move, **When** the build is inspected,
   **Then** the underlying units, combat, and outcome remain authentic gameplay.

### Edge Cases

- The player releases the drag, switches fingers, or drags outside the safe lane.
- All landing craft are destroyed before reaching a gate or the shore.
- Two moving gates overlap visually or cross while a formation is passing through.
- The visible unit count reaches the simulation cap during a multiplier event.
- A device drops below the performance floor during peak crowd collision or VFX.
- A boss is defeated while a special ability, destruction sequence, or landing transition
  is already playing.
- The app closes during reward or loadout persistence and is reopened.
- Water, smoke, color grading, or UI obscures the player force on a small screen.
- A referenced third-party asset loses its source or license record before release.

## Requirements *(mandatory)*

### Must-Have Requirements (P0)

- **FR-001**: The player MUST control the flagship through one-handed horizontal dragging.
- **FR-002**: The flagship MUST deploy landing craft continuously during active combat.
- **FR-003**: Gates MUST support multiplication, addition, conversion, and hazard outcomes
  with a readable value before the player commits.
- **FR-004**: The displayed force count MUST match the result represented by the visible
  formation within the approved display tolerance.
- **FR-005**: Surviving landing craft MUST reach shore, release crews, and transfer their
  combat contribution to the land phase.
- **FR-006**: Friendly and hostile forces MUST remain distinguishable by color, silhouette,
  motion direction, and UI even when hundreds of agents overlap.
- **FR-007**: Each level MUST present its primary threat within the opening three seconds.
- **FR-008**: Combat MUST provide visible feedback for hits, losses, gate effects, boss
  damage, destruction, victory, and failure.
- **FR-009**: Level 1, "ميناء المائة شراع", MUST contain one easy gate choice, one risky
  gate choice, one prisoner rescue, a beach landing, and the Harbor Guardian boss.
- **FR-010**: Level 2, "مضيق السلاسل", MUST contain three readable lanes, moving hazards,
  shore cannons, a chain blockade, and a two-stage armored warship boss.
- **FR-011**: Level 3, "قلعة العاصفة", MUST contain storm movement, a force-versus-powder
  choice, a fortress landing, and two consecutive assault phases.
- **FR-012**: The player MUST be able to win, fail, and retry every level independently.
- **FR-013**: A retry MUST return the player to meaningful control in under three seconds.
- **FR-014**: The slice MUST include one flagship slot, one crew slot, and one captain
  ability slot with at least two meaningfully different options in each slot.
- **FR-015**: Loadout selections and earned slice rewards MUST survive an app restart.
- **FR-016**: The slice MUST include a reward presentation that identifies what was earned
  and how it changes a future attempt.
- **FR-017**: Every store-facing gameplay moment MUST be reachable from a clean save within
  the first ten minutes and recorded in the traceability table below.
- **FR-018**: The game MUST provide a reduced-effects fallback when the performance floor
  cannot be maintained, without changing gate arithmetic or combat outcomes.
- **FR-019**: The slice MUST be playable offline after installation.
- **FR-020**: Every non-original asset and code dependency MUST have a recorded source,
  license, permitted use, and modification note before it appears in a release build.

### Nice-to-Have Requirements (P1)

- **FR-021**: Encounters SHOULD support optional branching routes that rejoin before the
  boss without requiring separate level scenes.
- **FR-022**: The player SHOULD be able to review a concise battle summary showing peak
  force, decisive gate, losses, and boss completion time.
- **FR-023**: The slice SHOULD support subtle device haptics for multiplier gates, broadside
  fire, boss armor breaks, and victory.
- **FR-024**: The main visual settings SHOULD offer quality presets without exposing
  technical terminology to the player.

### Future Considerations (P2)

- **FR-025**: The content model MAY later support campaign maps, additional captains,
  additional ship classes, live events, and monetization without rewriting level logic.
- **FR-026**: Cooperative or competitive modes MAY be explored only after a separate online
  architecture and fairness specification is approved.

### Key Entities

- **Level Definition**: Encounter identity, environment, phases, gates, hazards, objectives,
  boss sequence, reward, and direct-launch state.
- **Gate Definition**: Outcome type, value, movement, eligible force type, feedback, and
  risk presentation.
- **Force**: Current unit count, role composition, formation, state, and display tolerance.
- **Unit Role**: Friendly or hostile role, movement profile, combat contribution, visual
  identity, and upgrade tier.
- **Flagship**: Control bounds, deployment pattern, appearance, and loadout contribution.
- **Captain Ability**: Charge rule, effect, presentation, duration, and cooldown.
- **Boss Encounter**: Phases, health or armor, attacks, reactions, failure pressure, and
  victory condition.
- **Loadout**: Selected flagship, crew role, captain ability, and persisted ownership.
- **Reward**: Source, presentation, ownership change, and future gameplay effect.
- **Quality Evidence**: Build identifier, device class, performance capture, visual review,
  source-size report, and store-promise trace.
- **Asset License Record**: Asset name, source, author, license, permitted use, changes,
  and included project paths.

## Visual Quality & Performance Contract *(mandatory for player-facing features)*

- **Visual Target**: Premium stylized 3D with cohesive proportions, readable silhouettes,
  turquoise/ivory/gold friendly forces, crimson/charcoal enemies, luminous violet/gold
  decisions, expressive water, cloth, recoil, impacts, destruction, and reward motion.
- **Required Benchmark Scene**: A mobile build of Level 1 containing one final-quality
  flagship, one friendly crew, one enemy, one multiplier gate, representative water,
  beach and fortress materials, final lighting, UI, crowd impact VFX, and the Harbor
  Guardian reaction cycle.
- **Performance Budget**: Smooth primary play at 60 frames per second with 300 visible
  agents on the agreed mid-range device; never below the 30 frames-per-second floor in a
  500-agent stress case on the agreed low-end device after fallback activates.
- **Asset Acceptance**: Every final asset passes silhouette-at-phone-size, material palette,
  rig and animation, VFX readability, LOD transition, import consistency, and licensing
  review inside a representative build.
- **Code-Size Contract**: Every authored source file remains below 1,500 non-blank lines,
  files at 1,000 lines receive a recorded split plan, and the normal target is 500 lines
  or fewer. Generated and vendor files are reported separately and not hand-edited.
- **Capture Evidence**: Approval requires portrait gameplay video, stills from primary and
  low-end builds, frame-time evidence at multiplier and boss peaks, a source-size report,
  and an updated license manifest.

## Marketing Promise Traceability *(mandatory for store-facing features)*

| Promised Store Moment | Reachable In-Game Location | Acceptance Evidence |
|-----------------------|----------------------------|---------------------|
| Tiny fleet faces an overwhelming harbor army | Level 1 opening, under 30 seconds from clean save | Matching build capture and direct-launch replay |
| Landing craft multiply through a visible ×4 gate | Level 1 first gate set, under 60 seconds | Gate arithmetic test and portrait capture |
| A large force lands and attacks a giant guardian | Level 1 final phase, under 2 minutes | Full uninterrupted gameplay capture |
| Three sea lanes offer distinct risks and rewards | Level 2 opening, within first 7 minutes | Choice/outcome test for all three lanes |
| Storm fleet breaches a two-stage fortress | Level 3 direct campaign path, within first 10 minutes | Full level capture and clean-save timing record |
| Victory grants a blueprint that changes the loadout | Level 1 reward and loadout flow | Reward persistence and replay comparison |

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: At least 80% of first-time internal playtesters move the flagship and select
  a positive gate within 30 seconds without written instruction.
- **SC-002**: At least 70% of first-time internal playtesters complete Level 1 within two
  attempts and can describe why their force grew or failed.
- **SC-003**: At least 80% of five-second screenshot tests correctly identify the friendly
  force, hostile force, next decision, and primary threat.
- **SC-004**: At least 70% of target-player playtesters rate crowd growth, landing impact,
  and boss payoff at 4 out of 5 or higher.
- **SC-005**: All three levels can be launched, completed, failed, and retried independently
  without unfinished content or manual intervention.
- **SC-006**: Primary play maintains 60 frames per second with 300 visible agents on the
  agreed mid-range device; the 500-agent stress scene remains at or above 30 frames per
  second on the agreed low-end device with fallback enabled.
- **SC-007**: Retry returns meaningful control within three seconds in at least 95% of
  measured attempts.
- **SC-008**: Every store-preview shot is reproduced from the production build within the
  first ten minutes of a clean save, with zero absent or behaviorally misleading moments.
- **SC-009**: Zero authored source files reach 1,500 non-blank lines, and every authored
  file at or above 1,000 lines has an approved decomposition record before merge.
- **SC-010**: Zero third-party assets or dependencies enter a release candidate without a
  complete and compatible license record.

## Assumptions

- The first platform is portrait Android mobile; iOS packaging follows after the slice
  proves its quality and performance targets.
- The slice is single-player and offline, with local save data only.
- A small team can use licensed CC0 assets for greybox and secondary props, while creating
  bespoke hero ships, captains, gates, UI, and store-facing environments.
- Factions, places, and characters are fictionalized even when inspired by Mediterranean
  naval history.
- Reference devices will be named during technical planning after checking the devices
  physically available to the team.
- The store-preview camera may be more cinematic than normal play, but cannot alter unit
  choices, arithmetic, combat results, or content availability.
