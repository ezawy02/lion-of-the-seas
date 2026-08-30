import bpy
import math
import os
from mathutils import Vector
from mathutils.bvhtree import BVHTree


ROOT = "/Users/apple/Desktop/أسد البحار Lion of the Seas"
SOURCE = os.path.join(
    ROOT,
    "ArtSource/Blender/Incoming/Tripo/"
    "L01-ENV-016_Fortress_R6_ApprovedConcept_R1_REVIEW/"
    "L01-ENV-016_Fortress_R6_ApprovedConcept_R1_REVIEW_PBR.glb",
)
OUT_DIR = os.path.join(
    ROOT, "ArtSource/Blender/Review/Level01FortressR6/Tripo_R6_REVIEW"
)
ASSET_DIR = os.path.join(
    ROOT, "Assets/_Project/Art/Models/Environment/Level01/FortressR6"
)


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def mat(name, color, roughness=0.65, metallic=0.0):
    material = bpy.data.materials.new(name)
    material.diffuse_color = color
    material.use_nodes = True
    bsdf = material.node_tree.nodes.get("Principled BSDF")
    bsdf.inputs["Base Color"].default_value = color
    bsdf.inputs["Roughness"].default_value = roughness
    bsdf.inputs["Metallic"].default_value = metallic
    return material


def cube(name, location, scale, material, bevel=0.0):
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.scale = scale
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    if bevel:
        modifier = obj.modifiers.new("Edge softening", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
    obj.data.materials.append(material)
    return obj


def cylinder(name, location, radius, depth, material, rotation=(0, 0, 0), vertices=16):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=location, rotation=rotation
    )
    obj = bpy.context.object
    obj.name = name
    obj.data.materials.append(material)
    return obj


def banner(name, location, width, height, navy, gold):
    body = cube(name, location, (width / 2, 0.045, height / 2), navy, 0.025)
    # Three pale-gold wave bars and a compact lion-like crest marker.
    for index in range(3):
        z = location[2] - height * 0.30 + index * height * 0.105
        cube(
            f"{name}_Wave_{index + 1}",
            (location[0], location[1] - 0.055, z),
            (width * 0.36, 0.018, height * 0.018),
            gold,
            0.018,
        )
    cylinder(
        f"{name}_Crest",
        (location[0], location[1] - 0.06, location[2] + height * 0.12),
        width * 0.17,
        0.035,
        gold,
        rotation=(math.pi / 2, 0, 0),
        vertices=12,
    )
    return body


def cannon(location, wood, iron):
    x, y, z = location
    cube("Cannon carriage", (x, y, z), (0.85, 0.48, 0.24), wood, 0.08)
    for dx in (-0.62, 0.62):
        cylinder(
            f"Cannon wheel {dx}",
            (x + dx, y, z - 0.28),
            0.34,
            0.16,
            wood,
            rotation=(math.pi / 2, 0, 0),
            vertices=16,
        )
    barrel = cylinder(
        "Cannon barrel",
        (x, y - 0.2, z + 0.35),
        0.22,
        2.6,
        iron,
        rotation=(math.pi / 2, 0, 0),
        vertices=20,
    )
    barrel.rotation_euler.x += math.radians(-8)
    cylinder(
        "Cannon muzzle",
        (x, y - 1.52, z + 0.53),
        0.30,
        0.22,
        iron,
        rotation=(math.pi / 2, 0, 0),
        vertices=20,
    )


def point_camera(camera, target):
    direction = Vector(target) - camera.location
    camera.rotation_euler = direction.to_track_quat("-Z", "Y").to_euler()


def export_unity_textures():
    targets = {
        "Color_": "Fortress_R6_BaseColor.png",
        "NormalGL_": "Fortress_R6_Normal.png",
        "ORM_": "Fortress_R6_ORM.png",
    }
    for prefix, filename in targets.items():
        image = next((value for value in bpy.data.images if value.name.startswith(prefix)), None)
        if image is None:
            raise RuntimeError(f"Missing generated texture: {prefix}")
        image.filepath_raw = os.path.join(ASSET_DIR, filename)
        image.file_format = "PNG"
        image.save()


def front_surface(tree, x, z, fallback):
    hit, _normal, _index, _distance = tree.ray_cast(
        Vector((x, -30.0, z)), Vector((0.0, 1.0, 0.0)), 60.0
    )
    return hit.y - 0.08 if hit else fallback


def top_surface(tree, x, y, fallback):
    hit, _normal, _index, _distance = tree.ray_cast(
        Vector((x, y, 30.0)), Vector((0.0, 0.0, -1.0)), 60.0
    )
    return hit.z + 0.9 if hit else fallback


