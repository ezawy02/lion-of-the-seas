import bpy, os, zipfile, shutil, math, json, csv
from mathutils import Vector

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), '..', '..'))
INCOMING = os.path.join(ROOT, 'ArtSource', 'Blender', 'Incoming')
ZIP_DIR = os.path.join(INCOMING, 'source_zips')
OUT_ROOT = os.path.join(ROOT, 'Assets', '_Project', 'Art')
EXTRACT = os.path.join(INCOMING, 'extracted')
os.makedirs(EXTRACT, exist_ok=True)

ASSETS = {
    1: ('L01-CHR-001_Hayreddin_Barbarossa', 'Characters', 'character', 30000, True),
    2: ('L01-CHR-002_Friendly_Marine', 'Characters', 'character', 10000, True),
    3: ('L01-CHR-003_Hostile_Infantry', 'Characters', 'character', 10000, True),
    4: ('L01-PRP-001_Shore_Cannon', 'Environment', 'prop', 2000, False),
    5: ('L01-SHP-001_Flagship', 'Ships', 'flagship', 35000, False),
    6: ('L01-SHP-002_Landing_Craft', 'Ships', 'landing_craft', 3000, False),
    7: ('L01-SHP-003_Hostile_Patrol_Boat', 'Ships', 'landing_craft', 3000, False),
    8: ('L01-PRP-002_Lion_Wave_Banner', 'Environment', 'prop', 1500, False),
}

def clear():
    bpy.ops.object.mode_set(mode='OBJECT') if bpy.context.object and bpy.context.object.mode != 'OBJECT' else None
    bpy.ops.object.select_all(action='SELECT')
    bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.armatures, bpy.data.curves, bpy.data.cameras, bpy.data.lights):
        for block in list(datablocks):
            if block.users == 0: datablocks.remove(block)

def import_source(path):
    ext = os.path.splitext(path)[1].lower()
    if ext == '.glb': bpy.ops.import_scene.gltf(filepath=path)
    elif ext == '.fbx': bpy.ops.import_scene.fbx(filepath=path, use_image_search=True)
    else: raise RuntimeError(path)
    return [o for o in bpy.context.scene.objects if o.type == 'MESH']

def tri_count(meshes):
    return sum(len(p.vertices) - 2 for o in meshes for p in o.data.polygons)

def bounds(meshes):
    pts=[]
    for o in meshes:
        pts += [o.matrix_world @ v.co for v in o.data.vertices]
    if not pts: return Vector((0,0,0)), Vector((1,1,1))
    lo=Vector((min(p.x for p in pts),min(p.y for p in pts),min(p.z for p in pts)))
    hi=Vector((max(p.x for p in pts),max(p.y for p in pts),max(p.z for p in pts)))
    return lo,hi

def decimate(meshes, target):
    before=tri_count(meshes)
    if before <= target: return before, tri_count(meshes)
    ratio=max(0.01, min(1.0, target/float(before)))
    for o in meshes:
        bpy.context.view_layer.objects.active=o; o.select_set(True)
        m=o.modifiers.new('Mobile_Optimization_Decimate','DECIMATE'); m.ratio=ratio; m.use_collapse_triangulate=True
        bpy.ops.object.modifier_apply(modifier=m.name)
        o.select_set(False)
    return before, tri_count(meshes)

