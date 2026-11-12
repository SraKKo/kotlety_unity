using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
// Thrown salmon: flies straight along the crosshair, slows down, then arcs back to the thrower.
// It pierces enemies and can hit each of them once on the way out and once on the way back.
public class LososBoomerang : MonoBehaviour {
 const string ModelPath="Weapons/Losos/SalmonFish",TexturePath="Weapons/Losos/FishTexture",WholeFish="Fish1";
 const float Radius=.25f;
 static LososTuning tuning;static Material material;static Mesh mesh;static bool warned;
 public static LososTuning Tuning {get{if(!tuning)tuning=Resources.Load<LososTuning>("Combat/Losos");if(!tuning)tuning=ScriptableObject.CreateInstance<LososTuning>();return tuning;}}
 public static LososBoomerang Active {get;private set;}
 // Only one salmon exists: while it is in the air the hand is empty and cannot throw again.
 public static bool InFlight=>Active;
 GameObject owner;Transform home,spinner;Vector3 direction,side,velocity;float age,travelled,returnAge;bool returning;
 readonly HashSet<EntityId> struck=new HashSet<EntityId>();
 public bool Returning=>returning;
 // Orbit upgrade: the last enemy hit on the way out is circled before the fish returns.
 int orbitLaps,lapsDone;bool orbiting;Enemy orbitTarget;Collider orbitCollider;float orbitAngle,orbitTurned,orbitDistance,orbitLift;
 public bool Orbiting=>orbiting;
 public void SetOrbit(int laps){orbitLaps=Mathf.Max(0,laps);}

