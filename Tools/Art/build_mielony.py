import bpy, math, random, os, json
from pathlib import Path
from mathutils import Vector
ROOT = str(Path(__file__).resolve().parents[2])
OUT = os.path.join(ROOT,"Assets","Resources","Weapons","Mielony")
DOC = os.path.join(ROOT,"Documentation","Art","Mielony")
rng=random.Random(47)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
verts=[];faces=[];colors=[]
palette=[(.19,.055,.025,1),(.29,.085,.035,1),(.39,.14,.055,1),(.48,.20,.075,1),(.57,.27,.10,1),(.67,.35,.14,1),(.76,.46,.21,1)]
segments=14
radii=[1+rng.uniform(-.09,.09) for _ in range(segments)]
for ring,(radius,height) in enumerate([(.66,-.20),(.97,-.12),(1,.05),(.77,.19),(.36,.235)]):
 for i in range(segments):
  angle=i*math.tau/segments
  verts.append((math.cos(angle)*radius*radii[i]*.5,math.sin(angle)*radius*radii[i]*.43,height+rng.uniform(-.022,.022)))
for ring in range(4):
 for i in range(segments):
  a=ring*segments+i;b=ring*segments+(i+1)%segments;c=b+segments;d=a+segments
  faces.extend([(a,b,c),(a,c,d)])
  choices=range(0,4) if ring<2 else range(1,7)
  colors.extend([palette[rng.choice(choices)],palette[rng.choice(choices)]])
for ring,height,flip in [(0,-.205,True),(4,.245,False)]:
 center=len(verts);verts.append((0,0,height))
 for i in range(segments):
  triangle=(center,ring*segments+i,ring*segments+(i+1)%segments)
  faces.append(tuple(reversed(triangle)) if flip else triangle)
  colors.append(palette[rng.randrange(0,3) if flip else rng.randrange(2,6)])
# Sparse angular browned crumbs are part of the same mesh and draw call.
for _ in range(16):
 angle=rng.random()*math.tau;radius=math.sqrt(rng.random())*.37
 x=math.cos(angle)*radius;y=math.sin(angle)*radius*.83;z=.235-radius*.16
 size=rng.uniform(.015,.032);base=len(verts)
 verts.extend([(x-size,y-size,z),(x+size,y-size,z),(x+size,y+size,z),(x-size,y+size,z),(x,y,z+size*.8)])
 for a,b,c in [(0,1,4),(1,2,4),(2,3,4),(3,0,4)]:
  faces.append((base+a,base+b,base+c));colors.append(palette[rng.choice([1,3,4,5,6])])
mesh=bpy.data.meshes.new("MielonyPSXMesh");mesh.from_pydata(verts,[],faces);mesh.update()
obj=bpy.data.objects.new("MielonyPSX",mesh);bpy.context.collection.objects.link(obj)
color_attribute=mesh.color_attributes.new(name="Color",type='BYTE_COLOR',domain='CORNER')
for poly,color in zip(mesh.polygons,colors):
 poly.use_smooth=False
 for loop in poly.loop_indices:color_attribute.data[loop].color_srgb=color
material=bpy.data.materials.new("MielonyPSX_VertexColors");material.use_nodes=True
nodes=material.node_tree.nodes;shader=nodes.get('Principled BSDF');shader.inputs['Roughness'].default_value=1
attribute=nodes.new('ShaderNodeVertexColor');attribute.layer_name="Color";material.node_tree.links.new(attribute.outputs['Color'],shader.inputs['Base Color']);mesh.materials.append(material)
bpy.context.view_layer.objects.active=obj;obj.select_set(True)
bpy.ops.export_scene.fbx(filepath=os.path.join(OUT,"MielonyPSX.fbx"),use_selection=True,object_types={'MESH'},axis_forward='-Z',axis_up='Y',apply_unit_scale=True,use_mesh_modifiers=True,add_leaf_bones=False,bake_anim=False,colors_type='SRGB')
# An editable source and a small orthographic preview of the actual exported geometry.
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.samples=32
scene.world.color=(.15,.15,.15)
bpy.ops.object.camera_add(location=(1.2,-1.7,1.1));camera=bpy.context.object;camera.rotation_euler=(Vector((0,0,.02))-camera.location).to_track_quat('-Z','Y').to_euler();camera.data.type='ORTHO';camera.data.ortho_scale=1.5;scene.camera=camera
bpy.ops.object.light_add(type='AREA',location=(0,-2,3));bpy.context.object.data.energy=240;bpy.context.object.data.shape='DISK';bpy.context.object.data.size=3
scene.render.resolution_x=480;scene.render.resolution_y=360;scene.render.resolution_percentage=100;scene.render.image_settings.file_format='PNG';scene.render.film_transparent=True
scene.view_settings.view_transform='Standard';scene.render.filepath=os.path.join(DOC,"MielonyPSX-preview.png")
bpy.ops.wm.save_as_mainfile(filepath=os.path.join(DOC,"MielonyPSX.blend"));bpy.ops.render.render(write_still=True)
with open(os.path.join(DOC,"model-info.json"),'w') as f:json.dump({'vertices':len(verts),'triangles':len(faces),'paletteColors':len(palette),'flatShaded':True,'vertexColors':True},f,indent=2)