def add_rig(meshes, name):
    lo,hi=bounds(meshes); h=max(hi.z-lo.z, 1.0); cx=(lo.x+hi.x)/2; cy=(lo.y+hi.y)/2
    bpy.ops.object.armature_add(enter_editmode=True, location=(0,0,0))
    arm=bpy.context.object; arm.name=name+'_Rig'; arm.data.name=name+'_Rig'
    eb=arm.data.edit_bones[0]; eb.name='root'; eb.head=(cx,cy,lo.z); eb.tail=(cx,cy,lo.z+h*.20)
    def bone(n, a, b, parent='root'):
        x=arm.data.edit_bones.new(n); x.head=a; x.tail=b; x.parent=arm.data.edit_bones.get(parent); return x
    z0=lo.z; z1=lo.z+h*.20; z2=lo.z+h*.42; z3=lo.z+h*.62; z4=lo.z+h*.78; z5=hi.z
    bone('spine',(cx,cy,z1),(cx,cy,z2)); bone('chest',(cx,cy,z2),(cx,cy,z3),'spine'); bone('neck',(cx,cy,z3),(cx,cy,z4),'chest'); bone('head',(cx,cy,z4),(cx,cy,z5),'neck')
    shoulder=max((hi.x-lo.x)*.22, h*.08); hip=max((hi.x-lo.x)*.14, h*.05)
    for side,s in [('L',-1),('R',1)]:
        bone('upper_arm_'+side,(cx+s*shoulder,cy,z3),(cx+s*shoulder*1.25,cy,z2*.98+z0*.02),'chest')
        bone('lower_arm_'+side,(cx+s*shoulder*1.25,cy,z2*.98+z0*.02),(cx+s*shoulder*1.35,cy,z2*.75+z0*.25),'upper_arm_'+side)
        bone('hand_'+side,(cx+s*shoulder*1.35,cy,z2*.75+z0*.25),(cx+s*shoulder*1.35,cy,z2*.60+z0*.40),'lower_arm_'+side)
        bone('upper_leg_'+side,(cx+s*hip,cy,z1),(cx+s*hip,cy,z0+h*.07),'root')
        bone('lower_leg_'+side,(cx+s*hip,cy,z0+h*.07),(cx+s*hip,cy,z0),'upper_leg_'+side)
    bpy.ops.object.mode_set(mode='OBJECT')
    arm.show_in_front=True
    for o in meshes:
        if not o.data.vertices: continue
        mod=o.modifiers.new('Humanoid_Rig','ARMATURE'); mod.object=arm
        # Stable blockout weights by height, with a small root influence.
        groups={b.name:o.vertex_groups.new(name=b.name) for b in arm.data.bones}
        for v in o.data.vertices:
            p=o.matrix_world @ v.co; t=(p.z-lo.z)/h
            if t<.15: bn='root'
            elif t<.42: bn='spine'
            elif t<.62: bn='chest'
            elif t<.78: bn='neck'
            else: bn='head'
            groups[bn].add([v.index],1.0,'REPLACE')
    return arm

def export(path, meshes, arm=None):
    os.makedirs(os.path.dirname(path), exist_ok=True)
    bpy.ops.object.select_all(action='DESELECT')
    for o in meshes: o.select_set(True)
    if arm: arm.select_set(True); bpy.context.view_layer.objects.active=arm
    else: bpy.context.view_layer.objects.active=meshes[0]
    asset_id=os.path.basename(path).replace('_Rigged_Optimized.fbx','').replace('_Optimized.fbx','')
    texture_root=os.path.join(OUT_ROOT,'Textures','Level01'); os.makedirs(texture_root,exist_ok=True)
    roles={'texture_diffuse':'BaseColor','texture_metallic-texture_roughness':'MetallicRoughness','texture_metallic':'Metallic','texture_roughness':'Roughness','texture_normal':'Normal','texture_emissive':'Emissive'}
    for img in bpy.data.images:
        source_name=os.path.splitext(img.name)[0]
        if source_name not in roles or img.size[0] == 0: continue
        stem=asset_id+'_'+roles[source_name]; img.name=stem; img.filepath_raw=os.path.join(texture_root,stem+'.png'); img.file_format='PNG'; img.save()
    bpy.ops.export_scene.fbx(filepath=path, use_selection=True, object_types={'MESH','ARMATURE'}, add_leaf_bones=False, bake_anim=False, apply_scale_options='FBX_SCALE_ALL', path_mode='RELATIVE', embed_textures=False)

