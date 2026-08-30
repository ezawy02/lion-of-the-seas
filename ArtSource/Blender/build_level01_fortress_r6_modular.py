"""Build the approved Level 01 R6 fortress as editable modular review geometry."""

from __future__ import annotations

import json
from math import radians
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
ENV = ROOT / "Assets/_Project/Art/Environment"
TEX = ROOT / "Assets/_Project/Art/Textures/Level01"
R6_TEX = TEX / "Fortress_R6"
SOURCE_ROOT = ROOT / "ArtSource/Blender/Environment/L01-ENV-015_Fortress_R6_Modular"
UNITY_FBX = ENV / "L01-ENV-015_Fortress_R6_Modular_R5_REVIEW.fbx"
BLEND = SOURCE_ROOT / "L01-ENV-015_Fortress_R6_Modular_R5_REVIEW.blend"
RENDER = ROOT / "Artifacts/Local/Approval/Level01FortressModules/Fortress_R6_Modular_3D_R5_REVIEW.png"
MANIFEST = SOURCE_ROOT / "asset_manifest_R5_REVIEW.json"
CONCEPT = ROOT / "Assets/_Project/Art/Concepts/Level01/Fortress_R6_APPROVED_CONCEPT.png"

ASSETS = {
    "wall": ("L01-ENV-001_Fortress_Wall_Module", 42_000, True),
    "tower": ("L01-ENV-002_Fortress_Tower_Module", 34_000, True),
    "gate": ("L01-ENV-003_Fortress_Main_Gate_Module", 42_000, True),
    "house": ("L01-ENV-005_Mediterranean_Coastal_House", 18_000, False),
    "palm": ("L01-ENV-006_Palm_Tree_Cluster", 8_000, False),
    "rock": ("L01-ENV-007_Limestone_Rock_Cluster", 5_000, False),
    "cannon": ("L01-PRP-001_Shore_Cannon", 2_000, False),
    "banner": ("L01-PRP-002_Lion_Wave_Banner", 1_500, False),
    "scaffold": ("L01-PRP-011_Wooden_Siege_Scaffold", 7_000, False),
}


def clear_scene() -> None:
    if bpy.context.object and bpy.context.object.mode != "OBJECT":
        bpy.ops.object.mode_set(mode="OBJECT")
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)
    for collection in list(bpy.data.collections):
        if collection.name != "Collection":
            bpy.data.collections.remove(collection)


def collection(name: str) -> bpy.types.Collection:
    value = bpy.data.collections.get(name) or bpy.data.collections.new(name)
    if value.name not in bpy.context.scene.collection.children:
        bpy.context.scene.collection.children.link(value)
    return value


def move_to_collection(obj: bpy.types.Object, target: bpy.types.Collection) -> None:
    for current in list(obj.users_collection):
        current.objects.unlink(obj)
    target.objects.link(obj)


def triangle_count(obj: bpy.types.Object) -> int:
    return sum(len(polygon.vertices) - 2 for polygon in obj.data.polygons)


def decimate(obj: bpy.types.Object, target: int) -> None:
    before = triangle_count(obj)
    if before <= target:
        return
    modifier = obj.modifiers.new("R6_Mobile_LOD0", "DECIMATE")
    modifier.ratio = target / float(before)
    modifier.use_collapse_triangulate = True
    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    obj.select_set(False)


def image(path: Path, non_color: bool = False) -> bpy.types.Image | None:
    if not path.exists():
        return None
    value = bpy.data.images.load(str(path), check_existing=True)
    if non_color:
        value.colorspace_settings.name = "Non-Color"
    return value


def material(asset_id: str, recolor_red: bool) -> bpy.types.Material:
    value = bpy.data.materials.new(f"{asset_id}_R6_REVIEW")
    value.use_nodes = True
    nodes = value.node_tree.nodes
    links = value.node_tree.links
    nodes.clear()
    output = nodes.new("ShaderNodeOutputMaterial")
    shader = nodes.new("ShaderNodeBsdfPrincipled")
    shader.inputs["Roughness"].default_value = 0.68
    base = nodes.new("ShaderNodeTexImage")
    base.name = "R6_BaseColor"
    base_path = R6_TEX / f"{asset_id}_WarmLimestone_BaseColor.png" if recolor_red else TEX / f"{asset_id}_BaseColor.png"
    base.image = image(base_path)
    if base.image:
        links.new(base.outputs["Color"], shader.inputs["Base Color"])
    else:
        shader.inputs["Base Color"].default_value = (0.62, 0.48, 0.32, 1.0)

    normal_image = image(TEX / f"{asset_id}_Normal.png", non_color=True)
    if normal_image:
        normal_texture = nodes.new("ShaderNodeTexImage")
        normal_texture.image = normal_image
        normal = nodes.new("ShaderNodeNormalMap")
        normal.inputs["Strength"].default_value = 0.65
        links.new(normal_texture.outputs["Color"], normal.inputs["Color"])
        links.new(normal.outputs["Normal"], shader.inputs["Normal"])

    packed_image = image(TEX / f"{asset_id}_MetallicRoughness.png", non_color=True)
    if packed_image:
        packed = nodes.new("ShaderNodeTexImage")
        packed.image = packed_image
        channels = nodes.new("ShaderNodeSeparateColor")
        channels.mode = "RGB"
        links.new(packed.outputs["Color"], channels.inputs["Color"])
        if not recolor_red:
            links.new(channels.outputs["Blue"], shader.inputs["Metallic"])
        links.new(channels.outputs["Green"], shader.inputs["Roughness"])
    links.new(shader.outputs["BSDF"], output.inputs["Surface"])
    return value


