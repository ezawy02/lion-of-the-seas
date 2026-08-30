import json
import math
import sys
from pathlib import Path

import bpy
from mathutils import Vector


def reset_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.materials, bpy.data.images,
                       bpy.data.cameras, bpy.data.lights):
        for block in list(datablocks):
            if block.users == 0:
                datablocks.remove(block)


def bounds(objects):
    points = []
    for obj in objects:
        if obj.type != "MESH":
            continue
        points.extend(obj.matrix_world @ Vector(corner) for corner in obj.bound_box)
    minimum = Vector((min(p.x for p in points), min(p.y for p in points), min(p.z for p in points)))
    maximum = Vector((max(p.x for p in points), max(p.y for p in points), max(p.z for p in points)))
    return minimum, maximum


def aim(obj, target):
    obj.rotation_euler = ((Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler())


def setup_render(meshes, output):
    minimum, maximum = bounds(meshes)
    center = (minimum + maximum) * 0.5
    size = maximum - minimum
    radius = max(size.x, size.y, size.z) * 0.72

    camera_data = bpy.data.cameras.new("ReviewCamera")
    camera = bpy.data.objects.new("ReviewCamera", camera_data)
    bpy.context.scene.collection.objects.link(camera)
    camera.location = center + Vector((radius * 1.5, -radius * 2.2, radius * 1.2))
    camera_data.lens = 52
    aim(camera, center)
    bpy.context.scene.camera = camera

    for name, location, energy, size_value in (
        ("Key", center + Vector((-radius, -radius * 1.5, radius * 2.2)), 1300, radius),
        ("Fill", center + Vector((radius * 2, -radius, radius)), 700, radius * 1.5),
        ("Rim", center + Vector((0, radius * 2, radius * 1.8)), 900, radius),
    ):
        data = bpy.data.lights.new(name, "AREA")
        data.energy = energy
        data.shape = "DISK"
        data.size = max(size_value, 1.0)
        light = bpy.data.objects.new(name, data)
        light.location = location
        aim(light, center)
        bpy.context.scene.collection.objects.link(light)

    world = bpy.context.scene.world or bpy.data.worlds.new("ReviewWorld")
    bpy.context.scene.world = world
    world.color = (0.025, 0.03, 0.04)
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.render.filepath = str(output)
    scene.view_settings.look = "AgX - Medium High Contrast"
    bpy.ops.render.render(write_still=True)
    return minimum, maximum


def inspect(glb_path, output_dir):
    reset_scene()
    bpy.ops.import_scene.gltf(filepath=str(glb_path))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    minimum, maximum = setup_render(meshes, output_dir / f"{glb_path.parent.name}.png")
    triangles = sum(len(poly.vertices) - 2 for obj in meshes for poly in obj.data.polygons)
    vertices = sum(len(obj.data.vertices) for obj in meshes)
    materials = sorted({slot.material.name for obj in meshes for slot in obj.material_slots if slot.material})
    armatures = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
    return {
        "uuid": glb_path.parent.name,
        "source": str(glb_path),
        "mesh_objects": len(meshes),
        "vertices": vertices,
        "triangles": triangles,
        "materials": materials,
        "material_count": len(materials),
        "armatures": len(armatures),
        "bones": sum(len(obj.data.bones) for obj in armatures),
        "bounds_min": list(minimum),
        "bounds_max": list(maximum),
        "dimensions": list(maximum - minimum),
    }


def main():
    root = Path(sys.argv[sys.argv.index("--") + 1]).resolve()
    output_dir = root / "Review"
    output_dir.mkdir(exist_ok=True)
    results = []
    for glb_path in sorted(root.glob("*/base_basic_pbr.glb")):
        results.append(inspect(glb_path, output_dir))
    (output_dir / "inspection.json").write_text(json.dumps(results, indent=2), encoding="utf-8")


main()
