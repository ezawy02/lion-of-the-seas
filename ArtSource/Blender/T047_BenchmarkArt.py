"""T047 — authored Level 1 benchmark art.

Blender 5.1.1 / metric units / local +Z forward.  Run with:
  Blender.app/Contents/MacOS/Blender --background --python T047_BenchmarkArt.py

The scene is deliberately generated from authored profiles and layered shapes rather
than stock primitive-only placeholders.  Each export collection is also tagged with
mobile budget and review metadata so the Unity handoff remains auditable.
"""
import bpy, bmesh, math, os, shutil
from mathutils import Vector
from math import sin, cos, pi

ROOT = os.path.abspath(os.path.join(os.path.dirname(__file__), "../.."))
OUT_SOURCE = os.path.join(ROOT, "ArtSource", "Blender")
OUT_ART = os.path.join(ROOT, "Assets", "_Project", "Art")
RENDER_DIR = os.path.join(OUT_SOURCE, "Renders")
for p in (OUT_SOURCE, RENDER_DIR, os.path.join(OUT_ART, "Ships"),
          os.path.join(OUT_ART, "Characters"), os.path.join(OUT_ART, "Environment")):
    os.makedirs(p, exist_ok=True)

PAL = {
    "navy": (0.025, 0.10, 0.19, 1), "teal": (0.02, 0.55, 0.57, 1),
    "foam": (0.72, 0.96, 0.91, 1), "ivory": (0.92, 0.83, 0.63, 1),
    "gold": (0.78, 0.39, 0.09, 1), "crimson": (0.56, 0.035, 0.045, 1),
    "charcoal": (0.055, 0.06, 0.075, 1), "copper": (0.47, 0.16, 0.075, 1),
    "orange": (0.95, 0.22, 0.035, 1), "sand": (0.72, 0.50, 0.27, 1),
    "limestone": (0.83, 0.70, 0.49, 1), "stone_dark": (0.28, 0.22, 0.18, 1),
    "sea": (0.015, 0.23, 0.30, 1), "vegetation": (0.08, 0.31, 0.20, 1),
    "violet": (0.24, 0.12, 0.54, 1), "sky": (0.20, 0.48, 0.68, 1),
}
MATS = {}

def mat(name, color, metallic=0.0, rough=0.55, emission=None):
    m = bpy.data.materials.new(name); m.diffuse_color = (*color[:3], 1)
    m.use_nodes = True
    bs = m.node_tree.nodes.get("Principled BSDF")
    bs.inputs["Base Color"].default_value = color
    bs.inputs["Roughness"].default_value = rough
    bs.inputs["Metallic"].default_value = metallic
    if emission:
        bs.inputs["Emission Color"].default_value = (*emission[:3], 1)
        bs.inputs["Emission Strength"].default_value = 2.5
    return m

def materials():
    for n, c in PAL.items(): MATS[n] = mat("MAT_T047_" + n, c)
    MATS["brass"] = mat("MAT_T047_brass", (0.75, 0.36, 0.06, 1), .8, .28)
    MATS["water_glow"] = mat("MAT_T047_water_glow", PAL["sea"], .1, .18, PAL["teal"])
    MATS["gate_glow"] = mat("MAT_T047_gate_glow", PAL["violet"], .1, .3, (0.38, .12, 1, 1))
    MATS["gate_face"] = mat("MAT_T047_gate_face", (.075, .025, .20, 1), .12, .28, (.12, .03, .30, 1))
    MATS["gate_number"] = mat("MAT_T047_gate_number", (1.0, .79, .24, 1), .25, .22, (1.0, .48, .05, 1))
    MATS["sail_teal"] = mat("MAT_T047_sail_teal", (0.06, .42, .41, 1), .0, .8)
    MATS["sail_cream"] = mat("MAT_T047_sail_cream", (.86, .76, .58, 1), .0, .82)
    MATS["danger"] = mat("MAT_T047_danger", PAL["orange"], 0, .32, PAL["orange"])

def clean():
    bpy.ops.object.select_all(action="SELECT"); bpy.ops.object.delete(use_global=False)
    for datablocks in (bpy.data.meshes, bpy.data.curves, bpy.data.materials, bpy.data.cameras, bpy.data.lights):
        for block in list(datablocks):
            if block.users == 0: datablocks.remove(block)

def collection(name, parent=None):
    c = bpy.data.collections.new(name); (parent or bpy.context.scene.collection).children.link(c)
    c["t047_asset"] = name; c["source"] = "T047_BenchmarkArt.py"; return c

def link(obj, c):
    for oc in list(obj.users_collection): oc.objects.unlink(obj)
    c.objects.link(obj); return obj

