using UnityEngine;
using System.Collections.Generic;
namespace RetyKotlety {
public class Projectile : MonoBehaviour {
 public Vector3 direction; public float speed,damage; public int bounces; public bool hostile; public GameObject owner;
 bool piercing;float bleedDamage,bleedInterval,bleedDuration;
 readonly HashSet<EntityId> piercedVisited=new HashSet<EntityId>();
 public void SetPiercing(bool value){piercing=value;}
 public void SetBleed(float damage,float interval,float duration){bleedDamage=damage;bleedInterval=interval;bleedDuration=duration;}
 void ApplyBleed(Enemy enemy){if(bleedDamage<=0||enemy.health<=0)return;var status=enemy.GetComponent<EnemyBleed>();if(!status)status=enemy.gameObject.AddComponent<EnemyBleed>();status.Apply(bleedDamage,bleedInterval,bleedDuration);}
 int chainHitsRemaining;float chainRange;bool chainStarted;Enemy chainTarget;
 readonly HashSet<EntityId> chainVisited=new HashSet<EntityId>();
 public void EnableEnemyRicochet(int additionalTargets,float range){chainHitsRemaining=Mathf.Max(0,additionalTargets);chainRange=Mathf.Max(0,range);}
 bool Retarget(){
  Enemy nearest=null;Vector3 aim=Vector3.zero;float best=chainRange*chainRange;
  const int terrain=~((1<<8)|(1<<9)|(1<<11));
  foreach(var collider in Physics.OverlapSphere(transform.position,chainRange,1<<9,QueryTriggerInteraction.Ignore)){
   var enemy=collider.GetComponentInParent<Enemy>();if(!enemy||enemy.health<=0||chainVisited.Contains(enemy.GetEntityId()))continue;
   var point=collider.bounds.center;var delta=point-transform.position;float distance=delta.sqrMagnitude;if(distance>=best||distance<.0001f)continue;
   if(Physics.SphereCast(transform.position,Radius,delta.normalized,out _,Mathf.Sqrt(distance),terrain,QueryTriggerInteraction.Ignore))continue;
   best=distance;nearest=enemy;aim=point;
  }
  if(!nearest)return false;chainTarget=nearest;direction=(aim-transform.position).normalized;transform.forward=direction;return true;
 }
 float life=10; const float Radius=.09f;
 ParticleSystem crumbs;TrailRenderer streak;Transform visualRoot;Camera visibilityCamera;float visualDiameter=.2f;
 static Material outlineMaterial,crumbMaterial;
 public static Projectile Spawn(Vector3 pos,Vector3 dir,float speed,float damage,int bounces,bool hostile,GameObject visual,GameObject owner){
  var g=new GameObject(hostile?"Pomidor":"Kotlet");g.transform.position=pos;g.transform.forward=dir;
  var graphics=new GameObject("Model pocisku");graphics.transform.SetParent(g.transform,false);
  if(visual){var model=Instantiate(visual,graphics.transform);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;foreach(var c in model.GetComponentsInChildren<Collider>())Destroy(c);}else{var m=GameObject.CreatePrimitive(PrimitiveType.Sphere);m.transform.SetParent(graphics.transform,false);m.transform.localScale=Vector3.one*.2f;Destroy(m.GetComponent<Collider>());m.GetComponent<Renderer>().material.color=Color.red;}
  var p=g.AddComponent<Projectile>();p.direction=dir.normalized;p.speed=speed;p.damage=damage;p.bounces=bounces;p.hostile=hostile;p.owner=owner;p.visualRoot=graphics.transform;p.AddVisibility();return p;
 }
 void AddVisibility(){
  var renderers=visualRoot.GetComponentsInChildren<Renderer>();
  if(renderers.Length>0){var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);visualDiameter=Mathf.Max(.01f,Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));}
  if(!outlineMaterial){var shader=Resources.Load<Shader>("Effects/ProjectileOutline");if(shader)outlineMaterial=new Material(shader){name="Projectile outline"};}
  if(outlineMaterial){
   foreach(var mesh in GetComponentsInChildren<MeshFilter>()){
    if(!mesh.sharedMesh)continue;
    var border=new GameObject("Obrys pocisku");border.transform.SetParent(mesh.transform,false);border.layer=mesh.gameObject.layer;
    border.AddComponent<MeshFilter>().sharedMesh=mesh.sharedMesh;
    var renderer=border.AddComponent<MeshRenderer>();
    var materials=new Material[mesh.sharedMesh.subMeshCount];for(int i=0;i<materials.Length;i++)materials[i]=outlineMaterial;
    renderer.sharedMaterials=materials;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
   }
  }
  if(!crumbMaterial)crumbMaterial=new Material(Shader.Find("Sprites/Default")){name="Brown square crumbs",mainTexture=Texture2D.whiteTexture};
  var trail=new GameObject("Kwadratowy ślad pocisku");trail.transform.SetParent(transform,false);
  crumbs=trail.AddComponent<ParticleSystem>();crumbs.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=crumbs.main;main.playOnAwake=false;main.loop=true;main.simulationSpace=ParticleSystemSimulationSpace.World;
  main.startLifetime=new ParticleSystem.MinMaxCurve(.08f,.16f);main.startSpeed=new ParticleSystem.MinMaxCurve(0,.2f);
  main.startSize=new ParticleSystem.MinMaxCurve(.025f,.055f);
  var crumbColors=new Gradient();crumbColors.SetKeys(new[]{new GradientColorKey(new Color(.24f,.09f,.025f),0),new GradientColorKey(new Color(.55f,.28f,.09f),.7f),new GradientColorKey(new Color(1,.88f,.48f),.78f),new GradientColorKey(new Color(1,.97f,.77f),1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,1)});
  main.startColor=new ParticleSystem.MinMaxGradient(crumbColors){mode=ParticleSystemGradientMode.RandomColor};
  main.maxParticles=512;main.gravityModifier=.15f;main.startRotation=0;
  var emission=crumbs.emission;emission.rateOverTime=0;emission.rateOverDistance=30;
  var shape=crumbs.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.035f;
  var sizes=crumbs.sizeOverLifetime;sizes.enabled=true;sizes.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
  var colors=crumbs.colorOverLifetime;colors.enabled=true;var gradient=new Gradient();
  gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.5f),new GradientAlphaKey(0,1)});colors.color=gradient;
  var particles=crumbs.GetComponent<ParticleSystemRenderer>();particles.sharedMaterial=crumbMaterial;particles.renderMode=ParticleSystemRenderMode.Billboard;
  particles.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;particles.receiveShadows=false;
  crumbs.Play();
  if(!hostile){
   var glow=new GameObject("Jasna smuga pocisku");glow.transform.SetParent(transform,false);
   streak=glow.AddComponent<TrailRenderer>();streak.sharedMaterial=crumbMaterial;
   streak.time=Mathf.Clamp(2.2f/Mathf.Max(1,speed),.025f,.09f);streak.minVertexDistance=.035f;
   streak.alignment=LineAlignment.View;streak.widthMultiplier=.075f;
   streak.widthCurve=AnimationCurve.Linear(0,1,1,0);
   streak.startColor=new Color(1,.96f,.7f,.9f);streak.endColor=new Color(1,.72f,.28f,0);
   streak.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;streak.receiveShadows=false;
  }
 }
 void LateUpdate(){
  if(hostile||!visualRoot)return;
  if(!visibilityCamera)visibilityCamera=GameSession.Instance&&GameSession.Instance.player?GameSession.Instance.player.view:Camera.main;
  if(!visibilityCamera)return;
  float depth=Vector3.Dot(transform.position-visibilityCamera.transform.position,visibilityCamera.transform.forward);
  if(depth<=0)return;
  float screenHeight=visibilityCamera.orthographic?2*visibilityCamera.orthographicSize:2*depth*Mathf.Tan(visibilityCamera.fieldOfView*.5f*Mathf.Deg2Rad);
  float minimumDiameter=screenHeight*10/Mathf.Max(1,visibilityCamera.pixelHeight);
  // Enlarge graphics only; movement and collision radius retain their original values.
  float scale=Mathf.Clamp(minimumDiameter/visualDiameter,1,2.5f);
  visualRoot.localScale=Vector3.one*scale;
  if(streak)streak.widthMultiplier=.075f*Mathf.Sqrt(scale);
 }
 void OnDestroy(){
  if(streak){streak.transform.SetParent(null,true);streak.emitting=false;Destroy(streak.gameObject,streak.time+.05f);}
  if(!crumbs)return;
  crumbs.transform.SetParent(null,true);crumbs.Stop(true,ParticleSystemStopBehavior.StopEmitting);
  Destroy(crumbs.gameObject,.2f);
 }
 public static Projectile[] SpawnVolley(Vector3 position,Vector3 forward,Vector3 up,GameObject visual,GameObject owner,float spread=6f){
  forward=forward.normalized;
  var right=Vector3.Cross(up,forward).normalized;
  var shots=new Projectile[3];float radius=Mathf.Tan(Mathf.Clamp(spread,0,89)*Mathf.Deg2Rad);
  for(int i=0;i<shots.Length;i++){
   // Left, centre, right in the camera's horizontal plane, including while aiming up or down.
   var direction=(forward+right*((i-1)*radius)).normalized;
   shots[i]=Spawn(position,direction,100,20,0,false,visual,owner);
  }
  return shots;
 }

 void Update(){float dt=Time.deltaTime;if(dt<=0)return;life-=dt;if(life<=0){Destroy(gameObject);return;}
  if(chainStarted){
   if(!chainTarget||chainTarget.health<=0){if(!Retarget()){Destroy(gameObject);return;}}
   var collider=chainTarget.GetComponent<Collider>();if(collider)direction=(collider.bounds.center-transform.position).normalized;
  }
  var stepStart=transform.position;float distance=speed*dt;
  var hits=Physics.SphereCastAll(transform.position,Radius,direction,distance,~0,QueryTriggerInteraction.Ignore);
  System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
  foreach(var hit in hits){if(owner&&hit.collider.transform.IsChildOf(owner.transform))continue;
   var enemy=hit.collider.GetComponentInParent<Enemy>();var player=hit.collider.GetComponentInParent<PlayerController>();
   if(hostile&&enemy)continue;if(!hostile&&player)continue;
   if(!hostile&&enemy&&enemy.health<=0)continue;
   if(!hostile&&enemy&&chainStarted&&chainVisited.Contains(enemy.GetEntityId()))continue;
   if(!hostile&&enemy&&piercing&&piercedVisited.Contains(enemy.GetEntityId()))continue;
   transform.position=hit.point+hit.normal*(Radius+.02f);
   if(hostile&&player){if(player.Blocking){hostile=false;owner=player.gameObject;direction=player.view.transform.forward;transform.position=player.view.transform.position+direction;Effect.Emit(hit.point,hit.normal,ImpactEffect.Deflect);}else{player.Damage(damage);Destroy(gameObject);}return;}
   if(!hostile&&enemy){
    chainVisited.Add(enemy.GetEntityId());enemy.Damage(damage,hit.point,hit.normal,false);ApplyBleed(enemy);
    if(piercing){piercedVisited.Add(enemy.GetEntityId());continue;}
    if(chainStarted)chainHitsRemaining--;
    if(chainHitsRemaining>0){chainStarted=true;if(Retarget()){Effect.Emit(hit.point,hit.normal,ImpactEffect.Bounce);return;}}
    Destroy(gameObject);return;
   }
   if(chainStarted){Effect.Emit(hit.point,hit.normal,ImpactEffect.Food);Destroy(gameObject);return;}
   if(--bounces>0){direction=Vector3.Reflect(direction,hit.normal);transform.forward=direction;Effect.Emit(hit.point,hit.normal,ImpactEffect.Bounce);var g=GameSession.Instance;if(g)g.Sound(g.bounceSound,hit.point);}else{Effect.Emit(hit.point,hit.normal,ImpactEffect.Food);Destroy(gameObject);}return;
  }
  transform.position=stepStart+direction*distance;transform.Rotate(0,0,600*dt,Space.Self);
 }
}
}

