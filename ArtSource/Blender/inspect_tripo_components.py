import bpy
import bmesh


SOURCE = (
    "/Users/apple/Desktop/أسد البحار Lion of the Seas/ArtSource/Blender/Incoming/Tripo/"
    "L01-ENV-016_Fortress_R6_ApprovedConcept_R1_REVIEW/"
    "L01-ENV-016_Fortress_R6_ApprovedConcept_R1_REVIEW_PBR.glb"
)

bpy.ops.import_scene.gltf(filepath=SOURCE)
obj = next(
    item for item in bpy.context.scene.objects
    if item.type == "MESH" and len(item.data.vertices) > 100
)
mesh = bmesh.new()
mesh.from_mesh(obj.data)
unseen = set(mesh.verts)
components = []
while unseen:
    seed = unseen.pop()
    stack = [seed]
    component = [seed]
    while stack:
        vertex = stack.pop()
        for edge in vertex.link_edges:
            other = edge.other_vert(vertex)
            if other in unseen:
                unseen.remove(other)
                stack.append(other)
                component.append(other)
    components.append(component)

for index, component in enumerate(sorted(components, key=len, reverse=True)[:30]):
    points = [obj.matrix_world @ vertex.co for vertex in component]
    low = tuple(round(min(point[axis] for point in points), 3) for axis in range(3))
    high = tuple(round(max(point[axis] for point in points), 3) for axis in range(3))
    print("COMP", index, "verts", len(component), "low", low, "high", high)
