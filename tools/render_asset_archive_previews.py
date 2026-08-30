"""Render local GLB/FBX archive contents for visual intake review.

This helper is intended to run inside Blender in background mode. It imports one
model, frames it with an orthographic camera, writes a transparent preview, and
prints basic mesh/material statistics for the intake manifest.
"""

from __future__ import annotations

import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def argument(name: str) -> str:
    args = sys.argv[sys.argv.index("--") + 1 :]
    index = args.index(name)
    return args[index + 1]


def optional_argument(name: str, default: str) -> str:
    args = sys.argv[sys.argv.index("--") + 1 :]
    if name not in args:
        return default
    return args[args.index(name) + 1]


def import_model(path: Path) -> None:
    suffix = path.suffix.lower()
    if suffix == ".glb" or suffix == ".gltf":
        bpy.ops.import_scene.gltf(filepath=str(path))
    elif suffix == ".fbx":
        bpy.ops.import_scene.fbx(filepath=str(path))
    else:
        raise ValueError(f"Unsupported model format: {suffix}")


def world_bounds(objects: list[bpy.types.Object]) -> tuple[Vector, Vector]:
    points = [obj.matrix_world @ Vector(corner) for obj in objects for corner in obj.bound_box]
    return (
        Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points))),
        Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points))),
    )


def main() -> None:
    source = Path(argument("--input")).resolve()
    output = Path(argument("--output")).resolve()
    view = optional_argument("--view", "isometric")
    output.parent.mkdir(parents=True, exist_ok=True)

    bpy.ops.wm.read_factory_settings(use_empty=True)
    import_model(source)
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    if not meshes:
        raise RuntimeError(f"No meshes found in {source}")

    minimum, maximum = world_bounds(meshes)
    center = (minimum + maximum) * 0.5
    size = maximum - minimum
    radius = max(size.x, size.y, size.z) * 0.65

    camera_data = bpy.data.cameras.new("IntakeCamera")
    camera = bpy.data.objects.new("IntakeCamera", camera_data)
    bpy.context.scene.collection.objects.link(camera)
    view_offsets = {
        "front": Vector((0, -radius * 3.0, radius * 0.35)),
        "left": Vector((-radius * 3.0, 0, radius * 0.35)),
        "back": Vector((0, radius * 3.0, radius * 0.35)),
        "right": Vector((radius * 3.0, 0, radius * 0.35)),
        "isometric": Vector((radius * 1.8, -radius * 2.4, radius * 1.5)),
    }
    if view not in view_offsets:
        raise ValueError(f"Unsupported view: {view}")
    camera.location = center + view_offsets[view]
    direction = center - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()
    camera_data.type = "ORTHO"
    camera_data.ortho_scale = radius * 2.5
    bpy.context.scene.camera = camera

    world = bpy.data.worlds.new("IntakeWorld")
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.045, 0.055, 0.075, 1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.35
    bpy.context.scene.world = world

    for energy, rotation in ((1800, (math.radians(50), 0, math.radians(35))), (900, (math.radians(65), 0, math.radians(-120)))):
        light_data = bpy.data.lights.new("IntakeLight", "AREA")
        light_data.energy = energy
        light_data.shape = "DISK"
        light_data.size = radius * 2.0
        light = bpy.data.objects.new("IntakeLight", light_data)
        bpy.context.scene.collection.objects.link(light)
        light.location = center + Vector((radius * 1.5, -radius * 1.5, radius * 2.0))
        light.rotation_euler = rotation

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 640
    scene.render.resolution_y = 640
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = True
    scene.render.filepath = str(output)
    scene.view_settings.look = "AgX - Medium High Contrast"
    bpy.ops.render.render(write_still=True)

    for mesh in meshes:
        mesh.data.calc_loop_triangles()
    triangles = sum(len(mesh.data.loop_triangles) for mesh in meshes)
    vertices = sum(len(mesh.data.vertices) for mesh in meshes)
    materials = {slot.material.name for mesh in meshes for slot in mesh.material_slots if slot.material}
    armatures = sum(1 for obj in bpy.context.scene.objects if obj.type == "ARMATURE")
    print(
        "INTAKE_STATS"
        f"|file={source.name}|meshes={len(meshes)}|vertices={vertices}"
        f"|triangles={triangles}|materials={len(materials)}|armatures={armatures}"
        f"|size={size.x:.4f},{size.y:.4f},{size.z:.4f}"
    )


if __name__ == "__main__":
    main()
