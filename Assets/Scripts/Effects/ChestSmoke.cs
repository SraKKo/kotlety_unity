using UnityEngine;
namespace RetyKotlety {
public static class ChestSmoke {
 static Material material;
 static Material SmokeMaterial(){
  if(material)return material;
  material=new Material(Shader.Find("Sprites/Default")){name="Czarne kwadraty dymu",mainTexture=Texture2D.whiteTexture};return material;
 }
 public static ParticleSystem Emit(Bounds chest,float duration){
  // Independent root lets the remaining smoke fade after the chest is removed.
  var root=new GameObject("Dym otwartej skrzynki");root.transform.position=new Vector3(chest.center.x,chest.max.y-.04f,chest.center.z);root.transform.rotation=Quaternion.Euler(-90,0,0);
  var system=root.AddComponent<ParticleSystem>();system.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=system.main;main.playOnAwake=false;main.loop=false;main.duration=duration;main.startLifetime=new ParticleSystem.MinMaxCurve(.9f,1.2f);main.startSpeed=new ParticleSystem.MinMaxCurve(.08f,.18f);
  main.startSize=new ParticleSystem.MinMaxCurve(.06f,.1f);main.startColor=new Color(.008f,.007f,.006f,1);main.startRotation=0;main.maxParticles=1600;main.simulationSpace=ParticleSystemSimulationSpace.World;
  var emission=system.emission;emission.rateOverTime=1000;
  var shape=system.shape;shape.shapeType=ParticleSystemShapeType.Box;shape.scale=new Vector3(chest.size.x*.85f,chest.size.z*.85f,.16f);
  var size=system.sizeOverLifetime;size.enabled=true;size.size=new ParticleSystem.MinMaxCurve(1,new AnimationCurve(new Keyframe(0,1),new Keyframe(.75f,1),new Keyframe(1,.25f)));
  var fade=system.colorOverLifetime;fade.enabled=true;var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.8f),new GradientAlphaKey(0,1)});fade.color=gradient;
  var noise=system.noise;noise.enabled=true;noise.strength=.025f;noise.frequency=.45f;noise.scrollSpeed=.2f;
  var renderer=root.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=SmokeMaterial();renderer.renderMode=ParticleSystemRenderMode.Billboard;renderer.sortMode=ParticleSystemSortMode.Distance;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
  system.Play();system.Emit(600);Object.Destroy(root,duration+1.4f);return system;
 }
}
}
