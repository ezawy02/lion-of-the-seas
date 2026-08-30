import bpy
import math
import os
from mathutils import Vector


ROOT = "/Users/apple/Desktop/أسد البحار Lion of the Seas"
OUT = os.path.join(ROOT, "ArtSource/Blender/Review/Level01FortressR6/Modular_R7_REVIEW")
ASSET = os.path.join(ROOT, "Assets/_Project/Art/Models/Environment/Level01/FortressR6")


def material(name, color, roughness=0.75, metallic=0.0):
    value = bpy.data.materials.new(name)
    value.diffuse_color = color
    value.use_nodes = True
    shader = value.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = color
    shader.inputs["Roughness"].default_value = roughness
    shader.inputs["Metallic"].default_value = metallic
    return value


def masonry_material(name, brick_color, mortar_color):
    value = material(name, brick_color, 0.84)
    nodes = value.node_tree.nodes
    links = value.node_tree.links
    shader = nodes.get("Principled BSDF")
    coords = nodes.new("ShaderNodeTexCoord")
    noise = nodes.new("ShaderNodeTexNoise")
    ramp = nodes.new("ShaderNodeValToRGB")
    bump = nodes.new("ShaderNodeBump")
    noise.noise_dimensions = "3D"
    noise.inputs["Scale"].default_value = 5.5
    noise.inputs["Detail"].default_value = 5.0
    noise.inputs["Roughness"].default_value = 0.72
    ramp.color_ramp.elements[0].color = mortar_color
    ramp.color_ramp.elements[1].color = brick_color
    bump.inputs["Strength"].default_value = 0.22
    bump.inputs["Distance"].default_value = 0.08
    links.new(coords.outputs["Generated"], noise.inputs["Vector"])
    links.new(noise.outputs["Fac"], ramp.inputs["Fac"])
    links.new(ramp.outputs["Color"], shader.inputs["Base Color"])
    links.new(noise.outputs["Fac"], bump.inputs["Height"])
    links.new(bump.outputs["Normal"], shader.inputs["Normal"])
    return value


def box(name, location, dimensions, mat, bevel=0.08):
    bpy.ops.mesh.primitive_cube_add(location=location)
    value = bpy.context.object
    value.name = name
    value.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    value.data.materials.append(mat)
    if bevel:
        mod = value.modifiers.new("Soft limestone edges", "BEVEL")
        mod.width = bevel
        mod.segments = 2
    return value


def cylinder(name, location, radius, depth, mat, vertices=12):
    bpy.ops.mesh.primitive_cylinder_add(
        vertices=vertices, radius=radius, depth=depth, location=location
    )
    value = bpy.context.object
    value.name = name
    value.data.materials.append(mat)
    return value


def crenels(name, center, width, depth, z, mat, count=None):
    count = count or max(3, round(width / 1.25))
    step = width / count
    for index in range(count):
        x = center - width / 2 + step * (index + 0.5)
        box(f"{name}_Crenel_{index:02}", (x, 0, z), (step * 0.56, depth, 0.72), mat, 0.05)


def wall(name, center_x, y, width, depth, height, base_z, stone, crenel=True):
    body = box(name, (center_x, y, base_z + height / 2), (width, depth, height), stone, 0.12)
    if crenel:
        count = max(3, round(width / 1.25))
        step = width / count
        for index in range(count):
            x = center_x - width / 2 + step * (index + 0.5)
            box(f"{name}_Crenel_{index:02}", (x, y, base_z + height + 0.36),
                (step * 0.58, depth * 1.05, 0.72), stone, 0.05)
    return body


def square_tower(name, x, y, width, depth, height, base_z, stone, roof=None):
    wall(name, x, y, width, depth, height, base_z, stone)
    if roof:
        bpy.ops.mesh.primitive_cone_add(vertices=4, radius1=width * 0.72, radius2=0,
                                        depth=roof, location=(x, y, base_z + height + roof * 0.58))
        cap = bpy.context.object
        cap.name = f"{name}_Terracotta_Roof"
        cap.rotation_euler.z = math.radians(45)
        cap.data.materials.append(terracotta)


def round_tower(name, x, y, radius, height, base_z, stone, roof=True):
    cylinder(name, (x, y, base_z + height / 2), radius, height, stone, 16)
    for index in range(12):
        angle = math.tau * index / 12
        box(f"{name}_Crenel_{index:02}",
            (x + math.cos(angle) * radius * 0.82,
             y + math.sin(angle) * radius * 0.82,
             base_z + height + 0.34), (0.62, 0.62, 0.68), stone, 0.05)
    if roof:
        bpy.ops.mesh.primitive_cone_add(vertices=16, radius1=radius * 1.05, radius2=0,
                                        depth=1.45, location=(x, y, base_z + height + 1.0))
        value = bpy.context.object
        value.name = f"{name}_Terracotta_Roof"
        value.data.materials.append(terracotta)


