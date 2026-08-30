# Level 01 Asset Reuse Lock

This registry prevents duplicate reference generation and duplicate 3D conversion for
Level 01. It is a production inventory, not user art approval or Art Lock.

## Inventory summary

- 34 single-view 3D source references are registered across desktop folders 01, 03, 04,
  and 05.
- All 34 references now have distinct Unity-ready prepared FBX outputs.
- The 4 opening-match references in folder 05 were converted, visually identified, mobile
  optimized, and imported for Unity review.
- The palm was reconverted from its existing PNG; its new source is distinct from the
  multiplier-gate GLB and its optimized Unity FBX is prepared.
- No additional single-view 3D reference image is required for Level 01.

## Folder 01 — already modeled and prepared

- L01-CHR-001 Hayreddin Barbarossa
- L01-CHR-002 Friendly Marine
- L01-CHR-003 Hostile Infantry
- L01-PRP-001 Shore Cannon
- L01-PRP-002 Fictional Lion-Wave Banner
- L01-SHP-001 Original Flagship (prototype/reference; superseded in the opening by SHP-004)
- L01-SHP-002 Landing Craft
- L01-SHP-003 Hostile Patrol Boat

## Folder 03 — already modeled and prepared

- L01-CHR-004 Harbor Guardian
- L01-GAT-001 Multiplier Gate
- L01-ENV-001 Fortress Wall
- L01-ENV-002 Fortress Tower
- L01-ENV-003 Fortress Main Gate
- L01-ENV-004 Harbor Dock
- L01-ENV-005 Coastal House
- L01-ENV-007 Limestone Rock Cluster
- L01-PRP-003 Beach Supply Crates
- L01-PRP-004 Captive Sailmakers Rescue Raft/Cage
- L01-PRP-005 Blueprint Reward Chest

## Folder 03 — corrected conversion prepared

- L01-ENV-006 Palm Tree Cluster. The old numbered `7.zip` remains known-bad and must never
  be reused. The named replacement archive and optimized FBX are the valid sources.

## Folder 04 — already modeled and prepared

- L01-ENV-008 Coastal Vegetation
- L01-ENV-009 Shoreline Rock/Sand Cluster
- L01-PRP-006 Rope/Fishing Net
- L01-PRP-007 Cannonball Ammo Tray
- L01-PRP-008 Anchor/Mooring Bollard
- L01-PRP-009 Shipwreck Debris
- L01-PRP-010 Fortress Brazier
- L01-PRP-011 Siege Scaffold
- L01-PRP-012 Fortress Gate Door
- L01-PRP-013 Harbor Pottery/Supplies

## Folder 05 — modeled and prepared for Unity review

- L01-SHP-004 Reference-Match Hero Flagship
- L01-ENV-010 Left Coastal Cliff
- L01-ENV-011 Right Artillery Cliff
- L01-ENV-012 Mediterranean Mountain City Backdrop

Prepared outputs:

- `Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_ReferenceMatch_Optimized.fbx`
- `Assets/_Project/Art/Environment/L01-ENV-010_Left_Coastal_Cliff_Optimized.fbx`
- `Assets/_Project/Art/Environment/L01-ENV-011_Right_Artillery_Cliff_Optimized.fbx`
- `Assets/_Project/Art/Environment/L01-ENV-012_Mediterranean_Mountain_City_Backdrop_Optimized.fbx`
- `Assets/_Project/Art/Environment/L01-ENV-006_Palm_Tree_Cluster_Optimized.fbx`

## Explicit reuse decisions

- Build the landing beach from L01-ENV-009 plus the authored water/shoreline material; do
  not generate another beach image.
- Use the captive already represented by L01-PRP-004 and reuse L01-CHR-002 where an
  animated rescued sailor is needed; do not generate a duplicate captive character.
- Present the blueprint through 2D UI/VFX emerging from L01-PRP-005; do not generate a
  duplicate 3D reward chest or blueprint prop.
- Pose existing friendly marines as boat crews; do not generate seated character variants.
- Keep banners as the separate fictional L01-PRP-002 asset; do not embed national flags in
  new geometry.

## Visual reference classification

These images are not 3D source assets and must never be sent through the image-to-3D
conversion folder. Their classification controls how they may be used:

- `REF_Level01_Opening.png` — **execution-reference**; opening approach, camera,
  movement direction, asset placement, and scale; no multiplier gate.
- `REF_Level01_Traversal_GateRescue.png` — **execution-reference**; multiplier gate,
  captive-sailmaker rescue, camera, movement direction, placement, and scale.
- `REF_Level01_BeachLanding.png` — **execution-reference**; beach landing transition,
  camera, movement direction, placement, and scale; no multiplier gate.
- `REF_Level01_BossBattle.png` — **poster-key-art**; mood, palette, silhouettes, and
  thematic detail only. It MUST NOT control blockout camera, direction, scale, placement,
  measurement, or gameplay layout.
- `REF_Level01_VictoryReward.png` — **poster-key-art**; mood, palette, silhouettes, and
  thematic detail only. It MUST NOT control blockout camera, direction, scale, placement,
  measurement, or gameplay layout.

An image classified as poster/key art may become an execution reference only when the user
explicitly approves that exact image for that exact implementation purpose. All Unity revisions
remain unapproved until the user reviews the exact revision in Unity and explicitly approves it.
