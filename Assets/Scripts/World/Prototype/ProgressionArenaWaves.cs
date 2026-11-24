using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety {
// A finite sample run; shop pauses freeze the schedule along with combat.
public class ProgressionArenaWaves : MonoBehaviour {
 public int[] waveSizes={18,24,32,40,48,56,64,72,84};
 const int MaxAlive=48;
 int issued;
 float nextPack;
 float chestNoticeUntil;
 public float breakDuration=6;
 public Vector3[] spawnPoints={new Vector3(-17,0,-16),new Vector3(17,0,-16),new Vector3(-15,0,5),new Vector3(15,0,5),new Vector3(-10,0,24),new Vector3(10,0,24)};
 public int Wave {get;private set;}
 public bool Complete {get;private set;}
 public int LivingCount=>enemies.Count;
 readonly List<Enemy> enemies=new List<Enemy>();float nextWave;bool waiting=true,ready;
 GUIStyle text;
 System.Collections.IEnumerator Start(){
  yield return null;
  var player=GameSession.Instance.player;foreach(var kind in new[]{FoodKind.Schabowy,FoodKind.Chrupiaca,FoodKind.Mielony,FoodKind.Losos})player.Receive(kind);player.Equip(FoodKind.Schabowy);
  nextWave=Time.time+8;ready=true;
 }
 void Update(){
  var session=GameSession.Instance;if(!ready||!session||session.paused||!session.player)return;
  enemies.RemoveAll(e=>!e||e.health<=0);
  if(Complete){if(Vector3.Distance(session.player.transform.position,new Vector3(0,1,29))<3&&Input.GetKeyDown(KeyCode.F))session.Win();return;}
  if(waiting){
   if(Time.time>=nextWave)SpawnWave(session);
   return;
  }
  if(issued<waveSizes[Wave-1]){
   if(Time.time>=nextPack && enemies.Count<MaxAlive) SpawnPack(session);
   return;
  }
  if(enemies.Count>0)return;
  if(Wave%3==0)DropChest();
  if(Wave==waveSizes.Length){Complete=true;return;}
  waiting=true;
  nextWave=Time.time+breakDuration;
  session.player.RestoreHealth(15);
 }
 void SpawnWave(GameSession session){
  Wave++;
  issued=0;
  waiting=false;
  SpawnPack(session);
 }
 void SpawnPack(GameSession session){
  int count=Mathf.Min(5+Wave, Mathf.Min(MaxAlive-enemies.Count,waveSizes[Wave-1]-issued));
  nextPack=Time.time+Mathf.Max(1.1f,2.7f-.15f*Wave);
  for(int i=0;i<count;i++){
   int ordinal=issued+1;
   bool pounder=Wave>=2 && ordinal%23==12;
   bool mortar=Wave>=4 && ordinal%29==18 && !pounder;
   bool ranged=mortar || (!pounder && Wave>=2 && ordinal%8==0);
   var source=ranged?session.shootingPrefab:session.meleePrefab;
   var capsule=source.GetComponent<CapsuleCollider>();
   var agent=source.GetComponent<NavMeshAgent>();
   float scale=source.transform.lossyScale.y;
   if(pounder||mortar)scale*=EliteAreaAttack.SizeMultiplier(mortar);
   if(!FindEntry(session.player.transform.position,capsule,agent,scale,out var position))break;
   ++issued;
   var enemy=Instantiate(source,position,Quaternion.identity,transform).GetComponent<Enemy>();
   enemy.name=pounder?"Tluczek":mortar?"Pieprznik":"Fala "+Wave;
   enemy.ConfigureHorde(Wave,pounder||mortar);
   enemy.gameObject.SetActive(true);
   enemies.Add(enemy);
  }
 }
 // Entries are authored on the arena floor. A nearby NavMesh polygon can also be on
 // furniture or underneath a ramp, so check the physical floor and the whole body.
 const int SolidMask = ~((1 << 8) | (1 << 9) | (1 << 11));
 bool FindEntry(Vector3 player, CapsuleCollider body, NavMeshAgent agent, float scale, out Vector3 position){
  for(int attempt=0;attempt<spawnPoints.Length;attempt++){
   var anchor=spawnPoints[(issued+Wave+attempt)%spawnPoints.Length];
   var scatter=Random.insideUnitCircle*2.5f;
   var candidate=anchor+new Vector3(scatter.x,0,scatter.y);
   if(!NavMesh.SamplePosition(candidate,out var nav,.75f,NavMesh.AllAreas))continue;
   if(Mathf.Abs(nav.position.y-anchor.y)>.3f || Vector3.Distance(player,nav.position)<10)continue;
   if(!Physics.Raycast(nav.position+Vector3.up*.5f,Vector3.down,out var floor,1f,SolidMask,QueryTriggerInteraction.Ignore))continue;
   if(Mathf.Abs(floor.point.y-anchor.y)>.2f || floor.normal.y<.98f)continue;

   var pivot=floor.point+Vector3.up*(agent.baseOffset*scale);
   var centre=pivot+body.center*scale;
   float radius=body.radius*scale;
   float halfSegment=Mathf.Max(0,body.height*scale*.5f-radius);
   // Leave a small gap at the sole for floor contact and reserve room at the sides.
   const float margin=.08f;
   var bottom=centre-Vector3.up*halfSegment;
   bottom.y=Mathf.Max(bottom.y,floor.point.y+radius+margin+.03f);
   var top=centre+Vector3.up*halfSegment;
   if(Physics.CheckCapsule(bottom,top,radius+margin,SolidMask,QueryTriggerInteraction.Ignore))continue;
   position=pivot;
   return true;
  }
  position=default;
  return false;
 }
 void DropChest(){
  var chest=GameObject.CreatePrimitive(PrimitiveType.Cube);
  chest.name="Skrzynia po fali "+Wave;
  chest.transform.SetParent(transform);
  chest.transform.position=new Vector3((Wave/3-2)*3,.7f,-16);
  chest.transform.localScale=Vector3.one*1.4f;
  chest.AddComponent<UpgradeChest>();
  chest.AddComponent<UpgradeChestVisual>();
  chestNoticeUntil=Time.time+10;
 }
 void OnGUI(){
  var session=GameSession.Instance;if(!session||session.paused||session.won)return;
  if(text==null)text=new GUIStyle(GUI.skin.label){font=Resources.Load<Font>("UI/PixelAE-Bold"),fontSize=17,wordWrap=true};
  var matrix=GUI.matrix;GUI.matrix=Matrix4x4.TRS(Vector3.zero,Quaternion.identity,new Vector3(Screen.width/1280f,Screen.height/720f,1));
  var color=GUI.color;GUI.color=new Color(.025f,.035f,.055f,.88f);GUI.DrawTexture(new Rect(18,80,350,108),Texture2D.whiteTexture);GUI.color=color;
  string status=Complete?"Wszystkie fale pokonane!\nZbierz punkty i podejdz do wyjscia.\nNiebieska strefa na koncu: F — zakoncz":waiting?"Nastepna fala za "+Mathf.CeilToInt(Mathf.Max(0,nextWave-Time.time))+" s\nZbierz punkty / B: sklep po awansie":"Fala "+Wave+" / "+waveSizes.Length+"   Wrogowie: "+LivingCount+"\nZabijaj, zbieraj punkty i zdobywaj XP.";
  if(Time.time<chestNoticeUntil)status+="\nSkrzynia broni przy zoltym polu!";
  GUI.Label(new Rect(30,88,325,95),"ARENA PROGRESJI\n"+status,text);GUI.matrix=matrix;
 }
}
}
