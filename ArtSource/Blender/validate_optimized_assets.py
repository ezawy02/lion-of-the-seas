import bpy, os, json
ROOT=os.path.abspath(os.path.join(os.path.dirname(__file__),'..','..'))
paths=[]
for d,_,fs in os.walk(os.path.join(ROOT,'Assets','_Project','Art')):
    for f in fs:
        if f.endswith('_Optimized.fbx'): paths.append(os.path.join(d,f))
rows=[]
for p in sorted(paths):
    bpy.ops.object.select_all(action='SELECT'); bpy.ops.object.delete(use_global=False)
    bpy.ops.import_scene.fbx(filepath=p, use_image_search=True)
    meshes=[o for o in bpy.context.scene.objects if o.type=='MESH']
    arms=[o for o in bpy.context.scene.objects if o.type=='ARMATURE']
    tris=sum(len(poly.vertices)-2 for o in meshes for poly in o.data.polygons)
    rows.append({'file':os.path.relpath(p,ROOT),'triangles':tris,'mesh_objects':len(meshes),'armatures':len(arms),'bones':sum(len(a.data.bones) for a in arms),'materials':len({m.name for o in meshes for m in o.data.materials if m})})
print(json.dumps(rows,indent=2))