def render_character_preview(name, meshes, arm):
    lo,hi=bounds(meshes); center=(lo+hi)*0.5; size=max((hi-lo).length,1.0)
    scene=bpy.context.scene; scene.render.engine='BLENDER_EEVEE'; scene.render.resolution_x=768; scene.render.resolution_y=1024; scene.render.resolution_percentage=100
    scene.render.image_settings.file_format='PNG'; scene.world.color=(0.012,0.02,0.04)
    bpy.ops.object.camera_add(location=(center.x+size*.85,center.y-size*2.6,center.z+size*.12))
    cam=bpy.context.object; scene.camera=cam; cam.data.lens=58
    direction=Vector(center)-cam.location; cam.rotation_euler=direction.to_track_quat('-Z','Y').to_euler()
    bpy.ops.object.light_add(type='AREA', location=(center.x+size*.8,center.y-size*1.3,center.z+size*1.5)); key=bpy.context.object; key.data.energy=900; key.data.shape='DISK'; key.data.size=size*1.2; key.rotation_euler=(Vector(center)-key.location).to_track_quat('-Z','Y').to_euler()
    bpy.ops.object.light_add(type='AREA', location=(center.x-size,center.y-size*.7,center.z+size*.5)); fill=bpy.context.object; fill.data.energy=450; fill.data.size=size; fill.rotation_euler=(Vector(center)-fill.location).to_track_quat('-Z','Y').to_euler()
    if arm: arm.hide_render=True
    outdir=os.path.join(INCOMING,'CharacterPreviews'); os.makedirs(outdir,exist_ok=True); scene.render.filepath=os.path.join(outdir,name+'.png'); bpy.ops.render.render(write_still=True)

rows=[]
for idx,(name,folder,kind,target,rigged) in ASSETS.items():
    clear(); zpath=os.path.join(ZIP_DIR, f'{idx}.zip'); ex=os.path.join(EXTRACT,str(idx)); os.makedirs(ex,exist_ok=True)
    with zipfile.ZipFile(zpath) as z: z.extractall(ex)
    candidates=[os.path.join(ex,f) for f in os.listdir(ex) if f.lower().endswith(('.glb','.fbx'))]
    # Prefer PBR, then base FBX, then any source.
    candidates.sort(key=lambda p: (('pbr' not in os.path.basename(p).lower()), ('basic' not in os.path.basename(p).lower()), p))
    src=candidates[0]; meshes=import_source(src)
    before=tri_count(meshes); b0,b1=decimate(meshes,target)
    arm=add_rig(meshes,name) if rigged else None
    suffix='_Rigged_Optimized' if rigged else '_Optimized'
    out=os.path.join(OUT_ROOT,folder,name+suffix+'.fbx')
    export(out,meshes,arm)
    if rigged: render_character_preview(name,meshes,arm)
    rows.append({'zip':f'{idx}.zip','source_file':os.path.relpath(src,ROOT),'asset':name,'kind':kind,'original_triangles':before,'optimized_triangles':b1,'target_triangles':target,'rig_status':'simple humanoid blockout rig + height weights' if rigged else 'not applicable','output':os.path.relpath(out,ROOT),'limitations':'Source geometry/materials preserved where FBX permits; rig is blockout, no authored animation; user Unity review required.'})

manifest=os.path.join(OUT_ROOT,'ART_ASSET_OPTIMIZATION_MANIFEST.csv')
with open(manifest,'w',newline='',encoding='utf-8') as f:
    w=csv.DictWriter(f,fieldnames=rows[0].keys()); w.writeheader(); w.writerows(rows)
with open(os.path.join(OUT_ROOT,'ART_ASSET_OPTIMIZATION_MANIFEST.md'),'w',encoding='utf-8') as f:
    f.write('# Mobile asset preparation manifest\n\nGenerated locally from the eight numbered source ZIPs. Originals are preserved in `ArtSource/Blender/Incoming/source_zips`. These are preparation/blockout outputs only; no asset is final or approved.\n\n')
    for r in rows: f.write(f"- `{r['zip']}` → `{r['asset']}`: {r['original_triangles']} → {r['optimized_triangles']} triangles; rig: {r['rig_status']}; output `{r['output']}`.\n")
print(json.dumps(rows,indent=2))
