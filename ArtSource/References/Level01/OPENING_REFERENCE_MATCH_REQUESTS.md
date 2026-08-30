# Level 01 Opening — Reference Match Asset Requests

Target reference: `REF_Level01_Opening.png`

This is a preparation checklist, not an approval or Art Lock record. Every revision still
requires the user's visual review inside Unity.

## Match rule

- Target one locked 9:16 gameplay camera.
- No multiplier gate is visible in the opening phase.
- Use only the user's authored character identity and approved asset shapes.
- Each requested source image must show exactly one asset on a transparent background.
- Do not include turnarounds, labels, scenery, ground planes, shadows, or multiple views.

## New 3D sources required

### L01-SHP-004 — Reference-Match Hero Flagship

- Exact high-stern silhouette seen in the opening reference.
- Large decorated stern castle with navy/turquoise painted wood and aged-gold trim.
- Lion relief centered on the stern.
- Two tall masts, large ivory lateen sails, rigging, wheel, railings, and a readable rear deck.
- Clear standing area for Hayreddin behind the wheel.
- No national flag embedded in the mesh; flag sockets only. The existing fictional lion-wave
  banner remains a separate swappable asset.
- Clean waterline, no wake, crew, ocean, background, or camera-dependent billboards.

### L01-ENV-010 — Left Coastal Cliff Gateway

- Tall warm-limestone cliff matching the left edge of the reference.
- Layered rocks, sparse Mediterranean shrubs, and a small eroded shoreline foot.
- Concave inner edge framing the sea channel; broad outside mass for screen-edge cropping.
- No buildings, water plane, sky, boats, or baked background.

### L01-ENV-011 — Right Artillery Cliff Gateway

- Warm-limestone cliff matching the right edge of the reference.
- Flat fortified top sized for the existing tower and shore-cannon assets.
- Strong inner overhang framing the sea channel and a readable lower shoreline.
- No tower, cannon, muzzle flash, water plane, sky, boats, or baked background.

### L01-ENV-012 — Mediterranean Mountain City Backdrop

- One distant layered city-and-mountain silhouette built for the locked opening camera.
- Warm Ottoman/Mediterranean coastal houses, towers, terraces, and muted blue-grey mountains.
- Lower edge must meet the harbor/fortress modules without a visible seam.
- Background-detail tier only; no foreground cliffs, water, boats, UI, or baked sky.

## Existing assets to reuse after correction

- Hayreddin: `L01-CHR-001_Hayreddin_Barbarossa_Rigged_Optimized.fbx`.
- Friendly crew: `L01-CHR-002_Friendly_Marine_Rigged_Optimized.fbx`, posed as seated rowers.
- Escort boat: `L01-SHP-002_Landing_Craft_Optimized.fbx`.
- Enemy patrol boat: `L01-SHP-003_Hostile_Patrol_Boat_Optimized.fbx`.
- Fortress, towers, gate, houses, rocks, cannon, and fictional lion-wave banner from the
  existing Level 01 authored set.

## Local Unity work after the four new models arrive

- Lock camera and object screen-space bounds against the reference.
- Pose Hayreddin and the escort crews without changing their identity or clothing design.
- Build the water, wakes, impact ring/splash, cannon muzzle flash, clouds, haze, and lighting.
- Recreate the top HUD and bottom drag instruction as dedicated 2D UI sprites.
- Capture the exact Unity revision for user review; no automated check can approve it.
