"""Prepare the R7 sail-direction revision for a Unity review import."""

from __future__ import annotations

from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
OUTPUT_FBX = ROOT / "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_R7_Direction_Optimized.fbx"
OUTPUT_BLEND = ROOT / "ArtSource/Blender/Revisions/L01-SHP-004_Sails_R7_DIRECTION_PROTOTYPE/L01-SHP-004_R7_UnityExport.blend"
HULL_TRIANGLE_TARGET = 18000
TOTAL_TRIANGLE_BUDGET = 35000


def triangle_count(obj: bpy.types.Object) -> int:
    if obj.type != "MESH":
        return 0
    return sum(max(0, len(polygon.vertices) - 2) for polygon in obj.data.polygons)


def apply_all_modifiers(obj: bpy.types.Object) -> None:
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    for modifier in list(obj.modifiers):
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)


for scene_object in list(bpy.context.scene.objects):
    if scene_object.type == "CURVE":
        bpy.ops.object.select_all(action="DESELECT")
        scene_object.select_set(True)
        bpy.context.view_layer.objects.active = scene_object
        bpy.ops.object.convert(target="MESH")

mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
for mesh_object in mesh_objects:
    apply_all_modifiers(mesh_object)

hull = bpy.data.objects.get("USER_SOURCE__Hull_And_Details_Preserved")
if hull is None:
    raise RuntimeError("The preserved user hull was not found in the R7 revision.")

hull_triangles = triangle_count(hull)
if hull_triangles > HULL_TRIANGLE_TARGET:
    decimate = hull.modifiers.new("Unity_Mobile_Triangle_Budget", "DECIMATE")
    decimate.decimate_type = "COLLAPSE"
    decimate.ratio = HULL_TRIANGLE_TARGET / hull_triangles
    decimate.use_collapse_triangulate = True
    bpy.context.view_layer.objects.active = hull
    hull.select_set(True)
    bpy.ops.object.modifier_apply(modifier=decimate.name)
    hull.select_set(False)

mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
total_triangles = sum(triangle_count(obj) for obj in mesh_objects)
if total_triangles > TOTAL_TRIANGLE_BUDGET:
    raise RuntimeError(
        f"Unity export is {total_triangles} triangles; budget is {TOTAL_TRIANGLE_BUDGET}."
    )

bpy.ops.object.select_all(action="DESELECT")
for mesh_object in mesh_objects:
    mesh_object.select_set(True)
bpy.context.view_layer.objects.active = hull

OUTPUT_FBX.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.export_scene.fbx(
    filepath=str(OUTPUT_FBX),
    use_selection=True,
    object_types={"MESH"},
    apply_unit_scale=True,
    apply_scale_options="FBX_SCALE_ALL",
    axis_forward="-Z",
    axis_up="Y",
    add_leaf_bones=False,
    bake_anim=False,
    path_mode="COPY",
    embed_textures=True,
)
bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT_BLEND))

print(f"UNITY_FBX={OUTPUT_FBX}")
print(f"UNITY_BLEND={OUTPUT_BLEND}")
print(f"UNITY_TRIANGLES={total_triangles}")
