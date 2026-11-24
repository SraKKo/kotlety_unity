using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety {
// Owns displacement while launched; batting adds swept crowd collisions to the same motion.
[RequireComponent(typeof(Enemy))]
public class EnemyLaunch : MonoBehaviour {
 public bool Active {get;private set;}
 public bool Batted {get;private set;}
 Vector3 horizontal;float collisionDamage;CombatBonuses bonuses;
 readonly System.Collections.Generic.HashSet<Enemy> struck=new System.Collections.Generic.HashSet<Enemy>();
 public void Bat(Vector3 direction,PanBowlingStats stats,CombatBonuses snapshot){
  Begin(5);Batted=true;direction.y=0;if(direction.sqrMagnitude<.001f)direction=transform.forward;
  horizontal=direction.normalized*stats.Speed;collisionDamage=stats.Damage;bonuses=snapshot;struck.Clear();struck.Add(GetComponent<Enemy>());age=0;
 }
 public void Shove(Vector3 direction){Begin(2);direction.y=0;horizontal=direction.normalized*4;}

 NavMeshAgent agent;CapsuleCollider body;Vector3 origin;float speed,age,feetOffset;bool updatePosition,wasStopped,controlledAgent;
 const float Gravity=24;const int Terrain=~((1<<8)|(1<<9)|(1<<11));
 public void Begin(float impulse){
  if(impulse<=0)return;
  if(Active){speed=Mathf.Max(speed,impulse);return;}
  agent=GetComponent<NavMeshAgent>();body=GetComponent<CapsuleCollider>();origin=transform.position;feetOffset=body?transform.position.y-body.bounds.min.y:0;
  speed=impulse;age=0;Active=true;controlledAgent=agent&&agent.enabled&&agent.isOnNavMesh;
  if(controlledAgent){updatePosition=agent.updatePosition;wasStopped=agent.isStopped;agent.isStopped=true;agent.updatePosition=false;}
 }
 void FixedUpdate(){
  if(!Active)return;var session=GameSession.Instance;if(session&&session.paused)return;
  float dt=Time.fixedDeltaTime;age+=dt;float dy=speed*dt-.5f*Gravity*dt*dt;speed-=Gravity*dt;
  if(dy>0&&body){
   var head=new Vector3(transform.position.x,body.bounds.max.y+.01f,transform.position.z);
   if(Physics.Raycast(head,Vector3.up,out var ceiling,dy+.02f,Terrain,QueryTriggerInteraction.Ignore)){dy=Mathf.Max(0,ceiling.distance-.02f);speed=0;}
  }
  if(horizontal.sqrMagnitude>.001f){
   var delta=horizontal*dt;var center=body?body.bounds.center:transform.position+Vector3.up;
   float radius=body?Mathf.Max(.1f,Mathf.Min(body.bounds.extents.x,body.bounds.extents.z)):.35f;
   bool wall=Physics.SphereCast(center,radius,delta.normalized,out var obstacle,delta.magnitude+.03f,Terrain,QueryTriggerInteraction.Ignore);
   float distance=wall?Mathf.Max(0,obstacle.distance-.03f):delta.magnitude;
   if(Batted){
    // Overlap covers existing contacts; the sweep prevents fast bodies skipping small enemies.
    var contacts=Physics.OverlapSphere(center,radius,1<<9,QueryTriggerInteraction.Ignore);
    foreach(var collider in contacts)Hit(collider,center);
    foreach(var hit in Physics.SphereCastAll(center,radius,delta.normalized,distance,1<<9,QueryTriggerInteraction.Ignore))Hit(hit.collider,hit.point);
   }
   transform.position+=delta.normalized*distance;
   if(wall){Finish();return;}
  }
  if(dy<=0){
   var feet=transform.position-Vector3.up*feetOffset+Vector3.up*.1f;
   if(Physics.Raycast(feet,Vector3.down,out var ground,-dy+.12f,Terrain,QueryTriggerInteraction.Ignore)&&ground.normal.y>.4f){if(horizontal.sqrMagnitude>.001f)transform.position=new Vector3(transform.position.x,ground.point.y+feetOffset,transform.position.z);Finish();return;}
  }
  transform.position+=Vector3.up*dy;
  if((horizontal.sqrMagnitude<.001f&&transform.position.y<origin.y)||age>3)Finish();
 }
 void Hit(Collider collider,Vector3 point){
  var enemy=collider.GetComponentInParent<Enemy>();if(!enemy||enemy.health<=0||!struck.Add(enemy))return;
  enemy.Damage(bonuses.Damage(collisionDamage,enemy.IsAirborne),point,-horizontal.normalized,true);
  if(enemy.health>0){var push=enemy.GetComponent<EnemyLaunch>();if(!push)push=enemy.gameObject.AddComponent<EnemyLaunch>();if(!push.Batted)push.Shove(horizontal);}
 }
 void Finish(){
  if(!Active)return;Active=false;
  bool moved=horizontal.sqrMagnitude>.001f;var destination=moved?transform.position:origin;
  if(moved&&controlledAgent){if(NavMesh.SamplePosition(destination,out var landing,3,NavMesh.AllAreas))destination=landing.position;else destination=origin;}
  transform.position=destination;
  if(controlledAgent&&agent&&agent.enabled&&agent.isOnNavMesh){agent.nextPosition=destination;agent.updatePosition=updatePosition;agent.isStopped=wasStopped;}
  controlledAgent=false;horizontal=Vector3.zero;Batted=false;
  var enemy=GetComponent<Enemy>();if(enemy&&enemy.health<=0)Destroy(gameObject);
 }
 void OnDisable(){Finish();}
}
}
