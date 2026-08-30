import bpy
import os


ROOT = "/Users/apple/Desktop/أسد البحار Lion of the Seas"
OUTPUT = os.path.join(
    ROOT, "Assets/_Project/Art/Environment/"
    "L01-ENV-015_Fortress_R6_Modular_R5_VISIBLE_REVIEW.fbx"
)


def main():
    exportables = []
    for obj in bpy.context.scene.objects:
        if obj.type != "MESH":
            continue
        if obj.name.startswith("SOURCE_") or obj.name.startswith("REVIEW_"):
            continue
        if obj.hide_render:
            continue
        exportables.append(obj)

    bpy.ops.object.select_all(action="DESELECT")
    for obj in exportables:
        obj.hide_set(False)
        obj.select_set(True)
    bpy.context.view_layer.objects.active = exportables[0]
    bpy.ops.export_scene.fbx(
        filepath=OUTPUT,
        use_selection=True,
        apply_unit_scale=True,
        use_mesh_modifiers=True,
        add_leaf_bones=False,
        path_mode="COPY",
        embed_textures=True,
    )
    print(f"VISIBLE_EXPORT objects={len(exportables)} output={OUTPUT}")


if __name__ == "__main__":
    main()