def arched_door(name, x, y, base_z, width, height, dark, trim):
    box(f"{name}_Dark_Rect", (x, y, base_z + height * 0.36),
        (width, 0.18, height * 0.72), dark, 0.02)
    arch = cylinder(f"{name}_Dark_Arch", (x, y, base_z + height * 0.72), width / 2, 0.18, dark, 24)
    arch.rotation_euler.x = math.pi / 2
    for side in (-1, 1):
        box(f"{name}_Trim_{side}", (x + side * width * 0.62, y - 0.03, base_z + height * 0.42),
            (0.34, 0.28, height * 0.84), trim, 0.04)
    for index in range(9):
        angle = math.pi * index / 8
        px = x + math.cos(angle) * width * 0.62
        pz = base_z + height * 0.72 + math.sin(angle) * width * 0.62
        block = box(f"{name}_ArchStone_{index:02}", (px, y - 0.03, pz),
                    (0.38, 0.28, 0.48), trim, 0.04)
        block.rotation_euler.y = -angle


def banner(name, x, y, z, width, height, navy, gold):
    box(name, (x, y, z), (width, 0.12, height), navy, 0.035)
    crest = cylinder(f"{name}_Crest", (x, y - 0.09, z + height * 0.15), width * 0.17, 0.12, gold, 12)
    crest.rotation_euler.x = math.pi / 2
    for index in range(3):
        box(f"{name}_Wave_{index}", (x, y - 0.09, z - height * 0.22 - index * height * 0.09),
            (width * 0.68, 0.08, height * 0.035), gold, 0.025)


def scaffold(name, x, y, z, width, height, wood):
    for side in (-1, 1):
        box(f"{name}_Post_{side}", (x + side * width / 2, y, z + height / 2),
            (0.22, 0.22, height), wood, 0.03)
    for level in (0.15, 0.5, 0.85):
        box(f"{name}_Beam_{level}", (x, y, z + height * level), (width, 0.22, 0.22), wood, 0.03)
    for side in (-1, 1):
        brace = box(f"{name}_Brace_{side}", (x, y - 0.05, z + height / 2),
                    (0.18, 0.16, math.sqrt(width * width + height * height)), wood, 0.02)
        brace.rotation_euler.y = side * math.atan2(width, height)


def rock_cluster(name, x, y, z, scale, rock):
    for index, offset in enumerate(((-0.6, 0, 0.15), (0, 0.1, 0.35), (0.65, -0.08, 0.2))):
        bpy.ops.mesh.primitive_ico_sphere_add(subdivisions=1, radius=scale * (0.75 + index * 0.12),
                                             location=(x + offset[0] * scale, y + offset[1], z + offset[2] * scale))
        value = bpy.context.object
        value.name = f"{name}_{index}"
        value.scale.z = 0.72
        value.data.materials.append(rock)


def palm(name, x, y, z, scale, trunk, leaf):
    stem = cylinder(f"{name}_Trunk", (x, y, z + 1.35 * scale), 0.12 * scale, 2.7 * scale, trunk, 10)
    stem.rotation_euler.y = math.radians(-4)
    for index in range(8):
        angle = math.tau * index / 8
        blade = box(f"{name}_Leaf_{index}",
                    (x + math.cos(angle) * 0.65 * scale,
                     y + math.sin(angle) * 0.65 * scale,
                     z + 2.75 * scale), (1.5 * scale, 0.18 * scale, 0.07 * scale), leaf, 0.03)
        blade.rotation_euler.z = angle


def cannon(name, x, y, z, wood, iron):
    box(f"{name}_Carriage", (x, y, z), (1.25, 0.75, 0.38), wood, 0.08)
    barrel = cylinder(f"{name}_Barrel", (x, y - 0.75, z + 0.42), 0.22, 2.2, iron, 18)
    barrel.rotation_euler.x = math.pi / 2
    for side in (-1, 1):
        wheel = cylinder(f"{name}_Wheel_{side}", (x + side * 0.52, y, z - 0.17), 0.32, 0.16, wood, 14)
        wheel.rotation_euler.y = math.pi / 2


