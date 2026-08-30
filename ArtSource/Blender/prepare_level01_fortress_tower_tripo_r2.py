"""Prepare the generated Level 01 fortress tower as a local Unity review asset."""

from pathlib import Path
import json

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
ASSET_ID = "L01-ENV-014_Fortress_Tower_TripoV31_R2"
INPUT = ROOT / "ArtSource/Blender/Incoming/Tripo/L01-ENV-014_Fortress_Tower_Modular_R2_REVIEW/retopology_35k/L01-ENV-014_Fortress_Tower_TripoV31_R2_Retopo35K_REVIEW.fbx"
SOURCE_DIR = ROOT / "ArtSource/Blender/Environment" / ASSET_ID
BLEND = SOURCE_DIR / f"{ASSET_ID}_Optimized_REVIEW.blend"
FBX = ROOT / "Assets/_Project/Art/Environment" / f"{ASSET_ID}_Optimized_REVIEW.fbx"
TEXTURES = ROOT / "Assets/_Project/Art/Textures/Level01"
PREVIEW = ROOT / "Artifacts/Local/Approval/Level01FortressModules/Tower_TripoV31_R2_Optimized_REVIEW.png"


def triangles(value):
    value.data.calc_loop_triangles()
    return len(value.data.loop_triangles)


def remove_generation_artifacts():
    meshes = [value for value in bpy.context.scene.objects if value.type == "MESH"]
    candidates = [value for value in meshes if triangles(value) > 100]
    if len(candidates) != 1:
        raise RuntimeError(f"Expected one generated tower mesh, found {len(candidates)}")
    tower = candidates[0]
    for value in list(bpy.context.scene.objects):
        if value != tower:
            bpy.data.objects.remove(value, do_unlink=True)
    tower.name = ASSET_ID
    tower.data.name = ASSET_ID + "_Mesh"
    return tower


def save_textures(tower):
    material = tower.data.materials[0]
    roles = {}
    for link in material.node_tree.links:
        node = link.from_node
        if node.bl_idname != "ShaderNodeTexImage" or node.image is None:
            continue
        target = link.to_socket.name
        if target == "Base Color":
            roles["BaseColor"] = node.image
        elif link.to_node.bl_idname == "ShaderNodeNormalMap":
            roles["Normal"] = node.image
        elif target == "Roughness":
            roles["Roughness"] = node.image
        elif target == "Metallic":
            roles["Metallic"] = node.image
    required = {"BaseColor", "Normal", "Roughness", "Metallic"}
    if set(roles) != required:
        raise RuntimeError(f"Generated tower texture roles are incomplete: {sorted(roles)}")
    TEXTURES.mkdir(parents=True, exist_ok=True)
    outputs = []
    for role, image in roles.items():
        path = TEXTURES / f"{ASSET_ID}_{role}.png"
        image.filepath_raw = str(path)
        image.file_format = "PNG"
        image.save()
        outputs.append(path)
    material.name = ASSET_ID
    return outputs


def normalize_tower(tower):
    bpy.context.view_layer.objects.active = tower
    tower.select_set(True)
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    target_height = 5.6
    factor = target_height / tower.dimensions.z
    tower.scale = Vector((factor, factor, factor))
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    points = [tower.matrix_world @ vertex.co for vertex in tower.data.vertices]
    low = Vector((min(point.x for point in points), min(point.y for point in points), min(point.z for point in points)))
    high = Vector((max(point.x for point in points), max(point.y for point in points), max(point.z for point in points)))
    offset = Vector((-(low.x + high.x) * 0.5, -(low.y + high.y) * 0.5, -low.z))
    for vertex in tower.data.vertices:
        vertex.co += offset
    tower.data.update()
    bpy.ops.object.shade_smooth_by_angle()


def save_and_export(tower):
    SOURCE_DIR.mkdir(parents=True, exist_ok=True)
    FBX.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(BLEND), check_existing=False)
    if FBX.exists():
        FBX.unlink()
    bpy.ops.object.select_all(action="DESELECT")
    tower.select_set(True)
    bpy.context.view_layer.objects.active = tower
    if [value for value in bpy.context.scene.objects if value.type == "MESH"] != [tower]:
        raise RuntimeError("Generated tower export scene contains an unexpected mesh")
    bpy.ops.export_scene.fbx(
        filepath=str(FBX), use_selection=True, object_types={"MESH"},
        axis_forward="-Z", axis_up="Y", apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL", add_leaf_bones=False,
        bake_anim=False, mesh_smooth_type="FACE", path_mode="RELATIVE",
        embed_textures=False,
    )


def render_preview(tower):
    bpy.ops.object.camera_add(location=(8.2, -9.5, 6.9))
    camera = bpy.context.object
    target = Vector((0.0, 0.0, 2.7))
    camera.rotation_euler = (target - camera.location).to_track_quat("-Z", "Y").to_euler()
    camera.data.lens = 58
    bpy.context.scene.camera = camera
    bpy.ops.object.light_add(type="AREA", location=(-4.0, -5.5, 9.0))
    bpy.context.object.data.energy = 1250
    bpy.context.object.data.size = 5.0
    bpy.ops.object.light_add(type="AREA", location=(5.0, 3.0, 5.0))
    bpy.context.object.data.energy = 700
    bpy.context.object.data.size = 4.0
    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 768
    scene.render.resolution_y = 768
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = True
    scene.view_settings.look = "AgX - Medium High Contrast"
    PREVIEW.parent.mkdir(parents=True, exist_ok=True)
    scene.render.filepath = str(PREVIEW)
    bpy.ops.render.render(write_still=True)


def write_manifest(tower, texture_paths):
    payload = {
        "asset_id": ASSET_ID,
        "source": str(INPUT.relative_to(ROOT)),
        "prepared_blend": str(BLEND.relative_to(ROOT)),
        "unity_fbx": str(FBX.relative_to(ROOT)),
        "triangles": triangles(tower),
        "bounds_metres": [round(value, 4) for value in tower.dimensions],
        "textures": [str(path.relative_to(ROOT)) for path in texture_paths],
        "status": "Unity user review required",
    }
    (SOURCE_DIR / "asset_manifest.json").write_text(json.dumps(payload, indent=2) + "\n", encoding="utf-8")
    print(json.dumps(payload, indent=2))


def main():
    bpy.ops.wm.read_factory_settings(use_empty=True)
    bpy.ops.import_scene.fbx(filepath=str(INPUT))
    tower = remove_generation_artifacts()
    textures = save_textures(tower)
    normalize_tower(tower)
    save_and_export(tower)
    render_preview(tower)
    write_manifest(tower, textures)


if __name__ == "__main__":
    main()
