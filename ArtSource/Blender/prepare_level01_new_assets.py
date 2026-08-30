"""Prepare the second Level 01 intake for Unity mobile blockout use.

Runs inside Blender. The numbered ZIP/GLB files were visually matched before this
mapping was authored. Main archive 7 is intentionally excluded because it is byte-for-
byte identical to archive 9 (the multiplier gate), so the palm remains a blockout.
"""

from __future__ import annotations

import csv
import json
import os
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
INCOMING = ROOT / "ArtSource/Blender/Incoming/level01_new"
OUT = ROOT / "Assets/_Project/Art"

# group, archive, asset id, output folder, triangle target, needs blockout rig
ASSETS = [
    ("main", 1, "L01-CHR-004_Harbor_Guardian_Boss", "Characters", 30000, True),
    ("main", 2, "L01-ENV-001_Fortress_Wall_Module", "Environment", 8000, False),
    ("main", 3, "L01-ENV-002_Fortress_Tower_Module", "Environment", 10000, False),
    ("main", 4, "L01-ENV-003_Fortress_Main_Gate_Module", "Environment", 12000, False),
    ("main", 5, "L01-ENV-004_Mediterranean_Harbor_Dock_Module", "Environment", 8000, False),
    ("main", 6, "L01-ENV-005_Mediterranean_Coastal_House", "Environment", 10000, False),
    ("main", 8, "L01-ENV-007_Limestone_Rock_Cluster", "Environment", 5000, False),
    ("main", 9, "L01-GAT-001_Multiplier_Gate_Arch_Buoy", "Environment", 8000, False),
    ("main", 10, "L01-PRP-003_Beach_Supply_Crate_Cluster", "Environment", 5000, False),
    ("main", 11, "L01-PRP-004_Captive_Sailmakers_Rescue_Raft_Cage", "Environment", 8000, False),
    ("main", 12, "L01-PRP-005_Blueprint_Reward_Chest", "Environment", 4000, False),
    ("details", 1, "L01-ENV-008_Coastal_Vegetation_Clump", "Environment", 3000, False),
    ("details", 2, "L01-ENV-009_Shoreline_Rock_Sand_Cluster", "Environment", 4000, False),
    ("details", 3, "L01-PRP-006_Rope_Fishing_Net_Unit", "Environment", 4000, False),
    ("details", 4, "L01-PRP-007_Cannonball_Ammo_Tray", "Environment", 2500, False),
    ("details", 5, "L01-PRP-008_Anchor_Mooring_Bollard", "Environment", 5000, False),
    ("details", 6, "L01-PRP-009_Shipwreck_Debris_Cluster", "Environment", 5000, False),
    ("details", 7, "L01-PRP-010_Fortress_Brazier", "Environment", 5000, False),
    ("details", 8, "L01-PRP-011_Wooden_Siege_Scaffold", "Environment", 7000, False),
    ("details", 9, "L01-PRP-012_Fortress_Gate_Door", "Environment", 8000, False),
    ("details", 10, "L01-PRP-013_Harbor_Pottery_Supplies", "Environment", 5000, False),
]


def clear() -> None:
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def triangles(meshes: list[bpy.types.Object]) -> int:
    return sum(len(poly.vertices) - 2 for obj in meshes for poly in obj.data.polygons)


def bounds(meshes: list[bpy.types.Object]) -> tuple[Vector, Vector]:
    points = [obj.matrix_world @ vertex.co for obj in meshes for vertex in obj.data.vertices]
    return (
        Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points))),
        Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points))),
    )


def decimate(meshes: list[bpy.types.Object], target: int) -> int:
    before = triangles(meshes)
    if before <= target:
        return before
    ratio = max(0.02, min(1.0, target / float(before)))
    for obj in meshes:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new("Mobile_Blockout_Decimate", "DECIMATE")
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)
    return triangles(meshes)


