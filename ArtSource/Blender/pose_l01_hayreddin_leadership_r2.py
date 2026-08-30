"""Create a non-destructive leadership pose from the user's rigged Hayreddin asset."""

from __future__ import annotations

import json
import math
from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Assets/_Project/Art/Characters/L01-CHR-001_Hayreddin_Barbarossa_Rigged_Optimized.fbx"
REVISION = ROOT / "ArtSource/Blender/Characters/L01-CHR-001_Hayreddin_Barbarossa/R2_LeadershipPose_REVIEW"
BLEND_OUT = REVISION / "L01-CHR-001_Hayreddin_Barbarossa_R2_LeadershipPose_REVIEW.blend"
FBX_OUT = ROOT / "Assets/_Project/Art/Characters/L01-CHR-001_Hayreddin_Barbarossa_Rigged_Optimized_R2_LeadershipPose_REVIEW.fbx"
REPORT_OUT = REVISION / "L01-CHR-001_Hayreddin_Barbarossa_R2_LeadershipPose_Report.json"


def triangles(meshes: list[bpy.types.Object]) -> int:
    total = 0
    for mesh in meshes:
        mesh.data.calc_loop_triangles()
        total += len(mesh.data.loop_triangles)
    return total


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(SOURCE))

rigs = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
if len(rigs) != 1 or not meshes:
    raise RuntimeError("Expected one Hayreddin armature and at least one skinned mesh.")

rig = rigs[0]
required = {"chest", "upper_arm_R", "lower_arm_R", "hand_R", "upper_arm_L", "lower_arm_L"}
missing = sorted(required.difference(rig.pose.bones.keys()))
if missing:
    raise RuntimeError(f"Hayreddin rig is missing bones: {missing}")

# The source character faces local -Y. Rotate the existing right arm forward at
# shoulder height to reproduce the command gesture without replacing the mesh.
right_upper = rig.pose.bones["upper_arm_R"]
right_upper.rotation_mode = "XYZ"
right_upper.rotation_euler.x = math.radians(-82.0)
right_upper.rotation_euler.z = math.radians(-4.0)

right_lower = rig.pose.bones["lower_arm_R"]
right_lower.rotation_mode = "XYZ"
right_lower.rotation_euler.x = math.radians(-5.0)
right_lower.rotation_euler.z = math.radians(5.0)

right_hand = rig.pose.bones["hand_R"]
right_hand.rotation_mode = "XYZ"
right_hand.rotation_euler.x = math.radians(6.0)

# Keep the left hand lower and slightly forward toward the helm.
left_upper = rig.pose.bones["upper_arm_L"]
left_upper.rotation_mode = "XYZ"
left_upper.rotation_euler.x = math.radians(-18.0)
left_upper.rotation_euler.z = math.radians(7.0)
left_lower = rig.pose.bones["lower_arm_L"]
left_lower.rotation_mode = "XYZ"
left_lower.rotation_euler.x = math.radians(-22.0)

bpy.context.view_layer.objects.active = rig
bpy.ops.object.mode_set(mode="POSE")
bpy.ops.pose.armature_apply(selected=False)
bpy.ops.object.mode_set(mode="OBJECT")
bpy.context.view_layer.update()

REVISION.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_OUT), check_existing=False)

bpy.ops.object.select_all(action="DESELECT")
for mesh in meshes:
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
    "source": str(SOURCE.relative_to(ROOT)),
    "revision": "R2 Leadership Pose REVIEW",
    "mesh_objects": len(meshes),
    "triangles": triangles(meshes),
    "bones": len(rig.data.bones),
    "preserves_user_mesh": True,
    "changes": [
        "right arm raised forward for the command gesture",
        "left arm shifted slightly toward the helm",
        "pose applied as the new bind pose for static Unity presentation",
    ],
    "blend": str(BLEND_OUT.relative_to(ROOT)),
    "fbx": str(FBX_OUT.relative_to(ROOT)),
    "status": "Unity and user visual review required",
}
REPORT_OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
