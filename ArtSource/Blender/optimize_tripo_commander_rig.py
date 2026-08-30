"""Optimize the Tripo-generated Mixamo-compatible commander rig for Unity review."""

from pathlib import Path
import json

import bpy


ROOT = Path(__file__).resolve().parents[2]
ASSET_ID = "L03-CHR-001_Storm_Fortress_Commander"
FOLDER = ROOT / "ArtSource/Blender/Characters" / ASSET_ID
SOURCE = FOLDER / f"{ASSET_ID}_TripoV25_MixamoRig.glb"
BLEND_OUT = FOLDER / f"{ASSET_ID}_TripoRig_Optimized_REVIEW.blend"
FBX_OUT = ROOT / "Assets/_Project/Art/Characters" / f"{ASSET_ID}_TripoRig_Optimized_REVIEW.fbx"
REPORT_OUT = FOLDER / f"{ASSET_ID}_TripoRig_Optimization_Report.json"
TARGET_TRIANGLES = 80000


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.gltf(filepath=str(SOURCE))
all_meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
meshes = [obj for obj in all_meshes if len(obj.data.materials) > 0 and len(obj.vertex_groups) > 0]
for helper in [obj for obj in all_meshes if obj not in meshes]:
    bpy.data.objects.remove(helper, do_unlink=True)
armatures = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
rig = armatures[0]


def triangles(objects):
    total = 0
    for obj in objects:
        obj.data.calc_loop_triangles()
        total += len(obj.data.loop_triangles)
    return total


before = triangles(meshes)
ratio = TARGET_TRIANGLES / float(before)
for obj in meshes:
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    modifier = obj.modifiers.new("Mobile_LOD0_Skinned_Decimate", "DECIMATE")
    modifier.decimate_type = "COLLAPSE"
    modifier.ratio = ratio
    modifier.use_collapse_triangulate = True
    while obj.modifiers.find(modifier.name) > 0:
        bpy.ops.object.modifier_move_up(modifier=modifier.name)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)

after = triangles(meshes)
weighted_vertices = sum(
    1 for obj in meshes for vertex in obj.data.vertices
    if any(group.weight > 0.0001 for group in vertex.groups)
)
total_vertices = sum(len(obj.data.vertices) for obj in meshes)

bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_OUT), check_existing=False)
bpy.ops.object.select_all(action="DESELECT")
for obj in meshes:
    obj.select_set(True)
rig.select_set(True)
bpy.context.view_layer.objects.active = rig
bpy.ops.export_scene.fbx(
    filepath=str(FBX_OUT),
    use_selection=True,
    object_types={"MESH", "ARMATURE"},
    axis_forward="-Z",
    axis_up="Y",
    apply_scale_options="FBX_SCALE_ALL",
    add_leaf_bones=False,
    bake_anim=False,
    mesh_smooth_type="FACE",
    path_mode="RELATIVE",
    embed_textures=False,
)

report = {
    "asset_id": ASSET_ID,
    "source_triangles": before,
    "optimized_triangles": after,
    "target_triangles": TARGET_TRIANGLES,
    "mesh_objects": len(meshes),
    "bones": len(rig.data.bones),
    "bone_names": [bone.name for bone in rig.data.bones],
    "vertices": total_vertices,
    "weighted_vertices": weighted_vertices,
    "weight_coverage": weighted_vertices / max(1, total_vertices),
    "blend": str(BLEND_OUT.relative_to(ROOT)),
    "fbx": str(FBX_OUT.relative_to(ROOT)),
    "status": "Unity deformation review required",
}
REPORT_OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
