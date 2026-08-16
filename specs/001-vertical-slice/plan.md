# Implementation Plan: First Playable Vertical Slice

**Branch**: `001-vertical-slice` | **Date**: 2026-08-16 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `specs/001-vertical-slice/spec.md`

## Summary

Build a portrait Android vertical slice for "أسد البحار: فتوحات المتوسط" that proves a
complete sea-to-land crowd loop across three short levels. Use a data-oriented crowd
simulation, pooled gameplay presentation, GPU-instanced crowd rendering, and a restrained
URP art pipeline to reach premium stylized quality without sacrificing mobile frame rate.
Keep every authored file below 1,500 non-blank lines, persist only the small local slice
state, and capture store media from the same production scenes players can reach.

## Technical Context

**Language/Version**: C# supported by Unity 6.3 LTS; Burst-compatible C# subset for jobs
**Primary Dependencies**: Unity 6.3 LTS, URP, Input System, Cinemachine, Burst,
Collections, Mathematics, Shader Graph, Test Framework, Performance Testing
**Storage**: ScriptableObject authoring data plus versioned local JSON save; no backend
**Testing**: Unity EditMode, PlayMode, Performance tests, deterministic replay fixtures,
and physical-device visual/performance captures
**Target Platform**: Portrait Android, ARM64, Android 9/API 28 minimum for the slice;
iOS packaging is a later follow-up
**Project Type**: Single Unity mobile game
**Rendering & Art Pipeline**: URP Forward renderer with quality-tier assets; Blender 5.1.1
is installed locally for bespoke models and animation; FBX is the initial interchange
format; Shader Graph and pooled particle VFX provide stylized water and combat feedback
**Reference Devices**: Primary class: Snapdragon 778G/Exynos 1380 or equivalent with
6 GB RAM; floor class: Snapdragon 680/Helio G85 or equivalent with 4 GB RAM. Exact
physical model and OS version are recorded before the Performance Gate.
**Performance Goals**: 60 fps at 300 visible agents on the primary class; at least 30 fps
in the 500-agent floor stress scene; retry to player control in under three seconds
**Constraints**: Offline; one-handed 9:16 play; no per-agent Rigidbody or Animator stack;
authored source below 1,500 non-blank lines and split review at 1,000; no unlicensed assets
**Scale/Scope**: Three levels, two options in each of three loadout slots, one reward loop,
two quality presets, five direct gameplay trace moments, and one 30-second store preview

## Constitution Check

*GATE: Passed before Phase 0 research. Re-checked after Phase 1 design.*

- [x] Core loop is independently playable and testable before meta or content expansion.
- [x] An in-engine art benchmark and objective visual review criteria are defined.
- [x] Mid-range 60 fps target, low-end 30 fps floor, and crowd stress test are planned.
- [x] Authored files stay below 1,500 non-blank lines with a split review at 1,000.
- [x] Every store-facing scene maps to reachable gameplay in the specification.
- [x] Third-party assets and code require a compatible license and recorded source.
- [x] The plan avoids unneeded online, open-world, economy, and full-ECS complexity.

**Post-design re-check**: Passed. The data model keeps level content outside core logic;
the contracts define measurable visual, performance, code-size, licensing, and store-truth
gates; direct-launch scenes allow every user story to be validated independently.

## Architecture Decisions

### 1. Hybrid data-oriented crowd, not full ECS

- Store hot agent state in structure-of-arrays NativeArray buffers.
- Simulate movement, gate processing, formation targets, and coarse combat through Burst
  jobs at a fixed simulation step.
- Use a uniform spatial grid for neighbors and combat candidates; do not run all-pairs
  distance checks.
- Keep orchestration, level flow, UI, hero objects, bosses, and authoring in conventional
  small Unity components.
- Defer full Entities/ECS adoption unless the 500-agent stress scene fails after profiling.

### 2. Instanced crowd presentation

- Render ordinary crowd units in batches through GPU instancing with per-instance matrices,
  team color, state, and animation phase.
- Use simple shader-driven run/idle/hit motion or a small baked pose set for ordinary units.
- Reserve skeletal Animator components for captains, bosses, and close hero units only.
- Pool landing craft, hit VFX, projectiles, debris, UI numbers, and audio sources.

### 3. Data-driven encounters

- Author levels, gates, waves, units, bosses, rewards, loadouts, and quality profiles as
  versioned definitions referenced by stable identifiers.
- Treat scene objects as presentation anchors; source-of-truth values live in definitions
  and runtime state, not arbitrary MonoBehaviour fields spread across prefabs.
- Separate deterministic gameplay events from presentation subscribers so VFX quality can
  change without changing arithmetic or combat outcomes.

### 4. Mobile-first art quality

- Use one key directional light, baked/static lighting where practical, restrained shadows,
  SRP batching, simple lit materials, and quality-specific URP assets.
- Build stylized water from a low-cost Shader Graph material using normal motion, shoreline
  foam, depth tint where budget allows, and authored wake meshes; no physical ocean solver.
