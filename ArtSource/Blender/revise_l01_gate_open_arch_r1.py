"""Create a local REVIEW revision of L01-GAT-001 with a clear sailing arch."""

from __future__ import annotations

import json
from collections import defaultdict
from pathlib import Path

import bpy
from mathutils import Vector


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Assets/_Project/Art/Environment/L01-GAT-001_Multiplier_Gate_Arch_Buoy_Optimized.fbx"
REVISION = ROOT / "ArtSource/Blender/Environment/L01-GAT-001_Multiplier_Gate_Arch_Buoy/OpenArch_R1_REVIEW"
ASSET_ID = "L01-GAT-001_Multiplier_Gate_OpenArch_R1_REVIEW"
BLEND_OUT = REVISION / f"{ASSET_ID}.blend"
FBX_OUT = ROOT / f"Assets/_Project/Art/Environment/{ASSET_ID}.fbx"
PREVIEW_OUT = REVISION / f"{ASSET_ID}.png"
REPORT_OUT = REVISION / f"{ASSET_ID}_Report.json"


def connected_components(mesh: bpy.types.Mesh) -> list[list[int]]:
    adjacency: dict[int, list[int]] = defaultdict(list)
    for edge in mesh.edges:
        a, b = edge.vertices
        adjacency[a].append(b)
        adjacency[b].append(a)
    visited: set[int] = set()
    result: list[list[int]] = []
    for vertex in mesh.vertices:
        if vertex.index in visited:
            continue
        stack = [vertex.index]
        visited.add(vertex.index)
        component: list[int] = []
        while stack:
            index = stack.pop()
            component.append(index)
            for neighbor in adjacency[index]:
                if neighbor not in visited:
                    visited.add(neighbor)
                    stack.append(neighbor)
        result.append(component)
    return result


def clear_center_sailing_lane(obj: bpy.types.Object) -> tuple[int, int]:
    removed: list[int] = []
    removed_components = 0
    for component in connected_components(obj.data):
        points = [obj.data.vertices[index].co for index in component]
        minimum_x = min(point.x for point in points)
        maximum_x = max(point.x for point in points)
        maximum_z = max(point.z for point in points)
        center_x = (minimum_x + maximum_x) * 0.5
        # The source's center buoy occupies the lower-middle third. Preserve the
        # top crest, curtains, and both side columns while opening the water lane.
        if abs(center_x) < 0.45 and maximum_z < 1.15:
            removed.extend(component)
            removed_components += 1

    bpy.context.view_layer.objects.active = obj
    obj.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.select_all(action="DESELECT")
    bpy.ops.object.mode_set(mode="OBJECT")
    for index in removed:
        obj.data.vertices[index].select = True
    bpy.ops.object.mode_set(mode="EDIT")
    bpy.ops.mesh.delete(type="VERT")
    bpy.ops.object.mode_set(mode="OBJECT")
    return removed_components, len(removed)


def look_at(obj: bpy.types.Object, target: Vector) -> None:
    obj.rotation_euler = (target - obj.location).to_track_quat("-Z", "Y").to_euler()


def render_preview(obj: bpy.types.Object) -> None:
    bpy.ops.mesh.primitive_plane_add(size=8, location=(0, 0, -0.03))
    ground = bpy.context.object
    ground.name = "REVIEW_WaterPlane"
    material = bpy.data.materials.new("REVIEW_Water")
    material.diffuse_color = (0.02, 0.23, 0.30, 1)
    ground.data.materials.append(material)

    world = bpy.context.scene.world or bpy.data.worlds.new("REVIEW_World")
    bpy.context.scene.world = world
    world.color = (0.10, 0.24, 0.34)
    for location, energy, size in (((-3, -4, 5), 1100, 4), ((4, 0, 3), 700, 3)):
        bpy.ops.object.light_add(type="AREA", location=location)
        light = bpy.context.object
        light.data.energy = energy
        light.data.shape = "DISK"
        light.data.size = size
        look_at(light, Vector((0, 0, 0.9)))

    bpy.ops.object.camera_add(location=(3.0, -5.5, 2.7))
    camera = bpy.context.object
    camera.data.lens = 58
    look_at(camera, Vector((0, 0, 0.9)))
    bpy.context.scene.camera = camera

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 900
    scene.render.resolution_y = 900
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.filepath = str(PREVIEW_OUT)
    scene.view_settings.look = "AgX - Medium High Contrast"
    bpy.ops.render.render(write_still=True)


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(SOURCE))
meshes = [obj for obj in bpy.context.scene.objects if obj.type == "MESH"]
if len(meshes) != 1:
    raise RuntimeError(f"Expected one gate mesh, found {len(meshes)}.")
gate = meshes[0]
gate.name = ASSET_ID
removed_components, removed_vertices = clear_center_sailing_lane(gate)
if removed_vertices < 500:
    raise RuntimeError("Center-lane cleanup removed too little geometry to open the arch.")

REVISION.mkdir(parents=True, exist_ok=True)
bpy.ops.object.select_all(action="DESELECT")
gate.select_set(True)
bpy.context.view_layer.objects.active = gate
bpy.ops.export_scene.fbx(
    filepath=str(FBX_OUT),
    use_selection=True,
    object_types={"MESH"},
    axis_forward="-Z",
    axis_up="Y",
    apply_scale_options="FBX_SCALE_ALL",
    add_leaf_bones=False,
    bake_anim=False,
    mesh_smooth_type="FACE",
    path_mode="RELATIVE",
    embed_textures=False,
)
render_preview(gate)
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_OUT), check_existing=False)

report = {
    "source": str(SOURCE.relative_to(ROOT)),
    "revision": "Open Arch R1 REVIEW",
    "preserves_source_materials": True,
    "removed_center_components": removed_components,
    "removed_center_vertices": removed_vertices,
    "blend": str(BLEND_OUT.relative_to(ROOT)),
    "fbx": str(FBX_OUT.relative_to(ROOT)),
    "preview": str(PREVIEW_OUT.relative_to(ROOT)),
    "status": "Unity and user visual review required",
}
REPORT_OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
