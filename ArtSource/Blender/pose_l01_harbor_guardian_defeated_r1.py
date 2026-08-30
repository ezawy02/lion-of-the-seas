"""Create the local REVIEW kneeling pose for the Level 01 Harbor Guardian."""

from __future__ import annotations

import json
import math
from collections import defaultdict
from pathlib import Path

import bpy
from mathutils import Matrix, Vector


ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "Assets/_Project/Art/Characters/L01-CHR-004_Harbor_Guardian_Boss_Rigged_Optimized.fbx"
REVISION = ROOT / "ArtSource/Blender/Characters/L01-CHR-004_Harbor_Guardian_Boss/DefeatedKneel_R1_REVIEW"
ASSET_ID = "L01-CHR-004_Harbor_Guardian_DefeatedKneel_R1_REVIEW"
BLEND_OUT = REVISION / f"{ASSET_ID}.blend"
PREVIEW_OUT = REVISION / f"{ASSET_ID}.png"
PREVIEW_FRONT_OUT = REVISION / f"{ASSET_ID}_Front.png"
PREVIEW_SIDE_OUT = REVISION / f"{ASSET_ID}_Side.png"
REPORT_OUT = REVISION / f"{ASSET_ID}_Report.json"
FBX_OUT = ROOT / f"Assets/_Project/Art/Characters/{ASSET_ID}.fbx"


def rotate(rig: bpy.types.Object, bone_name: str, x: float = 0, y: float = 0, z: float = 0) -> None:
    bone = rig.pose.bones[bone_name]
    bone.rotation_mode = "XYZ"
    bone.rotation_euler = tuple(math.radians(value) for value in (x, y, z))


def repair_leg_joints(rig: bpy.types.Object) -> None:
    """Place the malformed source leg bones at usable anatomical pivots."""
    bpy.context.view_layer.objects.active = rig
    rig.select_set(True)
    bpy.ops.object.mode_set(mode="EDIT")
    for suffix, x in (("L", -0.18), ("R", 0.18)):
        upper = rig.data.edit_bones[f"upper_leg_{suffix}"]
        lower = rig.data.edit_bones[f"lower_leg_{suffix}"]
        upper.head = (x, 0.0, 0.72)
        upper.tail = (x, 0.0, 0.38)
        lower.head = upper.tail
        lower.tail = (x, 0.0, 0.08)
        lower.use_connect = True
    bpy.ops.object.mode_set(mode="OBJECT")


def rebind_limb_weights(rig: bpy.types.Object, meshes: list[bpy.types.Object]) -> None:
    """Bind only disconnected leg pieces; preserve authored torso/cape weights."""
    for mesh in meshes:
        groups = {
            name: mesh.vertex_groups.get(name) or mesh.vertex_groups.new(name=name)
            for name in ("upper_leg_L", "lower_leg_L", "upper_leg_R", "lower_leg_R")
        }
        adjacency = defaultdict(list)
        for edge in mesh.data.edges:
            a, b = edge.vertices
            adjacency[a].append(b)
            adjacency[b].append(a)

        visited = set()
        weighted_counts = defaultdict(int)
        for vertex in mesh.data.vertices:
            if vertex.index in visited:
                continue
            stack = [vertex.index]
            visited.add(vertex.index)
            component = []
            while stack:
                index = stack.pop()
                component.append(index)
                for neighbor in adjacency[index]:
                    if neighbor not in visited:
                        visited.add(neighbor)
                        stack.append(neighbor)

            points = [mesh.data.vertices[index].co for index in component]
            minimum_z = min(point.z for point in points)
            maximum_z = max(point.z for point in points)
            minimum_x = min(point.x for point in points)
            maximum_x = max(point.x for point in points)
            center_x = (minimum_x + maximum_x) * 0.5
            center_z = (minimum_z + maximum_z) * 0.5
            is_leg_piece = (
                minimum_z < 0.50 and maximum_z < 0.95 and
                -0.32 < center_x < 0.66 and maximum_x - minimum_x < 0.40
            )
            if not is_leg_piece:
                continue

            side = "L" if center_x < 0.14 else "R"
            segment = "lower" if center_z < 0.36 else "upper"
            target_name = f"{segment}_leg_{side}"
            for group in mesh.vertex_groups:
                group.remove(component)
            groups[target_name].add(component, 1.0, "REPLACE")
            weighted_counts[target_name] += len(component)

        print("Manual leg weights:", dict(weighted_counts))

    missing_groups = set()
    for bone_name in (
        "upper_leg_L", "lower_leg_L", "upper_leg_R", "lower_leg_R",
    ):
        for mesh in meshes:
            group = mesh.vertex_groups.get(bone_name)
            if group is None or not any(
                assignment.group == group.index and assignment.weight > 0.001
                for vertex in mesh.data.vertices for assignment in vertex.groups
            ):
                missing_groups.add(bone_name)
    if missing_groups:
        raise RuntimeError(f"Manual leg binding missed groups: {sorted(missing_groups)}")