def slit_window(name, x, y, z, width, height, dark, trim):
    box(f"{name}_Inset", (x, y, z), (width, 0.13, height), dark, 0.025)
    box(f"{name}_Sill", (x, y - 0.05, z - height * 0.58),
        (width * 1.5, 0.22, 0.22), trim, 0.035)
    box(f"{name}_Lintel", (x, y - 0.05, z + height * 0.58),
        (width * 1.5, 0.22, 0.22), trim, 0.035)


def facade_courses(name, x, y, base_z, width, height, stone):
    rows = max(3, int(height / 0.72))
    for row in range(1, rows):
        z = base_z + row * height / rows
        box(f"{name}_Course_{row:02}", (x, y, z), (width, 0.10, 0.055), stone, 0.01)
    columns = max(2, int(width / 1.15))
    for row in range(rows):
        z = base_z + (row + 0.5) * height / rows
        offset = 0.5 if row % 2 else 0.0
        for column in range(columns):
            px = x - width / 2 + (column + offset) * width / columns
            if px <= x - width / 2 + 0.12 or px >= x + width / 2 - 0.12:
                continue
            box(f"{name}_Joint_{row:02}_{column:02}", (px, y - 0.015, z),
                (0.045, 0.12, height / rows * 0.78), stone, 0.005)
def point_at(obj, target):
    obj.rotation_euler = (Vector(target) - obj.location).to_track_quat("-Z", "Y").to_euler()


