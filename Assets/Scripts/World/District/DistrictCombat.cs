using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety {
public class DistrictCombat : MonoBehaviour {
 [Serializable]
 public class Encounter {
  public string name;
  public Vector3 centre;
  public Vector3[] entries;
  public int melee=7, ranged=3;
  [NonSerialized] public bool started,complete;
  [NonSerialized] public int issued;
  [NonSerialized] public float nextSpawn;
  [NonSerialized] public List<Enemy> enemies=new List<Enemy>();
 }
 public Encounter[] encounters;
 const int MaxActive=24;
 bool ready;
 GUIStyle label;
 IEnumerator Start() {
  yield return null;
  var player=GameSession.Instance.player;
  foreach(var kind in new[]{FoodKind.Schabowy,FoodKind.Chrupiaca,FoodKind.Mielony,FoodKind.Losos})player.Receive(kind);
  player.Equip(FoodKind.Schabowy);
  player.pierogi=3;
  ready=true;
 }
 void Update() {
  var session=GameSession.Instance;
  if(!ready||!session||session.paused||session.won||!session.player)return;
  int active=0;
  foreach(var encounter in encounters) {
   encounter.enemies.RemoveAll(enemy=>!enemy||enemy.health<=0);
   active+=encounter.enemies.Count;
  }
  for(int zone=0;zone<encounters.Length;zone++) {
   var encounter=encounters[zone];
   if(encounter.complete)continue;
   if(!encounter.started) {
    if(Vector3.Distance(session.player.transform.position,encounter.centre)>30)continue;
    encounter.started=true;
    encounter.nextSpawn=Time.time+1;
   }
   if(encounter.issued<encounter.melee+encounter.ranged) {
    if(Time.time<encounter.nextSpawn||active>=MaxActive)continue;
    encounter.nextSpawn=Time.time+1.5f;
    for(int i=0;i<3&&active<MaxActive&&encounter.issued<encounter.melee+encounter.ranged;i++) {
     bool shooting=encounter.issued>=encounter.melee;
     var template=shooting?session.shootingPrefab:session.meleePrefab;
     if(!FindSpawn(encounter,template,session.player.transform.position,out var position))break;
     var enemy=Instantiate(template,position,Quaternion.identity,transform).GetComponent<Enemy>();
     enemy.ConfigureHorde(zone+1,false);
     enemy.name=encounter.name+(shooting?" / strzelec":" / patrol");
     enemy.gameObject.SetActive(true);
     encounter.enemies.Add(enemy);
     encounter.issued++;
     active++;
    }
   } else if(encounter.enemies.Count==0) {
    encounter.complete=true;
    session.player.RestoreHealth(15);
    var chest=GameObject.CreatePrimitive(PrimitiveType.Cube);
    chest.name="Nagroda / "+encounter.name;
    var chestPoint=encounter.centre;
    if(Physics.Raycast(chestPoint+Vector3.up*2,Vector3.down,out var floor,4,~((1<<8)|(1<<9)|(1<<11)),QueryTriggerInteraction.Ignore))chestPoint=floor.point;
    chest.transform.position=chestPoint+Vector3.up*.7f;
    chest.transform.localScale=Vector3.one*1.4f;
    chest.AddComponent<UpgradeChest>();
    chest.AddComponent<UpgradeChestVisual>();
   }
  }
 }
 bool FindSpawn(Encounter encounter,GameObject template,Vector3 player,out Vector3 position) {
  var body=template.GetComponent<CapsuleCollider>();
  var agent=template.GetComponent<NavMeshAgent>();
  float scale=template.transform.lossyScale.y;
  for(int attempt=0;attempt<encounter.entries.Length;attempt++) {
   var entry=encounter.entries[(encounter.issued+attempt)%encounter.entries.Length];
   if(Vector3.Distance(entry,player)<7||!NavMesh.SamplePosition(entry,out var nav,.8f,NavMesh.AllAreas))continue;
   if(Mathf.Abs(nav.position.y-entry.y)>.3f)continue;
   position=nav.position+Vector3.up*agent.baseOffset*scale;
   var centre=position+body.center*scale;
   float radius=body.radius*scale;
   float half=Mathf.Max(0,body.height*.5f*scale-radius);
   var bottom=centre-Vector3.up*half;
   bottom.y=Mathf.Max(bottom.y,nav.position.y+radius+.1f);
   if(Physics.CheckCapsule(bottom,centre+Vector3.up*half,radius+.05f,~((1<<8)|(1<<9)|(1<<11)),QueryTriggerInteraction.Ignore))continue;
   return true;
  }
  position=default;
  return false;
 }
 void OnGUI() {
  var session=GameSession.Instance;
  if(!ready||!session||session.paused||session.won)return;
  int complete=0;
  foreach(var encounter in encounters)if(encounter.complete)complete++;
  if(label==null)label=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("UI/PixelAE-Bold"),fontSize=17};
  GUI.Label(new Rect(22,100,560,50),"OSIEDLE ZELAZNA / Sektory "+complete+" / "+encounters.Length+"\nDachy, podworka i skroty / Tab: build",label);
  if(complete==encounters.Length&&Vector3.Distance(session.player.transform.position,PostSovietDistrict.Spawn)<5) {
   GUI.Label(new Rect(22,155,560,36),"Dzielnica oczyszczona. F: zakoncz przy przystanku.",label);
   if(Event.current.type==EventType.KeyDown&&Event.current.keyCode==KeyCode.F)session.Win();
  }
 }
}
}