def look_at(obj: bpy.types.Object, target: Vector) -> None:
    obj.rotation_euler = (target - obj.location).to_track_quat("-Z", "Y").to_euler()


def setup_preview(meshes: list[bpy.types.Object]) -> None:
    world = bpy.context.scene.world or bpy.data.worlds.new("REVIEW_World")
    bpy.context.scene.world = world
    world.color = (0.008, 0.012, 0.018)
    world.use_nodes = True
    world.node_tree.nodes["Background"].inputs["Color"].default_value = (0.008, 0.012, 0.018, 1)
    world.node_tree.nodes["Background"].inputs["Strength"].default_value = 0.16

    bpy.ops.mesh.primitive_plane_add(size=20, location=(0, 0, 0))
    ground = bpy.context.object
    ground.name = "REVIEW_Ground"
    material = bpy.data.materials.new("REVIEW_Ground_Material")
    material.diffuse_color = (0.055, 0.065, 0.075, 1)
    ground.data.materials.append(material)

    for location, energy, size, color in (
        ((-3.5, -4.5, 6.0), 1200, 4.0, (1.0, 0.78, 0.58)),
        ((4.0, -1.5, 3.5), 850, 3.0, (0.38, 0.68, 1.0)),
        ((0.0, 3.0, 5.0), 1000, 2.5, (1.0, 0.92, 0.72)),
    ):
        bpy.ops.object.light_add(type="AREA", location=location)
        light = bpy.context.object
        light.data.energy = energy
        light.data.shape = "DISK"
        light.data.size = size
        light.data.color = color
        look_at(light, Vector((0, 0, 1.0)))

    bounds = [corner for mesh in meshes for corner in [mesh.matrix_world @ Vector(c) for c in mesh.bound_box]]
    minimum = Vector((min(v.x for v in bounds), min(v.y for v in bounds), min(v.z for v in bounds)))
    maximum = Vector((max(v.x for v in bounds), max(v.y for v in bounds), max(v.z for v in bounds)))
    center = (minimum + maximum) * 0.5
    height = maximum.z - minimum.z
    bpy.ops.object.camera_add(location=(3.2, -6.5, max(2.2, height * 0.78)))
    camera = bpy.context.object
    camera.data.lens = 66
    look_at(camera, Vector((center.x, center.y, minimum.z + height * 0.46)))
    bpy.context.scene.camera = camera

    scene = bpy.context.scene
    scene.render.engine = "BLENDER_EEVEE"
    scene.render.resolution_x = 900
    scene.render.resolution_y = 1100
    scene.render.resolution_percentage = 100
    scene.render.image_settings.file_format = "PNG"
    scene.render.film_transparent = False
    scene.render.filepath = str(PREVIEW_OUT)
    scene.view_settings.look = "AgX - Medium High Contrast"
    bpy.ops.render.render(write_still=True)

    camera.location = (0.0, -7.0, max(2.0, height * 0.68))
    look_at(camera, Vector((center.x, center.y, minimum.z + height * 0.40)))
    scene.render.filepath = str(PREVIEW_FRONT_OUT)
    bpy.ops.render.render(write_still=True)

    camera.location = (7.0, 0.0, max(2.0, height * 0.68))
    look_at(camera, Vector((center.x, center.y, minimum.z + height * 0.40)))
    scene.render.filepath = str(PREVIEW_SIDE_OUT)
    bpy.ops.render.render(write_still=True)


bpy.ops.wm.read_factory_settings(use_empty=True)
bpy.ops.import_scene.fbx(filepath=str(SOURCE))
rigs = [obj for obj in bpy.context.scene.objects if obj.type == "ARMATURE"]
meshes = [
    obj for obj in bpy.context.scene.objects
    if obj.type == "MESH" and any(modifier.type == "ARMATURE" for modifier in obj.modifiers)
]
if len(rigs) != 1 or not meshes:
    raise RuntimeError("Expected one Harbor Guardian armature and at least one mesh.")

