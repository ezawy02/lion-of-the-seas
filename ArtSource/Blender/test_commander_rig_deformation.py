"""Render bind and stress poses for the commander review rig."""

from pathlib import Path
from math import radians

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
ASSET_ID = "L03-CHR-001_Storm_Fortress_Commander"
FOLDER = ROOT / "ArtSource/Blender/Characters" / ASSET_ID
SOURCE = FOLDER / f"{ASSET_ID}_Rigged_Optimized_REVIEW.blend"
bpy.ops.wm.open_mainfile(filepath=str(SOURCE))

mesh = next(obj for obj in bpy.context.scene.objects if obj.type == "MESH")
rig = next(obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE")
points = [mesh.matrix_world @ Vector(corner) for corner in mesh.bound_box]
low = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
high = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
center = (low + high) * 0.5
radius = max((high - low).length * 0.5, 0.1)


def aim_at(obj, point):
    obj.rotation_euler = (point - obj.location).to_track_quat("-Z", "Y").to_euler()


bpy.ops.object.camera_add(location=center + Vector((1.35, -1.75, 1.05)).normalized() * radius * 3.25)
camera = bpy.context.object
camera.data.lens = 55
aim_at(camera, center)

for location, energy, size in (
    (center + Vector((-radius, -radius * 1.6, radius * 2.2)), 130, radius * 2.5),
    (center + Vector((radius * 1.8, -radius * 0.4, radius * 0.8)), 55, radius * 2.0),
    (center + Vector((0, radius * 1.8, radius * 1.5)), 100, radius * 1.6),
):
    bpy.ops.object.light_add(type="AREA", location=location)
    light = bpy.context.object
    light.data.energy = energy
    light.data.size = size
    aim_at(light, center)

scene = bpy.context.scene
scene.camera = camera
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 768
scene.render.resolution_y = 768
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.image_settings.color_mode = "RGBA"
scene.render.film_transparent = True
scene.world.color = (0.025, 0.025, 0.025)
scene.view_settings.look = "AgX - Medium High Contrast"

scene.render.filepath = str(FOLDER / f"{ASSET_ID}_Rig_BindPose_REVIEW.png")
bpy.ops.render.render(write_still=True)

# Moderate stress pose: shoulders, elbows and legs all move away from bind pose.
for name, angles in {
    "LeftArm": (0, radians(-28), radians(12)),
    "LeftForeArm": (radians(-18), 0, radians(-25)),
    "RightArm": (0, radians(18), radians(-10)),
    "RightForeArm": (radians(22), 0, radians(22)),
    "LeftUpLeg": (radians(-10), 0, radians(5)),
    "LeftLeg": (radians(18), 0, 0),
    "RightUpLeg": (radians(8), 0, radians(-4)),
}.items():
    bone = rig.pose.bones[name]
    bone.rotation_mode = "XYZ"
    bone.rotation_euler = angles

bpy.context.view_layer.update()
scene.render.filepath = str(FOLDER / f"{ASSET_ID}_Rig_StressPose_REVIEW.png")
bpy.ops.render.render(write_still=True)
print("Rendered commander rig bind and stress poses")
