"""Generate only ProgressionArena; MovementArena and its authored scene stay untouched."""
import json
from build_arena_scene import ROOT, build, meta

items=[]
def box(group,name,position,size,color=(.65,.68,.73,1),rotation=(0,0,0),surface='Concrete',chest=False,collider=True):
    items.append(dict(group=group,name=name,position=dict(zip('xyz',position)),size=dict(zip('xyz',size)),rotation=dict(zip('xyz',rotation)),color=dict(zip('rgba',color)),collider=collider,text='',surface=surface,upgradeChest=chest))
def sign(name,text,position,rotation=(0,180,0),size=.10):
    items.append(dict(group='06 Instrukcje',name=name,position=dict(zip('xyz',position)),size=dict(x=1,y=1,z=1),rotation=dict(zip('xyz',rotation)),color=dict(r=1,g=.87,b=.36,a=1),collider=False,text=text,textSize=size))

box('01 Podloze','Podloze 64 x 72',(0,-.5,0),(64,1,72),(.7,.72,.7,1),surface='Ground')
for name,pos,size in [('Zachod',(-32.5,2.5,0),(1,6,73)),('Wschod',(32.5,2.5,0),(1,6,73)),('Polnoc',(0,2.5,36.5),(64,6,1)),('Poludnie',(0,2.5,-36.5),(64,6,1))]:
    box('01 Podloze','Mur '+name,pos,size,(.48,.52,.6,1))
box('02 Start i wyjscie','Strefa startu',(0,.015,-28),(18,.03,9),(.22,.45,.38,1),surface=None,collider=False)
box('02 Start i wyjscie','Wyjscie po 9 falach',(0,.025,29),(5,.05,5),(.18,.55,.85,1),surface=None,collider=False)
for x in [-5,5]:
    box('02 Start i wyjscie','Oslona startu',(x,1.0,-19),(3,2,2),(.57,.6,.64,1))
for x in [-24,24]:
    box('03 Rampy i slam','Platforma boczna 5 m',(x,2.5,13),(9,5,12),(.67,.65,.6,1))
    box('03 Rampy i slam','Rampa na platforme',(x,2.4,-2),(7,.6,19),(.67,.65,.6,1),rotation=(-15,0,0))
    box('03 Rampy i slam','Punkt zaczepienia',(x,7,16),(1,4,1),(.9,.5,.18,1),surface='Metal')
    sign('SLAM / HOOK','Q: HOOK\nSLAM Z PLATFORMY',(x,6.8,18),size=.10)
# A tall central pillar invites a hook and a longer fall without blocking the outer loop.
box('03 Rampy i slam','Wieza do hooka',(0,4.5,16),(4,9,4),(.55,.61,.68,1))
for x in [-8,8]:
    box('04 Slide i dash','Tunel - lewa sciana',(x-2.5,.8,-7),(1,1.6,9),(.6,.62,.65,1))
    box('04 Slide i dash','Tunel - prawa sciana',(x+2.5,.8,-7),(1,1.6,9),(.6,.62,.65,1))
    box('04 Slide i dash','Tunel - niski dach',(x,1.9,-7),(6,1.2,9),(.72,.55,.27,1),surface='Metal')
    sign('SLIDE','SLIDE',(x,2.9,-11.7),size=.12)
for x,z in [(-10,9),(10,9),(-6,20),(6,20)]:
    box('04 Slide i dash','Niska przeszkoda',(x,.55,z),(2.5,1.1,2.5),(.63,.6,.55,1))
box('05 Nagrody','Pole dostawy skrzyn',(0,.02,-16),(9,.04,3),(.88,.59,.12,1),surface=None,collider=False)
sign('Dostawy','SKRZYNIA CO 3 FALE',(0,1.8,-15),size=.075)
sign('Instrukcja startowa','ARENA PROGRESJI\n9 FAL / XP / PUNKTY / SKLEP\nZLOTE KOSTKI: PUNKTY   B: SKLEP PO AWANSIE',(0,4,-16),size=.095)
sign('Wyjscie','POKONAJ 9 FAL\nF: ZAKONCZ RUN',(0,4.2,34),size=.12)
for i,(x,z) in enumerate([(-17,-16),(17,-16),(-15,5),(15,5),(-10,24),(10,24)]):
    box('07 Wejscia fal','Znacznik wejscia '+str(i+1),(x,.012,z),(2,.024,2),(.75,.25,.17,1),surface=None,collider=False)

path=ROOT/'Assets/Resources/Levels/ProgressionArena.json'
path.write_text(json.dumps(dict(items=items),ensure_ascii=False,indent=2)+'\n')
meta(path,'TextScriptImporter')
build('ProgressionArena','ProgressionArenaBootstrap',[(-17,0,-16),(17,0,-16),(-15,0,5),(15,0,5),(-10,0,24),(10,0,24)])
