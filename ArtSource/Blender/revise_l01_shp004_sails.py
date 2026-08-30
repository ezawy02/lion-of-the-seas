"""Non-destructive sail and mast revision for the user's L01-SHP-004 source model."""

from __future__ import annotations

import math
from pathlib import Path

import bpy
import numpy as np
from mathutils import Matrix, Vector


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "ArtSource/Blender/Incoming/level01_opening_match/4/base_basic_pbr.glb"
OUTPUT = ROOT / "ArtSource/Blender/Revisions/L01-SHP-004_Sails_R7_DIRECTION_PROTOTYPE"


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def make_material(name: str, color: tuple[float, float, float, float], roughness: float, metallic: float = 0.0):
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = color
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Metallic"].default_value = metallic
    return material


def world_bounds(obj: bpy.types.Object) -> tuple[Vector, Vector]:
    coords = [obj.matrix_world @ vertex.co for vertex in obj.data.vertices]
    low = Vector(tuple(min(vertex[i] for vertex in coords) for i in range(3)))
    high = Vector(tuple(max(vertex[i] for vertex in coords) for i in range(3)))
    return low, high


def average_texture_color(obj: bpy.types.Object, pixels: np.ndarray) -> np.ndarray:
    uv_layer = obj.data.uv_layers.active
    if not uv_layer or not obj.data.loops:
        return np.zeros(3)
    step = max(1, len(obj.data.loops) // 96)
    colors = []
    height, width = pixels.shape[:2]
    for loop_index in range(0, len(obj.data.loops), step):
        uv = uv_layer.data[loop_index].uv
        x = min(width - 1, max(0, int((uv.x % 1.0) * (width - 1))))
        y = min(height - 1, max(0, int((uv.y % 1.0) * (height - 1))))
        colors.append(pixels[y, x, :3])
    return np.mean(colors, axis=0) if colors else np.zeros(3)


def remove_damaged_sails(source: bpy.types.Object) -> int:
    bpy.context.view_layer.objects.active = source
    source.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.separate(type="LOOSE")
    bpy.ops.object.mode_set(mode="OBJECT")

    image = bpy.data.images.get("texture_diffuse")
    width, height = image.size
    pixels = np.array(image.pixels[:], dtype=np.float32).reshape(height, width, 4)
    removed = []
    preserved = []
    for obj in [item for item in bpy.context.scene.objects if item.type == "MESH"]:
        low, high = world_bounds(obj)
        dims = high - low
        center = (low + high) * 0.5
        color = average_texture_color(obj, pixels)
        luminance = float(np.mean(color))
        saturation = float(np.max(color) - np.min(color))
        # The generated source fused the old sails, yards and mast ornaments into more
        # than a thousand loose islands. Remove only the high central rig envelope;
        # the user's stern castle, lion relief, painted hull and deck stay untouched.
        inside_rig = center.y > -0.48 and high.z > 0.84 and abs(center.x) < 0.39
        detached_from_hull = low.z > 0.55 or dims.z > 0.25
        broad_fragment = dims.z > 0.07 and max(dims.x, dims.y) > 0.045
        pale_canvas = luminance > 0.30 and saturation < 0.34
        bow_canvas_fragment = center.y > 0.10 and high.z > 0.58 and low.z > 0.42 and broad_fragment and pale_canvas
        # R3 exposed several detached, torn islands whose sampled texture was too
        # saturated to be classified as canvas. They all sit above the deck inside
        # the old rig envelope, so remove that floating geometry by position. Hull,
        # stern castle and lion relief remain below this cutoff.
        pale_rig_fragment = (
            center.y > -0.62
            and low.z > 0.34
            and high.z > 0.46
            and abs(center.x) < 0.72
            and luminance > 0.24
            and saturation < 0.46
        )
        if (inside_rig and detached_from_hull) or bow_canvas_fragment or pale_rig_fragment:
            removed.append(obj)
        else:
            preserved.append(obj)

    for obj in removed:
        bpy.data.objects.remove(obj, do_unlink=True)

    bpy.ops.object.select_all(action="DESELECT")
    for obj in preserved:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = preserved[0]
    bpy.ops.object.join()
    preserved[0].name = "USER_SOURCE__Hull_And_Details_Preserved"
    return len(removed)


def cylinder_between(name: str, start: Vector, end: Vector, radius: float, material, vertices: int = 16):
    direction = end - start
    midpoint = (start + end) * 0.5
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=direction.length, location=midpoint)
    obj = bpy.context.object
    obj.name = name
    obj.rotation_euler = direction.to_track_quat("Z", "Y").to_euler()
    obj.data.materials.append(material)
    return obj


