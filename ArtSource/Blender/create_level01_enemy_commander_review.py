import bpy
import math
from pathlib import Path
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Assets/_Project/Art/Characters/L01-CHR-003_Hostile_Infantry_Rigged_Optimized.fbx"
OUT_DIR = ROOT / "ArtSource/Blender/Characters/L01-CHR-005_Enemy_Commander/R1_REVIEW"
UNITY_FBX = ROOT / "Assets/_Project/Art/Characters/L01-CHR-005_Enemy_Commander_R1_REVIEW.fbx"


def material(name, color, metallic=0.0, roughness=0.45):
    value = bpy.data.materials.new(name)
    value.diffuse_color = (*color, 1.0)
    value.metallic = metallic
    value.roughness = roughness
    value.use_nodes = True
    shader = value.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = (*color, 1.0)
    shader.inputs["Metallic"].default_value = metallic
    shader.inputs["Roughness"].default_value = roughness
    return value


RED = material("Commander Crimson", (0.34, 0.018, 0.012), 0.05, 0.38)
NAVY = material("Commander Navy", (0.018, 0.05, 0.09), 0.12, 0.35)
GOLD = material("Commander Antique Gold", (0.62, 0.31, 0.055), 0.72, 0.24)
STEEL = material("Commander Dark Steel", (0.07, 0.08, 0.09), 0.78, 0.28)


def bevel(obj, amount=0.04):
    mod = obj.modifiers.new("Soft Bevel", "BEVEL")
    mod.width = amount
    mod.segments = 2


def cube(name, location, scale, mat, bevel_size=0.04):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    bevel(obj, bevel_size)
    obj.data.materials.append(mat)
    return obj


def sphere(name, location, scale, mat):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=24, ring_count=12, location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(mat)
    return obj


def cylinder(name, location, radius, depth, mat, rotation=(0, 0, 0), vertices=20):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth,
                                       location=location, rotation=rotation)
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(mat)
    bevel(obj, 0.025)
    return obj


def normalize_body():
    bpy.ops.import_scene.fbx(filepath=str(SOURCE))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    corners = [obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
    minimum = Vector((min(v.x for v in corners), min(v.y for v in corners), min(v.z for v in corners)))
    maximum = Vector((max(v.x for v in corners), max(v.y for v in corners), max(v.z for v in corners)))
    height = maximum.z - minimum.z
    scale = 2.05 / max(height, 0.001)
    for obj in bpy.context.scene.objects:
        if obj.parent is None:
            obj.scale *= scale
    bpy.context.view_layer.update()
    corners = [obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
    floor = min(v.z for v in corners)
    center_x = (min(v.x for v in corners) + max(v.x for v in corners)) * 0.5
    center_y = (min(v.y for v in corners) + max(v.y for v in corners)) * 0.5
    for obj in bpy.context.scene.objects:
        if obj.parent is None:
            obj.location += Vector((-center_x, -center_y, -floor))
    for obj in meshes:
        obj.name = "EnemyCommander_BaseBody"


def build_armor():
    cube("Commander_Chestplate", (0, -0.11, 1.30), (0.29, 0.10, 0.25), NAVY, 0.07)
    cube("Commander_GoldChestBand", (0, -0.225, 1.34), (0.24, 0.022, 0.045), GOLD, 0.018)
    sphere("Commander_LeftPauldron", (-0.39, -0.01, 1.47), (0.21, 0.18, 0.13), GOLD)
    sphere("Commander_RightPauldron", (0.39, -0.01, 1.47), (0.21, 0.18, 0.13), GOLD)
    sphere("Commander_Helmet", (0, 0.00, 1.93), (0.24, 0.22, 0.16), STEEL)
    cylinder("Commander_HelmetBand", (0, -0.01, 1.84), 0.245, 0.055, GOLD)
    cube("Commander_LeftCheekGuard", (-0.20, -0.18, 1.74), (0.035, 0.025, 0.13), GOLD, 0.012)
    cube("Commander_RightCheekGuard", (0.20, -0.18, 1.74), (0.035, 0.025, 0.13), GOLD, 0.012)
    for index, x in enumerate((-0.12, -0.06, 0, 0.06, 0.12)):
        cube(f"Commander_Plume_{index}", (x, 0.02, 2.12 + abs(x) * 0.7),
             (0.035, 0.06, 0.24 - abs(x) * 0.45), RED, 0.02)
    cape = cube("Commander_Cape", (0, 0.16, 1.08), (0.42, 0.045, 0.62), RED, 0.05)
    cape.rotation_euler.x = math.radians(-7)
    shield = cylinder("Commander_Shield", (0.48, -0.34, 0.98), 0.36, 0.10, NAVY,
                      (math.radians(90), 0, 0), 32)
    cylinder("Commander_ShieldBoss", (0.48, -0.405, 0.98), 0.13, 0.08, GOLD,
             (math.radians(90), 0, 0), 24)
    sword = cube("Commander_SwordBlade", (-0.5, -0.24, 0.96), (0.035, 0.025, 0.54), STEEL, 0.015)
    sword.rotation_euler.y = math.radians(-12)
    cylinder("Commander_SwordHilt", (-0.5, -0.25, 0.43), 0.045, 0.28, GOLD,
             (0, math.radians(90), 0), 16)
    cylinder("Commander_SwordGrip", (-0.5, -0.25, 0.29), 0.035, 0.22, NAVY,
             (0, 0, 0), 16)
    return shield, sword


def look_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def render_preview():
    world = bpy.context.scene.world or bpy.data.worlds.new("Commander World")
    bpy.context.scene.world = world
    world.color = (0.025, 0.035, 0.055)
    bpy.ops.object.light_add(type="AREA", location=(3.5, -4.5, 5.2))
    bpy.context.object.data.energy = 1100
    bpy.context.object.data.shape = "DISK"
    bpy.context.object.data.size = 4.0
    look_at(bpy.context.object, (0, 0, 1.0))
    bpy.ops.object.light_add(type="AREA", location=(-3, 1.5, 3.2))
    bpy.context.object.data.energy = 650
    bpy.context.object.data.color = (0.4, 0.58, 1.0)
    bpy.context.object.data.size = 3.0
    look_at(bpy.context.object, (0, 0, 1.1))
    bpy.ops.object.camera_add(location=(3.2, -5.5, 2.55))
    camera = bpy.context.object
    camera.data.lens = 58
    look_at(camera, (0, 0, 1.05))
    bpy.context.scene.camera = camera
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 720
    scene.render.resolution_y = 960
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = True
    scene.render.filepath = str(OUT_DIR / "L01-CHR-005_Enemy_Commander_R1_REVIEW.png")
    bpy.ops.render.render(write_still=True)


def main():
    OUT_DIR.mkdir(parents=True, exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    normalize_body()
    build_armor()
    render_preview()
    bpy.ops.wm.save_as_mainfile(filepath=str(OUT_DIR / "L01-CHR-005_Enemy_Commander_R1_REVIEW.blend"))
    exportable = [obj for obj in bpy.context.scene.objects if obj.type in {"MESH", "ARMATURE"}]
    bpy.ops.object.select_all(action="DESELECT")
    for obj in exportable:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = exportable[0]
    bpy.ops.export_scene.fbx(filepath=str(UNITY_FBX), use_selection=True, apply_unit_scale=True,
                             add_leaf_bones=False, bake_anim=False, path_mode="AUTO")


main()
