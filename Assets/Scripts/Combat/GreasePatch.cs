using System.Collections.Generic;
using UnityEngine;
namespace RetyKotlety {
public class GreasePatch : MonoBehaviour {
 static Material material;Mesh mesh;Vector3 point,normal;float radius,duration,damage,interval,age,accumulator,slowMultiplier;MeshRenderer visual;MaterialPropertyBlock properties;
 readonly HashSet<EntityId> hit=new HashSet<EntityId>();
 static Material Yellow {get{if(!material)material=new Material(Shader.Find("Sprites/Default")){name="Zolty tluszcz",mainTexture=Texture2D.whiteTexture,color=new Color(1,.73f,.035f,.85f)};return material;}}
 public static GreasePatch Spawn(Vector3 point,Vector3 normal,MielonyTuning profile){
  var g=new GameObject("Plama tluszczu");g.transform.SetPositionAndRotation(point+normal*.025f,Quaternion.FromToRotation(Vector3.up,normal));
  var patch=g.AddComponent<GreasePatch>();patch.point=point;patch.normal=normal;patch.radius=Mathf.Max(.1f,profile.greaseRadius);patch.duration=Mathf.Max(.1f,profile.greaseDuration);patch.damage=Mathf.Max(0,profile.tickDamage);patch.interval=Mathf.Max(.01f,profile.tickInterval);
  patch.slowMultiplier=Mathf.Clamp(profile.slowMultiplier,.1f,1);patch.Build();Splatter(point,normal);return patch;
 }
 void Build(){
  const int segments=64;var vertices=new Vector3[segments+1];var triangles=new int[segments*3];
  for(int i=0;i<segments;i++){
   float angle=i*Mathf.PI*2/segments;float edge=radius*(.94f+.035f*Mathf.Sin(angle*7)+.025f*Mathf.Cos(angle*11));vertices[i+1]=new Vector3(Mathf.Cos(angle)*edge,0,Mathf.Sin(angle)*edge);
   triangles[i*3]=0;triangles[i*3+1]=(i+1)%segments+1;triangles[i*3+2]=i+1;
  }
  mesh=new Mesh{name="Plama tluszczu"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;
  properties=new MaterialPropertyBlock();visual=gameObject.AddComponent<MeshRenderer>();visual.sharedMaterial=Yellow;visual.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;visual.receiveShadows=false;
 }
 void FixedUpdate(){
  float step=Mathf.Min(Time.fixedDeltaTime,Mathf.Max(0,duration-age));age+=step;accumulator+=step;
  while(accumulator+.000001f>=interval){accumulator-=interval;Tick();}
  float alpha=1-Mathf.InverseLerp(duration*.8f,duration,age);properties.SetColor("_Color",new Color(1,.73f,.035f,.85f*alpha));visual.SetPropertyBlock(properties);
  if(age>=duration)Destroy(gameObject);
 }
 void Tick(){
  hit.Clear();var origin=point+normal*.2f;const int terrain=~((1<<8)|(1<<9)|(1<<11));
  foreach(var collider in Physics.OverlapSphere(point+normal*.8f,radius+1,1<<9,QueryTriggerInteraction.Ignore)){
   var enemy=collider.GetComponentInParent<Enemy>();if(!enemy||enemy.health<=0||hit.Contains(enemy.GetEntityId()))continue;
   var closest=collider.ClosestPoint(origin);var offset=closest-point;float height=Vector3.Dot(offset,normal);
   if(height<-.2f||height>1.8f||Vector3.ProjectOnPlane(offset,normal).sqrMagnitude>radius*radius)continue;
   var delta=collider.bounds.center-origin;if(Physics.Raycast(origin,delta.normalized,delta.magnitude,terrain,QueryTriggerInteraction.Ignore))continue;
   hit.Add(enemy.GetEntityId());enemy.ApplySlow(slowMultiplier,interval+.05f);enemy.Damage(damage,closest,normal,false);
  }
 }
 public static void Splatter(Vector3 point,Vector3 normal){
  var g=new GameObject("Rozprysk tluszczu");g.transform.position=point+normal*.08f;g.transform.rotation=Quaternion.LookRotation(normal);
  var p=g.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=p.main;main.playOnAwake=false;main.loop=false;main.duration=.5f;main.startLifetime=new ParticleSystem.MinMaxCurve(.25f,.5f);main.startSpeed=new ParticleSystem.MinMaxCurve(2,5);main.startSize=new ParticleSystem.MinMaxCurve(.05f,.14f);main.startColor=new Color(1,.8f,.08f);main.gravityModifier=1;main.maxParticles=48;main.simulationSpace=ParticleSystemSimulationSpace.World;
  var emission=p.emission;emission.rateOverTime=0;var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=75;shape.radius=.12f;
  var sizes=p.sizeOverLifetime;sizes.enabled=true;sizes.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));p.GetComponent<ParticleSystemRenderer>().sharedMaterial=Yellow;p.Play();p.Emit(48);Destroy(g,.65f);
 }
 void OnDestroy(){if(mesh)Destroy(mesh);}
}
}
