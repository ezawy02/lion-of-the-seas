# Quickstart: Begin the Vertical Slice

This guide starts implementation only after the specification, plan, and task list are
approved. It does not bypass the Greybox, Art Lock, Performance, or Store Truth gates.

## 1. Read the project contracts

Read in this order:

1. `AGENTS.md`
2. `.specify/memory/constitution.md`
3. `specs/001-vertical-slice/spec.md`
4. `specs/001-vertical-slice/plan.md`
5. `specs/001-vertical-slice/contracts/gameplay-contract.md`
6. `specs/001-vertical-slice/contracts/art-quality-contract.md`
7. `specs/001-vertical-slice/contracts/delivery-quality-contract.md`
8. `specs/001-vertical-slice/tasks.md`

## 2. Match the pinned tools when editor work is available

The project pins Unity `6000.3.22f1`; machine installations can differ. During the current
source remediation, do not start Unity or Blender. Use the editor-free checks in README
and see `source-remediation.md` for the unverified import/playthrough/art gates.

- When editor work is authorized, use Unity Hub as needed.
- Use Unity `6000.3.22f1`, matching `ProjectSettings/ProjectVersion.txt`.
- Include Android Build Support, Android SDK & NDK Tools, and OpenJDK.
- Do not add cloud services or sign the app during the vertical-slice bootstrap.
- Record the exact editor revision in `ProjectSettings/ProjectVersion.txt` after creation.

## 3. Create the Unity project

- Create a new **Universal 3D** project in this repository root.
- Keep the existing `.specify/`, `specs/`, README, AGENTS, and Git metadata.
- Use the project name `LionOfTheSeas` inside Unity.
- Set the default orientation to portrait and prepare a 9:16 Game view.
- Add only the packages listed in `plan.md`; avoid broad asset or framework imports.
- Create assembly definitions and folders exactly as listed in the plan before gameplay code.

## 4. Establish guardrails before features

- Add `tools/check-source-size.sh` and verify it ignores generated/vendor code.
- Create `THIRD_PARTY_NOTICES.md` entries before importing any non-original asset.
- Configure Primary and Reduced URP assets.
- Create `Benchmark_Stress` and the minimal quality-evidence capture structure.
- Add EditMode and PlayMode test assemblies.

## 5. Build the M1 greybox in acceptance order

1. Direct launch and deterministic battle seed.
2. One-handed flagship movement.
3. Pooled landing-craft deployment.
4. One ×4 gate and arithmetic test.
5. Sea traversal to landing zone.
6. Crew transfer and coarse land combat.
7. Harbor Guardian health, defeat, and failure pressure.
8. Victory, failure, and retry under three seconds.

Do not add final art, additional levels, a campaign map, economy, or monetization during M1.

## 6. Pass the M2 benchmark gates

- Demonstrate 300 visible agents at the primary 60 fps target.
- Demonstrate the 500-agent floor stress case at or above 30 fps with fallback.
- Complete `Benchmark_Art` using the exact acceptance list in the Art Quality Contract.
- Capture the same deterministic sequence in Primary and Reduced quality.
- Approve both performance and art before production work starts on Level 2.

## 7. Validate every increment

At each user-story checkpoint:

- Run EditMode, PlayMode, and relevant Performance tests.
- Build to a physical Android device.
- Run the source-size report.
- Update license records.
- Capture portrait evidence from the build.
- Compare delivered behavior to acceptance scenarios and store traceability.

## Stop conditions

Stop the current milestone and fix the cause when:

- The core loop is not understood without explanation.
- The primary or floor performance target fails.
- The art benchmark looks inconsistent at phone size.
- A changed authored file would exceed 1,000 physical lines, or a legacy oversized file
  is being edited before a behavior-preserving split.
- Any asset has unclear rights.
- A proposed store shot cannot be reproduced from production gameplay.