def mesh_obj(name, verts, faces, material, c, bevel=0.0):
    me = bpy.data.meshes.new(name + "_MESH"); me.from_pydata(verts, [], faces); me.update()
    ob = bpy.data.objects.new(name, me); c.objects.link(ob)
    if material: ob.data.materials.append(MATS[material] if isinstance(material, str) else material)
    if bevel:
        mod = ob.modifiers.new("Authored edge treatment", "BEVEL"); mod.width = bevel; mod.segments = 2
        bpy.context.view_layer.objects.active = ob; ob.select_set(True); bpy.ops.object.modifier_apply(modifier=mod.name); ob.select_set(False)
    return ob

def cube(name, loc, scale, material, c, bevel=.04, rot=None):
    bpy.ops.mesh.primitive_cube_add(location=loc, rotation=rot or (0,0,0)); ob = bpy.context.object; ob.name = name
    ob.dimensions = scale; bpy.ops.object.transform_apply(location=False, rotation=False, scale=True)
    link(ob, c); ob.data.materials.append(MATS[material]);
    if bevel:
        mod=ob.modifiers.new("Soft stylized edges", "BEVEL"); mod.width=bevel; mod.segments=2
        bpy.context.view_layer.objects.active=ob; bpy.ops.object.modifier_apply(modifier=mod.name)
    return ob

def cyl(name, loc, radius, depth, material, c, vertices=12, rot=None, bevel=.02):
    bpy.ops.mesh.primitive_cylinder_add(vertices=vertices, radius=radius, depth=depth, location=loc, rotation=rot or (0,0,0))
    ob=bpy.context.object; ob.name=name; link(ob,c); ob.data.materials.append(MATS[material])
    if bevel:
        mod=ob.modifiers.new("Edge roll", "BEVEL"); mod.width=bevel; mod.segments=2
        bpy.context.view_layer.objects.active=ob; bpy.ops.object.modifier_apply(modifier=mod.name)
    return ob

def uv(name, loc, scale, material, c, segments=16, rings=8):
    bpy.ops.mesh.primitive_uv_sphere_add(segments=segments, ring_count=rings, location=loc); ob=bpy.context.object; ob.name=name
    ob.scale=scale; bpy.ops.object.transform_apply(location=False, rotation=False, scale=True); link(ob,c); ob.data.materials.append(MATS[material]); return ob

def cone(name, loc, r1, r2, depth, material, c, vertices=10, rot=None):
    bpy.ops.mesh.primitive_cone_add(vertices=vertices, radius1=r1, radius2=r2, depth=depth, location=loc, rotation=rot or (0,0,0))
    ob=bpy.context.object; ob.name=name; link(ob,c); ob.data.materials.append(MATS[material]); return ob

def torus(name, loc, major, minor, material, c, rot=None, major_segments=24, minor_segments=8):
    bpy.ops.mesh.primitive_torus_add(major_radius=major, minor_radius=minor, major_segments=major_segments, minor_segments=minor_segments, location=loc, rotation=rot or (0,0,0))
    ob=bpy.context.object; ob.name=name; link(ob,c); ob.data.materials.append(MATS[material]); return ob

def profile_extrude(name, points, depth, material, c, bevel=.02):
    # points are clockwise XY; extruded along Z
    n=len(points); verts=[(x,y,-depth/2) for x,y in points]+[(x,y,depth/2) for x,y in points]
    faces=[tuple(range(n)), tuple(range(n,2*n))]
    for i in range(n): j=(i+1)%n; faces.append((i,j,n+j,n+i))
    return mesh_obj(name, verts, faces, material, c, bevel)

def hull(name, length, width, height, material, c, detail=1):
    # A tapered, faceted Mediterranean hull from authored waterline stations.
    stations=[(-length/2, .10, .0),(-length*.34,width*.47,height*.22),(-length*.10,width/2,height*.52),
              (length*.22,width*.46,height*.64),(length*.42,width*.32,height*.48),(length/2,.05,height*.18)]
    verts=[]; rings=4 if detail == "coarse" else (8 if detail else 5)
    for z, w, y in stations:
        for i in range(rings):
            a=2*pi*i/rings; verts.append((sin(a)*w, y + cos(a)*height*.32, z))
    faces=[]
    for s in range(len(stations)-1):
        for i in range(rings): j=(i+1)%rings; faces.append((s*rings+i,s*rings+j,(s+1)*rings+j,(s+1)*rings+i))
    faces += [tuple(range(rings-1,-1,-1)), tuple((len(stations)-1)*rings+i for i in range(rings))]
    return mesh_obj(name, verts, faces, material, c, 0 if detail == "coarse" else (.025 if detail else .012))

