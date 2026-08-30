"""Create the Level 01 R9 two-lateen-and-helm review candidate."""

from __future__ import annotations

import json
import math
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Blender/Revisions/L01-SHP-004_R8_REFERENCE_MATCH_REVIEW/L01-SHP-004_R8_REFERENCE_MATCH_REVIEW.blend"
REVISION = ROOT / "ArtSource/Blender/Revisions/L01-SHP-004_R9_TWO_LATEEN_HELM_REVIEW"
BLEND_OUT = REVISION / "L01-SHP-004_R9_TWO_LATEEN_HELM_REVIEW.blend"
FBX_OUT = ROOT / "Assets/_Project/Art/Ships/L01-SHP-004_Hero_Flagship_R9_TwoLateenHelm_REVIEW.fbx"
REPORT_OUT = REVISION / "L01-SHP-004_R9_TwoLateenHelm_Report.json"


def triangles(obj: bpy.types.Object) -> int:
    if obj.type != "MESH":
        return 0
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def cylinder_between(name: str, start: Vector, end: Vector, radius: float) -> bpy.types.Object:
    midpoint = (start + end) * 0.5
    direction = end - start
    bpy.ops.mesh.primitive_cylinder_add(vertices=10, radius=radius, depth=direction.length, location=midpoint)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    return obj


if not SOURCE.exists():
    raise FileNotFoundError(SOURCE)

bpy.ops.wm.open_mainfile(filepath=str(SOURCE))

# The reference contract calls for two large lateen sails. R8 contains three
# authored groups; remove only the complete fore group and its dedicated stay.
removed = []
for obj in list(bpy.context.scene.objects):
    if obj.name.startswith("REV_R1__Fore__") or obj.name == "REV_R1__ForeStay":
        removed.append(obj.name)
        bpy.data.objects.remove(obj, do_unlink=True)

# Build a readable wooden helm at the rear-deck standing position. The source
# ship points toward local -Y, so the stern is local +Y and the wheel lies in XZ.
wheel_center = Vector((0.0, 0.56, 0.69))
bpy.ops.mesh.primitive_torus_add(
    major_radius=0.105,
    minor_radius=0.014,
    major_segments=24,
    minor_segments=8,
    location=wheel_center,
    rotation=(math.radians(90.0), 0.0, 0.0),
)
ring = bpy.context.object
ring.name = "REV_R9__HelmMastWood_Ring"

for index in range(8):
    angle = index * math.tau / 8.0
    direction = Vector((math.cos(angle), 0.0, math.sin(angle)))
    cylinder_between(
        f"REV_R9__HelmMastWood_Spoke_{index:02d}",
        wheel_center - direction * 0.122,
        wheel_center + direction * 0.122,
        0.006,
    )

bpy.ops.mesh.primitive_cylinder_add(
    vertices=16,
    radius=0.026,
    depth=0.075,
    location=wheel_center,
    rotation=(math.radians(90.0), 0.0, 0.0),
)
hub = bpy.context.object
hub.name = "REV_R9__HelmGold_Hub"

cylinder_between(
    "REV_R9__HelmMastWood_Stand",
    Vector((0.0, 0.59, 0.55)),
    Vector((0.0, 0.59, 0.70)),
    0.014,
)
cylinder_between(
    "REV_R9__HelmMastWood_Base",
    Vector((-0.08, 0.59, 0.54)),
    Vector((0.08, 0.59, 0.54)),
    0.012,
)

meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
total = sum(triangles(obj) for obj in meshes)
if total > 35000:
    raise RuntimeError(f"R9 has {total} triangles; mobile review budget is 35000.")

REVISION.mkdir(parents=True, exist_ok=True)
FBX_OUT.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_OUT), check_existing=False)

bpy.ops.object.select_all(action="DESELECT")
for obj in meshes:
    obj.select_set(True)
bpy.context.view_layer.objects.active = meshes[0]
bpy.ops.export_scene.fbx(
    filepath=str(FBX_OUT),
    use_selection=True,
    object_types={"MESH"},
    apply_unit_scale=True,
    apply_scale_options="FBX_SCALE_ALL",
    axis_forward="-Z",
    axis_up="Y",
    add_leaf_bones=False,
    bake_anim=False,
    path_mode="RELATIVE",
    embed_textures=False,
)

report = {
    "source": str(SOURCE.relative_to(ROOT)),
    "revision": "R9 Two Lateen + Helm REVIEW",
    "removed_fore_group": sorted(removed),
    "remaining_sails": sorted(obj.name for obj in meshes if "IvorySail" in obj.name and "Edge" not in obj.name and "Seam" not in obj.name),
    "helm_objects": sorted(obj.name for obj in meshes if "Helm" in obj.name),
    "triangles": total,
    "preserves_user_hull": bpy.data.objects.get("USER_SOURCE__Hull_And_Details_Preserved") is not None,
    "blend": str(BLEND_OUT.relative_to(ROOT)),
    "fbx": str(FBX_OUT.relative_to(ROOT)),
    "status": "Unity and user visual review required",
}
REPORT_OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
