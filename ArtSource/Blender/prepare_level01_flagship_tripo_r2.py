"""Prepare the Tripo v3.1 flagship R2 as a local Unity review candidate.

The high-resolution source GLB remains untouched. Outputs are explicitly REVIEW
assets and cannot become approved art without the user's Unity visual review.
"""

from __future__ import annotations

import json
import math
from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
ASSET_ID = "L01-SHP-004_Hero_Flagship_TripoV31_R2"
SOURCE = ROOT / "ArtSource/Blender/Incoming/TripoTests/L01-SHP-004_Multiview_v02_REVIEW/retopology_8k/L01-SHP-004_Hero_Flagship_TripoV31_Multiview_R2_Retopo8K.fbx"
BLEND = ROOT / f"ArtSource/Blender/Ships/{ASSET_ID}/{ASSET_ID}_Optimized_REVIEW.blend"
FBX = ROOT / f"Assets/_Project/Art/Ships/{ASSET_ID}_Optimized_REVIEW.fbx"
TEXTURES = ROOT / "Assets/_Project/Art/Textures/Level01"
TARGET_TRIANGLES = 35_000


def triangles(meshes: list[bpy.types.Object]) -> int:
    for obj in meshes:
        obj.data.calc_loop_triangles()
    return sum(len(obj.data.loop_triangles) for obj in meshes)


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(SOURCE), use_image_search=True)
meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
if not meshes:
    raise RuntimeError("The generated flagship GLB contains no mesh")

before = triangles(meshes)
ratio = min(1.0, TARGET_TRIANGLES / float(before))
for index, obj in enumerate(meshes):
    obj.name = ASSET_ID if len(meshes) == 1 else f"{ASSET_ID}_{index:02d}"
    obj.data.name = obj.name + "_Mesh"
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    # Tripo's ship length arrives on Blender X. The Level 1 builder expects the
    # vessel's bow/stern axis on Unity Z, which maps from Blender Y on FBX export.
    obj.rotation_euler.z += math.radians(90.0)
    bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    if ratio < 1.0:
        modifier = obj.modifiers.new("Mobile_LOD0_Decimate", "DECIMATE")
        modifier.decimate_type = "COLLAPSE"
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    bpy.ops.object.shade_smooth_by_angle()
    obj.select_set(False)

after = triangles(meshes)
TEXTURES.mkdir(parents=True, exist_ok=True)
texture_paths: list[str] = []
for image in bpy.data.images:
    lowered = f"{image.name} {image.filepath}".lower()
    if image.size[0] == 0 or image.source == "VIEWER":
        continue
    if "basecolor" in lowered or "base_color" in lowered or "diffuse" in lowered:
        role = "BaseColor"
    elif "normal" in lowered:
        role = "Normal"
    elif "orm" in lowered:
        role = "ORM"
    elif "metallic" in lowered:
        role = "Metallic"
    elif "roughness" in lowered:
        role = "Roughness"
    else:
        continue
    destination = TEXTURES / f"{ASSET_ID}_{role}.png"
    image.filepath_raw = str(destination)
    image.file_format = "PNG"
    image.save()
    texture_paths.append(str(destination.relative_to(ROOT)))

BLEND.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND), check_existing=False)

FBX.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action="DESELECT")
for obj in meshes:
    obj.select_set(True)
bpy.context.view_layer.objects.active = meshes[0]
bpy.ops.export_scene.fbx(
    filepath=str(FBX),
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

summary = {
    "asset_id": ASSET_ID,
    "source": str(SOURCE.relative_to(ROOT)),
    "original_triangles": before,
    "target_triangles": TARGET_TRIANGLES,
    "optimized_triangles": after,
    "blend": str(BLEND.relative_to(ROOT)),
    "fbx": str(FBX.relative_to(ROOT)),
    "textures": sorted(texture_paths),
    "status": "Unity user review required",
}
(BLEND.parent / "preparation_summary.json").write_text(
    json.dumps(summary, indent=2), encoding="utf-8"
)
print("FLAGSHIP_PREP=" + json.dumps(summary))
