"""Static arena checks. Physics, NavMesh and playability require Unity Play Mode."""
import json
import math
import re
from pathlib import Path

ROOT=Path(__file__).resolve().parent.parent
layout=json.loads((ROOT/'Assets/Resources/Levels/MovementArena.json').read_text())['items']
scene=(ROOT/'Assets/Scenes/MovementArena.unity').read_text()
checks=[]
def check(value,label):
    assert value,label
    checks.append('PASS '+label)

ids=re.findall(r'^--- !u!\d+ &(\d+)',scene,re.M)
check(len(ids)==len(set(ids)),'Scene fileIDs are unique')
for ref in re.finditer(r'\{fileID: (\d+)([^}]*)\}',scene):
    check(ref[1]=='0' or 'guid:' in ref[2] or ref[1] in ids,'Local scene reference resolves: '+ref[1])
for item in layout:
    check(all(item['size'][k]>0 for k in 'xyz'),'Positive dimensions: '+item['name'])
    check(all(abs(item['position'][k])<=57 for k in 'xz'),'Geometry lies inside arena bounds: '+item['name'])
    check(json.dumps(item['name'],ensure_ascii=False) in scene,'Layout item appears in editable scene: '+item['name'])
roofs=[i for i in layout if i['name'].startswith('Dach')]
check(len(roofs)==2,'Two slide shortcuts exist')
for roof in roofs:
    clearance=roof['position']['y']-roof['size']['y']/2
    check(abs(clearance-1.35)<.001,'Slide clearance is 1.35m')
    check(clearance>1 and clearance<2,'Crouching fits; standing does not')
ramps=[i for i in layout if 'Rampa' in i['name'] or 'Podejscie' in i['name']]
check(len(ramps)==8,'Eight ramps provide alternate routes')
for ramp in ramps:
    angle=abs(ramp['rotation']['x'])
    check(angle<30,'Ramp slope is below 30 degrees')
    rise=ramp['size']['z']*math.sin(math.radians(angle))
    expected=8 if 'Podejscie' in ramp['name'] else 4
    check(abs(rise-expected)<.001,'Ramp spans the intended height')
check(sum(i['name']=='HAK - cel' for i in layout)==4,'Four hook targets exist')
check(sum(i['name']=='Sciana do odbicia' for i in layout)==4,'Two pairs of wall-jump walls exist')
check('Assets/Scenes/MovementArena.unity' in (ROOT/'ProjectSettings/EditorBuildSettings.asset').read_text(),'Arena is registered in build settings')
report=ROOT/'Validation/Reports/movement-arena-static.txt'
report.write_text('\n'.join(checks)+'\nNOT RUN: Unity physics, NavMesh, Play Mode and visual verification.\n')
print(f'PASS: {len(checks)} static checks. Runtime checks require Unity.')