def rope(name: str, points: list[Vector], material, radius: float = 0.0025):
    curve = bpy.data.curves.new(name, "CURVE")
    curve.dimensions = "3D"
    curve.resolution_u = 2
    curve.bevel_depth = radius
    curve.bevel_resolution = 2
    spline = curve.splines.new("BEZIER")
    spline.bezier_points.add(len(points) - 1)
    for handle, point in zip(spline.bezier_points, points):
        handle.co = point
        handle.handle_left_type = "AUTO"
        handle.handle_right_type = "AUTO"
    obj = bpy.data.objects.new(name, curve)
    bpy.context.collection.objects.link(obj)
    obj.data.materials.append(material)
    return obj


def sail_patch(name: str, p0: Vector, p1: Vector, p2: Vector, sail_material, seam_material, side: float):
    subdivisions = 16
    vertices = []
    index = {}
    for row in range(subdivisions + 1):
        for column in range(subdivisions + 1 - row):
            a = row / subdivisions
            b = column / subdivisions
            c = 1.0 - a - b
            point = p0 * c + p1 * a + p2 * b
            point.x += side * (0.005 + 0.052 * 27.0 * max(0.0, a * b * c))
            point.z -= 0.035 * math.sin(math.pi * b) * max(0.0, c)
            point.y -= 0.032 * math.sin(math.pi * a) * max(0.0, b)
            index[(row, column)] = len(vertices)
            vertices.append(tuple(point))

    faces = []
    for row in range(subdivisions):
        for column in range(subdivisions - row):
            a = index[(row, column)]
            b = index[(row + 1, column)]
            c = index[(row, column + 1)]
            faces.append((a, b, c))
            if column < subdivisions - row - 1:
                d = index[(row + 1, column + 1)]
                faces.append((b, d, c))

    mesh = bpy.data.meshes.new(name)
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    sail = bpy.data.objects.new(name, mesh)
    bpy.context.collection.objects.link(sail)
    sail.data.materials.append(sail_material)
    for polygon in sail.data.polygons:
        polygon.use_smooth = True
    subdivision = sail.modifiers.new("Canvas_Subdivision", "SUBSURF")
    subdivision.subdivision_type = "CATMULL_CLARK"
    subdivision.levels = 1
    subdivision.render_levels = 1
    solidify = sail.modifiers.new("Canvas_Thickness", "SOLIDIFY")
    solidify.thickness = 0.003
    solidify.offset = 0.0
    bevel = sail.modifiers.new("Soft_Canvas_Edges", "BEVEL")
    bevel.width = 0.002
    bevel.segments = 2

    rope(f"{name}__Edge01", [p0, p1], seam_material, 0.0035)
    rope(f"{name}__Edge12", [p1, p2], seam_material, 0.0035)
    rope(f"{name}__Edge20", [p2, p0], seam_material, 0.0035)
    for fraction in (0.2, 0.4, 0.6, 0.8):
        start = p0.lerp(p1, fraction) + Vector((side * 0.006, 0.0, 0.0))
        end = p0.lerp(p2, fraction) + Vector((side * 0.006, 0.0, 0.0))
        rope(f"{name}__Seam_{int(fraction * 10)}", [start, end], seam_material, 0.0012)
    return sail