- Model hero ships and bosses as bespoke assets. Prototype props may use compatible CC0
  kits, but every imported asset must be restyled and recorded in the license manifest.
- Validate art in the portrait gameplay camera at phone size, not only in modeling renders.

### 5. Small-code enforcement

- Organize by gameplay domain and assembly definition instead of one global scripts folder.
- A repository check counts non-blank lines in authored C# and fails at 1,500.
- Files at 1,000 lines produce a warning and cannot receive new responsibility without a
  recorded split task. The team target is 500 lines or fewer.
- Generated, package-cache, and vendor files are excluded from the limit and never edited.

## Project Structure

### Documentation

```text
specs/001-vertical-slice/
├── spec.md
├── plan.md
├── research.md
├── data-model.md
├── quickstart.md
├── checklists/
│   └── requirements.md
├── contracts/
│   ├── gameplay-contract.md
│   ├── art-quality-contract.md
│   └── delivery-quality-contract.md
└── tasks.md
```

### Unity source

```text
Assets/
└── _Project/
    ├── Art/
    │   ├── Characters/
    │   ├── Environment/
    │   ├── Ships/
    │   ├── UI/
    │   └── VFX/
    ├── Audio/
    ├── Data/
    │   ├── Bosses/
    │   ├── Gates/
    │   ├── Levels/
    │   ├── Loadouts/
    │   ├── Quality/
    │   └── Units/
    ├── Materials/
    ├── Prefabs/
    ├── Scenes/
    │   ├── Bootstrap.unity
    │   ├── Frontend.unity
    │   ├── Level_01_HundredSails.unity
    │   ├── Level_02_ChainStrait.unity
    │   ├── Level_03_StormFortress.unity
    │   ├── Benchmark_Art.unity
    │   └── Benchmark_Stress.unity
    ├── Scripts/
    │   ├── Core/
    │   ├── Crowd/
    │   ├── Combat/
    │   ├── Gates/
    │   ├── Levels/
    │   ├── Loadout/
    │   ├── Persistence/
    │   ├── Presentation/
    │   └── UI/
    ├── Settings/
    └── Tests/
        ├── EditMode/
        ├── PlayMode/
        └── Performance/
ArtSource/
└── Blender/
    ├── Characters/
    ├── Environment/
    └── Ships/
Packages/
ProjectSettings/
tools/
├── check-source-size.sh
└── capture-quality-evidence.sh
THIRD_PARTY_NOTICES.md
```

**Structure Decision**: A single Unity project keeps the vertical slice simple. Assembly
definitions divide Core, Crowd, Gameplay, Presentation, and Tests so dependencies remain
explicit. No server, shared package repository, or second application is justified yet.

## Delivery Milestones

### M0 — Specification and repository baseline

- Constitution, spec, plan, contracts, tasks, Unity `.gitignore`, README, and license
  manifest template are approved.
- Private GitHub repository contains only this project.

### M1 — Greybox core loop

- Bootstrap, direct level launch, flagship drag, deployment, one multiplier gate, landing,
  coarse combat, guardian health, victory, failure, and fast retry work in Level 1.
- EditMode arithmetic and PlayMode loop tests pass.

### M2 — Performance and art benchmark

- Data-oriented crowd and instanced presentation pass the 300/500-agent benchmark gates.
- Final-quality flagship, one crew, one enemy, gate, water, environment slice, UI, boss,
  lighting, and representative VFX pass the art contract.
- No expansion to Level 2 before both gates pass.

### M3 — Level 1 vertical slice

- Level 1, reward, loadout change, persistence, replay difference, audio, haptics, and
  accessibility readability pass acceptance.
- First external/internal target-player test evaluates comprehension and satisfaction.

### M4 — Levels 2 and 3

- Three-lane risk/reward, chain blockade, armored warship, storm movement, powder choice,
  and two-stage fortress are delivered as data-driven extensions of proven systems.

### M5 — Release candidate and store preview

- Both quality presets, device captures, source-size report, regression suite, license
  audit, and all marketing traceability records pass.
- The 30-second preview is captured from the production build and linked to its build ID.

## Verification Strategy

- **EditMode**: gate arithmetic, force caps, configuration validation, save migration,
  reward idempotency, loadout validation, event ordering, and source-size report parser.
- **PlayMode**: direct-launch completion for each user story, landing transition, boss phase
  changes, fail/retry, app pause/resume, persistence, reduced-effects outcome parity.
- **Performance**: 300-agent primary and 500-agent floor scenes, multiplier burst, landing,
  peak combat, boss destruction, reward presentation, and memory allocation spikes.
- **Visual**: five-second readability test, art benchmark checklist, phone-size captures,
  LOD/material/VFX review, and comparison across quality presets.
- **Store truth**: clean-save timing plus uninterrupted capture for every promised moment.

## Complexity Tracking

No constitution violations are approved. Full ECS, online services, a physical ocean,
per-agent physics, runtime-generated navmeshes, and a large content framework were rejected
because the simpler hybrid design can validate the three-level slice first.
