# T047 Art Review - Rejected

The current procedural Blender/FBX slice is retained only as a technical composition and
import prototype. It is not approved as final or store-facing art.

## Reasons

- Hero geometry is materially below the authored Tier A guides: Flagship 6,990 triangles
  versus 20,000-35,000; Harbor Guardian 6,580 versus 15,000-30,000.
- Forms read as low-poly primitives instead of detailed premium stylized assets.
- Assets lack authored UV layouts, baked normal detail, PBR texture sets, rigged hero
  animation, controlled roughness variation, and production prefab/LODGroup setup.
- The current Blender beauty render does not prove appearance inside Unity URP.
- The visual language does not yet match the requested polished crowd-action reference.

## Required replacement direction

1. Rebuild Tier A flagship and Guardian with production topology, UVs, baked normals,
   coherent PBR atlases, hero silhouettes, and animation-ready hierarchy.
2. Rebuild crew/enemy as efficient stylized characters with clear faction anatomy,
   costume, weapon, and baked-pose motion rather than assembled primitives.
3. Replace the gate with a polished in-engine readable UI/mesh treatment.
4. Author Unity prefabs with URP materials, LODGroup, shadows, VFX anchors, and validated
   portrait-camera presentation.
5. Approve only from an in-engine Benchmark_Art scene, never from Blender renders alone.

Status: **REJECTED - T047 reopened.**
