using UnityEngine;
namespace RetyKotlety {
public class Projectile : MonoBehaviour {
 public Vector3 direction; public float speed,damage; public int bounces; public bool hostile; public GameObject owner;
 float life=10; const float Radius=.09f;
 public static Projectile Spawn(Vector3 pos,Vector3 dir,float speed,float damage,int bounces,bool hostile,GameObject visual,GameObject owner){
  var g=new GameObject(hostile?"Pomidor":"Kotlet");g.transform.position=pos;g.transform.forward=dir;
  if(visual){var model=Instantiate(visual,g.transform);model.transform.localPosition=Vector3.zero;model.transform.localRotation=Quaternion.identity;model.transform.localScale=Vector3.one;foreach(var c in model.GetComponentsInChildren<Collider>())Destroy(c);}else{var m=GameObject.CreatePrimitive(PrimitiveType.Sphere);m.transform.SetParent(g.transform,false);m.transform.localScale=Vector3.one*.2f;Destroy(m.GetComponent<Collider>());m.GetComponent<Renderer>().material.color=Color.red;}
  var p=g.AddComponent<Projectile>();p.direction=dir.normalized;p.speed=speed;p.damage=damage;p.bounces=bounces;p.hostile=hostile;p.owner=owner;return p;
 }
 public static Projectile[] SpawnVolley(Vector3 position,Vector3 forward,Vector3 up,GameObject visual,GameObject owner,float spread=2.5f){
  var shots=new Projectile[3];var right=Vector3.Cross(up,forward).normalized;
  for(int i=0;i<3;i++){var direction=Quaternion.AngleAxis((i-1)*spread,up)*forward;shots[i]=Spawn(position+right*((i-1)*.035f),direction,100,20,0,false,visual,owner);}return shots;
 }
 void Update(){float dt=Time.deltaTime;if(dt<=0)return;life-=dt;if(life<=0){Destroy(gameObject);return;}
  float distance=speed*dt;
  var hits=Physics.SphereCastAll(transform.position,Radius,direction,distance,~0,QueryTriggerInteraction.Ignore);
  System.Array.Sort(hits,(a,b)=>a.distance.CompareTo(b.distance));
  foreach(var hit in hits){if(owner&&hit.collider.transform.IsChildOf(owner.transform))continue;
   var enemy=hit.collider.GetComponentInParent<Enemy>();var player=hit.collider.GetComponentInParent<PlayerController>();
   if(hostile&&enemy)continue;if(!hostile&&player)continue;
   transform.position=hit.point+hit.normal*(Radius+.02f);
   if(hostile&&player){if(player.Blocking){hostile=false;owner=player.gameObject;direction=player.view.transform.forward;transform.position=player.view.transform.position+direction;Effect.Emit(hit.point,hit.normal,ImpactEffect.Deflect);}else{player.Damage(damage);Destroy(gameObject);}return;}
   if(!hostile&&enemy){enemy.Damage(damage,hit.point,hit.normal,false);Destroy(gameObject);return;}
   if(--bounces>0){direction=Vector3.Reflect(direction,hit.normal);transform.forward=direction;Effect.Emit(hit.point,hit.normal,ImpactEffect.Bounce);var g=GameSession.Instance;if(g)g.Sound(g.bounceSound,hit.point);}else{Effect.Emit(hit.point,hit.normal,ImpactEffect.Food);Destroy(gameObject);}return;
  }
  transform.position+=direction*distance;transform.Rotate(0,0,600*dt,Space.Self);
 }
}
}