def sail(name, x, y, z, width, height, material, c, stripe=False):
    # Slightly curved triangular cloth panel, deliberately asymmetrical for forward lean.
    verts=[(x,y,z),(x+width*.82,y+height*.02,z+width*.08),(x+width*.72,y+height,z+width*.16),(x-width*.04,y+height*.84,z)]
    ob=mesh_obj(name,verts,[(0,1,2,3)],material,c,.015)
    if stripe:
        for i in range(3):
            t=(i+1)/4; verts2=[(x+width*.05,y+height*t,z+.01),(x+width*.66,y+height*t+.03,z+width*.13),(x+width*.64,y+height*(t+.05),z+width*.14),(x+width*.04,y+height*(t+.05),z+.01)]
            mesh_obj(name+"_Stripe"+str(i),verts2,[(0,1,2,3)],"sail_cream" if i%2==0 else "sail_teal",c,.005)
    return ob

def text_mesh(name, body, loc, size, material, c, extrude=.025):
    cu=bpy.data.curves.new(name+"_FONT", "FONT"); cu.body=body; cu.align_x="CENTER"; cu.align_y="CENTER"; cu.size=size; cu.extrude=extrude; cu.bevel_depth=0; cu.resolution_u=1; cu.render_resolution_u=1
    # Times New Roman Bold has a closed, serifed 4: substantially less arrow-like
    # than the open-top sans numeral in a tiny portrait review.
    bold_font="/System/Library/Fonts/Supplemental/Times New Roman Bold.ttf"
    if os.path.exists(bold_font): cu.font=bpy.data.fonts.load(bold_font, check_existing=True)
    ob=bpy.data.objects.new(name,cu); c.objects.link(ob); ob.location=loc; ob.rotation_euler=(0,0,0); ob.data.materials.append(MATS[material]); bpy.context.view_layer.objects.active=ob; ob.select_set(True); bpy.ops.object.convert(target="MESH"); ob.select_set(False); return ob

def annotate(ob, tier, budget, lod="LOD0"):
    ob["asset_tier"]=tier; ob["triangle_guide"]=budget; ob["lod"]=lod; ob["authored_style"]="Mediterranean corsair toy-like premium"; return ob

def marker(c, tier, budget, note):
    c["tier"] = tier; c["lod0_triangle_guide"] = budget; c["review_note"] = note
    c["license"] = "Original procedural geometry authored in T047_BenchmarkArt.py"

def count(c):
    return sum(len(o.data.loop_triangles) for o in c.objects if o.type=="MESH")

def flagship(root):
    c=collection("Flagship"); marker(c,"Tier A",35000,"Hero player flagship: navy hull, turquoise sail, brass trim, open forward silhouette.")
    h=annotate(hull("Flagship_LOD0_Hull",8.6,2.35,1.55,"navy",c,1),"Tier A",35000)
    # Hull runs along Z.  Keep deck/wales longitudinal so the player reads a ship,
    # never a white-wing aircraft, in the head-on portrait camera.
    cube("Flagship_Deck",(0,.82,0),(1.65,.28,6.5),"copper",c,.08)
    cube("Flagship_Wale",(0,.28,0),(2.0,.22,7.5),"teal",c,.06)
    for z in (-3.55,3.5):
        cyl("Flagship_BowTrim",(0,.83,z),.13,1.15,"brass",c,12,rot=(0,pi/2,0),bevel=.02)
    for z in (-2.4,-.5,1.6):
        for x in (-.88,.88):
            cyl("Flagship_Cannon",(x,.95,z),.13,.7,"brass",c,10,rot=(pi/2,0,0),bevel=.018)
            cube("Flagship_CannonMount",(x,.83,z),(0.34,.18,.5),"charcoal",c,.025)
    for z in (-1.2,1.2):
        cyl("Flagship_Mast",(0,2.2,z),.13,3.35,"wood" if "wood" in MATS else "copper",c,12)
        sail("Flagship_Sail",.14,2.25,z,1.7,2.45,"sail_teal",c,True)
        cyl("Flagship_Rigging",(0,1.75,z),.035,2.9,"ivory",c,8)
    # stern cabin and crowned ram: recognizable close-up details
    cube("Flagship_Cabin",(0,1.5,2.35),(1.35,1.05,1.15),"ivory",c,.13)
    cube("Flagship_CabinBand",(0,1.95,2.95),(1.45,.16,.8),"gold",c,.03)
    cone("Flagship_Ram",(0,1.08,-4.0),.45,.03,1.0,"gold",c,10,rot=(pi/2,0,0))
    for i in range(7): torus("Flagship_Rail",(-.95+i*.32,1.18,2.5),.13,.035,"brass",c,rot=(pi/2,0,0))
    # wake is a separate authored ribbon for independent shadow/overdraw control.
    wake=[(-1.1,.12,-4.3),(-.65,.08,-5.6),(0,.05,-6.35),(.65,.08,-5.6),(1.1,.12,-4.3)]
    mesh_obj("Flagship_Wake",[(x,y,z) for x,y,z in wake]+[(x*.52,y+.02,z+.2) for x,y,z in wake],[(0,1,6,5),(1,2,7,6),(2,3,8,7),(3,4,9,8)],"foam",c,.01)
    # actual LOD variants keep silhouette, shed trim and high-frequency detail.
    for lod, d in (("LOD1",0),("LOD2",0)):
        cc=collection("Flagship_"+lod); marker(cc,"Tier A",35000,"Reduced trim silhouette for mobile LOD.")
        if lod == "LOD2":
            # Coarsest mobile silhouette: one faceted hull, deck, mast and sail.
            # Deliberately omits the second rig and all trim, ~half LOD1 geometry.
            annotate(hull("Flagship_"+lod+"_Hull",8.0,2.18,1.42,"navy",cc,"coarse"),"Tier A",35000,lod)
            cube("Flagship_"+lod+"_Deck",(0,.76,0),(1.48,.22,6.0),"teal",cc,.025)
            z=0
            cyl("Flagship_"+lod+"_Mast",(0,2.1,z),.12,3.0,"copper",cc,8)
            sail("Flagship_"+lod+"_Sail",.12,2.1,z,1.55,2.25,"sail_teal",cc,False)
        else:
            annotate(hull("Flagship_"+lod+"_Hull",8.3,2.27,1.5,"navy",cc,d),"Tier A",35000,lod)
            cube("Flagship_"+lod+"_Deck",(0,.8,0),(1.55,.25,6.2),"teal",cc,.06)
            for z in (-1.1,1.1):
                cyl("Flagship_"+lod+"_Mast",(0,2.1,z),.12,3.0,"copper",cc,8)
                sail("Flagship_"+lod+"_Sail",.12,2.1,z,1.4,2.1,"sail_teal",cc,False)
    return c

