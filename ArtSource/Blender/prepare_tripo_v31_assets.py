"""Create Unity-ready review candidates from the six Tripo V3.1 source models.

The canonical high-resolution GLBs under ArtSource/Blender are never overwritten.
Every output remains a review candidate until the user approves it inside Unity.
"""

from __future__ import annotations

import csv
import json
from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
ART = ROOT / "Assets/_Project/Art"

# Asset id, source category, output category, triangle target, texture level.
ASSETS = [
    ("L02-SHP-001_Armored_Warship_Boss", "Ships", "Ships", 50000, "Level02"),
    ("L02-PRP-001_Floating_Naval_Mine", "Environment", "Environment", 50000, "Level02"),
    ("L02-PRP-002_Heavy_Chain_Link_Unit", "Environment", "Environment", 80000, "Level02"),
    ("L03-SHP-001_Gunpowder_Skiff", "Ships", "Ships", 80000, "Level03"),
    ("L03-PRP-001_Gunpowder_Barrel_Cluster", "Environment", "Environment", 50000, "Level03"),
    ("L03-CHR-001_Storm_Fortress_Commander", "Characters", "Characters", 80000, "Level03"),
]


def clear_scene() -> None:
    bpy.ops.wm.read_factory_settings(use_empty=True)


def mesh_objects() -> list[bpy.types.Object]:
    return [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]


def triangle_count(meshes: list[bpy.types.Object]) -> int:
    total = 0
    for obj in meshes:
        obj.data.calc_loop_triangles()
        total += len(obj.data.loop_triangles)
    return total


def decimate(meshes: list[bpy.types.Object], target: int) -> int:
    before = triangle_count(meshes)
    if before <= target:
        return before
    ratio = max(0.001, min(1.0, target / float(before)))
    for obj in meshes:
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new("Mobile_LOD0_Decimate", "DECIMATE")
        modifier.decimate_type = "COLLAPSE"
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.select_set(False)
    return triangle_count(meshes)


def apply_mesh_cleanup(meshes: list[bpy.types.Object], asset_id: str) -> None:
    for index, obj in enumerate(meshes):
        obj.name = asset_id if len(meshes) == 1 else f"{asset_id}_{index:02d}"
        obj.data.name = obj.name + "_Mesh"
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
        bpy.ops.object.shade_smooth_by_angle()
        obj.select_set(False)


def export_textures(asset_id: str, texture_level: str) -> list[str]:
    destination = ART / "Textures" / texture_level
    destination.mkdir(parents=True, exist_ok=True)
    exported = []
    for image in bpy.data.images:
        lowered = image.name.lower()
        if image.size[0] == 0 or image.source == "VIEWER":
            continue
        if "color" in lowered:
            role = "BaseColor"
        elif "normal" in lowered:
            role = "Normal"
        elif "orm" in lowered:
            role = "ORM"
        else:
            continue
        path = destination / f"{asset_id}_{role}.png"
        image.filepath_raw = str(path)
        image.file_format = "PNG"
        image.save()
        exported.append(str(path.relative_to(ROOT)))
    return sorted(set(exported))


def save_blend(asset_id: str, source_category: str) -> Path:
    path = ROOT / "ArtSource/Blender" / source_category / asset_id / f"{asset_id}_Optimized_REVIEW.blend"
    bpy.ops.wm.save_as_mainfile(filepath=str(path), check_existing=False)
    return path


def export_fbx(asset_id: str, output_category: str, meshes: list[bpy.types.Object]) -> Path:
    path = ART / output_category / f"{asset_id}_Optimized_REVIEW.fbx"
    bpy.ops.object.select_all(action="DESELECT")
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        object_types={"MESH"},
        axis_forward="-Z",
        axis_up="Y",
        apply_scale_options="FBX_SCALE_ALL",
        add_leaf_bones=False,
        bake_anim=False,
        mesh_smooth_type="FACE",
        path_mode="RELATIVE",
        embed_textures=False,
    )
    return path


rows = []
for asset_id, source_category, output_category, target, texture_level in ASSETS:
    clear_scene()
    source = ROOT / "ArtSource/Blender" / source_category / asset_id / f"{asset_id}_TripoV31_PBR.glb"
    bpy.ops.import_scene.gltf(filepath=str(source))
    meshes = mesh_objects()
    before = triangle_count(meshes)
    after = decimate(meshes, target)
    apply_mesh_cleanup(meshes, asset_id)
    textures = export_textures(asset_id, texture_level)
    blend_path = save_blend(asset_id, source_category)
    fbx_path = export_fbx(asset_id, output_category, meshes)
    rows.append({
        "asset_id": asset_id,
        "source": str(source.relative_to(ROOT)),
        "original_triangles": before,
        "target_triangles": target,
        "optimized_triangles": after,
        "optimized_blend": str(blend_path.relative_to(ROOT)),
        "review_fbx": str(fbx_path.relative_to(ROOT)),
        "textures": ";".join(textures),
        "rig": "unrigged",
        "status": "Unity user review required",
    })

manifest = ART / "TRIPO_V31_REVIEW_ASSET_MANIFEST.csv"
with manifest.open("w", newline="", encoding="utf-8") as handle:
    writer = csv.DictWriter(handle, fieldnames=rows[0].keys())
    writer.writeheader()
    writer.writerows(rows)

print(json.dumps(rows, indent=2))
