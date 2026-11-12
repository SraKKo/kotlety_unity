using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety {
// Vertical displacement temporarily owns position; NavMesh resumes at the same ground location.
[RequireComponent(typeof(Enemy))]
public class EnemyLaunch : MonoBehaviour {
 public bool Active {get;private set;}
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
  if(dy<=0){
   var feet=transform.position-Vector3.up*feetOffset+Vector3.up*.1f;
   if(Physics.Raycast(feet,Vector3.down,out var ground,-dy+.12f,Terrain,QueryTriggerInteraction.Ignore)&&ground.normal.y>.4f){Finish();return;}
  }
  transform.position+=Vector3.up*dy;
  if(transform.position.y<origin.y||age>3)Finish();
 }
 void Finish(){
  if(!Active)return;Active=false;transform.position=origin;
  if(controlledAgent&&agent&&agent.enabled&&agent.isOnNavMesh){agent.nextPosition=origin;agent.updatePosition=updatePosition;agent.isStopped=wasStopped;}
  controlledAgent=false;
 }
 void OnDisable(){Finish();}
}
}
