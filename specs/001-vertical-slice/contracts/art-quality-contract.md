# Art Quality Contract

## Intent

The slice must look deliberately authored rather than assembled from unrelated assets.
"Premium" means consistent shape language, palette, lighting, motion, feedback, and phone-size
readability. It does not mean maximum polygon count or expensive effects.

## Art pillars

1. **Readable spectacle**: the player, enemy, decision, and objective are identifiable in
   a five-second portrait screenshot.
2. **Mediterranean pirate fantasy**: warm stone, painted wood, cloth sails, brass, sea foam,
   fortress silhouettes, and fictional corsair identity.
3. **Toy-like weight**: simplified proportions and materials combined with convincing recoil,
   wake, impact, collapse, and boss reaction.
4. **Controlled richness**: detail concentrates on flagship, captain, boss, gate, shoreline,
   and reward; ordinary agents and distant props remain efficient.

## Palette contract

| Role | Core colors | Usage rule |
|------|-------------|------------|
| Friendly | Turquoise, deep navy, ivory, aged gold | Dominant on units, wakes, selection, and positive feedback |
| Hostile | Crimson, charcoal, burnt copper | Dominant on enemies, hazards, fortress pressure, and negative feedback |
| Positive decision | Violet-blue and gold | Must not resemble damage or enemy red |
| Hazard | Orange-red, black, warning white | Visible before commitment |
| Environment | Sea teal, warm sand, limestone, muted vegetation | Must preserve unit contrast |

Color is reinforced through silhouette, icon, motion direction, and value contrast so the
game remains understandable under common color-vision differences.

## Shape language

- Friendly shapes lean forward, use upward sails, rounded shields, and open negative space.
- Hostile shapes use broader masses, downward spikes, closed armor, and square fortification.
- Gates have a consistent arch/buoy frame and a large central numeric plane.
- Bosses must be recognizable at 10% of portrait-screen height and in silhouette alone.
- Ordinary units share one base rig/proportion family per faction to protect batching and
  visual cohesion.

## Asset tiers

### Tier A — Store-facing hero assets

Flagship, captain, Level 1 guardian, Level 2 armored warship, Level 3 fortress boss, main
gates, reward chest/blueprint, and the first environment vista.

- Bespoke modeling and textures required.
- Clean silhouette at phone size and close camera push-in.
- Dedicated reaction, destruction, and reward motion.
- LOD0/LOD1/LOD2 plus validated shadow behavior where applicable.

### Tier B — Repeated gameplay assets

Ordinary crews, defenders, landing craft, cannons, walls, rocks, foliage, and standard VFX.

- Shared atlases/material families and modular construction encouraged.
- May begin from compatible licensed sources after restyling and license recording.
- Must match proportion, palette, roughness, and edge treatment of Tier A.

### Tier C — Background dressing

Distant ships, skyline, clouds, islands, small props, and non-interactive debris.

- Aggressive LOD, simplified materials, baked detail, and silhouette-first modeling.
- Cannot compete with gates, forces, or boss health presentation.

## Mobile budgets for the benchmark

Budgets are starting caps and can tighten after device profiling.

| Asset | LOD0 triangle guide | Material guide | Animation guide |
|------|---------------------|----------------|-----------------|
| Ordinary crowd unit | 800–1,500 | One shared instanced material | Shader/baked pose phase |
| Landing craft | 1,500–3,000 | One shared material family | Kinematic + simple parts |
| Flagship | 20,000–35,000 | Up to three atlas materials | Skeletal/transform hero motion |
| Boss character | 15,000–30,000 | Up to three materials | Skeletal Animator |
| Boss warship | 30,000–50,000 | Up to four atlas materials | Transform/rig hybrid |
| Repeated prop | 300–2,000 | Shared atlas | None or shader motion |

- Texture sets default to 1K for repeated assets and 2K for Tier A atlases; 4K requires a
  measured close-up need and import downscales for mobile.
- Alpha overdraw, transparent foam, smoke, and particles must be reviewed in peak combat.
- One hero key light and restrained shadow casters are preferred over many dynamic lights.

## Benchmark scene acceptance

`Benchmark_Art` passes only when a representative mobile build contains:

- One final-quality flagship and wake.
- At least 60 friendly and 60 hostile instanced units in active motion.
- One ×4 gate with before/after force feedback.
- Final-style water, beach, fortress slice, sky, fog, and lighting.
- Harbor Guardian entrance, hit reaction, armor/health feedback, and defeat beat.
- Representative broadside, gate, landing, hit, destruction, and reward VFX/audio.
- Primary and Reduced quality captures from the exact same camera and battle seed.

## Review checklist

- [ ] Friendly, hostile, gate, and boss read in a five-second phone-size screenshot.
- [ ] Materials share consistent roughness, edge treatment, saturation, and scale.
- [ ] No unmodified asset-pack style is visible in a store-facing frame.
- [ ] Water contact exists at ship, shore, projectile, and destruction points.
- [ ] Multiplication feels larger through formation, sound, number motion, and camera response.
- [ ] Hit and death feedback is readable without filling the screen with opaque particles.
- [ ] Boss attacks have warnings; boss reactions clearly acknowledge player damage.
- [ ] LOD transitions and quality-preset switches do not expose broken silhouettes.
- [ ] Primary and Reduced profiles preserve gameplay clarity and event timing.
- [ ] All non-original assets have complete license records.

Any failed item blocks Art Lock. A still render cannot substitute for the playable benchmark.
