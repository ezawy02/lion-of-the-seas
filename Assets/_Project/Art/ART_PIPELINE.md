# Art Pipeline

This is the Unity-side handoff contract for stylized premium mobile art. Editable
sources remain in `ArtSource/Blender/`; reviewed game-ready exports and import settings
belong in this directory. The portrait gameplay camera and the two URP quality profiles
are the approval context. Do not add an asset, texture, or package merely to satisfy a
pipeline rule.

## Naming and folders

- Use `PascalCase` for hero and modular asset basenames: `Flagship`, `HarborGuardian`,
  `GateMultiplier`. Use stable, descriptive suffixes only where needed: `_LOD0`, `_LOD1`,
  `_LOD2`, `_COL`, `_SK`, `_MAT`, `_TEX`, `_N`, `_M`, `_R`, `_A`, and `_FX`.
- Match the source basename, FBX basename, prefab basename, and primary material family.
  Do not encode version, quality tier, or pipeline state in filenames; record those in a
  note or review record.
- Place imported assets in the matching `Characters/`, `Environment/`, `Ships/`, `UI/`, or
  `VFX/` folder. Keep textures beside their asset or in its clearly named `Textures/`
  subfolder. Keep source `.blend` files and renders out of `Assets/`.
- One prefab is the reviewed runtime entry point. Keep meshes, materials, colliders,
  animation clips, and LOD configuration owned by that prefab rather than scene copies.

## Geometry, scale, and pivots

- Author in Blender 5.1.1 with metric units. Apply object scale before export; use Unity
  metres, with one Blender unit representing one metre.
- The asset root is at world origin, local Y is up, and model forward is local +Z in the
  source. The FBX handoff uses Blender forward `-Z`, up `Y`, with Apply Transform enabled;
  verify the resulting Unity forward direction in the prefab.
- Put the root pivot at the gameplay contact or rotation point: keel/waterline for ships,
  foot contact for characters, hinge/rotation axis for gates, and ground contact for props.
  Do not repair an incorrect pivot with an arbitrary scene offset.
- Apply transforms and recalculate normals before export. Keep collision geometry simple,
  named with `_COL`, and separate from render geometry; do not export hidden helpers.

## Materials, textures, and atlases

- Prefer URP-compatible simple lit materials, shared shader variants, SRP Batcher
  compatibility, and opaque geometry. Use transparency only when the effect requires it;
  review foam, smoke, and particles for peak overdraw in combat.
- Use one shared material for ordinary crowd units and one shared material family for
  repeated props and landing craft. Tier A assets may use up to three atlas materials,
  except boss warships which may use up to four.
- Default texture sizes are 1K for repeated assets and 2K for Tier A atlases. A 4K source
  needs a measured close-up justification and must import downscaled for mobile.
- Pack compatible assets by material family and texel density, not by arbitrary folder. An
  atlas must keep albedo/base color, normal, mask/roughness, and emission choices aligned;
  leave padding around islands and avoid mip bleeding. Do not atlas UI, VFX flipbooks, or
  assets with incompatible filtering/wrap requirements.
- Unity texture defaults: sRGB for color data, linear for normals/masks, compressed mobile
  format selected per platform, mipmaps for world geometry, and no read/write unless a
  runtime system explicitly needs it. Record an exception beside the asset.

## LOD and mobile budgets

Use the art contract's starting caps and validate silhouette, shadow behavior, and material
continuity from the portrait camera. LOD transitions must not expose broken topology or
change gameplay readability.

| Asset class | LOD0 guide | LOD policy |
| --- | ---: | --- |
| Ordinary crowd unit | 800–1,500 triangles | LOD1 for mid distance; LOD2/billboard only as a profiled far fallback |
| Landing craft | 1,500–3,000 | Preserve hull and sail silhouette; share materials |
| Flagship | 20,000–35,000 | LOD0/1/2; validate wake and shadow separately |
| Boss character | 15,000–30,000 | LOD0/1/2; preserve readable armor and reaction silhouette |
| Boss warship | 30,000–50,000 | LOD0/1/2; preserve weapon, hull, and fortress silhouette |
| Repeated prop | 300–2,000 | Aggressive LOD and baked detail for background use |

- Configure a `LODGroup` on the prefab with screen-relative transitions chosen from actual
  phone captures; start with conservative fades and tune after profiling.
- Keep ordinary agents instanced and shader/baked-pose driven. Skeletal `Animator` stacks
  are reserved for captains, bosses, and close hero units.
- Use the fewest shadow casters that preserve contact and boss readability. Check both
  Primary and Reduced profiles from the same camera and battle seed.

## FBX export and Unity import checklist

1. In Blender, keep only intended mesh, armature, and animation objects in `EXPORT`; apply
   geometry-affecting modifiers and remove cameras, lights, empties, and helpers.
2. Export selection-only FBX to the matching `Assets/_Project/Art/` folder with transforms
   applied, forward `-Z`, up `Y`, Apply Transform enabled, mesh normals/tangents enabled,
   leaf bones disabled, and animation/actions included only when required.
3. In Unity, verify scale is `1,1,1` at the reviewed root, orientation and pivot are correct,
   normals/tangents are as intended, material slots are planned, and no unintended objects
   were imported. Set rig type and animation import only for assets that need them.
4. Add the planned `LODGroup`, colliders, shared materials, and prefab references. Confirm
   no hidden source helper, duplicate material, or accidental read/write texture remains.
5. Review in the portrait benchmark: five-second silhouette/readability, contact with water
   or ground, LOD transitions, shadow behavior, Reduced profile parity, and peak overdraw.
   Record any non-default decision in a short text note beside the asset.

An asset is not approved by a modeling render alone. Licensing records belong in
`THIRD_PARTY_NOTICES.md` when third-party material is actually introduced; this document
does not authorize new assets or cloud processing.