rig = rigs[0]
required = {
    "root", "spine", "chest", "neck", "head",
    "upper_leg_L", "lower_leg_L", "upper_leg_R", "lower_leg_R",
    "upper_arm_L", "lower_arm_L", "upper_arm_R", "lower_arm_R",
}
missing = sorted(required.difference(rig.pose.bones.keys()))
if missing:
    raise RuntimeError(f"Harbor Guardian rig is missing bones: {missing}")

repair_leg_joints(rig)
rebind_limb_weights(rig, meshes)

# Both-knees surrender pose: thighs fold forward and the lower legs fold back so
# the pelvis settles above the heels. The torso remains visibly bowed.
rig.pose.bones["root"].location.z = -0.40
rotate(rig, "spine", x=17)
rotate(rig, "chest", x=12, z=-4)
rotate(rig, "neck", x=13)
rotate(rig, "head", x=15, z=5)
rotate(rig, "upper_leg_L", x=-72, y=-3, z=-6)
rotate(rig, "lower_leg_L", x=165)
rotate(rig, "upper_leg_R", x=-72, y=3, z=6)
rotate(rig, "lower_leg_R", x=165)
rotate(rig, "upper_arm_L", x=-38, y=-4, z=-10)
rotate(rig, "lower_arm_L", x=-40, z=-8)
rotate(rig, "upper_arm_R", x=-55, y=4, z=12)
rotate(rig, "lower_arm_R", x=-32, z=8)

bpy.context.view_layer.update()
for bone_name in ("upper_leg_L", "lower_leg_L", "upper_leg_R", "lower_leg_R"):
    bone = rig.pose.bones[bone_name]
    print(bone_name, "head", tuple(round(value, 3) for value in bone.head),
          "tail", tuple(round(value, 3) for value in bone.tail))

REVISION.mkdir(parents=True, exist_ok=True)

# Bake the visible deformation to static meshes. The editable rig and pose remain
# in the .blend, while Unity receives exactly the reviewed kneeling silhouette.
depsgraph = bpy.context.evaluated_depsgraph_get()
baked_meshes = []
for mesh in meshes:
    evaluated = mesh.evaluated_get(depsgraph)
    baked_data = bpy.data.meshes.new_from_object(evaluated, depsgraph=depsgraph)
    baked = bpy.data.objects.new(mesh.name + "_DefeatedKneel", baked_data)
    bpy.context.scene.collection.objects.link(baked)
    baked.matrix_world = mesh.matrix_world.copy()
    baked.data.transform(baked.matrix_world)
    baked.matrix_world = Matrix.Identity(4)
    baked_meshes.append(baked)

vertices = [vertex.co for mesh in baked_meshes for vertex in mesh.data.vertices]
minimum_z = min(vertex.z for vertex in vertices)
center_x = (min(vertex.x for vertex in vertices) + max(vertex.x for vertex in vertices)) * 0.5
center_y = (min(vertex.y for vertex in vertices) + max(vertex.y for vertex in vertices)) * 0.5
for baked in baked_meshes:
    baked.data.transform(Matrix.Translation(Vector((-center_x, -center_y, -minimum_z))))
bpy.context.view_layer.update()

for mesh in meshes:
    mesh.hide_render = True
rig.hide_render = True
bpy.ops.wm.save_as_mainfile(filepath=str(BLEND_OUT), check_existing=False)

bpy.ops.object.select_all(action="DESELECT")
for mesh in baked_meshes:
    mesh.select_set(True)
bpy.context.view_layer.objects.active = baked_meshes[0]
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

setup_preview(baked_meshes)
report = {
    "source": str(SOURCE.relative_to(ROOT)),
    "revision": "Defeated Kneel R1 REVIEW",
    "preserves_user_mesh": True,
    "pose": "both knees grounded, feet folded behind, torso bowed, arms lowered",
    "blend": str(BLEND_OUT.relative_to(ROOT)),
    "fbx": str(FBX_OUT.relative_to(ROOT)),
    "preview": str(PREVIEW_OUT.relative_to(ROOT)),
    "front_preview": str(PREVIEW_FRONT_OUT.relative_to(ROOT)),
    "side_preview": str(PREVIEW_SIDE_OUT.relative_to(ROOT)),
    "status": "Unity and user visual review required",
}
REPORT_OUT.write_text(json.dumps(report, indent=2), encoding="utf-8")
print(json.dumps(report, indent=2))