def crew(root, hostile=False):
    name="HostileEnemy" if hostile else "FriendlyCrew"; c=collection(name)
    tier="Tier B"; budget=1500; marker(c,tier,budget,"Instanced ordinary unit; one shared material family, readable faction silhouette.")
    f="crimson" if hostile else "teal"; dark="charcoal" if hostile else "navy"; metal="copper" if hostile else "gold"; skin="sand"
    def unit(prefix, lod, x=0):
        cc=c if lod=="LOD0" else collection(name+"_"+lod); marker(cc,tier,budget,"Crowd LOD silhouette; shader pose driven in runtime.")
        # Forward-leaning friendly vs. broad closed hostile stance.
        seg=12 if hostile else 10; ring=6 if hostile else 5
        torso=uv(prefix+"_Torso",(x,.75,0),(.34,.52,.26 if not hostile else .38),f,cc,seg if lod=="LOD0" else 8,ring if lod=="LOD0" else 4)
        head=uv(prefix+"_Head",(x,1.48,.02),(.25,.26,.24),skin,cc,seg if lod=="LOD0" else 8,ring)
        cone(prefix+"_Hat",(x,1.78,.02),.34,.10,.38,dark,cc,8)
        for side in (-1,1):
            arm=cyl(prefix+"_Arm",(x+side*.38,1.02,.02),.11,.72,f,cc,6 if not hostile else 8,rot=(0,pi/2 + (0.22*side if not hostile else 0),0),bevel=.015)
            cyl(prefix+"_Boot",(x+side*.15,.25,-.05),.13,.46,dark,cc,6 if not hostile else 8,rot=(0,0,0))
        # Open shield reads friendly; angular shoulder/spike reads hostile.
        if hostile:
            for side in (-1,1): cone(prefix+"_ShoulderSpike",(x+side*.44,1.22,.02),.19,0,.42,metal,cc,6,rot=(0,pi/2,0))
            cone(prefix+"_Blade",(x,1.05,.5),.12,0,.85,metal,cc,6,rot=(pi/2,0,0))
        else:
            torus(prefix+"_Shield",(x-.43,1.08,.05),.27,.075,metal,cc,rot=(pi/2,0,0),major_segments=12,minor_segments=6)
            cyl(prefix+"_Spear",(x+.40,1.25,.02),.035,1.35,metal,cc,6,rot=(0,0,pi/2))
        annotate(torso,tier,budget,lod); return cc
    unit(name+"_Unit", "LOD0"); unit(name+"_Unit", "LOD1"); unit(name+"_Unit", "LOD2")
    return c

