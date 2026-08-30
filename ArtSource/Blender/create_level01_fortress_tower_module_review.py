"""Create one modular Level 01 fortress tower review asset locally."""

from math import radians
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
FBX = ROOT / "Assets/_Project/Art/Environment/L01-ENV-014_Fortress_Tower_Modular_REVIEW.fbx"
BLEND = ROOT / "ArtSource/Blender/Environment/L01-ENV-014_Fortress_Tower_Modular_REVIEW/L01-ENV-014_Fortress_Tower_Modular_REVIEW.blend"
RENDER = ROOT / "Artifacts/Local/Approval/Level01FortressModules/Tower_Module_R1_REVIEW.png"
VIEW_DIR = ROOT / "ArtSource/Blender/Incoming/Tripo/L01-ENV-014_Fortress_Tower_Modular_R2_REVIEW/inputs"
STONE_TEXTURE = ROOT / "Assets/_Project/Art/Textures/Level01/L01-ENV-002_Fortress_Tower_Module_BaseColor.png"


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def stone_material():
    material = bpy.data.materials.new("L01-ENV-002_Fortress_Tower_Module")
    material.use_nodes = True
    nodes = material.node_tree.nodes
    nodes.clear()
    output = nodes.new("ShaderNodeOutputMaterial")
    shader = nodes.new("ShaderNodeBsdfPrincipled")
    shader.inputs["Roughness"].default_value = 0.78
    texture = nodes.new("ShaderNodeTexImage")
    texture.image = bpy.data.images.load(str(STONE_TEXTURE), check_existing=True)
    texture.projection = "BOX"
    texture.projection_blend = 0.2
    coordinates = nodes.new("ShaderNodeTexCoord")
    mapping = nodes.new("ShaderNodeMapping")
    mapping.inputs["Scale"].default_value = (1.8, 1.8, 1.8)
    links = material.node_tree.links
    links.new(coordinates.outputs["Generated"], mapping.inputs["Vector"])
    links.new(mapping.outputs["Vector"], texture.inputs["Vector"])
    links.new(texture.outputs["Color"], shader.inputs["Base Color"])
    links.new(shader.outputs["BSDF"], output.inputs["Surface"])
    return material


def dark_material():
    material = bpy.data.materials.new("Tower_Arrow_Slit_Dark")
    material.diffuse_color = (0.025, 0.018, 0.012, 1.0)
    material.roughness = 0.95
    return material


def box(name, location, dimensions, material, bevel=0.035):
    bpy.ops.mesh.primitive_cube_add(location=location)
    value = bpy.context.object
    value.name = name
    value.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    value.data.materials.append(material)
    if bevel > 0:
        modifier = value.modifiers.new("Worn stone edge", "BEVEL")
        modifier.width = bevel
        modifier.segments = 2
        bpy.context.view_layer.objects.active = value
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    return value


def battered_body(material):
    bottom = 3.05
    top = 2.62
    height = 3.35
    vertices = []
    for z, size in ((0.0, bottom), (height, top)):
        half = size * 0.5
        vertices.extend(((-half, -half, z), (half, -half, z), (half, half, z), (-half, half, z)))
    faces = ((0, 1, 2, 3), (4, 7, 6, 5), (0, 4, 5, 1), (1, 5, 6, 2), (2, 6, 7, 3), (4, 0, 3, 7))
    mesh = bpy.data.meshes.new("Tower_Battered_Base_Mesh")
    mesh.from_pydata(vertices, [], faces)
    mesh.update()
    value = bpy.data.objects.new("Tower_Battered_Base", mesh)
    bpy.context.collection.objects.link(value)
    value.data.materials.append(material)
    bevel = value.modifiers.new("Worn battered edges", "BEVEL")
    bevel.width = 0.045
    bevel.segments = 2
    bpy.context.view_layer.objects.active = value
    value.select_set(True)
    bpy.ops.object.modifier_apply(modifier=bevel.name)
    value.select_set(False)