def main():
    os.makedirs(OUT_DIR, exist_ok=True)
    os.makedirs(ASSET_DIR, exist_ok=True)
    clear_scene()
    bpy.ops.import_scene.gltf(filepath=SOURCE)
    export_unity_textures()
    fortress = next(obj for obj in bpy.context.scene.objects if obj.type == "MESH")
    fortress.name = "L01_ENV_016_Fortress_R6_Generated_Base_REVIEW"
    width = fortress.dimensions.x
    uniform_scale = 24.0 / width
    fortress.scale = (uniform_scale,) * 3
    bpy.context.view_layer.objects.active = fortress
    fortress.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    min_z = min((fortress.matrix_world @ Vector(corner)).z for corner in fortress.bound_box)
    fortress.location.z -= min_z

    navy = mat("Fortress banner navy", (0.025, 0.18, 0.20, 1), 0.72)
    gold = mat("Fortress banner gold", (0.82, 0.59, 0.20, 1), 0.58)
    wood = mat("Fortress warm wood", (0.24, 0.105, 0.035, 1), 0.72)
    iron = mat("Cannon dark iron", (0.035, 0.04, 0.042, 1), 0.32, 0.72)
    tree = BVHTree.FromObject(fortress, bpy.context.evaluated_depsgraph_get())

    # Front-facing modular accents; intentionally separate for later art revision.
    banners = [
        ("Banner left", -5.7, 6.1, 1.35, 3.6),
        ("Banner center", 0.0, 6.6, 1.45, 4.0),
        ("Banner tower", 5.3, 8.2, 1.25, 3.8),
        ("Banner gate", 8.5, 5.2, 1.0, 2.7),
    ]
    for name, x, z, width, height in banners:
        y = front_surface(tree, x, z, -10.0)
        banner(name, (x, y, z), width, height, navy, gold)
    cannon_x, cannon_y = 7.8, -4.8
    cannon_z = 9.3
    cannon((cannon_x, cannon_y, cannon_z), wood, iron)

    # Ground plane is review-only and excluded from the exported asset.
    ground = cube("REVIEW ground", (0, 0, -0.22), (15, 15, 0.2), mat("Review sand", (0.39, 0.29, 0.17, 1), 0.9))

    bpy.ops.object.light_add(type="AREA", location=(-10, -14, 24))
    key = bpy.context.object
    key.name = "REVIEW key light"
    key.data.energy = 2900
    key.data.shape = "DISK"
    key.data.size = 11
    point_camera(key, (0, 0, 5))
    bpy.ops.object.light_add(type="AREA", location=(13, 4, 13))
    fill = bpy.context.object
    fill.name = "REVIEW fill light"
    fill.data.energy = 1450
    fill.data.size = 10
    point_camera(fill, (0, 0, 5))

    bpy.ops.object.camera_add(location=(30, -38, 23))
    camera = bpy.context.object
    camera.name = "REVIEW camera"
    camera.data.lens = 56
    point_camera(camera, (0, 0, 6.2))
    bpy.context.scene.camera = camera

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1400
    scene.render.resolution_y = 900
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = os.path.join(OUT_DIR, "Fortress_R6_Tripo_R6_REVIEW.png")
    scene.render.film_transparent = False
    scene.world.color = (0.12, 0.14, 0.18)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT_DIR, "Fortress_R6_Tripo_R6_REVIEW.blend"))
    bpy.ops.render.render(write_still=True)

    ground.hide_render = True
    exportables = [obj for obj in scene.objects if obj.type == "MESH" and obj != ground]
    bpy.ops.object.select_all(action="DESELECT")
    for obj in exportables:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = fortress
    # Bake rotations and scales per object while preserving world positions.
    # This avoids FBX axis conversion displacing independently rotated accents.
    for obj in exportables:
        bpy.context.view_layer.objects.active = obj
        bpy.ops.object.select_all(action="DESELECT")
        obj.select_set(True)
        bpy.ops.object.transform_apply(location=False, rotation=True, scale=True)
    bpy.ops.object.select_all(action="DESELECT")
    for obj in exportables:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = fortress
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(ASSET_DIR, "L01-ENV-016_Fortress_R6_Tripo_R6_REVIEW.fbx"),
        use_selection=True,
        apply_unit_scale=True,
        add_leaf_bones=False,
        path_mode="COPY",
        embed_textures=True,
    )
    print(f"REVIEW_EXPORT objects={len(exportables)} render={scene.render.filepath}")


if __name__ == "__main__":
    main()
