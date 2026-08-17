# ArtSource

Local Blender source for bespoke game art. This directory contains editable source and
small text notes only; exported and imported game assets belong under `Assets/_Project/Art/`.
Do not commit renders, caches, backups, autosaves, or binary exports here unless a later task
explicitly names the file.

## Directory ownership

- `Blender/Characters/` — friendly and hostile crews, captains, guardians, and other
  character sources.
- `Blender/Environment/` — gates, shoreline, fortress pieces, rocks, foliage, hazards, and
  background dressing.
- `Blender/Ships/` — flagship, landing craft, warships, and ship bosses.

## Source conventions

- Use Blender 5.1.1 and keep one clearly named `.blend` source per hero asset or modular kit.
- Name files and exported objects with `PascalCase` asset names, for example
  `Flagship.blend`, `HarborGuardian.blend`, and `GateMultiplier.blend`.
- Keep collections grouped as `GEO`, `RIG`, `ANIM`, `FX`, and `EXPORT`; only intended mesh,
  armature, and animation objects belong in `EXPORT`.
- Set metric units, apply object scale, and use the asset root at world origin. Model forward
  along positive local Z, with local Y up; keep pivots at the gameplay-relevant contact or
  rotation point.
- Preserve a clean modifier/rig setup in the source. Apply modifiers that affect exported
  geometry, remove hidden helpers from `EXPORT`, and keep source-only controls outside it.
- Use the project art contract for silhouette, palette, tiers, LOD intent, texture sizes, and
  mobile budgets. Record any licensed starting material in `THIRD_PARTY_NOTICES.md` when it
  is actually introduced.

## FBX export handoff

For each approved asset, export an FBX into its matching Unity destination under
`Assets/_Project/Art/` (created by the asset task), preserving the same asset basename.
Use selection-only export from the `EXPORT` collection with transforms applied, forward `-Z`,
up `Y`, and Apply Transform enabled. Export mesh normals/tangents; include an armature and
actions only when the asset requires animation. Disable leaf bones and omit cameras, lights,
empties, and source helpers.

Before handoff, verify the FBX opens at the intended scale, has the expected pivot and
orientation, contains no unintended objects, and has the planned LOD/material slots. Add a
short text note beside the source when an export needs a non-default decision; do not hide
pipeline state in filenames.

## Review boundary

This README defines source ownership and handoff conventions only. Unity import settings,
atlas rules, detailed LOD thresholds, and runtime material setup are maintained in
`Assets/_Project/Art/ART_PIPELINE.md` by T010. No cloud upload or remote asset processing is
part of this workflow.