def battlements(material):
    merlon = (0.46, 0.5, 0.72)
    edge = 1.38
    points = (-0.92, 0.0, 0.92)
    for x in points:
        box("Tower_Merlon_Front", (x, -edge, 4.18), merlon, material)
        box("Tower_Merlon_Back", (x, edge, 4.18), merlon, material)
    for y in points:
        box("Tower_Merlon_Left", (-edge, y, 4.18), merlon, material)
        box("Tower_Merlon_Right", (edge, y, 4.18), merlon, material)
    for x in (-edge, edge):
        for y in (-edge, edge):
            box("Tower_Corner_Merlon", (x, y, 4.22), (0.58, 0.58, 0.8), material)


def arrow_slits(dark):
    for z in (1.25, 2.25):
        box("Tower_Arrow_Slit_Front", (0.0, -1.48, z), (0.16, 0.035, 0.58), dark, 0.015)
        box("Tower_Arrow_Slit_Left", (-1.48, 0.0, z), (0.035, 0.16, 0.58), dark, 0.015)
        box("Tower_Arrow_Slit_Right", (1.48, 0.0, z), (0.035, 0.16, 0.58), dark, 0.015)


def setup_render():
    bpy.ops.object.camera_add(location=(7.2, -8.6, 6.3))
    camera = bpy.context.object
    target = Vector((0.0, 0.0, 2.1))
    camera.rotation_euler = (target - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.lens = 58
    bpy.context.scene.camera = camera
    bpy.ops.object.light_add(type="AREA", location=(-4.5, -5.0, 8.5))
    bpy.context.object.data.energy = 1150
    bpy.context.object.data.shape = "DISK"
    bpy.context.object.data.size = 5.0
    bpy.ops.object.light_add(type="AREA", location=(5.0, 2.0, 4.5))
    bpy.context.object.data.energy = 650
    bpy.context.object.data.size = 4.0
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = True
    scene.view_settings.look = "AgX - Medium High Contrast"


def render_cardinal_views():
    camera = bpy.context.scene.camera
    camera.data.type = "ORTHO"
    camera.data.ortho_scale = 6.35
    target = Vector((0.0, 0.0, 2.15))
    views = {
        "Front": Vector((0.0, -9.0, 2.15)),
        "Left": Vector((-9.0, 0.0, 2.15)),
        "Back": Vector((0.0, 9.0, 2.15)),
        "Right": Vector((9.0, 0.0, 2.15)),
    }
    VIEW_DIR.mkdir(parents=True, exist_ok=True)
    for name, location in views.items():
        camera.location = location
        camera.rotation_euler = (target - location).to_track_quat("-Z", "Y").to_euler()
        bpy.context.scene.render.filepath = str(VIEW_DIR / f"Tower_{name}.png")
        bpy.ops.render.render(write_still=True)


def build():
    clear_scene()
    stone = stone_material()
    dark = dark_material()
    battered_body(stone)
    box("Tower_Stone_Band_Lower", (0.0, 0.0, 0.68), (3.0, 3.0, 0.22), stone)
    box("Tower_Stone_Band_Upper", (0.0, 0.0, 2.72), (2.72, 2.72, 0.24), stone)
    box("Tower_Parapet_Core", (0.0, 0.0, 3.62), (2.82, 2.82, 0.58), stone)
    box("Tower_Cannon_Deck", (0.0, 0.0, 3.92), (2.42, 2.42, 0.18), stone)
    battlements(stone)
    arrow_slits(dark)
    mesh_objects = [value for value in bpy.context.scene.objects if value.type == "MESH"]
    for value in mesh_objects:
        value.select_set(True)
    FBX.parent.mkdir(parents=True, exist_ok=True)
    BLEND.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND))
    bpy.ops.export_scene.fbx(
        filepath=str(FBX), use_selection=True, object_types={"MESH"}, apply_unit_scale=True,
        bake_space_transform=False, axis_forward="-Z", axis_up="Y", add_leaf_bones=False,
        path_mode="STRIP",
    )
    setup_render()
    RENDER.parent.mkdir(parents=True, exist_ok=True)
    bpy.context.scene.render.filepath = str(RENDER)
    bpy.ops.render.render(write_still=True)
    render_cardinal_views()
    print(f"Wrote {FBX}")
    print(f"Wrote {RENDER}")


if __name__ == "__main__":
    build()
