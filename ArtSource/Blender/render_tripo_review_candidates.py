"""Render consistent inspection images for Tripo review candidates."""

from pathlib import Path
import math

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
ASSETS = [
    ("Ships", "L02-SHP-001_Armored_Warship_Boss"),
    ("Environment", "L02-PRP-001_Floating_Naval_Mine"),
    ("Environment", "L02-PRP-002_Heavy_Chain_Link_Unit"),
    ("Ships", "L03-SHP-001_Gunpowder_Skiff"),
    ("Environment", "L03-PRP-001_Gunpowder_Barrel_Cluster"),
    ("Characters", "L03-CHR-001_Storm_Fortress_Commander"),
]


def bounds(objects):
    points = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    low = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
    high = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
    return low, high


def aim_at(obj, point):
    obj.rotation_euler = (point - obj.location).to_track_quat("-Z", "Y").to_euler()


for category, asset_id in ASSETS:
    folder = ROOT / "ArtSource/Blender" / category / asset_id
    blend = folder / f"{asset_id}_Optimized_REVIEW.blend"
    bpy.ops.wm.open_mainfile(filepath=str(blend))

    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    low, high = bounds(meshes)
    center = (low + high) * 0.5
    size = high - low
    radius = max(size.length * 0.5, 0.1)

    bpy.ops.object.camera_add()
    camera = bpy.context.object
    camera.data.lens = 55
    camera_direction = Vector((1.8, -0.55, 0.9)) if category == "Ships" else Vector((1.35, -1.75, 1.05))
    camera.location = center + camera_direction.normalized() * radius * 3.25
    aim_at(camera, center + Vector((0, 0, size.z * 0.03)))

    bpy.ops.object.light_add(type="AREA", location=center + Vector((-radius, -radius * 1.6, radius * 2.2)))
    key = bpy.context.object
    key.data.energy = 130
    key.data.shape = "DISK"
    key.data.size = radius * 2.5
    aim_at(key, center)

    bpy.ops.object.light_add(type="AREA", location=center + Vector((radius * 1.8, -radius * 0.4, radius * 0.8)))
    fill = bpy.context.object
    fill.data.energy = 55
    fill.data.size = radius * 2.0
    aim_at(fill, center)

    bpy.ops.object.light_add(type="AREA", location=center + Vector((0, radius * 1.8, radius * 1.5)))
    rim = bpy.context.object
    rim.data.energy = 100
    rim.data.size = radius * 1.6
    aim_at(rim, center)

    scene = bpy.context.scene
    if scene.world is None:
        scene.world = bpy.data.worlds.new("ReviewWorld")
    scene.world.color = (0.025, 0.025, 0.025)
    scene.camera = camera
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = True
    scene.render.filepath = str(folder / f"{asset_id}_Optimized_REVIEW_Render.png")
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.render.image_settings.color_mode = "RGBA"
    bpy.ops.render.render(write_still=True)
    print(f"Rendered {asset_id}")
