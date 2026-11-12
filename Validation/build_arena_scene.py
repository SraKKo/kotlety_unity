"""Write an editable Unity prototype scene from the shared arena layout (no editor required)."""
import json
import math
import re
import uuid
from pathlib import Path

ROOT = Path(__file__).resolve().parent.parent

def meta(path, importer='DefaultImporter'):
    path = Path(str(path) + '.meta')
    if not path.exists():
        folder = 'folderAsset: yes\n' if path.with_suffix('').is_dir() else ''
        path.write_text(f'fileFormatVersion: 2\nguid: {uuid.uuid4().hex}\n{folder}{importer}:\n  externalObjects: {{}}\n  userData: \n  assetBundleName: \n  assetBundleVariant: \n')
    return re.search(r'^guid: (\w+)', path.read_text(), re.M)[1]

def vec(value):
    return '{' + ', '.join(f'{axis}: {number:.8g}' for axis, number in value.items()) + '}'

def quaternion(euler):
    x,y,z = (euler[k]*math.pi/360 for k in 'xyz')
    # Unity applies Z, X, Y rotations, in that order.
    sx,cx,sy,cy,sz,cz = math.sin(x),math.cos(x),math.sin(y),math.cos(y),math.sin(z),math.cos(z)
    return dict(x=cy*sx*cz+sy*cx*sz,y=sy*cx*cz-cy*sx*sz,z=cy*cx*sz-sy*sx*cz,w=cy*cx*cz+sy*sx*sz)

def build():
    generated = ROOT/'Assets/Generated/MovementArena'
    generated.mkdir(exist_ok=True)
    meta(generated)
    layout = json.loads((ROOT/'Assets/Resources/Levels/MovementArena.json').read_text())['items']
    materials = {}
    for item in layout:
        if item['text']:
            continue
        c=item['color']; key=''.join(f'{round(c[k]*255):02X}' for k in 'rgba'); surface=item.get('surface')
        if surface: key=surface+'_'+key
        if key in materials:
            continue
        texture_properties='    m_TexEnvs: []'
        shader_reference='{fileID: 46, guid: 0000000000000000f000000000000000, type: 0}'
        if surface:
            texture_name={'Ground':'Ground.png','Concrete':'ConcreteTextureColor.png','Metal':'MetalicTextureColor.png'}[surface]
            texture_guid=re.search(r'guid: (\w+)',(ROOT/'Assets/Resources/PSX/Textures/Albedo'/(texture_name+'.meta')).read_text())[1]
            shader_guid=re.search(r'guid: (\w+)',(ROOT/'Assets/Resources/PSX/Materials/PSXWorld.shader.meta').read_text())[1]
            shader_reference=f'{{fileID: 4800000, guid: {shader_guid}, type: 3}}'
            texture_properties=f'    m_TexEnvs:\n    - _MainTex:\n        m_Texture: {{fileID: 2800000, guid: {texture_guid}, type: 3}}\n        m_Scale: {{x: 1, y: 1}}\n        m_Offset: {{x: 0, y: 0}}'
        path=generated/f'Arena_{key}.mat'
        guid=meta(path,'NativeFormatImporter')
        material_meta=Path(str(path)+'.meta')
        if 'mainObjectFileID:' not in material_meta.read_text():
            material_meta.write_text(material_meta.read_text().replace('NativeFormatImporter:\n','NativeFormatImporter:\n  mainObjectFileID: 2100000\n'))
        path.write_text(f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!21 &2100000
Material:
  serializedVersion: 8
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: Arena {key}
  m_Shader: {shader_reference}
  m_ValidKeywords: []
  m_InvalidKeywords: []
  m_LightmapFlags: 4
  m_EnableInstancingVariants: 0
  m_DoubleSidedGI: 0
  m_CustomRenderQueue: -1
  stringTagMap: {{}}
  disabledShaderPasses: []
  m_SavedProperties:
    serializedVersion: 3
{texture_properties}
    m_Ints: []
    m_Floats:
    - _WorldTiling: 0.35
    - _Glossiness: 0.12
    - _Metallic: 0
    m_Colors:
    - _Color: {vec(c)}
''')
        materials[key]=guid
    bootstrap=re.search(r'guid: (\w+)',(ROOT/'Assets/Scripts/World/Prototype/ArenaBootstrap.cs.meta').read_text())[1]
    sign=re.search(r'guid: (\w+)',(ROOT/'Assets/Scripts/World/Prototype/ArenaSign.cs.meta').read_text())[1]
    chest_visual=re.search(r'guid: (\w+)',(ROOT/'Assets/Scripts/Upgrades/UpgradeChestVisual.cs.meta').read_text())[1]
    chest=re.search(r'guid: (\w+)',(ROOT/'Assets/Scripts/Upgrades/UpgradeChest.cs.meta').read_text())[1]
    nodes=[]
    def node(name,parent=None,item=None,script=None):
        n=dict(id=1000+len(nodes)*10,name=name,parent=parent,item=item,script=script,children=[])
        if parent is not None:
            parent['children'].append(n)
        nodes.append(n)
        return n
    root=node('Movement Arena',script=bootstrap)
    geometry=node('Arena - geometria i trasy',root)
    groups={}
    for item in layout:
        if item['group'] not in groups:
            groups[item['group']]=node(item['group'],geometry)
        node(item['name'],groups[item['group']],item,script=chest if item.get('upgradeChest') else None)
    markers=node('Punkty startu przeciwnikow - referencja',root)
    for i,p in enumerate([(-42,0,-40),(42,0,-40),(-42,0,40),(42,0,40),(-42,4,0),(42,4,0),(0,8,34),(0,8,-34)]):
        node(f'Spawn {i+1}',markers,dict(position=dict(zip('xyz',p)),rotation=dict(x=0,y=0,z=0),size=dict(x=1,y=1,z=1),marker=True))
    source=(ROOT/'Assets/Scenes/MainMenu.unity').read_text()
    blocks=re.split(r'(?=--- !u!)',source)
    chunks=[blocks[0]]+[b for b in blocks[1:] if b.startswith(('--- !u!29 ','--- !u!104 ','--- !u!157 ','--- !u!196 '))]
    chunks=[b.replace('m_AmbientMode: 0','m_AmbientMode: 3').replace('m_SkyboxMaterial: {fileID: 10304, guid: 0000000000000000f000000000000000, type: 0}','m_SkyboxMaterial: {fileID: 0}') for b in chunks]
    for n in nodes:
        i=n['id'];item=n['item'];visual=item and not item.get('marker');label=visual and bool(item['text'])
        components=[i+1]+([i+2,i+3,i+4] if visual else [])+([i+5] if n['script'] or label else [])
        if item and item.get('upgradeChest'):
            components.append(i+6)
        if visual and not label and not item['collider']:
            components.remove(i+4)
        chunks.append(f'''--- !u!1 &{i}
GameObject:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  serializedVersion: 6
  m_Component:
'''+''.join(f'  - component: {{fileID: {c}}}\n' for c in components)+f'''  m_Layer: 0
  m_Name: {json.dumps(n['name'],ensure_ascii=False)}
  m_TagString: Untagged
  m_IsActive: 1
''')
        position=item['position'] if item else dict(x=0,y=0,z=0)
        rotation=item['rotation'] if item else dict(x=0,y=0,z=0)
        size=item['size'] if item else dict(x=1,y=1,z=1)
        children='\n'+''.join(f"  - {{fileID: {c['id']+1}}}\n" for c in n['children']) if n['children'] else ' []\n'
        chunks.append(f'''--- !u!4 &{i+1}
Transform:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i}}}
  serializedVersion: 2
  m_LocalRotation: {vec(quaternion(rotation))}
  m_LocalPosition: {vec(position)}
  m_LocalScale: {vec(size)}
  m_ConstrainProportionsScale: 0
  m_Children:{children}  m_Father: {{fileID: {n['parent']['id']+1 if n['parent'] else 0}}}
  m_LocalEulerAnglesHint: {vec(rotation)}
