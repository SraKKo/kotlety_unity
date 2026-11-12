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
 public static Projectile Spawn(Vector3 pos,Vector3 dir,float speed,float damage,int bounces,bool hostile,GameObject visual,GameObject owner){
  var g=new GameObject(hostile?"Pomidor":"Kotlet");g.transform.position=pos;g.transform.forward=dir;
  var graphics=new GameObject("Model pocisku");graphics.transform.SetParent(g.transform,false);
  if(visual){var model=Instantiate(visual,graphics.transform);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;foreach(var c in model.GetComponentsInChildren<Collider>())Destroy(c);}else{var m=GameObject.CreatePrimitive(PrimitiveType.Sphere);m.transform.SetParent(graphics.transform,false);m.transform.localScale=Vector3.one*.2f;Destroy(m.GetComponent<Collider>());m.GetComponent<Renderer>().material.color=Color.red;}
  var p=g.AddComponent<Projectile>();p.direction=dir.normalized;p.speed=speed;p.damage=damage;p.bounces=bounces;p.hostile=hostile;p.owner=owner;ProjectileVisual.Attach(g.transform,graphics.transform,!hostile,speed);return p;
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

