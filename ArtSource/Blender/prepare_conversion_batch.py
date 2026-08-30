import json
import sys
from pathlib import Path

import bpy


ASSETS = {
    "1d4a12ee-ef50-4cc3-9830-6dc71654cca6": (
        "L02-SHP-001_Armored_Warship_Boss_UserBatch_R2_REVIEW", 48000, "Ships"),
    "01e7622c-dc8f-46ac-8a24-856d8cee72c0": (
        "L03-CHR-001_Storm_Fortress_Commander_UserBatch_R2_REVIEW", 28000, "Characters"),
    "14f89371-722d-4bb9-83a7-f02afa0485dd": (
        "L02-PRP-001_Floating_Naval_Mine_UserBatch_R2_REVIEW", 12000, "Environment"),
    "16f89955-e0dd-4626-a1ac-2d4cbd2c669c": (
        "L03-PRP-001_Gunpowder_Barrel_Cluster_UserBatch_R2_REVIEW", 16000, "Environment"),
    "5523e01b-baf9-4e32-a9bb-9702acf1170f": (
        "L03-SHP-001_Gunpowder_Skiff_UserBatch_R2_REVIEW", 28000, "Ships"),
    "651116c7-19f5-4bce-984f-e098ecce376e": (
        "L01-CHR-005_Enemy_Commander_UserBatch_R2_REVIEW", 26000, "Characters"),
    "774097f9-ad65-49d1-bba8-33c4b4a1ccdc": (
        "L02-PRP-002_Heavy_Chain_Link_Unit_UserBatch_R2_REVIEW", 6000, "Environment"),
    "7744bff2-5410-48fe-8892-7a444505e49c": (
        "L01-CHR-004_Harbor_Guardian_UserBatch_R2_REVIEW", 30000, "Characters"),
}


def reset_scene():
    # Every source GLB uses the same generic image names. A full reset prevents
    # Blender from reusing the previous asset's images for the next import.
    bpy.ops.wm.read_factory_settings(use_empty=True)


def triangle_count(obj):
    return sum(len(poly.vertices) - 2 for poly in obj.data.polygons)


def prepare(source, name, target_triangles, editable, output):
    reset_scene()
    bpy.ops.import_scene.gltf(filepath=str(source))
    asset_id = name
    level = "Level" + asset_id[1:3]
    texture_root = output.parents[1] / "Textures" / level
    texture_root.mkdir(parents=True, exist_ok=True)
    texture_roles = {
        "texture_diffuse": "BaseColor",
        "texture_normal": "Normal",
        "texture_metallic-texture_roughness": "MetallicRoughness",
    }
    for image_name, role in texture_roles.items():
        image = bpy.data.images.get(image_name)
        if image is None:
            continue
        image.filepath_raw = str(texture_root / f"{asset_id}_{role}.png")
        image.file_format = "PNG"
        image.save()
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    for obj in bpy.context.scene.objects:
        obj.select_set(False)
    for obj in meshes:
        obj.select_set(True)
    bpy.context.view_layer.objects.active = meshes[0]
    if len(meshes) > 1:
        bpy.ops.object.join()
    model = bpy.context.view_layer.objects.active
    model.name = name
    before = triangle_count(model)
    if before > target_triangles:
        modifier = model.modifiers.new("UnityTriangleBudget", "DECIMATE")
        modifier.decimate_type = "COLLAPSE"
        modifier.ratio = max(0.05, target_triangles / before)
        modifier.use_collapse_triangulate = True
        bpy.ops.object.modifier_apply(modifier=modifier.name)
    for polygon in model.data.polygons:
        polygon.use_smooth = True
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    model.select_set(True)
    editable.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(editable))
    output.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.export_scene.fbx(
        filepath=str(output),
        use_selection=True,
        apply_unit_scale=True,
        bake_space_transform=False,
        add_leaf_bones=False,
        path_mode="COPY",
        embed_textures=True,
    )
    return {"asset": name, "source_triangles": before,
            "export_triangles": triangle_count(model), "editable": str(editable),
            "output": str(output)}


def main():
    arguments = sys.argv[sys.argv.index("--") + 1:]
    project = Path(arguments[0]).resolve()
    selected = set(arguments[1:])
    incoming = project / "ArtSource/Blender/Incoming/3DConversionBatch_2026-08-26"
    results = []
    for uuid, (name, budget, category) in ASSETS.items():
        if selected and uuid not in selected:
            continue
        source = incoming / uuid / "base_basic_pbr.glb"
        editable = project / "ArtSource/Blender" / category / name / f"{name}.blend"
        output = project / "Assets/_Project/Art" / category / f"{name}.fbx"
        results.append(prepare(source, name, budget, editable, output))
    report_name = "optimized_exports.json" if not selected else "optimized_exports_selected.json"
    report = incoming / "Review" / report_name
    report.write_text(json.dumps(results, indent=2), encoding="utf-8")


main()
