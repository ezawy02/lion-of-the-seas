"""Create a local humanoid review rig for the optimized fortress commander.

The source mesh is preserved. This produces a separate review candidate and does not
claim animation approval; deformation still requires in-Unity user review.
"""

from pathlib import Path
import json

import bpy
from mathutils import Matrix, Vector


ROOT = Path(__file__).resolve().parents[2]
ASSET_ID = "L03-CHR-001_Storm_Fortress_Commander"
FOLDER = ROOT / "ArtSource/Blender/Characters" / ASSET_ID
SOURCE = FOLDER / f"{ASSET_ID}_Optimized_REVIEW.blend"
BLEND_OUT = FOLDER / f"{ASSET_ID}_Rigged_Optimized_REVIEW.blend"
FBX_OUT = ROOT / "Assets/_Project/Art/Characters" / f"{ASSET_ID}_Rigged_Optimized_REVIEW.fbx"
REPORT_OUT = FOLDER / f"{ASSET_ID}_Rig_Report.json"


bpy.ops.wm.open_mainfile(filepath=str(SOURCE))
mesh = next(obj for obj in bpy.context.scene.objects if obj.type == "MESH")

# Move the bind-pose foot contact to Z=0 while preserving the exact mesh shape.
points = [mesh.matrix_world @ Vector(corner) for corner in mesh.bound_box]
low_z = min(point.z for point in points)
mesh.data.transform(Matrix.Translation((0.0, 0.0, -low_z)))
mesh.data.update()

bpy.ops.object.armature_add(enter_editmode=True, location=(0, 0, 0))
rig = bpy.context.object
rig.name = ASSET_ID + "_Rig"
rig.data.name = rig.name + "_Armature"
rig.show_in_front = True

edit_bones = rig.data.edit_bones
edit_bones.remove(edit_bones[0])


def add_bone(name, head, tail, parent=None, connected=False):
    bone = edit_bones.new(name)
    bone.head = head
    bone.tail = tail
    if parent:
        bone.parent = edit_bones[parent]
        bone.use_connect = connected
    return bone


add_bone("Root", (0, 0, 0.00), (0, 0, 0.10))
add_bone("Hips", (0, 0, 0.31), (0, 0, 0.43), "Root")
add_bone("Spine", (0, 0, 0.43), (0, 0, 0.56), "Hips", True)
add_bone("Spine1", (0, 0, 0.56), (0, 0, 0.68), "Spine", True)
add_bone("Neck", (0, 0, 0.68), (0, 0, 0.76), "Spine1", True)
add_bone("Head", (0, 0, 0.76), (0, 0, 0.94), "Neck", True)

for side, sign in (("Left", 1), ("Right", -1)):
    add_bone(side + "Shoulder", (sign * 0.015, 0, 0.66), (sign * 0.080, 0, 0.65), "Spine1")
    add_bone(side + "Arm", (sign * 0.080, 0, 0.65), (sign * 0.118, 0, 0.53), side + "Shoulder", True)
    add_bone(side + "ForeArm", (sign * 0.118, 0, 0.53), (sign * 0.120, 0, 0.40), side + "Arm", True)
    add_bone(side + "Hand", (sign * 0.120, 0, 0.40), (sign * 0.118, 0, 0.33), side + "ForeArm", True)
    add_bone(side + "UpLeg", (sign * 0.055, 0, 0.34), (sign * 0.055, 0, 0.20), "Hips")
    add_bone(side + "Leg", (sign * 0.055, 0, 0.20), (sign * 0.055, 0, 0.075), side + "UpLeg", True)
    add_bone(side + "Foot", (sign * 0.055, 0, 0.075), (sign * 0.055, -0.055, 0.025), side + "Leg", True)
    add_bone(side + "ToeBase", (sign * 0.055, -0.055, 0.025), (sign * 0.055, -0.105, 0.025), side + "Foot", True)

bpy.ops.object.mode_set(mode="OBJECT")

# Bind without changing the pose. Proximity weights are used because generated meshes
# can be non-manifold and Blender's heat solver may return no weights at all.
mesh.parent = rig
mesh.matrix_parent_inverse = rig.matrix_world.inverted()
modifier = mesh.modifiers.new("Commander_Humanoid_Rig", "ARMATURE")
modifier.object = rig


def distance_to_segment(point, head, tail):
    segment = tail - head
    length_squared = segment.length_squared
    if length_squared == 0:
        return (point - head).length
    factor = max(0.0, min(1.0, (point - head).dot(segment) / length_squared))
    return (point - (head + segment * factor)).length


deform_bones = [bone for bone in rig.data.bones if bone.name != "Root"]
groups = {bone.name: mesh.vertex_groups.new(name=bone.name) for bone in deform_bones}
for vertex in mesh.data.vertices:
    point = vertex.co
    candidates = []
    for bone in deform_bones:
        distance = distance_to_segment(point, bone.head_local, bone.tail_local)
        # Keep vertices on each body side from leaking into the opposite limb chain.
        if bone.name.startswith("Left") and point.x < -0.015:
            distance *= 2.5
        elif bone.name.startswith("Right") and point.x > 0.015:
            distance *= 2.5
        candidates.append((distance, bone.name))
    closest = sorted(candidates)[:4]
    raw = [(1.0 / ((distance + 0.018) ** 3), name) for distance, name in closest]
    total = sum(weight for weight, _ in raw)
    for weight, name in raw:
        groups[name].add([vertex.index], weight / total, "REPLACE")

weighted_vertices = 0
for vertex in mesh.data.vertices:
    if any(group.weight > 0.0001 for group in vertex.groups):
        weighted_vertices += 1

bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_OUT), check_existing=False)
bpy.ops.object.select_all(action="DESELECT")
mesh.select_set(True)
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
    "bones": len(rig.data.bones),
    "vertices": len(mesh.data.vertices),
    "weighted_vertices": weighted_vertices,
    "coverage": weighted_vertices / max(1, len(mesh.data.vertices)),
    "binding": "four-nearest-bone proximity weights",
    "blend": str(BLEND_OUT.relative_to(ROOT)),
    "fbx": str(FBX_OUT.relative_to(ROOT)),
    "status": "Unity deformation review required",
}
REPORT_OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
