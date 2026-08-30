"""Export only the missing aft lateen rig and helm from the R9 review source."""

from __future__ import annotations

import json
from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Blender/Revisions/L01-SHP-004_R9_TWO_LATEEN_HELM_REVIEW/L01-SHP-004_R9_TWO_LATEEN_HELM_REVIEW.blend"
OUTPUT = ROOT / "Assets/_Project/Art/Ships/L01-SHP-004_R9_AftLateen_Helm_Addon_REVIEW.fbx"
REPORT = ROOT / "ArtSource/Blender/Revisions/L01-SHP-004_R9_TWO_LATEEN_HELM_REVIEW/L01-SHP-004_R9_Addon_Report.json"

bpy.ops.wm.open_mainfile(filepath=str(SOURCE))

keep = []
for obj in list(bpy.context.scene.objects):
    if obj.type != "MESH":
        bpy.data.objects.remove(obj, do_unlink=True)
        continue
    if obj.name.startswith("REV_R1__Aft") or obj.name.startswith("REV_R9__Helm"):
        keep.append(obj)
    else:
        bpy.data.objects.remove(obj, do_unlink=True)

if not keep:
    raise RuntimeError("R9 aft-rig and helm objects were not found.")

bpy.ops.object.select_all(action="DESELECT")
for obj in keep:
    obj.select_set(True)
bpy.context.view_layer.objects.active = keep[0]
OUTPUT.parent.mkdir(parents=True, exist_ok=True)
bpy.ops.export_scene.fbx(
    filepath=str(OUTPUT),
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

triangles = 0
for obj in keep:
    obj.data.calc_loop_triangles()
    triangles += len(obj.data.loop_triangles)

report = {
    "source": str(SOURCE.relative_to(ROOT)),
    "output": str(OUTPUT.relative_to(ROOT)),
    "objects": sorted(obj.name for obj in keep),
    "triangles": triangles,
    "scope": "second lateen rig and helm only; no replacement hull",
    "status": "Unity and user visual review required",
}
REPORT.write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
