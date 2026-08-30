"""Create a non-destructive R8 gameplay-camera proportion pass from the user's R7 ship."""

from __future__ import annotations

from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
REVISION_DIR = ROOT / "ArtSource/Blender/Revisions/L01-SHP-004_R8_REFERENCE_MATCH_REVIEW"
OUTPUT_BLEND = REVISION_DIR / "L01-SHP-004_R8_REFERENCE_MATCH_REVIEW.blend"
OUTPUT_FBX = ROOT / "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_R8_ReferenceMatch_REVIEW.fbx"
DECK_HEIGHT = 0.57
RIG_VERTICAL_SCALE = 0.82
HULL_TRIANGLE_TARGET = 18000
TOTAL_TRIANGLE_BUDGET = 35000


def triangle_count(obj: bpy.types.Object) -> int:
    if obj.type != "MESH":
        return 0
    return sum(max(0, len(poly.vertices) - 2) for poly in obj.data.polygons)


def is_rig_object(obj: bpy.types.Object) -> bool:
    return obj.name.startswith("REV_R1__") or obj.name in {"REV_R1__ForeStay", "REV_R1__AftStay"}


REVISION_DIR.mkdir(parents=True, exist_ok=True)
OUTPUT_FBX.parent.mkdir(parents=True, exist_ok=True)

# Compress the authored rig around the deck line. The hull, lion relief, stern castle,
# deck, windows and gunwale remain byte-for-byte untouched in this revision step.
bpy.ops.object.select_all(action="DESELECT")
rig_objects = [obj for obj in bpy.context.scene.objects if is_rig_object(obj)]
for obj in rig_objects:
    obj.select_set(True)
bpy.context.view_layer.objects.active = rig_objects[0]
bpy.context.scene.cursor.location = (0.0, 0.0, DECK_HEIGHT)
bpy.context.scene.tool_settings.transform_pivot_point = "CURSOR"
bpy.ops.transform.resize(value=(1.0, 1.0, RIG_VERTICAL_SCALE), orient_type="GLOBAL")
bpy.context.scene.tool_settings.transform_pivot_point = "MEDIAN_POINT"

# Convert authored ropes to meshes and apply only the revision objects' modifiers.
for obj in list(bpy.context.scene.objects):
    if obj.type == "CURVE" and is_rig_object(obj):
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.convert(target="MESH")

for obj in [item for item in bpy.context.scene.objects if item.type == "MESH" and is_rig_object(item)]:
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    for modifier in list(obj.modifiers):
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)

hull = bpy.data.objects.get("USER_SOURCE__Hull_And_Details_Preserved")
if hull is None:
    raise RuntimeError("The preserved user hull is missing from the R7 source revision.")

hull_triangles = triangle_count(hull)
if hull_triangles > HULL_TRIANGLE_TARGET:
    modifier = hull.modifiers.new("Unity_Mobile_Triangle_Budget", "DECIMATE")
    modifier.decimate_type = "COLLAPSE"
    modifier.ratio = HULL_TRIANGLE_TARGET / hull_triangles
    modifier.use_collapse_triangulate = True
    bpy.context.view_layer.objects.active = hull
    hull.select_set(True)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    hull.select_set(False)

mesh_objects = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
total_triangles = sum(triangle_count(obj) for obj in mesh_objects)
if total_triangles > TOTAL_TRIANGLE_BUDGET:
    raise RuntimeError(f"R8 export is {total_triangles} triangles; budget is {TOTAL_TRIANGLE_BUDGET}.")

bpy.ops.object.select_all(action="DESELECT")
for obj in mesh_objects:
    obj.select_set(True)
bpy.context.view_layer.objects.active = hull
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
print(f"R8_BLEND={OUTPUT_BLEND}")
print(f"R8_FBX={OUTPUT_FBX}")
print(f"R8_RIG_VERTICAL_SCALE={RIG_VERTICAL_SCALE}")
print(f"R8_TRIANGLES={total_triangles}")
