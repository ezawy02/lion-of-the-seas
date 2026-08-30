"""Build the local-only Level 01 wooden landing gangway review asset."""

from pathlib import Path

import bpy


ROOT = Path(__file__).resolve().parents[2]
OUTPUT = ROOT / "Assets/_Project/Art/Environment/L01-ENV-013_Wooden_Landing_Gangway_REVIEW.fbx"
SOURCE = ROOT / "ArtSource/Blender/Revisions/L01-ENV-013_Wooden_Landing_Gangway_REVIEW.blend"


def clear_scene():
    bpy.ops.object.select_all(action="SELECT")
    bpy.ops.object.delete(use_global=False)


def wood_material():
    material = bpy.data.materials.new("L01-PRP-011_Wooden_Siege_Scaffold")
    material.diffuse_color = (0.24, 0.105, 0.035, 1.0)
    material.roughness = 0.72
    return material


def box(name, location, dimensions, material, bevel=0.04):
    bpy.ops.mesh.primitive_cube_add(location=location)
    value = bpy.context.object
    value.name = name
    value.dimensions = dimensions
    bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    value.data.materials.append(material)
    modifier = value.modifiers.new("Small worn edge", "BEVEL")
    modifier.width = bevel
    modifier.segments = 2
    bpy.context.view_layer.objects.active = value
    bpy.ops.object.modifier_apply(modifier=modifier.name)
    return value


def build():
    clear_scene()
    material = wood_material()
    length = 22.4
    plank_pitch = 0.8
    plank_count = 28

    for index in range(plank_count):
        y = -length * 0.5 + plank_pitch * 0.5 + index * plank_pitch
        plank = box(
            f"Gangway_Plank_{index + 1:02d}",
            (0.0, y, 0.0),
            (5.2, 0.7, 0.24),
            material,
            0.045,
        )
        plank.rotation_euler[1] = (index % 5 - 2) * 0.0025

    for side in (-1.75, 1.75):
        box("Gangway_Longitudinal_Beam", (side, 0.0, -0.32), (0.34, length, 0.42), material, 0.035)

    for row, y in enumerate((-9.6, -4.8, 0.0, 4.8, 9.6)):
        box(f"Gangway_Cross_Beam_{row + 1:02d}", (0.0, y, -0.48), (5.55, 0.32, 0.35), material, 0.035)
        for side in (-2.1, 2.1):
            box("Gangway_Support_Post", (side, y, -1.05), (0.34, 0.34, 1.55), material, 0.035)

    bpy.ops.object.select_all(action="SELECT")
    bpy.context.view_layer.objects.active = bpy.context.selected_objects[0]
    bpy.ops.object.join()
    gangway = bpy.context.object
    gangway.name = "L01-ENV-013_Wooden_Landing_Gangway_REVIEW"
    gangway.data.name = gangway.name + "_Mesh"
    bpy.ops.object.origin_set(type="ORIGIN_GEOMETRY", center="BOUNDS")
    gangway.location = (0.0, 0.0, 0.0)
    bpy.ops.object.transform_apply(location=True, rotation=True, scale=True)

    OUTPUT.parent.mkdir(parents=True, exist_ok=True)
    SOURCE.parent.mkdir(parents=True, exist_ok=True)
    bpy.ops.wm.save_as_mainfile(filepath=str(SOURCE))
    bpy.ops.export_scene.fbx(
        filepath=str(OUTPUT),
        use_selection=True,
        object_types={"MESH"},
        apply_unit_scale=True,
        bake_space_transform=False,
        axis_forward="-Z",
        axis_up="Y",
        add_leaf_bones=False,
        path_mode="STRIP",
    )
    print(f"Wrote {OUTPUT}")


if __name__ == "__main__":
    build()