def gate(root):
    c=collection("GateMultiplier"); marker(c,"Tier A",8000,"×4 decision gate: violet-blue positive energy, gold arch, numeric plane visible at portrait distance.")
    for x in (-1.8,1.8):
        cube("GateMultiplier_Pillar",(x,2.1,0),(.55,4.2,.68),"gold",c,.10)
        for y in (.65,1.55,2.45,3.35): cube("GateMultiplier_PillarBand",(x,y,.02),(.7,.12,.82),"ivory",c,.025)
    # arch ring from concentric semicircle strip, extruded in YZ plane
    verts=[]; faces=[]; n=18
    for i in range(n+1):
        a=pi*i/n; y=4.0+1.8*sin(a); x=1.8*cos(a)
        verts.extend([(x,y,-.34),(x,y,.34)])
    for i in range(n): faces.append((2*i,2*i+2,2*i+3,2*i+1))
    mesh_obj("GateMultiplier_Arch",verts,faces,"gold",c,.035)
    cube("GateMultiplier_Plane",(0,2.1,.39),(3.05,2.12,.10),"gate_face",c,.08)
    # Literal lowercase x4 uses a bold, closed-character font, avoiding the arrow-like
    # open-top 4 interpretation. The dark violet face is the deliberate backplate.
    # Portrait mirroring means physical "4 x" displays as left-to-right "x 4".
    text_mesh("GateMultiplier_Text","4 x",(0,2.13,.24),1.48,"gate_number",c,.060)
    for side in (-1,1):
        torus("GateMultiplier_Buoy",(side*2.35,.55,0),.34,.07,"teal",c,rot=(pi/2,0,0)); cone("GateMultiplier_BuoyTop",(side*2.35,.98,0),.15,0,.35,"foam",c,8)
    # directional chevrons on the floor warn before commitment.
    for z in (-1.2,-.4,.4,1.2):
        profile_extrude("GateMultiplier_Chevron",[(-.38,0),(.38,0),(.18,.28),(-.18,.28)],.05,"violet",c,.01)
    return c

def guardian(root):
    c=collection("HarborGuardian"); marker(c,"Tier A",30000,"Boss character: readable helmeted harbor warden with face, layered nautical armor, anchor-chain weapon and closed hostile silhouette.")
    def build(cc,lod,detail):
        marker(cc,"Tier A",30000,"LOD"+str(lod)+" helmet, face and anchor silhouette retained for mobile readability.")
        tag="HarborGuardian_L"+str(lod); coarse=(str(lod)=="2")
        seg=20 if detail else (8 if coarse else 12); rings=12 if detail else (5 if coarse else 7)
        # Broad, stacked torso reads as a harbor warden rather than a generic blob.
        uv(tag+"_Body",(0,1.72,.10),(1.08,1.12,.64),"charcoal",cc,seg,rings)
        armor_rows=((1.35,1.48,"copper"),(1.68,1.62,"crimson"),(2.00,1.42,"copper"),(2.27,1.13,"gold"))
        for y, width, color in (armor_rows[:2] if coarse else armor_rows):
            cube(tag+"_ArmorLamella",(0,y,-.57),(width,.27,.18),color,cc,.055)
        cube(tag+"_ChestKeel",(0,1.83,-.70),(.24,1.30,.18),"gold",cc,.045)
        # A deliberately modeled face: cheek guards, brow, glowing eyes and nose guard.
        uv(tag+"_Head",(0,3.05,-.02),(.57,.57,.48),"sand",cc,seg,rings)
        cube(tag+"_HelmetDome",(0,3.35,.02),(1.25,.44,.98),"charcoal",cc,.15)
        cube(tag+"_HelmetBrow",(0,3.20,-.53),(1.22,.18,.18),"copper",cc,.04)
        for side in (-1,1):
            cube(tag+"_CheekGuard",(side*.43,2.91,-.51),(.25,.52,.16),"charcoal",cc,.055,rot=(0,0,side*.25))
            cube(tag+"_Eye",(side*.22,3.08,-.61),(.22,.09,.075),"danger",cc,.025)
        cube(tag+"_NoseGuard",(0,2.96,-.64),(.14,.46,.12),"copper",cc,.035)
        if not coarse: cube(tag+"_MouthGrille",(0,2.75,-.57),(.48,.12,.12),"gold",cc,.025)
        # Crest and side fins create a recognisable hostile helm in silhouette.
        cone(tag+"_Crest",(0,4.06,.03),.42,.05,.72,"crimson",cc,8)
        if not coarse:
            for side in (-1,1): cone(tag+"_HelmFin",(side*.67,3.45,.02),.22,0,.52,"copper",cc,6,rot=(0,0,side*pi/2))
        for side in (-1,1):
            uv(tag+"_Shoulder",(side*1.02,2.40,.03),(.66,.48,.58),"copper",cc,16 if detail else 8,8 if detail else 5)
            if not coarse: cone(tag+"_ShoulderSpike",(side*1.43,2.57,.04),.28,0,.62,"crimson",cc,7,rot=(0,0,-side*pi/2))
            cyl(tag+"_Arm",(side*1.31,1.70,.04),.25,1.42,"charcoal",cc,12 if detail else 8,rot=(0,pi/2,0))
            if detail:
                for k in range(3): cube(tag+"_ArmScale",(side*1.33,1.36+k*.27,-.28),(.36,.18,.24),"copper",cc,.03)
        # Left: tower-shield bearing an anchor emblem. Right: large anchor on a visible chain.
        cube(tag+"_AnchorShield",(-1.58,1.72,-.42),(.24,1.72,1.34),"violet",cc,.14)
        if not coarse:
            torus(tag+"_ShieldRim",(-1.72,1.73,-.55),.59,.10,"gold",cc,rot=(0,pi/2,0))
            cyl(tag+"_ShieldAnchorStem",(-1.72,1.80,-.69),.065,.72,"gold",cc,8)
            torus(tag+"_ShieldAnchorRing",(-1.72,2.20,-.69),.18,.05,"gold",cc,rot=(pi/2,0,0),major_segments=12)
            for side in (-1,1): cone(tag+"_ShieldAnchorFluke",(-1.72+side*.18,1.38,-.69),.14,0,.38,"gold",cc,6,rot=(0,0,side*.65))
        cyl(tag+"_AnchorShaft",(1.75,2.14,-.06),.11,2.75,"gold",cc,10,rot=(0,0,-.34))
        torus(tag+"_AnchorRing",(2.20,3.40,-.06),.30,.07 if coarse else .09,"copper",cc,rot=(pi/2,0,0),major_segments=8 if coarse else 14,minor_segments=5 if coarse else 8)
        for side in (-1,1): cone(tag+"_AnchorFluke",(1.31+side*.62,.82,-.06),.26,0,.72,"copper",cc,7,rot=(0,0,side*.65))
        # Interlocking links give the weapon a nautical narrative at a glance.
        for i in range(2 if coarse else 5): torus(tag+"_ChainLink",(.58+i*.25,1.12+i*.22,-.36),.16,.045,"copper",cc,rot=(pi/2 if i%2 else 0,0,0),major_segments=8 if coarse else 12,minor_segments=4 if coarse else 6)
        for side in (-1,1): cyl(tag+"_Boot",(side*.47,.45,.08),.29,.88,"crimson",cc,10)
        annotate(bpy.context.object,"Tier A",30000,"LOD"+str(lod))
    build(c,"0",True)
    for lod, detail in ((1,False),(2,False)):
        cc=collection("HarborGuardian_LOD"+str(lod)); build(cc,str(lod),detail)
    return c

