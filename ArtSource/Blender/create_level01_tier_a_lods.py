"""Create review-only LODs from the current Level 1 Tier-A Unity assets.

Run with Blender in background mode. Outputs remain REVIEW candidates until the
user compares their silhouettes inside Unity and explicitly approves them.
"""

from __future__ import annotations

import json
from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
LOD_RATIOS = {"LOD1": 0.58, "LOD2": 0.28}

ASSETS = (
    {
        "asset_id": "L01-SHP-004_Hero_Flagship_TripoV31_R2",
        "source_kind": "blend",
        "source": ROOT
        / "ArtSource/Blender/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2/"
        / "L01-SHP-004_Hero_Flagship_TripoV31_R2_Optimized_REVIEW.blend",
        "source_dir": ROOT
        / "ArtSource/Blender/Ships/L01-SHP-004_Hero_Flagship_TripoV31_R2/LOD_R1_REVIEW",
        "unity_dir": ROOT / "Assets/_Project/Art/Ships",
    },
    {
        "asset_id": "L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized",
        "source_kind": "fbx",
        "source": ROOT
        / "Assets/_Project/Art/Characters/"
        / "L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized.fbx",
        "source_dir": ROOT
        / "ArtSource/Blender/Characters/L01-CHR-004_Harbor_Guardian_Boss/LOD_R1_REVIEW",
        "unity_dir": ROOT / "Assets/_Project/Art/Characters",
    },
)


def reset_and_load(asset: dict) -> None:
    bpy.ops.wm.read_factory_settings(use_empty=True)
    if asset["source_kind"] == "blend":
        bpy.ops.wm.open_mainfile(filepath=str(asset["source"]))
    else:
        bpy.ops.import_scene.fbx(filepath=str(asset["source"]))

    for obj in tuple(bpy.context.scene.objects):
        if obj.type not in {"MESH", "ARMATURE"}:
            bpy.data.objects.remove(obj, do_unlink=True)


def triangle_count(obj: bpy.types.Object) -> int:
    obj.data.calc_loop_triangles()
    return len(obj.data.loop_triangles)


def mesh_objects() -> list[bpy.types.Object]:
    return [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]


def simplify_meshes(ratio: float, suffix: str) -> tuple[int, int]:
    before = 0
    after = 0
    for obj in mesh_objects():
        before += triangle_count(obj)
        bpy.context.view_layer.objects.active = obj
        obj.select_set(True)
        modifier = obj.modifiers.new(name=f"SeaLion_{suffix}", type="DECIMATE")
        modifier.decimate_type = "COLLAPSE"
        modifier.ratio = ratio
        modifier.use_collapse_triangulate = True

        while obj.modifiers.find(modifier.name) > 0:
            bpy.ops.object.modifier_move_up(modifier=modifier.name)

        bpy.ops.object.modifier_apply(modifier=modifier.name)
        obj.name = f"{obj.name}_{suffix}"
        obj.data.name = f"{obj.data.name}_{suffix}"
        after += triangle_count(obj)
        obj.select_set(False)
    return before, after


def export_fbx(path: Path) -> None:
    bpy.ops.object.select_all(action="DESELECT")
    for obj in bpy.context.scene.objects:
        if obj.type in {"MESH", "ARMATURE"}:
            obj.select_set(True)

    bpy.ops.export_scene.fbx(
        filepath=str(path),
        use_selection=True,
        object_types={"MESH", "ARMATURE"},
        apply_unit_scale=True,
        apply_scale_options="FBX_SCALE_ALL",
        use_mesh_modifiers=True,
        mesh_smooth_type="FACE",
        add_leaf_bones=False,
        use_armature_deform_only=True,
        bake_anim=False,
        path_mode="AUTO",
    )


def validate_scene(asset: dict, before: int, after: int, ratio: float) -> dict:
    meshes = mesh_objects()
    if not meshes:
        raise RuntimeError(f"{asset['asset_id']}: no mesh remained after decimation")
    if after >= before:
        raise RuntimeError(f"{asset['asset_id']}: LOD did not reduce triangle count")

    expected = before * ratio
    tolerance = max(24, before * 0.03)
    if abs(after - expected) > tolerance:
        raise RuntimeError(
            f"{asset['asset_id']}: triangle result {after} is outside expected tolerance"
        )

    return {
        "triangle_count_before": before,
        "triangle_count_after": after,
        "actual_ratio": round(after / before, 4),
        "materials": sorted(
            {material.name for obj in meshes for material in obj.data.materials if material}
        ),
        "armature_count": sum(
            1 for obj in bpy.context.scene.objects if obj.type == "ARMATURE"
        ),
        "weighted_mesh_count": sum(1 for obj in meshes if obj.vertex_groups),
        "review_status": "REVIEW_ONLY_USER_APPROVAL_REQUIRED",
    }


def build_asset_lods(asset: dict) -> None:
    asset["source_dir"].mkdir(parents=True, exist_ok=True)
    asset["unity_dir"].mkdir(parents=True, exist_ok=True)
    report = {
        "asset_id": asset["asset_id"],
        "source": str(asset["source"].relative_to(ROOT)),
        "lods": {},
        "review_status": "REVIEW_ONLY_USER_APPROVAL_REQUIRED",
    }

    for suffix, ratio in LOD_RATIOS.items():
        reset_and_load(asset)
        before, after = simplify_meshes(ratio, suffix)
        result = validate_scene(asset, before, after, ratio)

        source_path = asset["source_dir"] / f"{asset['asset_id']}_{suffix}_REVIEW.blend"
        export_path = asset["unity_dir"] / f"{asset['asset_id']}_{suffix}_REVIEW.fbx"
        bpy.ops.wm.save_as_mainfile(filepath=str(source_path), compress=True)
        export_fbx(export_path)

        result["editable_source"] = str(source_path.relative_to(ROOT))
        result["unity_export"] = str(export_path.relative_to(ROOT))
        report["lods"][suffix] = result
        print(
            f"LOD_RESULT {asset['asset_id']} {suffix}: "
            f"{before} -> {after} triangles ({result['actual_ratio']:.2%})"
        )

    report_path = asset["source_dir"] / f"{asset['asset_id']}_LOD_R1_REVIEW.json"
    report_path.write_text(json.dumps(report, indent=2), encoding="utf-8")


def main() -> None:
    for asset in ASSETS:
        build_asset_lods(asset)
    print("LEVEL01_TIER_A_LODS_COMPLETE")


if __name__ == "__main__":
    main()
