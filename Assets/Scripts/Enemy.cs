using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety {
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour {
 public bool shooting,dummy; public float health=200; public float sightRange=12,attackRange=2;
 NavMeshAgent agent; float cooldown,pullTimer; Animation legacy;
 void Awake(){agent=GetComponent<NavMeshAgent>();}
 void Start(){if(!dummy&&!agent.enabled)agent.enabled=true;agent.speed=shooting?2:6;agent.angularSpeed=360;agent.stoppingDistance=shooting?12:1.8f;agent.updateRotation=false;legacy=WeaponPresentation.FindAnimation(gameObject,"walking");if(shooting){health=120;attackRange=18;}}
 void Update(){var g=GameSession.Instance;if(!g||g.paused||!g.player||dummy)return;float dt=Time.deltaTime;cooldown-=dt;pullTimer-=dt;if(pullTimer>0)return;
  var target=g.player;float distance=Vector3.Distance(transform.position,target.transform.position);
  if(!agent.isOnNavMesh)return;
  if(distance>sightRange){agent.isStopped=true;Animate("idle");return;}
  Vector3 origin=transform.position+Vector3.up*1.2f;Vector3 aim=target.view.transform.position-origin;
  bool visible=!Physics.Raycast(origin,aim.normalized,out var hit,aim.magnitude,~(1<<9),QueryTriggerInteraction.Ignore)||hit.collider.GetComponentInParent<PlayerController>();
  if(distance>attackRange||!visible){agent.isStopped=false;agent.SetDestination(target.transform.position);Animate("walking");Face(agent.desiredVelocity,dt);return;}
  agent.isStopped=true;Vector3 dir=target.transform.position-transform.position;dir.y=0;Face(dir,dt);
  Animate("attack");if(cooldown>0)return;cooldown=shooting?2:1;
  if(shooting)Projectile.Spawn(origin+aim.normalized*.7f,aim.normalized,20,50,0,true,g.tomatoModel,gameObject);else target.Damage(20);
 }
 void Face(Vector3 direction,float dt){direction.y=0;if(direction.sqrMagnitude>.001f){var facing=Quaternion.LookRotation(direction);if(!shooting)facing*=Quaternion.Euler(0,180,0);transform.rotation=Quaternion.Slerp(transform.rotation,facing,dt*7.5f);}}
 void Animate(string key){if(!legacy)return;foreach(AnimationState state in legacy){if(state.name.ToLowerInvariant().Contains(key)&&!legacy.IsPlaying(state.name)){state.wrapMode=key=="idle"||key=="walking"?WrapMode.Loop:WrapMode.Once;legacy.CrossFade(state.name,.15f);break;}}}
 public void Pull(Vector3 target,float distance){pullTimer=.15f;if(agent&&agent.isOnNavMesh){agent.isStopped=true;var pos=Vector3.MoveTowards(transform.position,target,distance);if(NavMesh.SamplePosition(pos,out var hit,3,NavMesh.AllAreas))agent.Warp(hit.position);}}
 public void Damage(float amount){Damage(amount,transform.position+Vector3.up,Vector3.up,false);}
 public void Damage(float amount,Vector3 point,Vector3 normal,bool melee){
  if(health<=0||amount<=0)return;float applied=Mathf.Min(health,amount);health-=applied;
  DamageNumber.Show(GetInstanceID(),applied,point+Vector3.up*.4f);Effect.Emit(point,normal,melee?ImpactEffect.Pan:ImpactEffect.Food);
  if(health<=0){Effect.Emit(transform.position+Vector3.up,Vector3.up,ImpactEffect.Death);Destroy(gameObject);}
 }
}
}