def mast_set(name: str, mast_y: float, mast_top: float, yard_low: Vector, yard_high: Vector, clew: Vector, materials, side: float):
    wood, gold, sail_mat, seam_mat, rig_mat = materials
    deck_z = 0.57
    mast = cylinder_between(f"{name}__Mast", Vector((0.0, mast_y, deck_z)), Vector((0.0, mast_y, mast_top)), 0.018, wood, 20)
    cylinder_between(f"{name}__LateenYard", yard_low, yard_high, 0.014, wood, 18)
    sail_patch(f"{name}__IvorySail", yard_low, yard_high, clew, sail_mat, seam_mat, side)
    for height in (deck_z + 0.18, deck_z + 0.42, mast_top - 0.10):
        bpy.ops.mesh.primitive_torus_add(major_radius=0.022, minor_radius=0.0045, major_segments=16, minor_segments=6, location=(0.0, mast_y, height))
        band = bpy.context.object
        band.name = f"{name}__GoldBand"
        band.data.materials.append(gold)
    bpy.ops.mesh.primitive_torus_add(
        major_radius=0.032,
        minor_radius=0.006,
        major_segments=20,
        minor_segments=7,
        location=(0.0, mast_y, mast_top - 0.13),
    )
    crow_ring = bpy.context.object
    crow_ring.name = f"{name}__CrowNestRing"
    crow_ring.data.materials.append(gold)
    bpy.ops.mesh.primitive_cone_add(
        vertices=18,
        radius1=0.021,
        radius2=0.002,
        depth=0.075,
        location=(0.0, mast_y, mast_top + 0.0375),
    )
    finial = bpy.context.object
    finial.name = f"{name}__GoldFinial"
    finial.data.materials.append(gold)
    rope(f"{name}__Stay_Port", [Vector((0.0, mast_y, mast_top - 0.02)), Vector((-0.31, mast_y - 0.30, 0.56))], rig_mat)
    rope(f"{name}__Stay_Starboard", [Vector((0.0, mast_y, mast_top - 0.02)), Vector((0.31, mast_y - 0.30, 0.56))], rig_mat)

    # Lateen yards and their canvas must cross the ship's heading so the sails
    # remain readable from the gameplay camera behind the stern. The prior
    # longitudinal alignment made every sail collapse into an edge-on line.
    pivot = Matrix.Translation(Vector((0.0, mast_y, 0.0)))
    yaw = Matrix.Rotation(math.radians(68.0), 4, "Z")
    for revision_object in bpy.context.scene.objects:
        belongs_to_sail_set = revision_object.name.startswith(f"{name}__") and (
            "LateenYard" in revision_object.name or "IvorySail" in revision_object.name
        )
        if belongs_to_sail_set:
            revision_object.matrix_world = pivot @ yaw @ pivot.inverted() @ revision_object.matrix_world
    return mast


def setup_render(objects: list[bpy.types.Object]):
    corners = [obj.matrix_world @ Vector(corner) for obj in objects if obj.type == "MESH" for corner in obj.bound_box]
    low = Vector(tuple(min(vertex[i] for vertex in corners) for i in range(3)))
    high = Vector(tuple(max(vertex[i] for vertex in corners) for i in range(3)))
    center = (low + high) * 0.5
    span = max(high.x - low.x, high.y - low.y, high.z - low.z)

    world = bpy.context.scene.world
    world.use_nodes = True
    background = world.node_tree.nodes.get("Background")
    background.inputs["Color"].default_value = (0.018, 0.025, 0.04, 1.0)
    background.inputs["Strength"].default_value = 0.32
    for location, energy, size in [((-3.0, -4.0, 5.0), 1450.0, 4.0), ((4.0, 1.0, 3.0), 950.0, 3.0), ((0.0, 4.0, 5.0), 750.0, 3.0)]:
        data = bpy.data.lights.new("Revision_Area", "AREA")
        data.energy = energy
        data.shape = "DISK"
        data.size = size
        light = bpy.data.objects.new("Revision_Area", data)
        bpy.context.collection.objects.link(light)
        light.location = Vector(location) * span + center
        light.rotation_euler = (center - light.location).to_track_quat("-Z", "Y").to_euler()

    camera_data = bpy.data.cameras.new("Revision_Camera")
    camera = bpy.data.objects.new("Revision_Camera", camera_data)
    bpy.context.collection.objects.link(camera)
    bpy.context.scene.camera = camera
    camera_data.lens = 58

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1600
    scene.render.resolution_y = 1600
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.view_settings.look = "AgX - Medium High Contrast"

    distance = span * 2.35
    views = {
        "stern_three_quarter": Vector((1.35, -1.55, 0.75)),
        "port": Vector((-1.8, 0.0, 0.45)),
        "bow_three_quarter": Vector((-1.25, 1.55, 0.65)),
        "starboard": Vector((1.8, 0.0, 0.45)),
    }
    for name, direction in views.items():
        camera.location = center + direction.normalized() * distance
        camera.rotation_euler = (center + Vector((0.0, 0.0, span * 0.07)) - camera.location).to_track_quat("-Z", "Y").to_euler()
        scene.render.filepath = str(OUTPUT / f"{name}.png")
        bpy.ops.render.render(write_still=True)


