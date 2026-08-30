"""Rebuild the three fortress modules from their intact source GLBs.

The first mobile blockout pass used 8k-12k triangles per large module.  That was
too destructive for the thin crenellations, canopies, rails, and gate ornaments.
This focused pass keeps a review-quality LOD0 while leaving the source GLBs intact.
"""

from __future__ import annotations

import json
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
INCOMING = ROOT / "ArtSource/Blender/Incoming/level01_new/main"
UNITY_OUT = ROOT / "Assets/_Project/Art/Environment"
REVIEW_ROOT = ROOT / "ArtSource/Blender/Environment"

ASSETS = [
    (2, "L01-ENV-001_Fortress_Wall_Module", 100_000),
    (3, "L01-ENV-002_Fortress_Tower_Module", 100_000),
    (4, "L01-ENV-003_Fortress_Main_Gate_Module", 120_000),
]


def clear() -> None:
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def triangle_count(meshes: list[bpy.types.Object]) -> int:
    return sum(len(poly.vertices) - 2 for obj in meshes for poly in obj.data.polygons)


def decimate(meshes: list[bpy.types.Object], target: int) -> int:
    before = triangle_count(meshes)
    if before <= target:
        return before
    ratio = min(1.0, target / float(before))
    for obj in meshes:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new("Fortress_HighDetail_LOD0", "DECIMATE")
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)
    return triangle_count(meshes)


def export_fbx(path: Path, meshes: list[bpy.types.Object]) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        object_types={"MESH"},
        add_leaf_bones=False,
        bake_anim=False,
        apply_scale_options="FBX_SCALE_ALL",
        path_mode="RELATIVE",
        embed_textures=False,
    )


def setup_review(meshes: list[bpy.types.Object]) -> None:
    points = [obj.matrix_world @ vertex.co for obj in meshes for vertex in obj.data.vertices]
    low = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
    high = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
    center = (low + high) * 0.5
    size = max(high.x - low.x, high.y - low.y, high.z - low.z)

    bpy.ops.object.camera_add(location=center + Vector((size * 1.35, -size * 1.65, size * 1.05)))
    camera = bpy.context.object
    camera.rotation_euler = (center - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.lens = 58
    bpy.context.scene.camera = camera

    bpy.ops.object.light_add(type="AREA", location=center + Vector((size, -size, size * 1.8)))
    bpy.context.object.data.energy = 1500
    bpy.context.object.data.shape = "DISK"
    bpy.context.object.data.size = size * 2.0
    bpy.ops.object.light_add(type="AREA", location=center + Vector((-size, size * 0.5, size)))
    bpy.context.object.data.energy = 900
    bpy.context.object.data.size = size * 1.5

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = True


report = []
for archive, asset_id, target in ASSETS:
    clear()
    bpy.ops.import_scene.gltf(filepath=str(INCOMING / str(archive) / "base_basic_pbr.glb"))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    before = triangle_count(meshes)
    after = decimate(meshes, target)

    review_dir = REVIEW_ROOT / asset_id
    review_dir.mkdir(parents=True, exist_ok=True)
    export_fbx(UNITY_OUT / f"{asset_id}_Optimized.fbx", meshes)
    bpy.ops.wm.save_as_mainfile(filepath=str(review_dir / f"{asset_id}_HighDetail_REVIEW.blend"))
    setup_review(meshes)
    bpy.context.scene.render.filepath = str(review_dir / f"{asset_id}_HighDetail_REVIEW.png")
    bpy.ops.render.render(write_still=True)

    report.append({
        "asset": asset_id,
        "source": str((INCOMING / str(archive) / "base_basic_pbr.glb").relative_to(ROOT)),
        "source_triangles": before,
        "review_triangles": after,
        "target_triangles": target,
        "unity_fbx": str((UNITY_OUT / f"{asset_id}_Optimized.fbx").relative_to(ROOT)),
        "status": "User Unity review required",
    })

report_path = REVIEW_ROOT / "LEVEL01_FORTRESS_HIGH_DETAIL_REPAIR_REPORT.json"
report_path.write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