''')
        if visual:
            c=item['color'];key=''.join(f'{round(c[k]*255):02X}' for k in 'rgba')
            if item.get('surface'):key=item['surface']+'_'+key
            material='{fileID: 10302, guid: 0000000000000000f000000000000000, type: 0}' if label else f'{{fileID: 2100000, guid: {materials[key]}, type: 2}}'
            chunks.append(f'''--- !u!23 &{i+2}
MeshRenderer:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i}}}
  m_Enabled: 1
  m_CastShadows: {0 if label or not item['collider'] else 1}
  m_ReceiveShadows: 1
  m_DynamicOccludee: 1
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RenderingLayerMask: 1
  m_Materials:
  - {material}
  m_SortingLayerID: 0
  m_SortingOrder: 0
''')
            if label:
                chunks.append(f'''--- !u!102 &{i+3}
TextMesh:
  serializedVersion: 3
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i}}}
  m_Text: {json.dumps(item['text'])}
  m_OffsetZ: 0
  m_CharacterSize: {item['textSize']}
  m_LineSpacing: 1
  m_Anchor: 4
  m_Alignment: 1
  m_TabSize: 4
  m_FontSize: 48
  m_FontStyle: 0
  m_RichText: 1
  m_Font: {{fileID: 0}}
  m_Color: {vec(c)}
''')
            else:
                chunks.append(f'''--- !u!33 &{i+3}
MeshFilter:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i}}}
  m_Mesh: {{fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}}
''')
                if item['collider']:
                    chunks.append(f'''--- !u!65 &{i+4}
BoxCollider:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i}}}
  m_Material: {{fileID: 0}}
  m_IsTrigger: 0
  m_Enabled: 1
  serializedVersion: 3
  m_Size: {{x: 1, y: 1, z: 1}}
  m_Center: {{x: 0, y: 0, z: 0}}
''')
        if n['script'] or label:
            chunks.append(f'''--- !u!114 &{i+5}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {n['script'] or sign}, type: 3}}
  m_Name: 
  m_EditorClassIdentifier: 
''')
        if item and item.get('upgradeChest'):
            chunks.append(f"""--- !u!114 &{i+6}
MonoBehaviour:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_GameObject: {{fileID: {i}}}
  m_Enabled: 1
  m_EditorHideFlags: 0
  m_Script: {{fileID: 11500000, guid: {chest_visual}, type: 3}}
  m_Name:
  m_EditorClassIdentifier:
""")
    # Remove unused TextMesh component slots from GameObject declarations.
    text='\n'.join(line.rstrip() for line in ''.join(chunks).splitlines())+'\n'
    for n in nodes:
        if n['item'] and n['item'].get('text'):
            text=text.replace(f"  - component: {{fileID: {n['id']+4}}}\n",'')
    text+='--- !u!1660057539 &9223372036854775807\nSceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n  - {fileID: 1001}\n'
    scene=ROOT/'Assets/Scenes/MovementArena.unity';scene.write_text(text)
    guid=meta(scene)
    settings=ROOT/'ProjectSettings/EditorBuildSettings.asset';content=settings.read_text()
    if 'Assets/Scenes/MovementArena.unity' not in content:
        content=content.replace('  m_configObjects:',f'  - enabled: 1\n    path: Assets/Scenes/MovementArena.unity\n    guid: {guid}\n  m_configObjects:')
        settings.write_text(content)
    print(f'Generated MovementArena: {len(layout)} layout items, {len(materials)} materials.')

if __name__=='__main__':
    build()
