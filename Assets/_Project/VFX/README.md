# T048 benchmark water and reaction effects

The benchmark family is deliberately small and authored for the portrait camera:

- `WaterSurface.prefab` — an 18x18 generated grid with analytic URP waves.
- `Wake.prefab` — a tapered 18-segment ribbon that stays legible behind the flagship.
- `FoamPatch.prefab` — a persistent low ring for shoreline/ship contact foam.
- `LandingSplash.prefab` — a broad, ivory ring for the sea-to-land transfer beat.
- `HitSplash.prefab` — a short, deterministic four-lobed splash for combat hits.
- `BossReaction.prefab` — two concentric rings with a brighter inner reaction beat.

`WaterVfxEffect` builds each silhouette once per instance and is safe to hand to the existing
`VfxPool<T>` bridge through `SetPoolRelease`. `Play` only mutates the transform and one
`MaterialPropertyBlock`; it does not instantiate, allocate, or modify authoritative battle state.
`SetQuality(profile.VfxDensity, profile.ProfileKind == Reduced)` supports Reduced mode by lowering
foam intensity and analytic wave work while preserving timing and event semantics.

Suggested event mapping for the existing presentation subscriber:

| Battle presentation kind | Prefab |
| --- | --- |
| LandingStarted / LandingCompleted | `LandingSplash` / `FoamPatch` |
| Hit | `HitSplash` |
| Boss phase changed | `BossReaction` |
| Flagship movement | pooled `Wake` |

Run `Sea Lion > Validation > T048 Water Benchmark` after import. The validator checks the
shader/material family and every pooled-ready prefab. Review on a portrait camera at phone size;
the palette is deep navy/turquoise water with high-contrast ivory foam and no texture dependency.
