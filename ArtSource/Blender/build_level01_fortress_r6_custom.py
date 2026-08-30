"""Build a custom modular fortress that follows the approved R6 concept silhouette."""

from __future__ import annotations

import json
from math import cos, pi, radians, sin
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
ENV = ROOT / "Assets/_Project/Art/Environment"
TEX = ROOT / "Assets/_Project/Art/Textures/Level01"
SOURCE_ROOT = ROOT / "ArtSource/Blender/Environment/L01-ENV-016_Fortress_R6_Custom_Modular"
ASSET_ID = "L01-ENV-016_Fortress_R6_Custom_Modular_R2_REVIEW"
FBX = ENV / f"{ASSET_ID}.fbx"
BLEND = SOURCE_ROOT / f"{ASSET_ID}.blend"
RENDER = ROOT / "Artifacts/Local/Approval/Level01FortressModules/Fortress_R6_Custom_Modular_3D_R2_REVIEW.png"
MANIFEST = SOURCE_ROOT / "asset_manifest_R2_REVIEW.json"
CONCEPT = ROOT / "Assets/_Project/Art/Concepts/Level01/Fortress_R6_APPROVED_CONCEPT.png"


def clear_scene() -> None:
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def principled_material(name: str, color: tuple[float, float, float, float], roughness: float) -> bpy.types.Material:
    material = bpy.data.materials.new(name)
    material.use_nodes = True
    shader = material.node_tree.nodes.get("Principled BSDF")
    shader.inputs["Base Color"].default_value = color
    shader.inputs["Roughness"].default_value = roughness
    return material


def stone_material() -> bpy.types.Material:
    material = principled_material("R6_Warm_Limestone", (0.68, 0.49, 0.30, 1.0), 0.82)
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    shader = nodes.get("Principled BSDF")
    coordinates = nodes.new("ShaderNodeTexCoord")
    mapping = nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (0.72, 0.72, 0.72)
    bricks = nodes.new("ShaderNodeTexBrick")
    bricks.offset = 0.5
    bricks.offset_frequency = 2
    bricks.squash = 1.0
    bricks.inputs["Color1"].default_value = (0.58, 0.38, 0.20, 1.0)
    bricks.inputs["Color2"].default_value = (0.83, 0.65, 0.40, 1.0)
    bricks.inputs["Mortar"].default_value = (0.16, 0.10, 0.055, 1.0)
    bricks.inputs["Scale"].default_value = 4.5
    bricks.inputs["Mortar Size"].default_value = 0.035
    bricks.inputs["Mortar Smooth"].default_value = 0.02
    noise = nodes.new("ShaderNodeTexNoise")
    noise.inputs["Scale"].default_value = 5.0
    noise.inputs["Detail"].default_value = 3.0
    noise.inputs["Roughness"].default_value = 0.72
    mix = nodes.new("ShaderNodeMixRGB")
    mix.blend_type = "MULTIPLY"
    mix.inputs[0].default_value = 0.18
    bump = nodes.new("ShaderNodeBump")
    bump.inputs["Strength"].default_value = 0.32
    bump.inputs["Distance"].default_value = 0.08
    links.new(coordinates.outputs["Object"], mapping.inputs["Vector"])
    links.new(mapping.outputs["Vector"], bricks.inputs["Vector"])
    links.new(mapping.outputs["Vector"], noise.inputs["Vector"])
    links.new(bricks.outputs["Color"], mix.inputs[1])
    links.new(noise.outputs["Color"], mix.inputs[2])
    links.new(mix.outputs[0], shader.inputs["Base Color"])
    links.new(bricks.outputs["Fac"], bump.inputs["Height"])
    links.new(bump.outputs["Normal"], shader.inputs["Normal"])
    return material


def textured_material(asset_id: str) -> bpy.types.Material:
    material = principled_material(f"{asset_id}_R6", (0.25, 0.22, 0.16, 1.0), 0.7)
    material.use_nodes = True
    nodes = material.node_tree.nodes
    links = material.node_tree.links
    shader = nodes.get("Principled BSDF")
    path = TEX / f"{asset_id}_BaseColor.png"
    if path.exists():
        texture = nodes.new("ShaderNodeTexImage")
        texture.image = bpy.data.images.load(str(path), check_existing=True)
        links.new(texture.outputs["Color"], shader.inputs["Base Color"])
    return material


