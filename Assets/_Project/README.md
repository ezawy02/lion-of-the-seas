# `_Project` ownership map

This folder contains the authored Unity source for the Lion of the Seas vertical
slice. Each top-level folder has one responsibility; keep runtime code, data,
presentation assets, and tests separated accordingly.

## Ownership

- `Art/` — imported, game-ready visual assets, organized by visual domain.
- `Audio/` — imported music, sound effects, and related audio assets.
- `Data/` — authored definitions and balancing data, grouped by domain.
- `Materials/` — shared shaders and material assets.
- `Prefabs/` — reusable Unity prefab compositions.
- `Scenes/` — Unity scene assets and scene-only compositions.
- `Scripts/` — runtime and editor code, grouped by system ownership.
- `Settings/` — project and quality configuration assets owned by the slice.
- `Tests/` — EditMode, PlayMode, and Performance test assets.

## File rules

- Keep source files focused and below 1,500 lines; split a file before it reaches
  that limit. The 1,000-line threshold is a warning.
- Use the project naming and import conventions defined by `ART_PIPELINE.md`
  when that document is added.
- Do not place generated builds, captures, secrets, or third-party source here.
- Unity-generated `.meta` files belong beside Unity assets and should remain
  tracked when the corresponding asset is tracked.

This scaffold intentionally contains no gameplay implementation or binary assets.
