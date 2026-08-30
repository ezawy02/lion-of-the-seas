"""Report geometry, bounds, and materials for Level 01 fortress source FBXs."""

from __future__ import annotations

import json
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
ASSET_ROOT = ROOT / "Assets/_Project/Art/Environment"
SOURCES = (
    "L01-ENV-001_Fortress_Wall_Module_Optimized.fbx",
    "L01-ENV-002_Fortress_Tower_Module_Optimized.fbx",
    "L01-ENV-003_Fortress_Main_Gate_Module_Optimized.fbx",
    "L01-ENV-006_Palm_Tree_Cluster_Optimized.fbx",
    "L01-ENV-007_Limestone_Rock_Cluster_Optimized.fbx",
    "L01-PRP-001_Shore_Cannon_Optimized.fbx",
    "L01-PRP-002_Lion_Wave_Banner_Optimized.fbx",
    "L01-PRP-011_Wooden_Siege_Scaffold_Optimized.fbx",
)


def clear_scene() -> None:
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def inspect_source(filename: str) -> dict[str, object]:
    clear_scene()
    path = ASSET_ROOT / filename
    bpy.ops.import_scene.fbx(filepath=str(path))
    meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
    points = [obj.matrix_world @ Vector(corner) for obj in meshes for corner in obj.bound_box]
    low = Vector(tuple(min(point[index] for point in points) for index in range(3)))
    high = Vector(tuple(max(point[index] for point in points) for index in range(3)))
    triangles = sum(len(polygon.vertices) - 2 for obj in meshes for polygon in obj.data.polygons)
    materials = sorted({slot.material.name for obj in meshes for slot in obj.material_slots if slot.material})
    return {
        "asset": filename,
        "meshes": len(meshes),
        "triangles": triangles,
        "bounds": [round(value, 4) for value in (high - low)],
        "minimum": [round(value, 4) for value in low],
        "maximum": [round(value, 4) for value in high],
        "materials": materials,
    }


print(json.dumps([inspect_source(source) for source in SOURCES], indent=2))