def import_master(key: str, masters: bpy.types.Collection) -> bpy.types.Object:
    asset_id, target, recolor = ASSETS[key]
    before = set(bpy.context.scene.objects)
    bpy.ops.import_scene.fbx(filepath=str(ENV / f"{asset_id}_Optimized.fbx"))
    meshes = [obj for obj in bpy.context.scene.objects if obj not in before and obj.type == "MESH"]
    if not meshes:
        raise RuntimeError(f"No mesh imported for {asset_id}")
    if len(meshes) > 1:
        bpy.ops.object.select_all(action="DESELECT")
        for mesh in meshes:
            mesh.select_set(True)
        bpy.context.view_layer.objects.active = meshes[0]
        bpy.ops.object.join()
    master = meshes[0]
    master.name = f"SOURCE_{asset_id}"
    decimate(master, target)
    master.data.materials.clear()
    master.data.materials.append(material(asset_id, recolor))
    for slot in master.material_slots:
        slot.link = "DATA"
    move_to_collection(master, masters)
    master.hide_render = True
    master.hide_viewport = True
    return master


def empty(name: str, parent: bpy.types.Object | None = None) -> bpy.types.Object:
    value = bpy.data.objects.new(name, None)
    bpy.context.scene.collection.objects.link(value)
    value.parent = parent
    return value


def instance(
    master: bpy.types.Object,
    name: str,
    parent: bpy.types.Object,
    location: tuple[float, float, float],
    scale: tuple[float, float, float],
    rotation_z: float = 0.0,
) -> bpy.types.Object:
    value = master.copy()
    value.name = name
    bpy.context.scene.collection.objects.link(value)
    value.data = master.data.copy()
    value.hide_viewport = False
    value.hide_render = False
    value.hide_set(False)
    value.location = location
    value.scale = scale
    value.rotation_euler[2] = radians(rotation_z)
    value.parent = parent
    return value


def build_modules(masters: dict[str, bpy.types.Object]) -> list[bpy.types.Object]:
    root = empty("L01-ENV-015_Fortress_R6_Modular_R5_REVIEW")
    root["approval_status"] = "USER REVIEW REQUIRED - NOT FINAL"
    root["approved_concept"] = str(CONCEPT.relative_to(ROOT))
    sections = {name: empty(f"R6_SECTION_{name}", root) for name in (
        "FrontWall", "MainGate", "LeftWatch", "RightArtillery", "RearKeep", "BaseProps"
    )}
    objects = [
        instance(masters["wall"], "R6_FrontWall_Core", sections["FrontWall"], (-1.8, -1.0, 0.55), (3.15, 2.8, 3.15)),
        instance(masters["gate"], "R6_MainGate_Right", sections["MainGate"], (3.35, -0.78, 0.5), (2.35, 2.05, 2.55)),
        instance(masters["tower"], "R6_LeftWatchTower", sections["LeftWatch"], (-4.55, -0.45, 0.55), (2.3, 2.3, 2.4)),
        instance(masters["tower"], "R6_RightArtilleryTower", sections["RightArtillery"], (5.05, 0.2, 0.6), (2.55, 2.55, 2.55)),
        instance(masters["tower"], "R6_RearKeep_Central", sections["RearKeep"], (0.9, 1.35, 0.75), (2.7, 2.7, 2.85)),
        instance(masters["house"], "R6_RearKeep_LeftHouse", sections["RearKeep"], (-1.8, 1.75, 1.1), (2.0, 2.0, 2.15), -6),
        instance(masters["house"], "R6_RearKeep_RightHouse", sections["RearKeep"], (3.15, 1.8, 1.25), (1.75, 1.75, 2.0), 7),
        instance(masters["scaffold"], "R6_Gate_Scaffold", sections["MainGate"], (5.35, -1.55, 0.6), (1.0, 0.75, 1.35), 0),
    ]
    for index, (x, y, z, scale, rotation) in enumerate((
        (-4.35, -1.85, 0.0, (2.2, 1.45, 0.8), -14),
        (-1.9, -2.1, 0.0, (2.0, 1.25, 0.66), 9),
        (0.45, -2.0, 0.0, (1.7, 1.1, 0.6), -5),
        (3.9, -1.95, 0.0, (1.85, 1.25, 0.7), 14),
        (5.7, -1.25, 0.0, (1.7, 1.1, 0.75), -8),
    ), start=1):
        objects.append(instance(masters["rock"], f"R6_BaseRock_{index:02d}", sections["BaseProps"], (x, y, z), scale, rotation))
    for index, (x, y, scale, rotation) in enumerate((
        (-3.4, -2.15, 0.82, -8),
        (-0.15, -2.25, 0.72, 12),
        (2.35, -2.1, 0.66, -10),
        (5.65, -1.2, 0.72, 9),
    ), start=1):
        objects.append(instance(masters["palm"], f"R6_PalmCluster_{index:02d}", sections["BaseProps"], (x, y, 0.38), (scale, scale, scale), rotation))
    for index, (x, y, z, scale) in enumerate((
        (-2.0, -2.0, 1.9, (0.44, 0.7, 1.1)),
        (0.15, -2.0, 1.9, (0.43, 0.68, 1.08)),
        (2.7, -1.7, 2.1, (0.4, 0.64, 1.0)),
        (4.25, -1.42, 2.35, (0.38, 0.6, 0.95)),
    ), start=1):
        objects.append(instance(masters["banner"], f"R6_LionBanner_{index:02d}", sections["FrontWall"], (x, y, z), scale, 0))
    return objects