 static Mesh FishMesh(){
  if(mesh)return mesh;
  var source=Resources.Load<GameObject>(ModelPath);
  if(source){
   // The FBX holds a whole fish, a cut fish and a steak; use only the whole one.
   Mesh largest=null;
   foreach(var filter in source.GetComponentsInChildren<MeshFilter>(true)){
    if(!filter.sharedMesh)continue;
    if(filter.name==WholeFish){mesh=filter.sharedMesh;break;}
    if(!largest||filter.sharedMesh.bounds.size.sqrMagnitude>largest.bounds.size.sqrMagnitude)largest=filter.sharedMesh;
   }
   if(!mesh)mesh=largest;
  }
  if(!mesh&&!warned){warned=true;Debug.LogWarning("Losos: brak modelu Resources/"+ModelPath+" - uzywam zastepczego ksztaltu.");}
  return mesh;
 }
 static Material FishMaterial(){
  if(material)return material;
  material=new Material(Shader.Find("Standard")){name="Losos",color=new Color(.86f,.5f,.42f)};material.SetFloat("_Glossiness",.1f);
  var texture=Resources.Load<Texture2D>(TexturePath);
  if(texture){texture.filterMode=FilterMode.Point;material.mainTexture=texture;material.color=Color.white;}
  return material;
 }
 // Builds the whole fish centred on the returned root: length along +Z, flat side facing +Y.
 public static GameObject Model(Transform parent,float length){
  var root=new GameObject("Losos");root.transform.SetParent(parent,false);
  var fish=FishMesh();
  if(!fish){
   var placeholder=GameObject.CreatePrimitive(PrimitiveType.Sphere);placeholder.name="Losos - placeholder";placeholder.transform.SetParent(root.transform,false);
   var collider=placeholder.GetComponent<Collider>();collider.enabled=false;if(Application.isPlaying)Destroy(collider);else DestroyImmediate(collider);
   placeholder.transform.localScale=new Vector3(length*.28f,length*.14f,length);placeholder.GetComponent<Renderer>().sharedMaterial=FishMaterial();return root;
  }
  var g=new GameObject(WholeFish);g.transform.SetParent(root.transform,false);
  g.AddComponent<MeshFilter>().sharedMesh=fish;g.AddComponent<MeshRenderer>().sharedMaterial=FishMaterial();
  // Orientation is derived from the mesh itself, so import axis settings do not matter.
  var size=fish.bounds.size;int longest=0,thinnest=0;
  for(int i=1;i<3;i++){if(size[i]>size[longest])longest=i;if(size[i]<size[thinnest])thinnest=i;}
  if(longest==thinnest||size[longest]<=.00001f)return root;
  Vector3 forward=Vector3.zero,up=Vector3.zero;forward[longest]=1;up[thinnest]=1;
  var rotation=Quaternion.Inverse(Quaternion.LookRotation(forward,up));float scale=length/size[longest];
  g.transform.localRotation=rotation;g.transform.localScale=Vector3.one*scale;g.transform.localPosition=-(rotation*fish.bounds.center)*scale;
  return root;
 }
 public static LososBoomerang Spawn(Vector3 position,Vector3 forward,Vector3 right,GameObject owner,Transform home){
  var t=Tuning;
  var g=new GameObject("Losos - bumerang");g.transform.position=position;g.transform.rotation=Quaternion.LookRotation(forward,Vector3.Cross(forward,right));
  var graphics=new GameObject("Model pocisku");graphics.transform.SetParent(g.transform,false);
  // Slight bank so the spinning fish reads as a disc from the player's point of view.
  var bank=new GameObject("Przechyl");bank.transform.SetParent(graphics.transform,false);bank.transform.localRotation=Quaternion.Euler(0,0,-20);
  var model=Model(bank.transform,t.modelLength);
  var boomerang=g.AddComponent<LososBoomerang>();boomerang.owner=owner;boomerang.home=home;boomerang.spinner=model.transform;
  boomerang.direction=forward.normalized;boomerang.side=right.normalized;
  ProjectileVisual.Attach(g.transform,graphics.transform,true,t.launchSpeed);
  Active=boomerang;return boomerang;
 }
 Vector3 HomePoint=>home?home.position-home.up*.25f:owner.transform.position;
 void BeginReturn(Vector3 initial){returning=true;returnAge=0;struck.Clear();velocity=initial+side*Tuning.curve;}
 void Update(){
  float dt=Time.deltaTime;if(dt<=0)return;
  if(!owner){Destroy(gameObject);return;}
  var t=Tuning;
  // Time spent circling an enemy does not count against the flight limit.
  if(!orbiting)age+=dt;
  // Safety net: the fish always comes back to the hand, even if something goes wrong on the way.
  if(age>=t.maxFlightTime){Destroy(gameObject);return;}
  if(spinner)spinner.Rotate(0,t.spinSpeed*dt,0,Space.Self);
  if(orbiting){Orbit(dt,t);return;}
  if(!returning){
   float progress=Mathf.Clamp01(travelled/Mathf.Max(.01f,t.range));
   float speed=t.launchSpeed*Mathf.Lerp(1,.2f,progress*progress);
   Move(direction*Mathf.Min(speed*dt,t.range-travelled+.001f),t);
   return;
  }
  returnAge+=dt;
  var offset=HomePoint-transform.position;float distance=offset.magnitude;
  if(distance<=t.catchDistance){Destroy(gameObject);return;}
  float returnSpeed=Mathf.Min(t.returnSpeed,t.launchSpeed*.2f+t.returnSpeed*returnAge*2.5f);
  // Steering gets stronger over time, so the arc always closes on a moving player.
  velocity=Vector3.MoveTowards(velocity,offset/distance*returnSpeed,t.returnSpeed*(3+returnAge*8)*dt);
  var step=velocity*dt;
  bool caught=Vector3.Dot(offset,step)>0&&step.magnitude>=distance-t.catchDistance;
  Move(step,t);
  if(caught&&this)Destroy(gameObject);
 }
 void Move(Vector3 step,LososTuning t){
  float distance=step.magnitude;if(distance<=.0001f)return;
  var heading=step/distance;
  // Outbound: everything except the player and first-person layers. Returning: enemies only,
  // so the fish cannot get stuck behind level geometry on its way back.
  int mask=returning?1<<9:~((1<<8)|(1<<11));
  var hits=Physics.SphereCastAll(transform.position,Radius,heading,distance,mask,QueryTriggerInteraction.Ignore);
  System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
  foreach(var hit in hits){
   if(hit.collider.transform.IsChildOf(owner.transform))continue;
   // A cast that starts inside a collider reports no usable point or normal.
   bool inside=hit.distance<=0&&hit.point==Vector3.zero;
   var enemy=hit.collider.GetComponentInParent<Enemy>();
   if(enemy){
    if(enemy.health<=0||!struck.Add(enemy.GetEntityId()))continue;
    enemy.Damage(t.damage,inside?hit.collider.bounds.center:hit.point,inside?-heading:hit.normal,false);
    if(!returning&&orbitLaps>0&&enemy.health>0&&IsLastEnemy(enemy,heading,t.range-travelled)){StartOrbit(enemy);return;}
    continue;
   }
   if(returning||hit.collider.GetComponentInParent<PlayerController>())continue;
   var normal=inside?-heading:hit.normal;
   if(!inside)transform.position=hit.point+hit.normal*(Radius+.02f);
   Effect.Emit(transform.position,normal,ImpactEffect.Bounce);
   var game=GameSession.Instance;if(game)game.Sound(game.bounceSound,transform.position);
   BeginReturn(Vector3.Reflect(heading,normal)*t.launchSpeed*.25f);
   return;
  }
  transform.position+=step;
  if(!returning){travelled+=distance;if(travelled>=t.range)BeginReturn(direction*t.launchSpeed*.2f);}
 }
 // True when no other living enemy lies further along the outbound path, up to the first wall.
 bool IsLastEnemy(Enemy current,Vector3 heading,float remaining){
  if(remaining<=0)return true;
  if(Physics.SphereCast(transform.position,Radius,heading,out var wall,remaining,~((1<<8)|(1<<9)|(1<<11)),QueryTriggerInteraction.Ignore))remaining=wall.distance;
  foreach(var hit in Physics.SphereCastAll(transform.position,Radius,heading,remaining,1<<9,QueryTriggerInteraction.Ignore)){
   var other=hit.collider.GetComponentInParent<Enemy>();
   if(other&&other!=current&&other.health>0&&!struck.Contains(other.GetEntityId()))return false;
  }
  return true;
 }
 Vector3 OrbitCentre=>orbitCollider?orbitCollider.bounds.center:orbitTarget.transform.position+Vector3.up;
 void StartOrbit(Enemy enemy){
  orbiting=true;orbitTarget=enemy;orbitCollider=enemy.GetComponent<Collider>();lapsDone=0;orbitTurned=0;
  var offset=transform.position-OrbitCentre;orbitLift=offset.y;offset.y=0;
  orbitDistance=Mathf.Max(.3f,offset.magnitude);orbitAngle=Mathf.Atan2(offset.x,offset.z)*Mathf.Rad2Deg;
 }
 void Orbit(float dt,LososTuning t){
  if(!orbitTarget||orbitTarget.health<=0){EndOrbit(t);return;}
  float turn=360/Mathf.Max(.05f,t.orbitLapTime)*dt;orbitAngle+=turn;orbitTurned+=turn;
  // Ease from the point of impact onto the orbit ring at the enemy's mid height.
  orbitDistance=Mathf.MoveTowards(orbitDistance,t.orbitRadius,t.launchSpeed*dt);orbitLift=Mathf.MoveTowards(orbitLift,0,12*dt);
  var centre=OrbitCentre;var outward=Quaternion.Euler(0,orbitAngle,0)*Vector3.forward;
  transform.position=centre+outward*orbitDistance+Vector3.up*orbitLift;
  // One hit per completed lap.
  while(orbiting&&orbitTurned>=360*(lapsDone+1)){
   lapsDone++;orbitTarget.Damage(t.damage,centre+outward*.3f,outward,false);
   if(lapsDone>=orbitLaps||orbitTarget.health<=0)EndOrbit(t);
  }
 }
 void EndOrbit(LososTuning t){
  orbiting=false;
  // Leave along the tangent so the return arc continues the circling motion.
  var tangent=Quaternion.Euler(0,orbitAngle+90,0)*Vector3.forward;
  float speed=Mathf.Min(t.returnSpeed,2*Mathf.PI*t.orbitRadius/Mathf.Max(.05f,t.orbitLapTime));
  var target=orbitTarget;orbitTarget=null;orbitCollider=null;
  returning=true;returnAge=0;struck.Clear();velocity=tangent*speed;
  // The circled enemy has already taken its hits; do not strike it again on the way back.
  if(target&&target.health>0)struck.Add(target.GetEntityId());
 }
 void OnDestroy(){if(Active==this)Active=null;}
}
}
