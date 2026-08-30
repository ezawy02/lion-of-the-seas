"""Prepare the Level 01 opening-reference asset intake for Unity mobile use.

Run with Blender in background mode. The source GLBs were visually matched to the
isolated references before this mapping was authored. Source geometry remains in
ArtSource/Blender/Incoming; this script writes optimized FBX handoff files and textures.
"""

from __future__ import annotations

import csv
import json
from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
INCOMING = ROOT / "ArtSource/Blender/Incoming/level01_opening_match"
OUT = ROOT / "Assets/_Project/Art"

# source folder, stable asset id, output folder, mobile LOD0 triangle target
ASSETS = [
    ("1", "L01-ENV-010_Left_Coastal_Cliff", "Environment", 12000),
    ("2", "L01-ENV-011_Right_Artillery_Cliff", "Environment", 15000),
    ("3", "L01-ENV-012_Mediterranean_Mountain_City_Backdrop", "Environment", 18000),
    ("4", "L01-SHP-004_Hero_Flagship_ReferenceMatch", "Ships", 35000),
    ("palm", "L01-ENV-006_Palm_Tree_Cluster", "Environment", 8000),
]


def clear_scene() -> None:
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def meshes() -> list[bpy.types.Object]:
    return [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]


def triangle_count(objects: list[bpy.types.Object]) -> int:
    total = 0
    for obj in objects:
        obj.data.calc_loop_triangles()
        total += len(obj.data.loop_triangles)
    return total


def optimize(objects: list[bpy.types.Object], target: int) -> int:
    before = triangle_count(objects)
    if before <= target:
        return before
    ratio = max(0.02, min(1.0, target / float(before)))
    for obj in objects:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new("Mobile_LOD0_Decimate", "DECIMATE")
        modifier.decimate_type = "COLLAPSE"
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)
    return triangle_count(objects)


def export_textures(asset_id: str) -> list[str]:
    texture_root = OUT / "Textures/Level01"
    texture_root.mkdir(parents=True, exist_ok=True)
    roles = {
        "texture_diffuse": "BaseColor",
        "texture_metallic-texture_roughness": "MetallicRoughness",
        "texture_metallic": "Metallic",
        "texture_roughness": "Roughness",
        "texture_normal": "Normal",
        "texture_emissive": "Emissive",
    }
    written = []
    for image in bpy.data.images:
        source_name = Path(image.name).stem
        role = roles.get(source_name)
        if not role or image.size[0] == 0:
            continue
        destination = texture_root / f"{asset_id}_{role}.png"
        image.name = destination.stem
        image.filepath_raw = str(destination)
        image.file_format = "PNG"
        image.save()
        written.append(str(destination.relative_to(ROOT)))
    return written


def export_fbx(destination: Path, objects: list[bpy.types.Object]) -> None:
    destination.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = objects[0]
    bpy.ops.export_scene.fbx(
        filepath=str(destination),
        use_selection=True,
        object_types={"MESH"},
        add_leaf_bones=False,
        bake_anim=False,
        apply_scale_options="FBX_SCALE_ALL",
        axis_forward="-Z",
        axis_up="Y",
        path_mode="RELATIVE",
        embed_textures=False,
    )


rows = []
for source_folder, asset_id, output_folder, target in ASSETS:
    clear_scene()
    source = INCOMING / source_folder / "base_basic_pbr.glb"
    bpy.ops.import_scene.gltf(filepath=str(source))
    imported = meshes()
    if not imported:
        raise RuntimeError(f"No mesh found in {source}")
    original = triangle_count(imported)
    optimized = optimize(imported, target)
    textures = export_textures(asset_id)
    output = OUT / output_folder / f"{asset_id}_Optimized.fbx"
    export_fbx(output, imported)
    rows.append(
        {
            "source": str(source.relative_to(ROOT)),
            "asset": asset_id,
            "original_triangles": original,
            "optimized_triangles": optimized,
            "target_triangles": target,
            "motion": "GameObject transform" if asset_id.startswith("L01-SHP") else "static/shader",
            "output": str(output.relative_to(ROOT)),
            "textures": ";".join(textures),
            "status": "Unity visual review required",
        }
    )

manifest = OUT / "LEVEL01_OPENING_MATCH_ASSET_MANIFEST.csv"
with manifest.open("w", newline="", encoding="utf-8") as file:
    writer = csv.DictWriter(file, fieldnames=rows[0].keys())
    writer.writeheader()
    writer.writerows(rows)

print(json.dumps(rows, indent=2))