clear_scene()
bpy.ops.import_scene.gltf(filepath=str(SOURCE))
source_model = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"][0]
removed_count = remove_damaged_sails(source_model)

wood = make_material("REV_R2__Aged_Mast_Wood", (0.16, 0.045, 0.008, 1.0), 0.42, 0.02)
gold = make_material("REV_R2__Aged_Gold_Bands", (0.46, 0.19, 0.025, 1.0), 0.28, 0.55)
sail = make_material("REV_R2__Aged_Ivory_Canvas", (0.76, 0.61, 0.36, 1.0), 0.76)
seam = make_material("REV_R2__Canvas_Seams", (0.24, 0.105, 0.028, 1.0), 0.72)
rigging = make_material("REV_R2__Dark_Rigging", (0.032, 0.010, 0.004, 1.0), 0.82)
materials = (wood, gold, sail, seam, rigging)

# Give the new canvas subtle woven variation instead of a flat generated surface.
sail_nodes = sail.node_tree.nodes
sail_links = sail.node_tree.links
sail_shader = sail_nodes.get("Principled BSDF")
fabric_noise = sail_nodes.new("ShaderNodeTexNoise")
fabric_noise.inputs["Scale"].default_value = 28.0
fabric_noise.inputs["Detail"].default_value = 2.0
fabric_noise.inputs["Roughness"].default_value = 0.60
fabric_ramp = sail_nodes.new("ShaderNodeValToRGB")
fabric_ramp.color_ramp.elements[0].color = (0.42, 0.30, 0.15, 1.0)
fabric_ramp.color_ramp.elements[1].color = (0.72, 0.60, 0.38, 1.0)
fabric_bump = sail_nodes.new("ShaderNodeBump")
fabric_bump.inputs["Strength"].default_value = 0.06
fabric_bump.inputs["Distance"].default_value = 0.015
sail_links.new(fabric_noise.outputs["Fac"], fabric_ramp.inputs["Fac"])
sail_links.new(fabric_ramp.outputs["Color"], sail_shader.inputs["Base Color"])
sail_links.new(fabric_noise.outputs["Fac"], fabric_bump.inputs["Height"])
sail_links.new(fabric_bump.outputs["Normal"], sail_shader.inputs["Normal"])

mast_set(
    "REV_R1__Aft",
    -0.36,
    1.48,
    Vector((0.012, -0.55, 0.86)),
    Vector((0.012, -0.08, 1.47)),
    Vector((0.012, -0.04, 0.91)),
    materials,
    1.0,
)
mast_set(
    "REV_R1__Main",
    -0.02,
    1.72,
    Vector((-0.012, -0.25, 0.78)),
    Vector((-0.012, 0.42, 1.67)),
    Vector((-0.012, 0.44, 0.84)),
    materials,
    1.0,
)
mast_set(
    "REV_R1__Fore",
    0.56,
    1.22,
    Vector((0.012, 0.36, 0.70)),
    Vector((0.012, 0.84, 1.20)),
    Vector((0.012, 0.86, 0.73)),
    materials,
    1.0,
)

rope("REV_R1__ForeStay", [Vector((0.0, 0.56, 1.20)), Vector((0.0, 0.92, 0.57))], rigging, 0.003)
rope("REV_R1__AftStay", [Vector((0.0, -0.36, 1.46)), Vector((0.0, -0.78, 0.73))], rigging, 0.003)

OUTPUT.mkdir(parents=True, exist_ok=True)
revision_objects = list(bpy.context.scene.objects)
setup_render(revision_objects)
bpy.ops.wm.save_as_mainfile(filepath=str(OUTPUT / "L01-SHP-004_Sails_R7_DIRECTION_PROTOTYPE.blend"))
print(f"L01_SHP004_SAILS_R7_DIRECTION={OUTPUT}")
print(f"REMOVED_DAMAGED_SAIL_COMPONENTS={removed_count}")