def apply_bevel(obj: bpy.types.Object, width: float = 0.05, segments: int = 2) -> None:
    modifier = obj.modifiers.new("R6_Worn_Edges", "BEVEL")
    modifier.width = width
    modifier.segments = segments
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)


def box(
    name: str,
    location: tuple[float, float, float],
    dimensions: tuple[float, float, float],
    material: bpy.types.Material,
    parent: bpy.types.Object,
    bevel: float = 0.045,
) -> bpy.types.Object:
    bpy.ops.mesh.primitive_cube_add(location=location)
    obj = bpy.context.object
    obj.name = name
    obj.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    obj.data.materials.append(material)
    obj.parent = parent
    if bevel:
        apply_bevel(obj, bevel)
    return obj


def prism(
    name: str,
    location: tuple[float, float, float],
    bottom: tuple[float, float],
    top: tuple[float, float],
    height: float,
    material: bpy.types.Material,
    parent: bpy.types.Object,
) -> bpy.types.Object:
    bx, by = bottom[0] * 0.5, bottom[1] * 0.5
    tx, ty = top[0] * 0.5, top[1] * 0.5
    vertices = [
        (-bx, -by, 0), (bx, -by, 0), (bx, by, 0), (-bx, by, 0),
        (-tx, -ty, height), (tx, -ty, height), (tx, ty, height), (-tx, ty, height),
    ]
    faces = ((0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (3, 7, 4, 0))
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    obj.location = location
    obj.data.materials.append(material)
    obj.parent = parent
    apply_bevel(obj, 0.06)
    return obj


def battlements(
    prefix: str,
    center: tuple[float, float, float],
    width: float,
    depth: float,
    material: bpy.types.Material,
    parent: bpy.types.Object,
    count_x: int,
    count_y: int,
    size: float = 0.42,
) -> list[bpy.types.Object]:
    objects = []
    z = center[2]
    for side_y in (-1, 1):
        y = center[1] + side_y * (depth * 0.5 - size * 0.5)
        for index in range(count_x):
            x = center[0] - width * 0.5 + size * 0.65 + index * (width - size * 1.3) / max(1, count_x - 1)
            objects.append(box(f"{prefix}_Merlon_FB_{side_y}_{index}", (x, y, z), (size, size, 0.58), material, parent, 0.025))
    for side_x in (-1, 1):
        x = center[0] + side_x * (width * 0.5 - size * 0.5)
        for index in range(1, max(1, count_y - 1)):
            y = center[1] - depth * 0.5 + size * 0.65 + index * (depth - size * 1.3) / max(1, count_y - 1)
            objects.append(box(f"{prefix}_Merlon_LR_{side_x}_{index}", (x, y, z), (size, size, 0.58), material, parent, 0.025))
    return objects


def arch_fill(
    name: str,
    center: tuple[float, float, float],
    width: float,
    spring_height: float,
    radius: float,
    depth: float,
    material: bpy.types.Material,
    parent: bpy.types.Object,
) -> bpy.types.Object:
    points = [(-width * 0.5, 0.0), (width * 0.5, 0.0), (width * 0.5, spring_height)]
    for index in range(9):
        angle = index * pi / 8.0
        points.append((radius * cos(angle), spring_height + radius * sin(angle)))
    points.append((-width * 0.5, spring_height))
    vertices = []
    for y in (-depth * 0.5, depth * 0.5):
        vertices.extend((x, y, z) for x, z in points)
    count = len(points)
    faces = [tuple(range(count)), tuple(range(count, count * 2))]
    for index in range(count):
        nxt = (index + 1) % count
        faces.append((index, nxt, nxt + count, index + count))
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    obj.location = center
    obj.data.materials.append(material)
    obj.parent = parent
    apply_bevel(obj, 0.025)
    return obj


def arch_trim(
    prefix: str,
    center: tuple[float, float, float],
    radius: float,
    material: bpy.types.Material,
    parent: bpy.types.Object,
) -> list[bpy.types.Object]:
    objects = []
    for side in (-1, 1):
        objects.append(box(f"{prefix}_Pier_{side}", (center[0] + side * radius, center[1], center[2]), (0.34, 0.25, radius * 1.95), material, parent, 0.025))
    for index in range(11):
        angle = index * pi / 10.0
        x = center[0] + radius * cos(angle)
        z = center[2] + radius + radius * sin(angle)
        stone = box(f"{prefix}_Voussoir_{index:02d}", (x, center[1], z), (0.42, 0.28, 0.42), material, parent, 0.02)
        stone.rotation_euler[1] = radians(90 - angle * 180 / pi)
        objects.append(stone)
    return objects


def roof(name: str, center: tuple[float, float, float], width: float, depth: float, height: float, material: bpy.types.Material, parent: bpy.types.Object) -> bpy.types.Object:
    vertices = [(-width / 2, -depth / 2, 0), (width / 2, -depth / 2, 0), (width / 2, depth / 2, 0), (-width / 2, depth / 2, 0), (0, 0, height)]
    faces = ((0, 1, 2, 3), (0, 4, 1), (1, 4, 2), (2, 4, 3), (3, 4, 0))
    mesh = bpy.data.meshes.new(f"{name}_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    obj = bpy.data.objects.new(name, mesh)
    bpy.context.scene.collection.objects.link(obj)
    obj.location = center
    obj.data.materials.append(material)
    obj.parent = parent
    apply_bevel(obj, 0.04)
    return obj


def empty(name: str, parent: bpy.types.Object | None = None) -> bpy.types.Object:
    obj = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(obj)
    obj.parent = parent
    return obj


def wall_section(name: str, center: tuple[float, float, float], width: float, height: float, stone: bpy.types.Material, trim: bpy.types.Material, parent: bpy.types.Object) -> list[bpy.types.Object]:
    x, y, z = center
    objects = [box(f"{name}_Body", (x, y, z + height * 0.5), (width, 1.25, height), stone, parent, 0.06)]
    for band_z in (z + 0.85, z + height - 0.55):
        objects.append(box(f"{name}_Band_{band_z:.2f}", (x, y - 0.67, band_z), (width + 0.08, 0.18, 0.16), trim, parent, 0.02))
    for buttress_x in (x - width * 0.38, x + width * 0.38):
        objects.append(prism(f"{name}_Buttress", (buttress_x, y - 0.72, z), (0.55, 0.5), (0.38, 0.38), height * 0.92, trim, parent))
    objects.extend(battlements(name, (x, y, z + height + 0.32), width, 1.25, stone, parent, max(4, int(width / 0.75)), 3, 0.42))
    return objects


def tower_section(name: str, center: tuple[float, float, float], width: float, depth: float, height: float, stone: bpy.types.Material, trim: bpy.types.Material, dark: bpy.types.Material, parent: bpy.types.Object, gazebo: bool = False) -> list[bpy.types.Object]:
    x, y, z = center
    objects = [prism(f"{name}_BatteredBody", center, (width * 1.12, depth * 1.12), (width, depth), height, stone, parent)]
    objects.append(box(f"{name}_Parapet", (x, y, z + height + 0.25), (width + 0.18, depth + 0.18, 0.55), trim, parent, 0.04))
    for band_z in (z + 0.9, z + height - 0.5):
        objects.append(box(f"{name}_Band_{band_z:.2f}", (x, y - depth * 0.56, band_z), (width + 0.12, 0.2, 0.18), trim, parent, 0.02))
    for slit_x in (-width * 0.22, width * 0.22):
        objects.append(box(f"{name}_ArrowSlit_{slit_x:.2f}", (x + slit_x, y - depth * 0.565, z + height * 0.48), (0.14, 0.09, 0.58), dark, parent, 0.018))
    objects.append(arch_fill(f"{name}_ArchedWindow", (x, y - depth * 0.57, z + height * 0.63), 0.58, 0.58, 0.29, 0.1, dark, parent))
    objects.extend(arch_trim(f"{name}_WindowTrim", (x, y - depth * 0.62, z + height * 0.63 + 0.3), 0.38, trim, parent))
    objects.extend(battlements(name, (x, y, z + height + 0.82), width + 0.15, depth + 0.15, stone, parent, max(4, int(width / 0.7)), max(4, int(depth / 0.7)), 0.44))
    if gazebo:
        post_z = z + height + 1.2
        for dx in (-width * 0.28, width * 0.28):
            for dy in (-depth * 0.28, depth * 0.28):
                objects.append(box(f"{name}_GazeboPost", (x + dx, y + dy, post_z), (0.13, 0.13, 1.35), dark, parent, 0.015))
    return objects


def import_prop(asset_id: str, name: str, location: tuple[float, float, float], scale: tuple[float, float, float], parent: bpy.types.Object, rotation_z: float = 0.0) -> bpy.types.Object:
    before = set(bpy.context.scene.objects)
    bpy.ops.import_scene.fbx(filepath=str(ENV / f"{asset_id}_Optimized.fbx"))
    meshes = [obj for obj in bpy.context.scene.objects if obj not in before and obj.type == "MESH"]
    if len(meshes) > 1:
        bpy.ops.object.select_all(action="DESELECT")
        for obj in meshes:
            obj.select_set(True)
        bpy.context.view_layer.objects.active = meshes[0]
        bpy.ops.object.join()
    obj = meshes[0]
    obj.name = name
    obj.location = location
    obj.scale = scale
    obj.rotation_euler[2] = radians(rotation_z)
    obj.parent = parent
    obj.data.materials.clear()
    obj.data.materials.append(textured_material(asset_id))
    return obj


def build() -> list[bpy.types.Object]:
    stone = stone_material()
    trim = principled_material("R6_Limestone_Trim", (0.82, 0.67, 0.46, 1.0), 0.76)
    dark = principled_material("R6_Dark_Recess", (0.055, 0.045, 0.035, 1.0), 0.9)
    wood = principled_material("R6_Warm_Wood", (0.24, 0.11, 0.045, 1.0), 0.72)
    roof_mat = principled_material("R6_Terracotta_Roof", (0.55, 0.16, 0.055, 1.0), 0.68)
    root = empty(ASSET_ID)
    root["approval_status"] = "USER UNITY REVIEW REQUIRED - NOT FINAL"
    root["approved_concept"] = str(CONCEPT.relative_to(ROOT))
    sections = {name: empty(f"R6_SECTION_{name}", root) for name in ("FrontWall", "MainGate", "LeftWatch", "RightArtillery", "RearKeep", "BaseProps")}
    objects = []
    objects += wall_section("R6_FrontWall", (-1.55, -0.75, 0.55), 6.2, 3.5, stone, trim, sections["FrontWall"])
    objects += tower_section("R6_LeftWatch", (-4.85, -0.45, 0.45), 2.65, 2.55, 4.9, stone, trim, dark, sections["LeftWatch"], gazebo=True)
    roof("R6_LeftWatch_Roof", (-4.85, -0.45, 7.0), 2.35, 2.25, 1.0, roof_mat, sections["LeftWatch"])

    objects += wall_section("R6_MainGateHouse", (2.4, -0.55, 0.5), 3.5, 4.45, stone, trim, sections["MainGate"])
    objects.append(arch_fill("R6_MainGate_Door", (2.4, -1.22, 0.55), 1.65, 1.75, 0.825, 0.18, dark, sections["MainGate"]))
    objects += arch_trim("R6_MainGate_Trim", (2.4, -1.34, 1.45), 1.05, trim, sections["MainGate"])

    objects += tower_section("R6_RightArtillery", (5.25, -0.1, 0.45), 3.25, 3.05, 4.5, stone, trim, dark, sections["RightArtillery"])
    objects.append(import_prop("L01-PRP-001_Shore_Cannon", "R6_RightArtillery_Cannon", (5.25, -1.05, 5.7), (0.62, 0.62, 0.62), sections["RightArtillery"], 0))

    objects += tower_section("R6_CentralKeep", (0.4, 1.65, 0.75), 3.0, 2.9, 6.7, stone, trim, dark, sections["RearKeep"])
    roof("R6_CentralKeep_Roof", (0.4, 1.65, 8.35), 3.15, 3.05, 1.15, roof_mat, sections["RearKeep"])
    objects += tower_section("R6_RearLeftTower", (-2.15, 1.75, 0.8), 2.2, 2.1, 4.9, stone, trim, dark, sections["RearKeep"])
    roof("R6_RearLeftTower_Roof", (-2.15, 1.75, 6.35), 2.25, 2.15, 0.9, roof_mat, sections["RearKeep"])
    objects += tower_section("R6_RearRightTower", (3.25, 1.75, 0.8), 2.1, 2.0, 5.3, stone, trim, dark, sections["RearKeep"])

    for index, (x, z, sx, sz) in enumerate(((-2.6, 2.25, 0.42, 1.1), (-0.4, 2.35, 0.42, 1.08), (2.45, 2.55, 0.4, 1.0), (4.25, 2.4, 0.38, 0.96)), start=1):
        objects.append(import_prop("L01-PRP-002_Lion_Wave_Banner", f"R6_LionBanner_{index:02d}", (x, -1.45, z), (sx, 0.62, sz), sections["FrontWall"], 0))

    for index, (x, y, scale, rotation) in enumerate(((-4.5, -1.8, (2.2, 1.4, 0.78), -12), (-1.8, -2.0, (2.0, 1.2, 0.64), 7), (0.8, -2.0, (1.7, 1.05, 0.58), -5), (3.8, -1.9, (1.8, 1.15, 0.68), 11), (5.8, -1.2, (1.65, 1.0, 0.7), -8)), start=1):
        objects.append(import_prop("L01-ENV-007_Limestone_Rock_Cluster", f"R6_BaseRock_{index:02d}", (x, y, 0.0), scale, sections["BaseProps"], rotation))
    for index, (x, y, scale, rotation) in enumerate(((-3.5, -2.05, 0.65, -8), (-0.3, -2.15, 0.55, 10), (2.1, -2.0, 0.52, -7), (5.6, -1.25, 0.56, 9)), start=1):
        objects.append(import_prop("L01-ENV-006_Palm_Tree_Cluster", f"R6_PalmCluster_{index:02d}", (x, y, 0.38), (scale, scale, scale), sections["BaseProps"], rotation))
    objects.append(import_prop("L01-PRP-011_Wooden_Siege_Scaffold", "R6_RightScaffold", (6.05, -1.45, 0.55), (0.85, 0.72, 1.28), sections["BaseProps"], 0))
    objects.extend([obj for obj in bpy.context.scene.objects if obj.type == "MESH" and obj not in objects])
    return list(dict.fromkeys(objects))


def setup_review() -> None:
    ground = principled_material("R6_ReviewGround", (0.44, 0.36, 0.26, 1.0), 0.92)
    box("REVIEW_Ground_NotExported", (0, 0, -0.2), (100, 100, 0.25), ground, empty("REVIEW_ONLY"), 0)
    bpy.ops.object.camera_add(location=(18.0, -25.0, 14.5))
    camera = bpy.context.object
    camera.name = "REVIEW_Camera_NotExported"
    target = Vector((0.45, -0.05, 3.2))
    camera.rotation_euler = (target - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.lens = 62
    bpy.context.scene.camera = camera
    bpy.ops.object.light_add(type="AREA", location=(-8, -10, 17))
    bpy.context.object.data.energy = 2700
    bpy.context.object.data.size = 9
    bpy.ops.object.light_add(type="AREA", location=(10, 4, 10))
    bpy.context.object.data.energy = 1500
    bpy.context.object.data.size = 10
    world = bpy.context.scene.world or bpy.data.worlds.new("R6_Custom_World")
    bpy.context.scene.world = world
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.31, 0.39, 0.47, 1.0)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.65
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1536
    scene.render.resolution_y = 1024
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = str(RENDER)
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.view_settings.exposure = 0.55


def export(objects: list[bpy.types.Object]) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
        parent = obj.parent
        while parent and not parent.name.startswith("REVIEW"):
            parent.select_set(True)
            parent = parent.parent
    FBX.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(filepath=str(FBX), use_selection=True, object_types={"EMPTY", "MESH"}, apply_unit_scale=True, bake_space_transform=False, axis_forward="-Z", axis_up="Y", add_leaf_bones=False, bake_anim=False, path_mode="STRIP")


def main() -> None:
    clear_scene()
    objects = build()
    export(objects)
    setup_review()
    BLEND.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND))
    RENDER.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.render.render(write_still=True)
    triangles = sum(sum(len(p.vertices) - 2 for p in obj.data.polygons) for obj in objects if obj.type == "MESH")
    report = {"asset_id": ASSET_ID, "status": "User Unity review required - not final", "approved_concept": str(CONCEPT.relative_to(ROOT)), "blend": str(BLEND.relative_to(ROOT)), "unity_fbx": str(FBX.relative_to(ROOT)), "review_render": str(RENDER.relative_to(ROOT)), "visible_triangles": triangles, "sections": sorted({obj.parent.name for obj in objects if obj.parent and obj.parent.name.startswith("R6_SECTION_")})}
    MANIFEST.write_text(json.dumps(report, indent=2), encoding="utf-8")
    print(json.dumps(report, indent=2))


if __name__ == "__main__":
    main()