def main():
    os.makedirs(OUT, exist_ok=True)
    os.makedirs(ASSET, exist_ok=True)
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)

    global terracotta
    stone = masonry_material("R7 warm limestone", (0.60, 0.45, 0.29, 1), (0.18, 0.12, 0.07, 1))
    trim = masonry_material("R7 pale limestone trim", (0.76, 0.62, 0.43, 1), (0.24, 0.17, 0.10, 1))
    terracotta = material("R7 terracotta roof", (0.38, 0.13, 0.045, 1), 0.74)
    navy = material("R7 deep teal banners", (0.015, 0.18, 0.19, 1), 0.68)
    gold = material("R7 banner gold", (0.91, 0.59, 0.16, 1), 0.55)
    wood = material("R7 scaffold wood", (0.22, 0.085, 0.025, 1), 0.8)
    iron = material("R7 cannon iron", (0.025, 0.03, 0.032, 1), 0.3, 0.65)
    dark = material("R7 gate shadow", (0.035, 0.025, 0.018, 1), 0.95)
    rock = material("R7 coastal rocks", (0.34, 0.27, 0.19, 1), 0.9)
    trunk = material("R7 palm trunk", (0.24, 0.11, 0.035, 1), 0.9)
    leaf = material("R7 palm leaf", (0.12, 0.24, 0.075, 1), 0.86)

    # Three-depth silhouette matching the approved reference.
    wall("Front_Curtain_Left", -5.4, -3.6, 9.2, 1.5, 6.3, 0, stone)
    wall("Front_Curtain_Right", 4.8, -3.6, 10.5, 1.5, 7.0, 0, stone)
    wall("Mid_Curtain", 0, 0.0, 17.5, 2.2, 7.8, 0, stone)
    wall("Rear_Curtain", 1.5, 3.2, 14.5, 2.0, 8.4, 0, stone)
    square_tower("Left_Front_Tower", -10.0, -3.2, 3.6, 3.8, 9.7, 0, stone)
    square_tower("Main_Gate_Tower", 7.3, -3.0, 5.3, 4.2, 11.3, 0, stone)
    square_tower("Central_High_Tower", 1.8, 0.9, 4.2, 4.0, 15.0, 0, stone, 1.5)
    square_tower("Rear_Right_Tower", 6.6, 3.2, 4.2, 4.0, 14.2, 0, stone, 1.35)
    square_tower("Rear_Left_Tower", -4.9, 3.0, 3.6, 3.6, 11.8, 0, stone, 1.2)
    round_tower("Front_Round_Tower_A", -2.7, -2.8, 2.0, 8.0, 0, stone, True)
    round_tower("Front_Round_Tower_B", 2.1, -2.7, 1.9, 8.4, 0, stone, True)
    round_tower("Left_Wood_Lookout", -10.2, -3.2, 2.05, 11.4, 0, stone, True)

    arched_door("Grand_Gate", 7.3, -5.17, 0, 2.55, 5.8, dark, trim)
    arched_door("Postern_Gate", 2.7, -4.42, 0, 1.65, 3.6, dark, trim)
    banner("Banner_Left", -5.7, -4.42, 4.35, 1.6, 4.7, navy, gold)
    banner("Banner_Center", 0.1, -4.42, 4.6, 1.8, 5.0, navy, gold)
    banner("Banner_Main", 7.3, -5.2, 7.7, 2.0, 5.4, navy, gold)
    banner("Banner_Right", 10.4, -4.42, 4.6, 1.25, 3.5, navy, gold)
    scaffold("Scaffold_Gate", 4.6, -4.7, 2.2, 3.1, 4.2, wood)
    scaffold("Scaffold_Right", 11.0, -2.6, 0.4, 2.8, 6.2, wood)
    cannon("Right_Battlement_Cannon", 9.2, -2.7, 7.8, wood, iron)

    # Rhythmic recessed windows keep the broad masses readable like the reference.
    for index, x in enumerate((-8.5, -6.7, -3.7, -1.2, 1.3, 4.1, 10.1)):
        slit_window(f"Front_Window_{index}", x, -4.39, 3.9 + 0.25 * (index % 2),
                    0.48, 1.35, dark, trim)
    for tower_name, x, y, top in (
        ("Left", -10.0, -5.13, 9.7), ("Central", 1.8, -1.13, 15.0),
        ("Gate", 7.3, -5.13, 11.3), ("RearRight", 6.6, 1.13, 14.2)):
        for level in range(2):
            slit_window(f"{tower_name}_Window_{level}", x, y,
                        top * (0.42 + 0.25 * level), 0.52, 1.45, dark, trim)

    # Strong horizontal stone bands break up the tall tower blocks.
    for x, y, width, z in ((-10, -5.15, 3.9, 6.7), (7.3, -5.15, 5.6, 7.4),
                            (1.8, -1.15, 4.5, 10.2), (6.6, 1.1, 4.5, 9.7)):
        box("Tower_Stone_Band", (x, y, z), (width, 0.34, 0.42), trim, 0.04)

    for name, x, y, width, height in (
        ("Courses_LeftWall", -5.4, -4.37, 9.1, 6.2),
        ("Courses_RightWall", 4.8, -4.37, 10.4, 6.9),
        ("Courses_LeftTower", -10.0, -5.13, 3.5, 9.6),
        ("Courses_GateTower", 7.3, -5.13, 5.2, 11.2),
        ("Courses_CentralTower", 1.8, -1.13, 4.1, 14.9)):
        facade_courses(name, x, y, 0.15, width, height - 0.3, stone)

    for index, x in enumerate((-10.8, -8.7, -6.8, -4.6, -2.0, 0.8, 3.3, 6.0, 8.7, 11.0)):
        rock_cluster(f"Base_Rocks_{index:02}", x, -5.0 + 0.16 * (index % 2), -0.2, 0.9 + 0.15 * (index % 3), rock)
    for index, x in enumerate((-9.4, -7.4, -4.8, -1.8, 1.1, 5.3, 9.7)):
        palm(f"Palm_{index:02}", x, -5.6, 0.15, 0.75 + 0.08 * (index % 3), trunk, leaf)

    ground = box("REVIEW ground", (0, 0, -0.55), (28, 18, 0.6),
                 material("R7 review sand", (0.48, 0.37, 0.24, 1), 0.95), 0.05)
    bpy.ops.object.light_add(type="AREA", location=(-13, -15, 24))
    key = bpy.context.object
    key.data.energy = 3300
    key.data.size = 12
    point_at(key, (0, 0, 6))
    bpy.ops.object.light_add(type="AREA", location=(14, -3, 15))
    fill = bpy.context.object
    fill.data.energy = 1700
    fill.data.size = 10
    point_at(fill, (0, 0, 6))
    bpy.ops.object.camera_add(location=(29, -37, 24))
    camera = bpy.context.object
    camera.data.lens = 57
    point_at(camera, (0, 0, 6.6))
    bpy.context.scene.camera = camera

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1400
    scene.render.resolution_y = 900
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = os.path.join(OUT, "Fortress_R7_Modular_REVIEW.png")
    scene.world.color = (0.58, 0.64, 0.72)
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT, "Fortress_R7_Modular_REVIEW.blend"))
    bpy.ops.render.render(write_still=True)

    ground.hide_render = True
    exportables = [obj for obj in scene.objects if obj.type == "MESH" and obj != ground]
    bpy.ops.object.select_all(action="DESELECT")
    for obj in exportables:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = exportables[0]
    bpy.ops.export_scene.fbx(
        filepath=os.path.join(ASSET, "L01-ENV-016_Fortress_R7_Modular_REVIEW.fbx"),
        use_selection=True, apply_unit_scale=True, add_leaf_bones=False, path_mode="AUTO"
    )
    print(f"R7_REVIEW objects={len(exportables)} render={scene.render.filepath}")


if __name__ == "__main__":
    main()
