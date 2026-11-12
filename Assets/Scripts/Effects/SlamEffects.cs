using UnityEngine;
namespace RetyKotlety {
public static class SlamEffects {
 static Material squares;
 static ParticleSystem Layer(Transform root,string name,float lifetime,int limit){
  var g=new GameObject(name);g.transform.SetParent(root,false);
  var system=g.AddComponent<ParticleSystem>();system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=system.main;main.playOnAwake=false;main.loop=false;main.duration=lifetime;main.startLifetime=lifetime;main.startSpeed=0;main.maxParticles=limit;main.simulationSpace=ParticleSystemSimulationSpace.World;
  var emission=system.emission;emission.enabled=false;var shape=system.shape;shape.enabled=false;
  var fade=system.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.3f),new GradientAlphaKey(0,1)});fade.color=gradient;
  var sizes=system.sizeOverLifetime;sizes.enabled=true;sizes.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.EaseInOut(0,1,1,0));
  var renderer=g.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=squares;renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
  system.Play();return system;
 }
 public static void Emit(Vector3 point,Vector3 normal,SlamImpact impact){
  if(!squares)squares=new Material(Shader.Find("Sprites/Default")){name="Slam - kwadratowe odlamki",mainTexture=Texture2D.whiteTexture};
  normal=normal.sqrMagnitude>.1f?normal.normalized:Vector3.up;
  var tangent=Vector3.Cross(normal,Mathf.Abs(normal.y)>.9f?Vector3.forward:Vector3.up).normalized;
  var bitangent=Vector3.Cross(normal,tangent).normalized;
  float power=impact.Power;
  float ringStart=Mathf.Min(.12f,impact.Radius*.2f);
  var root=new GameObject("Slam - fala uderzeniowa");root.transform.position=point;
  var wave=new GameObject("Czytelna granica slamu");wave.transform.SetParent(root.transform,false);
  wave.AddComponent<SlamWave>().Configure(squares,normal,tangent,bitangent,impact);
  var dust=Layer(root.transform,"Brazowe okruchy",.65f,120);
  var gravity=dust.main;gravity.gravityModifier=.8f;
  int crumbCount=Mathf.RoundToInt(Mathf.Lerp(40,100,power));
  for(int i=0;i<crumbCount;i++){
   float angle=Random.Range(0f,Mathf.PI*2);var radial=tangent*Mathf.Cos(angle)+bitangent*Mathf.Sin(angle);
   var particle=new ParticleSystem.EmitParams{position=point+normal*.15f+radial*Random.Range(0,ringStart),velocity=radial*Random.Range(.4f,1f)*(impact.Radius/.65f)+normal*Random.Range(1.5f,4f),startLifetime=Random.Range(.3f,.65f),startSize=Random.Range(.05f,.13f)*(1+power*.35f),startColor=Random.value<.22f?new Color(1,.9f,.55f):Color.Lerp(new Color(.24f,.09f,.025f),new Color(.65f,.36f,.12f),Random.value),rotation=Random.Range(0,90)};
   dust.Emit(particle,1);
  }
  Object.Destroy(root,.9f);
 }
}
}