def add_blockout_rig(meshes: list[bpy.types.Object], name: str) -> bpy.types.Object:
    low, high = bounds(meshes)
    height = max(high.z - low.z, 1.0)
    cx, cy = (low.x + high.x) * 0.5, (low.y + high.y) * 0.5
    bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
    armature = bpy.context.object
    armature.name = name + "_Rig"
    root = armature.data.edit_bones[0]
    root.name = "root"
    root.head, root.tail = (cx, cy, low.z), (cx, cy, low.z + height * 0.18)

    def bone(bone_name, head, tail, parent):
        value = armature.data.edit_bones.new(bone_name)
        value.head, value.tail = head, tail
        value.parent = armature.data.edit_bones[parent]

    levels = [low.z + height * part for part in (0.18, 0.42, 0.64, 0.79, 1.0)]
    bone("spine", (cx, cy, levels[0]), (cx, cy, levels[1]), "root")
    bone("chest", (cx, cy, levels[1]), (cx, cy, levels[2]), "spine")
    bone("neck", (cx, cy, levels[2]), (cx, cy, levels[3]), "chest")
    bone("head", (cx, cy, levels[3]), (cx, cy, levels[4]), "neck")
    shoulder = max((high.x - low.x) * 0.18, height * 0.09)
    hip = max((high.x - low.x) * 0.10, height * 0.05)
    for label, sign in (("L", -1), ("R", 1)):
        bone("upper_arm_" + label, (cx + sign * shoulder, cy, levels[2]), (cx + sign * shoulder * 1.45, cy, levels[1]), "chest")
        bone("lower_arm_" + label, (cx + sign * shoulder * 1.45, cy, levels[1]), (cx + sign * shoulder * 1.65, cy, levels[0]), "upper_arm_" + label)
        bone("hand_" + label, (cx + sign * shoulder * 1.65, cy, levels[0]), (cx + sign * shoulder * 1.7, cy, levels[0] - height * 0.08), "lower_arm_" + label)
        bone("upper_leg_" + label, (cx + sign * hip, cy, levels[0]), (cx + sign * hip, cy, low.z + height * 0.07), "root")
        bone("lower_leg_" + label, (cx + sign * hip, cy, low.z + height * 0.07), (cx + sign * hip, cy, low.z), "upper_leg_" + label)
    bpy.ops.object.mode_set(mode="OBJECT")
    armature.show_in_front = True
    for obj in meshes:
        modifier = obj.modifiers.new("Blockout_Rig", "ARMATURE")
        modifier.object = armature
        groups = {bone.name: obj.vertex_groups.new(name=bone.name) for bone in armature.data.bones}
        for vertex in obj.data.vertices:
            height_ratio = ((obj.matrix_world @ vertex.co).z - low.z) / height
            group = "root" if height_ratio < 0.18 else "spine" if height_ratio < 0.42 else "chest" if height_ratio < 0.64 else "neck" if height_ratio < 0.79 else "head"
            groups[group].add([vertex.index], 1.0, "REPLACE")
    return armature


def export_textures(asset_id: str) -> None:
    texture_root = OUT / "Textures/Level01"
    texture_root.mkdir(parents=True, exist_ok=True)
    roles = {
        "texture_diffuse": "BaseColor",
        "texture_metallic-texture_roughness": "MetallicRoughness",
        "texture_normal": "Normal",
        "texture_emissive": "Emissive",
    }
    for image in bpy.data.images:
        if image.name not in roles or image.size[0] == 0:
            continue
        stem = asset_id + "_" + roles[image.name]
        destination = texture_root / (stem + ".png")
        image.name = stem
        image.filepath_raw = str(destination)
        image.file_format = "PNG"
        image.save()


def export_fbx(path: Path, meshes: list[bpy.types.Object], armature: bpy.types.Object | None) -> None:
    path.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
    if armature:
        armature.select_set(True)
        bpy.context.view_layer.objects.active = armature
    else:
        bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.export_scene.fbx(
        filepath=str(path), use_selection=True, object_types={"MESH", "ARMATURE"},
        add_leaf_bones=False, bake_anim=False, apply_scale_options="FBX_SCALE_ALL",
        path_mode="RELATIVE", embed_textures=False,
    )


rows = []
for group, archive, asset_id, folder, target, rigged in ASSETS:
    clear()
    source = INCOMING / group / str(archive) / "base_basic_pbr.glb"
    bpy.ops.import_scene.gltf(filepath=str(source))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    before = triangles(meshes)
    after = decimate(meshes, target)
    armature = add_blockout_rig(meshes, asset_id) if rigged else None
    export_textures(asset_id)
    suffix = "_Rigged_Optimized.fbx" if rigged else "_Optimized.fbx"
    output = OUT / folder / (asset_id + suffix)
    export_fbx(output, meshes, armature)
    rows.append({
        "source_group": group, "archive": f"{archive}.zip", "asset": asset_id,
        "original_triangles": before, "optimized_triangles": after,
        "target_triangles": target, "rig": "15-bone blockout" if rigged else "n/a",
        "output": str(output.relative_to(ROOT)), "status": "User Unity review required",
    })

manifest = OUT / "LEVEL01_NEW_ASSET_MANIFEST.csv"
with manifest.open("w", newline="", encoding="utf-8") as file:
    writer = csv.DictWriter(file, fieldnames=rows[0].keys())
    writer.writeheader()
    writer.writerows(rows)

print(json.dumps(rows, indent=2))