def setup_render() -> None:
    ground_material = bpy.data.materials.new("R6_Neutral_Sand")
    ground_material.diffuse_color = (0.34, 0.27, 0.19, 1.0)
    bpy.ops.mesh.primitive_plane_add(size=200, location=(0.0, 0.0, -0.05))
    ground = bpy.context.object
    ground.name = "REVIEW_Ground_NotExported"
    ground.data.materials.append(ground_material)

    bpy.ops.object.camera_add(location=(13.8, -18.5, 10.5))
    camera = bpy.context.object
    camera.name = "REVIEW_Camera_NotExported"
    target = Vector((0.4, -0.1, 2.75))
    camera.rotation_euler = (target - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.lens = 60
    bpy.context.scene.camera = camera

    bpy.ops.object.light_add(type="AREA", location=(-7.0, -9.0, 15.0))
    key = bpy.context.object
    key.name = "REVIEW_KeyLight_NotExported"
    key.data.energy = 2600
    key.data.shape = "DISK"
    key.data.size = 8.0
    bpy.ops.object.light_add(type="AREA", location=(9.0, 3.0, 8.0))
    fill = bpy.context.object
    fill.name = "REVIEW_FillLight_NotExported"
    fill.data.energy = 1500
    fill.data.size = 9.0

    world = bpy.context.scene.world or bpy.data.worlds.new("R6_ReviewWorld")
    bpy.context.scene.world = world
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.17, 0.22, 0.28, 1.0)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.65
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 1536
    scene.render.resolution_y = 1024
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.render.filepath = str(RENDER)
    scene.view_settings.look = "AgX - Medium High Contrast"
    scene.view_settings.exposure = 0.65


def export(objects: list[bpy.types.Object]) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    for obj in objects:
        obj.select_set(True)
        parent = obj.parent
        while parent:
            parent.select_set(True)
            parent = parent.parent
    UNITY_FBX.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=str(UNITY_FBX),
        use_selection=True,
        object_types={"EMPTY", "MESH"},
        apply_unit_scale=True,
        bake_space_transform=False,
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        bake_anim=False,
        path_mode="STRIP",
    )


def main() -> None:
    clear_scene()
    masters_collection = collection("R6_SOURCE_MASTERS_NOT_EXPORTED")
    masters = {key: import_master(key, masters_collection) for key in ASSETS}
    objects = build_modules(masters)
    export(objects)
    setup_render()
    BLEND.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND))
    RENDER.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.render.render(write_still=True)
    manifest = {
        "asset_id": "L01-ENV-015_Fortress_R6_Modular_R5_REVIEW",
        "status": "User Unity review required - not final",
        "approved_concept": str(CONCEPT.relative_to(ROOT)),
        "blend": str(BLEND.relative_to(ROOT)),
        "unity_fbx": str(UNITY_FBX.relative_to(ROOT)),
        "review_render": str(RENDER.relative_to(ROOT)),
        "source_triangles": {key: triangle_count(value) for key, value in masters.items()},
        "visible_triangle_instances": sum(triangle_count(value) for value in objects),
        "sections": sorted({value.parent.name for value in objects if value.parent}),
    }
    MANIFEST.write_text(json.dumps(manifest, indent=2), encoding="utf-8")
    print(json.dumps(manifest, indent=2))


if __name__ == "__main__":
    main()
