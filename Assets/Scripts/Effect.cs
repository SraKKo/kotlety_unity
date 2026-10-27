using UnityEngine;
namespace RetyKotlety {
public enum ImpactEffect { Food, Pan, Bounce, Deflect, Death }
public static class Effect {
 static Material soft,pan,comic;static Mesh cube;
 static Material MaterialFor(ImpactEffect kind){
  if(kind==ImpactEffect.Pan){if(!pan)pan=Make(Resources.Load<Texture2D>("Effects/pan_fragments"));return pan;}
  if(kind==ImpactEffect.Death){if(!comic)comic=Make(Resources.Load<Texture2D>("Effects/bum"));return comic;}
  if(!soft){var texture=new Texture2D(32,32,TextureFormat.RGBA32,false){name="Soft particle",wrapMode=TextureWrapMode.Clamp};var pixels=new Color[1024];for(int y=0;y<32;y++)for(int x=0;x<32;x++){float radius=new Vector2((x-15.5f)/15.5f,(y-15.5f)/15.5f).magnitude;pixels[y*32+x]=new Color(1,1,1,Mathf.Clamp01((1-radius)*4));}texture.SetPixels(pixels);texture.Apply();soft=Make(texture);}return soft;
 }
 static Material Make(Texture texture){var m=new Material(Shader.Find("Sprites/Default")){name="Rety Kotlety particle",mainTexture=texture};return m;}
 public static ParticleSystem Emit(Vector3 point,Vector3 normal,ImpactEffect kind){
  var g=new GameObject("Efekt "+kind);g.transform.position=point;if(normal.sqrMagnitude>.001f)g.transform.rotation=Quaternion.LookRotation(normal);
  var p=g.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  bool death=kind==ImpactEffect.Death,metal=kind==ImpactEffect.Pan;int count=death?16:metal?15:kind==ImpactEffect.Bounce?5:8;
  float lifetime=death?.76f:metal?.5f:.3f;
  var main=p.main;main.playOnAwake=false;main.loop=false;main.duration=lifetime;main.startLifetime=new ParticleSystem.MinMaxCurve(lifetime*.65f,lifetime);main.startSpeed=new ParticleSystem.MinMaxCurve(death?1:2,death?3:5);main.startSize=new ParticleSystem.MinMaxCurve(death?.22f:metal?.06f:.035f,death?.55f:metal?.11f:.08f);main.maxParticles=count;main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=death?-.08f:metal?1:.55f;
  main.startColor=death||metal?Color.white:kind==ImpactEffect.Deflect?new Color(1,.95f,.55f):new Color(.9f,.55f,.16f);main.startRotation=new ParticleSystem.MinMaxCurve(0,Mathf.PI*2);
  var emission=p.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,(short)count)});
  var shape=p.shape;shape.shapeType=death?ParticleSystemShapeType.Sphere:ParticleSystemShapeType.Cone;shape.radius=death?.2f:.035f;shape.angle=metal?77:50;
  var colors=p.colorOverLifetime;colors.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.55f),new GradientAlphaKey(0,1)});colors.color=gradient;
  var size=p.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,.05f));
  var rotation=p.rotationOverLifetime;rotation.enabled=true;rotation.z=new ParticleSystem.MinMaxCurve(-5,5);
  var renderer=p.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=MaterialFor(kind);
  if(metal){if(!cube)cube=Resources.GetBuiltinResource<Mesh>("Cube.fbx");renderer.renderMode=ParticleSystemRenderMode.Mesh;renderer.mesh=cube;}
  else renderer.renderMode=ParticleSystemRenderMode.Billboard;
  p.Play();Object.Destroy(g,lifetime+.25f);return p;
 }
 public static void Burst(Vector3 point,Color color){Emit(point,Vector3.up,color==Color.red?ImpactEffect.Food:ImpactEffect.Bounce);}
 public static ParticleSystem Jet(Transform parent,Color color,Vector3 offset){
  var g=new GameObject("Dysza jetpacka");g.transform.SetParent(parent,false);g.transform.localPosition=offset;g.transform.localRotation=Quaternion.Euler(90,0,0);
  var p=g.AddComponent<ParticleSystem>();p.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);var main=p.main;main.playOnAwake=false;main.loop=true;main.duration=1;main.startLifetime=.6f;main.startSpeed=1.8f;main.startSize=.09f;main.startColor=color;main.maxParticles=32;main.simulationSpace=ParticleSystemSimulationSpace.World;main.gravityModifier=.15f;
  var emission=p.emission;emission.rateOverTime=24;var shape=p.shape;shape.shapeType=ParticleSystemShapeType.Cone;shape.angle=26.566f;shape.radius=.06f;
  var colors=p.colorOverLifetime;colors.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(0,1)});colors.color=gradient;p.GetComponent<ParticleSystemRenderer>().sharedMaterial=MaterialFor(ImpactEffect.Food);return p;
 }
}
public class JetpackEffects : MonoBehaviour {
 ParticleSystem red,yellow;PlayerController player;bool emitting;
 void Start(){player=GetComponent<PlayerController>();red=Effect.Jet(transform,new Color(.975f,0,.168f),new Vector3(-.2f,-.4f,-.4f));yellow=Effect.Jet(transform,new Color(1,.843f,0),new Vector3(.2f,-.4f,-.4f));}
 void Update(){bool active=player&&player.IsFlying;if(active==emitting)return;emitting=active;if(active){red.Play();yellow.Play();}else{red.Stop(true,ParticleSystemStopBehavior.StopEmitting);yellow.Stop(true,ParticleSystemStopBehavior.StopEmitting);}}
}
}
