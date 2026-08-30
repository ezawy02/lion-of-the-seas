"""Render the rigged commander from the four horizontal cardinal directions."""

from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
OUT = ROOT / "ArtSource/Blender/Characters/L03-CHR-001_Storm_Fortress_Commander/CardinalDiagnostics"
OUT.mkdir(parents=True, exist_ok=True)

for obj in list(bpy.context.scene.objects):
    if obj.type in {"CAMERA", "LIGHT"}:
        bpy.data.objects.remove(obj, do_unlink=True)

meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH" and not obj.hide_render]
points = [obj.matrix_world @ vertex.co for obj in meshes for vertex in obj.data.vertices]
low = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
high = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
center = (low + high) * 0.5
size = max(high.x - low.x, high.y - low.y, high.z - low.z)

bpy.ops.object.light_add(type="AREA", location=center + Vector((size, -size, size * 1.5)))
bpy.context.object.data.energy = 1400
bpy.context.object.data.size = size * 1.5
bpy.ops.object.light_add(type="AREA", location=center + Vector((-size, size, size)))
bpy.context.object.data.energy = 800
bpy.context.object.data.size = size

scene = bpy.context.scene
scene.render.engine = "BLENDER_EEVEE"
scene.render.resolution_x = 512
scene.render.resolution_y = 768
scene.render.resolution_percentage = 100
scene.render.image_settings.file_format = "PNG"
scene.render.film_transparent = True

views = {
    "Camera_From_PosX": Vector((1, 0, 0)),
    "Camera_From_NegX": Vector((-1, 0, 0)),
    "Camera_From_PosY": Vector((0, 1, 0)),
    "Camera_From_NegY": Vector((0, -1, 0)),
}
for name, direction in views.items():
    bpy.ops.object.camera_add(location=center + direction * size * 2.4 + Vector((0, 0, size * 0.08)))
    camera = bpy.context.object
    camera.rotation_euler = (center - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.lens = 60
    scene.camera = camera
    scene.render.filepath = str(OUT / f"{name}.png")
    bpy.ops.render.render(write_still=True)
    bpy.data.objects.remove(camera, do_unlink=True)