def environment(root):
    c=collection("MediterraneanHarbor"); marker(c,"Tier A",50000,"Level 1 vista: teal water, warm limestone harbor, fortress silhouette, beach and vegetation preserve player contrast.")
    # water, beach, dock
    cube("Harbor_Water",(0,-.12,0),(24,.18,30),"sea",c,.02)
    cube("Harbor_FoamLine",(0,.01,6.0),(20,.05,.34),"foam",c,.03)
    cube("Harbor_Beach",(0,.02,7.8),(20,.34,6.0),"sand",c,.10)
    for z in (4.7,5.8,6.9): cube("Harbor_DockPlank",(0,.38,z),(14,.16,.5),"copper",c,.05)
    # fortress wall and tower with inset battlements
    cube("Harbor_FortWall",(0,2.0,10.1),(18,4.0,1.4),"limestone",c,.12)
    for x in range(-8,9,2): cube("Harbor_Battlement",(x,4.35,10.1),(1.2,.75,1.5),"stone_dark",c,.06)
    for x in (-7.0,7.0):
        cyl("Harbor_Tower",(x,3.7,9.4),1.35,7.4,"limestone",c,12)
        cone("Harbor_TowerCap",(x,7.75,9.4),1.7,0,1.8,"crimson",c,8)
        for y in (2.8,4.2,5.6): torus("Harbor_TowerBand",(x,y,9.4),1.36,.08,"gold",c)
    # gatehouse arch and warm interior
    cube("Harbor_Gatehouse",(0,2.5,9.25),(5.4,5.0,1.2),"limestone",c,.10)
    profile_extrude("Harbor_GateOpening",[(-1.4,0),(1.4,0),(1.4,2.1),(.95,2.8),(-.95,2.8),(-1.4,2.1)],1.28,"stone_dark",c,.04)
    # olive trees and palms as deliberate silhouette clusters
    for x,z in ((-8,6.3),(-5.5,8.3),(5.2,7.5),(8.3,5.9)):
        cyl("Harbor_OliveTrunk",(x,1.2,z),.18,2.6,"copper",c,8)
        for i in range(6):
            a=i*pi/3; uv("Harbor_OliveCrown",(x+cos(a)*.52,2.65+sin(a)*.22,z+sin(a)*.52),(.62,.42,.62),"vegetation",c,8,4)
    # warm rocks at shoreline add depth and water contact cues.
    for x,z,s in ((-8,4,.7),(-6,5,.45),(7,4.5,.8),(9,6,.55)):
        uv("Harbor_ShoreRock",(x,.35,z),(s,.42,s*1.15),"limestone",c,10,5)
    return c

