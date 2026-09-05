# Source remediation — 2026-09-05

Baseline: `01ec872a25da4c027f835a85708077b043ede705` on `001-vertical-slice`.
Scope: close the source/integration gaps identified against the constitution, spec and
contracts, without starting Unity or Blender. The user's restriction covers headless editor
launches as well; only standalone compilation and managed domain checks are used.

## Implemented source changes

- Bootstrap and build settings now include the live Level 1 shell and all campaign shells
  and art scenes. Result controls connect reward, loadout selection, replay and next encounter.
- Level definitions supply opening, route, landing, enemy, boss, combat and campaign tuning.
- The coordinator accumulates fixed simulation steps instead of discarding long-frame time.
  Steering intent advances authoritative movement; presentation follows the resulting lane.
  Pause blocks simulation and attacks, and retry resets the clock and returns to traversal.
- Voyage craft retain individual IDs and contributions. Gate resolution happens at the
  route commitment boundary per craft. A neutral lane waits at the boundary. Rescue requires
  passing its lane, and destroyed craft cannot transfer contribution during landing.
- Army count and crew damage/cadence affect player volleys. Damage can distribute across
  live targets; only applied damage charges the ability. Land combat representation scales
  with force up to its display budget. Logical army count is shown independently of rendering.
- Chain Strait has three gate definitions, telegraphed lane hazards, a breakable chain and
  a separate armored-to-health boss boundary. Storm Fortress adds storm steering, powder
  conversion and consecutive outer-gate/commander assaults with preserved surviving force.
- Existing registered models are reused for bounded instanced campaign crowds. Existing art
  scene anchors drive camera/ship/boss presentation. Shared audio and haptics are connected.
- Reward ownership feeds the existing bilingual loadout prefab; repeated rewards are labeled
  as already owned. Campaign completion persists next-level access and its blueprint.
- Save recovery checks both JSON and schema validity before accepting the primary or backup.
  Loadout mutations refresh persisted data first, and the live audio/haptic bindings read settings.
- Reference documentation distinguishes execution images from poster/key art.

## Verification

`tools/check-csharp-without-editor.py` compiles runtime, editor and test C# together and also
compiles the 12 declared assemblies separately to catch dependency-boundary errors. It uses
the installed `6000.3.20f1` reference DLLs, which differ from the pinned `6000.3.22f1`; exact-version
asset import and player compilation remain pending.

Its optional managed runner executes selected engine-independent NUnit domain tests and a
gate-overflow/idempotency regression. It does not execute the MonoBehaviour/AssetDatabase
journey tests. New journey regressions cover fixed-step frame partitioning, missed rescue,
destroyed craft, pause, data binding, army damage, independent campaign wins/failures and retry.

The source-size script and Git whitespace check are additional static gates. No editor process,
asset import, player build, device playthrough, Blender render or remote generation is part of
this remediation.

## Required verification before calling the slice complete

1. Restore Git LFS objects and import with Unity `6000.3.22f1` when editor work is available.
2. Run the complete EditMode/PlayMode suites, especially the new journey regressions. Verify
   reward -> loadout -> replay and all three levels from clean and existing saves in a player.
3. Review gate/ship alignment, campaign crowd placement, landing motion, impact/death feedback,
   ship/crew visual identity for alternative loadouts, safe areas and Arabic on the exact revision.
   Source bindings do not certify phone-size readability or art acceptance.
4. Profile real 300/500-agent scenes on both device classes, including thermal behavior,
   allocations, render cost and Primary/Reduced outcome parity. Expanded instancing capacity
   is not a performance result.
5. Reconcile missing historical QA artifacts, approve/reject current art in Unity and capture
   the truthful store preview from the tested build. No art gate or release milestone is closed here.

The campaign presentation is newly connected source awaiting in-engine validation; it must
not be described as visually final or shipping-ready. This record supersedes stale README
statements that implementation has not started; it does not rewrite historical test results.
