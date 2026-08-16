<!--
Sync Impact Report
- Version change: template -> 1.0.0
- Added principles:
  - I. Prove the Core Loop First
  - II. Visual Quality Is a Shipping Requirement
  - III. Mobile Performance Has Hard Budgets
  - IV. The Store Promise Must Be Playable
  - V. Spec-Driven, Modular, and Legally Reusable
- Added sections:
  - Product and Technical Standards
  - Delivery Workflow and Quality Gates
- Templates updated:
  - ✅ .specify/templates/plan-template.md
  - ✅ .specify/templates/spec-template.md
  - ✅ .specify/templates/tasks-template.md
- Follow-up TODOs: none
-->

# أسد البحار / Lion of the Seas Constitution

## Core Principles

### I. Prove the Core Loop First (NON-NEGOTIABLE)
Every production phase MUST protect the one-minute playable loop: steer the flagship,
deploy landing craft, choose a multiplier or conversion gate, grow the force, land,
and defeat a visible objective. A greybox on a target mobile device MUST prove this
loop before final art, meta systems, monetization, additional modes, or content scale
are approved. Each feature MUST be independently playable and testable.

Rationale: the project succeeds only if the interaction shown in the store video is
fun in the player's hand, not merely attractive in a concept image.

### II. Visual Quality Is a Shipping Requirement
The project MUST maintain one approved art direction: premium stylized 3D,
Mediterranean pirate fantasy, readable silhouettes, turquoise/ivory/gold player
colors, and crimson/charcoal enemy colors. Final assets MUST pass an in-engine review
for silhouette, material consistency, lighting, animation, VFX response, and mobile
readability. Unmodified asset-pack mixtures, placeholder art in release captures, and
visual inconsistency are release blockers. Hero assets and the first store-facing
level MUST receive bespoke art even when licensed assets accelerate prototyping.

Rationale: perceived quality comes from consistency, lighting, motion, and feedback
as much as polygon count. Art is part of acceptance, not post-launch polish.

### III. Mobile Performance Has Hard Budgets
Gameplay MUST target 60 frames per second on the agreed mid-range Android reference
device and MUST remain playable at a 30 frames-per-second floor on the agreed low-end
device. The vertical slice MUST demonstrate at least 300 visible agents at the primary
target and a 500-agent stress test at the floor target. Spawning, combat, and VFX MUST
use pooling and scalable simulation/rendering. A separate full GameObject, Rigidbody,
and Animator stack per crowd unit is prohibited without measured justification.
Performance captures and profiler evidence MUST accompany every visual-quality gate.

Rationale: a beautiful crowd game that stutters during multiplication fails its core
promise.

### IV. The Store Promise Must Be Playable
Every gameplay scene used in store media or paid acquisition MUST exist in the shipped
build and be reachable during the first ten minutes unless clearly identified as a
cinematic. Choices shown in advertising MUST have the same consequences in-game. The
team MAY dramatize camera, timing, and presentation, but MUST NOT advertise absent
bosses, paths, units, environments, or rewards. The trailer and game MUST be captured
from the same production content pipeline.

Rationale: the project borrows the clarity and spectacle of successful crowd-game
marketing without relying on bait-and-switch.

### V. Spec-Driven, Modular, and Legally Reusable
No feature enters implementation without a specification, acceptance criteria,
dependencies, and explicit priority. Gameplay content MUST be data-driven so gates,
units, encounters, rewards, and levels can be tuned without rewriting core systems.
Authored source files MUST stay below 1,500 non-blank lines, trigger a decomposition
review at 1,000 lines, and target 500 lines or fewer. Methods MUST stay focused and
components MUST have one clear responsibility. Generated and third-party vendor files
are exempt from the line limit but MUST NOT be hand-edited. Third-party code and art
MUST have a documented license and source; unclear, copyleft-incompatible, ripped, or
store-extracted assets are prohibited.

Rationale: traceable specifications, small replaceable modules, and clean licensing
let a small team iterate quickly without creating legal or technical debt.

## Product and Technical Standards

- The vertical slice targets portrait mobile play using Unity 6.3 LTS and URP. Blender
  is the source tool for bespoke ships, characters, fortifications, rigging, and LODs.
- The first deliverable is three short levels sharing one complete sea-to-land loop,
  one loadout screen, one reward flow, and a truthful 30-second store-preview sequence.
- Initial scope excludes open-world sailing, online multiplayer, clans, live operations,
  a production backend, and a large economy. Those capabilities require later specs.
- Art reviews MUST use captures from representative mobile builds, not Blender renders
  alone. Reference images set a quality bar but MUST NOT be copied as copyrighted assets.
- Release candidates MUST include gameplay checks, device performance evidence, a
  visual review checklist, a source-file size report, and a license manifest update.
- Player-facing content MUST avoid direct use of modern political or national symbols;
  factions are fictionalized Mediterranean powers unless a later review approves
  historical content.

## Delivery Workflow and Quality Gates

1. **Specify**: approve player value, scope, success criteria, visual target, performance
   budget, code-size contract, and marketing-promise traceability.
2. **Greybox Gate**: prove the complete loop on a device with placeholder assets. If the
   loop is unclear or not satisfying, revise mechanics before adding final art.
3. **Art Lock Gate**: approve a playable benchmark containing one flagship, one crew
   class, one enemy class, one gate, water, lighting, UI, and representative VFX.
4. **Content Gate**: complete levels independently in priority order. Each level MUST
   remain playable and demonstrable without unfinished later levels.
5. **Performance Gate**: profile multiplication, peak combat, boss impact, destruction,
   and reward moments on reference devices; fix violations before adding content.
6. **Maintainability Gate**: reject authored files at or above 1,500 non-blank lines and
   require a documented split plan for files at or above 1,000 lines.
7. **Store Truth Gate**: validate every frame of store media against reachable gameplay
   and record the level and build used to capture it.
8. **Release Gate**: pass acceptance criteria, visual review, performance budgets,
   regression checks, maintainability report, and license audit.

Scope additions MUST remove equivalent scope, extend the schedule, or move to a new
specification. A milestone cannot be declared complete from screenshots alone.

## Governance

This constitution supersedes conflicting project habits and informal decisions. Every
specification, plan, task list, review, and release MUST demonstrate compliance. Any
amendment requires a written rationale, an impact review across active specs and
templates, and a semantic-version update. Removing or weakening a principle requires a
major version; adding or materially expanding governance requires a minor version; a
clarification requires a patch version. Compliance is reviewed during planning, after
design, and before release. Unjustified violations block implementation.

**Version**: 1.0.0 | **Ratified**: 2026-08-16 | **Last Amended**: 2026-08-16