def setup_scene():
    sc=bpy.context.scene; sc.unit_settings.system="METRIC"; sc.unit_settings.scale_length=1.0
    # Blender 5.1 exposes the realtime engine as BLENDER_EEVEE (EEVEE Next internally).
    sc.render.engine="BLENDER_EEVEE"; sc.render.resolution_x=720; sc.render.resolution_y=1280; sc.render.resolution_percentage=70
    sc.render.image_settings.file_format="PNG"; sc.render.film_transparent=False
    sc.world.color=(.03,.08,.12)
    # portrait review camera aimed at the hero corridor
    bpy.ops.object.camera_add(location=(14,10,-19)); cam=bpy.context.object; cam.name="T047_PortraitReviewCamera"; sc.camera=cam; cam.data.lens=52
    def track(ob, target): ob.rotation_euler=(Vector(target)-ob.location).to_track_quat('-Z','Y').to_euler()
    track(cam,(0,2.2,3.7))
    bpy.ops.object.light_add(type="AREA", location=(2,12,-5)); key=bpy.context.object; key.name="T047_Key"; key.data.energy=1500; key.data.shape="DISK"; key.data.size=8; track(key,(0,1.5,4))
    bpy.ops.object.light_add(type="AREA", location=(-9,6,8)); fill=bpy.context.object; fill.name="T047_WarmFill"; fill.data.energy=800; fill.data.color=(1,.42,.22); fill.data.size=6; track(fill,(0,2,8))
    bpy.ops.object.light_add(type="AREA", location=(8,5,-2)); rim=bpy.context.object; rim.name="T047_TealRim"; rim.data.energy=1000; rim.data.color=(.12,.65,1); rim.data.size=5; track(rim,(0,2,2))

def export_collection(c, basename, subdir):
    meshes=[o for o in c.all_objects if o.type=="MESH"]
    if not meshes: return
    bpy.ops.object.select_all(action="DESELECT")
    for o in meshes: o.select_set(True)
    bpy.context.view_layer.objects.active=meshes[0]
    out=os.path.join(OUT_ART,subdir,basename+".fbx")
    bpy.ops.export_scene.fbx(filepath=out, use_selection=True, object_types={'MESH'}, apply_scale_options='FBX_SCALE_ALL', axis_forward='-Z', axis_up='Y', add_leaf_bones=False, bake_anim=False, mesh_smooth_type='FACE')
    for o in meshes: o.select_set(False)
    with open(os.path.join(OUT_ART,subdir,basename+".review.txt"),"w",encoding="utf-8") as f:
        f.write("T047 authored benchmark asset\nasset=%s\ncollection=%s\ntriangles=%d\n"%(basename,c.name,count(c)))
        f.write("source=T047_BenchmarkArt.py\nlicense=Original procedural geometry; no third-party assets\n")

def stage_portrait_review():
    """Move authored collections into a legible portrait contact scene after export."""
    # A straight battle corridor deliberately reserves screen bands: player/wake,
    # friendly force, decision gate, hostile line, then the guardian and fortress.
    offsets={"Flagship":(0,0,-7.70), "FriendlyCrew":(-2.15,0,.35),
             "HostileEnemy":(2.25,1.55,5.80), "GateMultiplier":(0,1.35,2.15),
             # Boss occupies the central hostile horizon, above the gate's decision band.
             "HarborGuardian":(0,2.70,9.00), "MediterraneanHarbor":(0,0,1.2)}
    for prefix, delta in offsets.items():
        for c in bpy.data.collections:
            # LODs are delivery variants, never additional actors in a review render.
            if c.name == prefix:
                for o in c.objects:
                    # Stage, rather than export, applies portrait-only hierarchy scaling.
                    # A compact flagship reveals the decision plane; an enlarged raised
                    # guardian remains the first hostile read at phone size.
                    if prefix == "Flagship": o.scale *= .72
                    if prefix == "HarborGuardian": o.location *= 1.32; o.scale *= 1.32
                    o.location.x += delta[0]; o.location.y += delta[1]; o.location.z += delta[2]
    for c in bpy.data.collections:
        if "_LOD" in c.name:
            for o in c.objects: o.hide_render=True
    # Preview-only formations communicate the crowd battle without inflating the single
    # instanced-unit FBX handoff.  They share the same meshes/materials as their source.
    preview=bpy.data.collections.new("T047_PreviewFormations"); bpy.context.scene.collection.children.link(preview)
    def formation(source_name, offsets):
        source=bpy.data.collections.get(source_name)
        for i, delta in enumerate(offsets):
            for ob in source.objects:
                clone=ob.copy(); clone.data=ob.data.copy() if ob.data and ob.type == 'MESH' else ob.data
                clone.name=source_name+"_Preview_%02d_"%i+ob.name
                preview.objects.link(clone); clone.location += Vector(delta); clone.scale *= .60
    formation("FriendlyCrew", [(-1.20,0,-.25),(-.55,0,.18),(.45,0,.10),(1.10,0,.48),(-.10,0,.72)])
    formation("HostileEnemy", [(-1.25,0,-.38),(-.58,0,.15),(.48,0,.02),(1.15,0,.42),(.05,0,.72)])
    # Banner colors turn the small formations into immediate faction reads.
    for name, x, z, color in (("FriendlyBanner",-2.75,.95,"teal"),("HostileBanner",2.85,6.42,"crimson")):
        cyl(name+"_Pole",(x,1.45,z),.045,2.65,"gold",preview,8)
        cube(name+"_Cloth",(x+.34,2.08,z),(.62,.78,.07),color,preview,.04)
    cam=bpy.data.objects.get("T047_PortraitReviewCamera")
    cam.location=(0,8.7,-25.5); cam.data.lens=60
    cam.rotation_euler=(Vector((0,2.18,3.7))-cam.location).to_track_quat('-Z','Y').to_euler()

