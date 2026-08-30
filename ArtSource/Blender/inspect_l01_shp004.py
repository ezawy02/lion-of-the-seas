"""Render diagnostic views of the authored L01-SHP-004 source without editing it."""

from __future__ import annotations

import math
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Blender/Incoming/level01_opening_match/4/base_basic_pbr.glb"
OUTPUT = ROOT / "ArtSource/Blender/Diagnostics/L01-SHP-004_Source"


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def bounds(objects: list[bpy.types.Object]) -> tuple[Vector, Vector]:
    corners = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    low = Vector((min(v.x for v in corners), min(v.y for v in corners), min(v.z for v in corners)))
    high = Vector((max(v.x for v in corners), max(v.y for v in corners), max(v.z for v in corners)))
    return low, high


def look_at(camera: bpy.types.Object, target: Vector) -> None:
    camera.rotation_euler = (target - camera.location).to_track_quat("-Z", "Y").to_euler()


clear_scene()
bpy.ops.import_scene.gltf(filepath=str(SOURCE))
meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
low, high = bounds(meshes)
center = (low + high) * 0.5
span = max(high.x - low.x, high.y - low.y, high.z - low.z)

world = bpy.context.scene.world
world.color = (0.035, 0.045, 0.06)
world.use_nodes = True
background = world.node_tree.nodes.get("Background")
background.inputs["Color"].default_value = (0.018, 0.025, 0.04, 1.0)
background.inputs["Strength"].default_value = 0.3

for location, energy, size in [
    ((-3.0, -4.0, 5.0), 1300.0, 4.0),
    ((4.0, 1.0, 3.0), 900.0, 3.0),
    ((0.0, 4.0, 5.0), 700.0, 3.0),
]:
    light_data = bpy.data.lights.new(name="Diagnostic_Area", type="AREA")
    light_data.energy = energy
    light_data.shape = "DISK"
    light_data.size = size
    light = bpy.data.objects.new("Diagnostic_Area", light_data)
    bpy.context.collection.objects.link(light)
    light.location = Vector(location) * span + center
    light.rotation_euler = (center - light.location).to_track_quat("-Z", "Y").to_euler()

camera_data = bpy.data.cameras.new("Diagnostic_Camera")
camera = bpy.data.objects.new("Diagnostic_Camera", camera_data)
bpy.context.collection.objects.link(camera)
bpy.context.scene.camera = camera
camera_data.lens = 58

scene = bpy.context.scene
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 1024
scene.render.resolution_y = 1024
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.film_transparent = False
scene.render.image_settings.color_mode = "RGBA"
scene.view_settings.look = "AgX - Medium High Contrast"

OUTPUT.mkdir(parents=True, exist_ok=True)
distance = span * 2.35
views = {
    "stern_three_quarter": Vector((1.35, -1.55, 0.75)),
    "port": Vector((-1.8, 0.0, 0.45)),
    "bow_three_quarter": Vector((-1.25, 1.55, 0.65)),
    "starboard": Vector((1.8, 0.0, 0.45)),
}

for name, direction in views.items():
    camera.location = center + direction.normalized() * distance
    look_at(camera, center + Vector((0.0, 0.0, span * 0.08)))
    scene.render.filepath = str(OUTPUT / f"{name}.png")
    bpy.ops.render.render(write_still=True)

bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT / "L01-SHP-004_SourceInspection.blend"))
print(f"L01_SHP004_DIAGNOSTICS={OUTPUT}")
