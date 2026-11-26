using UnityEngine;
namespace RetyKotlety {
public class MielonyProjectile : MonoBehaviour {
 static MielonyTuning tuning;static Material material;static GameObject model;
 public static MielonyTuning Tuning {get{if(!tuning)tuning=Resources.Load<MielonyTuning>("Combat/Mielony");return tuning;}}
 float flightRate=1;
 Vector3 velocity;GameObject owner;CombatBonuses bonuses;float age;int bouncesRemaining;
 public int GroundBouncesRemaining=>bouncesRemaining;
 public void SetGroundBounces(int count){bouncesRemaining=Mathf.Max(0,count);}
 public static GameObject CreateVisual(Transform parent) {
  var root = new GameObject("Mielony PSX");
  root.transform.SetParent(parent,false);
  if(!model)model = Resources.Load<GameObject>("Weapons/Mielony/MielonyPSX");
  if(!model) {
   Debug.LogError("Brak modelu Weapons/Mielony/MielonyPSX");
   return root;
  }
  if(!material)material = new Material(Shader.Find("RetyKotlety/PSXFood")) { name="Mielony - przypieczona panierka" };
  var instance = Instantiate(model,root.transform,false);
  foreach(var renderer in instance.GetComponentsInChildren<MeshRenderer>())renderer.sharedMaterial = material;
  return root;
 }
 public static MielonyProjectile Spawn(Vector3 position,Vector3 forward,GameObject owner){
  var g=new GameObject("Mielony - rzut lukiem");g.transform.position=position;var graphics=new GameObject("Model pocisku");graphics.transform.SetParent(g.transform,false);var visual=CreateVisual(graphics.transform);visual.transform.localScale=Vector3.one*.32f;
  var projectile=g.AddComponent<MielonyProjectile>();projectile.owner=owner;projectile.bonuses=CombatBonuses.FromOwner(owner);projectile.velocity=forward.normalized*Tuning.launchSpeed+Vector3.up*Tuning.upwardBoost;
  projectile.flightRate=PlayerUpgrades.ShotSpeedFor(owner);
  ProjectileVisual.Attach(g.transform,graphics.transform,true,Tuning.launchSpeed*projectile.flightRate);return projectile;
 }
 void FixedUpdate(){
  float dt=Time.fixedDeltaTime;age+=dt;if(age>10){Destroy(gameObject);return;}
  dt*=flightRate;
  var next=transform.position+velocity*dt+Vector3.down*(.5f*Tuning.gravity*dt*dt);velocity+=Vector3.down*Tuning.gravity*dt;
  var step=next-transform.position;float distance=step.magnitude;if(distance<=.0001f)return;
  var hits=Physics.SphereCastAll(transform.position,.16f,step/distance,distance,~((1<<8)|(1<<11)),QueryTriggerInteraction.Ignore);
  System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
  foreach(var hit in hits){if(owner&&hit.collider.transform.IsChildOf(owner.transform))continue;
   Vector3 point=hit.point,normal=hit.normal;bool groundHit=normal.y>=.5f&&!hit.collider.GetComponentInParent<Enemy>();
   if(groundHit&&Vector3.Dot(velocity,normal)>=0)continue;
   if(normal.y<.5f||hit.collider.GetComponentInParent<Enemy>()){
    const int ground=~((1<<8)|(1<<9)|(1<<11));
    if(!Physics.Raycast(hit.point+hit.normal*.2f+Vector3.up*.5f,Vector3.down,out var floor,8,ground,QueryTriggerInteraction.Ignore)){GreasePatch.Splatter(hit.point,Vector3.up);Destroy(gameObject);return;}
    point=floor.point;normal=floor.normal;if(normal.y<.5f){Destroy(gameObject);return;}
   }
   GreasePatch.Spawn(point,normal,Tuning,bonuses);
   if(groundHit&&bouncesRemaining>0){
    bouncesRemaining--;velocity=Vector3.Reflect(velocity,normal)*Tuning.bounceRetention;
    float outward=Vector3.Dot(velocity,normal);if(outward<Tuning.minimumBounceSpeed)velocity+=normal*(Tuning.minimumBounceSpeed-outward);
    transform.position=point+normal*.18f;return;
   }
   Destroy(gameObject);return;
  }
  transform.position=next;transform.Rotate(180*dt,240*dt,0);
 }
}
}