def render_focus(prefix, filename, target, camera_location, lens=58, include_env=False):
    """Produce a readable phone-size proof shot without changing the saved source."""
    visible=[]
    for o in bpy.context.scene.objects:
        if o.type != "MESH": continue
        keep=o.name.startswith(prefix)
        if include_env: keep = keep or o.name.startswith("Harbor_")
        # Focus proof also uses LOD0 only; otherwise three coincident assets obscure form.
        is_lod=any("_LOD" in c.name for c in o.users_collection)
        o.hide_render=(not keep) or is_lod; visible.append(o)
    cam=bpy.data.objects.get("T047_PortraitReviewCamera"); cam.location=camera_location; cam.data.lens=lens
    cam.rotation_euler=(Vector(target)-cam.location).to_track_quat('-Z','Y').to_euler()
    bpy.context.scene.render.filepath=os.path.join(RENDER_DIR,filename); bpy.ops.render.render(write_still=True)
    for o in visible: o.hide_render=False

def main():
    clean(); materials(); setup_scene()
    root=bpy.context.scene.collection
    assets=[flagship(root),crew(root,False),crew(root,True),gate(root),guardian(root),environment(root)]
    # Collection summary is persisted in the source scene and external manifest.
    rows=[]
    for c in assets:
        rows.append((c.name,count(c),c.get("tier"),c.get("lod0_triangle_guide")))
    with open(os.path.join(OUT_SOURCE,"T047_asset_manifest.txt"),"w",encoding="utf-8") as f:
        f.write("T047 Level 1 authored benchmark asset manifest\n")
        for row in rows: f.write("%s | triangles=%d | %s | guide=%s\n"%row)
        f.write("All geometry is original procedural authoring in T047_BenchmarkArt.py.\n")
        f.write("Mobile review: portrait 720x1280 render, LOD collections explicit for Flagship and HarborGuardian.\n")
    export_collection(assets[0],"Flagship","Ships"); export_collection(assets[1],"FriendlyCrew","Characters"); export_collection(assets[2],"HostileEnemy","Characters")
    export_collection(assets[3],"GateMultiplier","Environment"); export_collection(assets[4],"HarborGuardian","Characters"); export_collection(assets[5],"MediterraneanHarbor","Environment")
    # Tier A mobile contract requires explicit LOD handoffs for the flagship and boss.
    for lod in ("Flagship_LOD1", "Flagship_LOD2"):
        export_collection(bpy.data.collections.get(lod), lod, "Ships")
    for lod in ("HarborGuardian_LOD1", "HarborGuardian_LOD2"):
        export_collection(bpy.data.collections.get(lod), lod, "Characters")
    # Keep a clean authored source with cameras/lights for review and rerender.
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT_SOURCE,"T047_BenchmarkArt.blend"))
    stage_portrait_review()
    bpy.ops.wm.save_as_mainfile(filepath=os.path.join(OUT_SOURCE,"T047_BenchmarkArt_Preview.blend"))
    bpy.context.scene.render.filepath=os.path.join(RENDER_DIR,"T047_BenchmarkArt.png"); bpy.ops.render.render(write_still=True)
    render_focus("Flagship", "T047_Flagship.png", (0,1.20,-7.70), (2.5,6.0,-20.0), 52)
    render_focus("GateMultiplier", "T047_GateMultiplier.png", (0,2.2,2.5), (0,4.2,-5.0), 58)
    render_focus("HarborGuardian", "T047_HarborGuardian.png", (0,6.00,9.00), (0,8.45,2.10), 58)
    print("T047 COMPLETE", rows)

if __name__ == "__main__": main()
