using UnityEngine;
namespace RetyKotlety {
// Currency is credited only on collection, independently of kill XP.
public class PointPickup : MonoBehaviour {
 static Material gold;int value;Vector3 rest;float age;bool collected;
 const int GroundMask=~((1<<8)|(1<<9)|(1<<11));
 public static void Spawn(Vector3 position,int value){
  if(value<=0)return;
  var root=GameObject.CreatePrimitive(PrimitiveType.Cube);root.name="Punkty +"+value;root.layer=2;
  var collider=root.GetComponent<Collider>();collider.enabled=false;Destroy(collider);
  if(Physics.Raycast(position+Vector3.up,Vector3.down,out var hit,80,GroundMask,QueryTriggerInteraction.Ignore))position=hit.point;
  root.transform.position=position+Vector3.up*.35f;root.transform.localScale=Vector3.one*.24f;
  if(!gold)gold=new Material(Shader.Find("Sprites/Default")){name="Zlote punkty",color=new Color(1,.8f,.15f)};
  root.GetComponent<Renderer>().sharedMaterial=gold;
  var pickup=root.AddComponent<PointPickup>();pickup.value=value;pickup.rest=root.transform.position;
 }
 void Update(){
  var session=GameSession.Instance;if(collected||!session||session.paused||!session.player)return;
  var progression=session.player.GetComponent<PlayerProgression>();if(!progression)return;
  age+=Time.deltaTime;transform.Rotate(35*Time.deltaTime,100*Time.deltaTime,0,Space.Self);
  var target=session.player.transform.position;float distance=Vector3.Distance(transform.position,target);
  // A ray check keeps magnet pickups on the player's side of walls/floors.
  bool reachable=distance<5&&!Physics.Linecast(transform.position,target,GroundMask,QueryTriggerInteraction.Ignore);
  if(reachable){transform.position=Vector3.MoveTowards(transform.position,target,14*Time.deltaTime);rest=transform.position;if(Vector3.Distance(transform.position,target)<.85f){collected=true;progression.State.AddPoints(value);Destroy(gameObject);}}
  else {transform.position=new Vector3(rest.x,rest.y+Mathf.Sin(age*3)*.08f,rest.z);}
 }
}
}
