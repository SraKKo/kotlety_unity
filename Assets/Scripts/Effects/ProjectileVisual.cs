using UnityEngine;
namespace RetyKotlety {
// Shared outline, crumbs, tracer and distance readability for straight and lobbed shots.
public class ProjectileVisual : MonoBehaviour {
 ParticleSystem crumbs;TrailRenderer streak;Transform visualRoot;Camera visibilityCamera;float visualDiameter=.2f,speed;bool hostile;
 static Material outlineMaterial,crumbMaterial;
 public static void Attach(Transform root,Transform graphics,bool friendly,float speed){var component=root.gameObject.AddComponent<ProjectileVisual>();component.visualRoot=graphics;component.hostile=!friendly;component.speed=speed;component.AddVisibility();}
 void AddVisibility(){
  var renderers=visualRoot.GetComponentsInChildren<Renderer>();
  if(renderers.Length>0){var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);visualDiameter=Mathf.Max(.01f,Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z)));}
  if(!outlineMaterial){var shader=Resources.Load<Shader>("Effects/ProjectileOutline");if(shader)outlineMaterial=new Material(shader){name="Projectile outline"};}
  if(outlineMaterial){
   foreach(var mesh in GetComponentsInChildren<MeshFilter>()){
    if(!mesh.sharedMesh)continue;
    var border=new GameObject("Obrys pocisku");border.transform.SetParent(mesh.transform,false);border.layer=mesh.gameObject.layer;
    border.AddComponent<MeshFilter>().sharedMesh=mesh.sharedMesh;
    var renderer=border.AddComponent<MeshRenderer>();
    var materials=new Material[mesh.sharedMesh.subMeshCount];for(int i=0;i<materials.Length;i++)materials[i]=outlineMaterial;
    renderer.sharedMaterials=materials;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
   }
  }
  if(!crumbMaterial)crumbMaterial=new Material(Shader.Find("Sprites/Default")){name="Brown square crumbs",mainTexture=Texture2D.whiteTexture};
  var trail=new GameObject("Kwadratowy ślad pocisku");trail.transform.SetParent(transform,false);
  crumbs=trail.AddComponent<ParticleSystem>();crumbs.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
  var main=crumbs.main;main.playOnAwake=false;main.loop=true;main.simulationSpace=ParticleSystemSimulationSpace.World;
  main.startLifetime=new ParticleSystem.MinMaxCurve(.08f,.16f);main.startSpeed=new ParticleSystem.MinMaxCurve(0,.2f);
  main.startSize=new ParticleSystem.MinMaxCurve(.025f,.055f);
  var crumbColors=new Gradient();crumbColors.SetKeys(new[]{new GradientColorKey(new Color(.24f,.09f,.025f),0),new GradientColorKey(new Color(.55f,.28f,.09f),.7f),new GradientColorKey(new Color(1,.88f,.48f),.78f),new GradientColorKey(new Color(1,.97f,.77f),1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,1)});
  main.startColor=new ParticleSystem.MinMaxGradient(crumbColors){mode=ParticleSystemGradientMode.RandomColor};
  main.maxParticles=512;main.gravityModifier=.15f;main.startRotation=0;
  var emission=crumbs.emission;emission.rateOverTime=0;emission.rateOverDistance=30;
  var shape=crumbs.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.035f;
  var sizes=crumbs.sizeOverLifetime;sizes.enabled=true;sizes.size=new ParticleSystem.MinMaxCurve(1,AnimationCurve.Linear(0,1,1,0));
  var colors=crumbs.colorOverLifetime;colors.enabled=true;var gradient=new Gradient();
  gradient.SetKeys(new[]{new GradientColorKey(Color.white,0),new GradientColorKey(Color.white,1)},new[]{new GradientAlphaKey(1,0),new GradientAlphaKey(1,.5f),new GradientAlphaKey(0,1)});colors.color=gradient;
  var particles=crumbs.GetComponent<ParticleSystemRenderer>();particles.sharedMaterial=crumbMaterial;particles.renderMode=ParticleSystemRenderMode.Billboard;
  particles.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;particles.receiveShadows=false;
  crumbs.Play();
  if(!hostile){
   var glow=new GameObject("Jasna smuga pocisku");glow.transform.SetParent(transform,false);
   streak=glow.AddComponent<TrailRenderer>();streak.sharedMaterial=crumbMaterial;
   streak.time=Mathf.Clamp(2.2f/Mathf.Max(1,speed),.025f,.09f);streak.minVertexDistance=.035f;
   streak.alignment=LineAlignment.View;streak.widthMultiplier=.075f;
   streak.widthCurve=AnimationCurve.Linear(0,1,1,0);
   streak.startColor=new Color(1,.96f,.7f,.9f);streak.endColor=new Color(1,.72f,.28f,0);
   streak.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;streak.receiveShadows=false;
  }
 }
 void LateUpdate(){
  if(hostile||!visualRoot)return;
  if(!visibilityCamera)visibilityCamera=GameSession.Instance&&GameSession.Instance.player?GameSession.Instance.player.view:Camera.main;
  if(!visibilityCamera)return;
  float depth=Vector3.Dot(transform.position-visibilityCamera.transform.position,visibilityCamera.transform.forward);
  if(depth<=0)return;
  float screenHeight=visibilityCamera.orthographic?2*visibilityCamera.orthographicSize:2*depth*Mathf.Tan(visibilityCamera.fieldOfView*.5f*Mathf.Deg2Rad);
  float minimumDiameter=screenHeight*10/Mathf.Max(1,visibilityCamera.pixelHeight);
  // Enlarge graphics only; movement and collision radius retain their original values.
  float scale=Mathf.Clamp(minimumDiameter/visualDiameter,1,2.5f);
  visualRoot.localScale=Vector3.one*scale;
  if(streak)streak.widthMultiplier=.075f*Mathf.Sqrt(scale);
 }
 void OnDestroy(){
  if(streak){streak.transform.SetParent(null,true);streak.emitting=false;Destroy(streak.gameObject,streak.time+.05f);}
  if(!crumbs)return;
  crumbs.transform.SetParent(null,true);crumbs.Stop(true,ParticleSystemStopBehavior.StopEmitting);
  Destroy(crumbs.gameObject,.2f);
 }
}
}
