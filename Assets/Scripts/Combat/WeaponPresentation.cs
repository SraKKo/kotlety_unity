using System;
using UnityEngine;
namespace RetyKotlety {
public class WeaponPresentation : MonoBehaviour {
 public Animation animationPlayer;
 public GameObject pan,schabowy,chrupiaca;
 GameObject mielony;float nextMielonyAttack;
 public float animationSpeed=1.465f;
 AnimationState state; Action impact; FoodKind weapon; float time,impactTime; bool reverse,holdBlock,foodAttached;
 Vector3 schabowyGrip,chrupiacaGrip,schabowyScale,chrupiacaScale;
 Quaternion schabowyRotation,chrupiacaRotation;
 public bool IsBusy=>state!=null;
 public bool Blocking=>holdBlock && state!=null && time>=.2088f;
 public static Animation FindAnimation(GameObject root,string key){foreach(var a in root.GetComponentsInChildren<Animation>(true))foreach(AnimationState s in a)if(s.name==key||s.name.EndsWith("/"+key)||s.name.EndsWith("|"+key))return a;return null;}
 public static Transform Find(Transform root,string name){foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;throw new InvalidOperationException("Brak węzła rig: "+name);}
 AnimationState Clip(string key){if(!animationPlayer)return null;foreach(AnimationState s in animationPlayer)if(s.name==key||s.name.EndsWith("/"+key)||s.name.EndsWith("|"+key))return s;return null;}
 void Sample(AnimationState clip,float t){animationPlayer.Stop();clip.wrapMode=WrapMode.ClampForever;clip.enabled=true;clip.weight=1;clip.speed=0;clip.time=t;animationPlayer.Sample();RestoreFoodGrips();}
 public void Equip(FoodKind kind){AttachFood();weapon=kind;impact=null;state=null;holdBlock=false;SetVisible();Idle();}
 void SetVisible(){if(pan)pan.SetActive(weapon==FoodKind.Pan);if(schabowy)schabowy.SetActive(weapon==FoodKind.Schabowy);if(chrupiaca)chrupiaca.SetActive(weapon==FoodKind.Chrupiaca);if(mielony)mielony.SetActive(weapon==FoodKind.Mielony);}
 void ResetPose(){var reset=Clip("RESET");if(reset!=null)Sample(reset,0);}
 void AttachFood(){
  if(!Application.isPlaying||foodAttached||!animationPlayer)return;
  var palm=Find(animationPlayer.transform,"Bone_002");
  // Fit the visible geometry to the palm instead of preserving scene offsets.
  AttachFood(schabowy,palm,"throw_schabowy");
  if(chrupiaca){
   // The imported gun contains three separate food models; hold only the middle one.
   foreach(Transform child in chrupiaca.transform)child.gameObject.SetActive(child.name=="chrup2");
  }
  AttachFood(chrupiaca,palm,"throw_chrupiaca");
  if(schabowy){schabowyGrip=schabowy.transform.localPosition;schabowyScale=schabowy.transform.localScale;schabowyRotation=schabowy.transform.localRotation;}
  if(chrupiaca){chrupiacaGrip=chrupiaca.transform.localPosition;chrupiacaScale=chrupiaca.transform.localScale;chrupiacaRotation=chrupiaca.transform.localRotation;}
  mielony=MielonyProjectile.Sphere(palm);mielony.layer=11;mielony.transform.localPosition=new Vector3(.08f,.3f,.08f);mielony.transform.localScale=Vector3.one*.55f;
  foodAttached=true;
 }
 void AttachFood(GameObject model,Transform palm,string pose){
  if(!model)return;ResetPose();var clip=Clip(pose);if(clip==null)return;
  Sample(clip,0);model.transform.SetParent(palm,false);
  model.transform.localPosition=Vector3.zero;
  model.transform.localRotation=Quaternion.Euler(90,45,0);
  model.transform.localScale=Vector3.one;
  var bounds=FoodBounds(model,palm);
  float size=Mathf.Max(bounds.size.x,Mathf.Max(bounds.size.y,bounds.size.z));
  if(size<=.0001f)return;
  // Bone units: wrist to finger roots is about 0.42; food extends past the palm.
  float diameter=model==schabowy?.7f:.8f;
  model.transform.localScale*=diameter/size;
  bounds=FoodBounds(model,palm);
  var grip=new Vector3(.08f,.3f,.08f);
  var camera=GetComponentInParent<Camera>();
  if(camera&&model==schabowy)grip+=palm.InverseTransformVector(camera.transform.up*.12f);
  model.transform.localPosition+=grip-bounds.center;
 }
 void RestoreFoodGrips(){
  if(!foodAttached)return;
  // Imported RESET curves still target the gun transform after it is reparented.
  // Restore the captured grip after every animation sample, including weapon switches.
  if(schabowy){schabowy.transform.SetLocalPositionAndRotation(schabowyGrip,schabowyRotation);schabowy.transform.localScale=schabowyScale;}
  if(chrupiaca){chrupiaca.transform.SetLocalPositionAndRotation(chrupiacaGrip,chrupiacaRotation);chrupiaca.transform.localScale=chrupiacaScale;}
 }
 static Bounds FoodBounds(GameObject model,Transform palm){
  var bounds=new Bounds();bool initialized=false;
  foreach(var mesh in model.GetComponentsInChildren<MeshFilter>(true)){
   if(!mesh.sharedMesh||!mesh.gameObject.activeSelf)continue;
   bool visible=true;for(var parent=mesh.transform.parent;parent&&parent!=model.transform;parent=parent.parent)if(!parent.gameObject.activeSelf){visible=false;break;}
   if(!visible)continue;var b=mesh.sharedMesh.bounds;
   for(int i=0;i<8;i++){
    var corner=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));
    var point=palm.InverseTransformPoint(mesh.transform.TransformPoint(corner));
    if(!initialized){bounds=new Bounds(point,Vector3.zero);initialized=true;}else bounds.Encapsulate(point);
   }
  }
  return bounds;
 }
 void Idle(){ResetPose();var idle=Clip(weapon==FoodKind.Pan?"melee_idle2":weapon==FoodKind.Chrupiaca?"throw_chrupiaca":"throw_schabowy");if(idle!=null)Sample(idle,0);}
 public bool Attack(Action onImpact){if(IsBusy||weapon==FoodKind.Mielony&&Time.time<nextMielonyAttack)return false;string name=weapon==FoodKind.Pan?"melee_attack2":weapon==FoodKind.Chrupiaca?"throw_chrupiaca":"throw_schabowy";state=Clip(name);if(state==null){Debug.LogError("Brak animacji "+name,this);return false;}if(weapon==FoodKind.Mielony)nextMielonyAttack=Time.time+(MielonyProjectile.Tuning?MielonyProjectile.Tuning.fireCooldown:.85f);impact=onImpact;impactTime=weapon==FoodKind.Pan?.8194f:weapon==FoodKind.Chrupiaca?.4f:.28f;time=0;reverse=false;holdBlock=false;Sample(state,0);return true;}
 public void SetBlock(bool held){if(weapon!=FoodKind.Pan)return;if(held&&!IsBusy){state=Clip("melee_block");time=0;reverse=false;holdBlock=state!=null;}else if(!held&&holdBlock){holdBlock=false;reverse=true;}}
 void LateUpdate(){if(!animationPlayer)return;animationPlayer.playAutomatically=false;animationPlayer.cullingType=AnimationCullingType.AlwaysAnimate;if(state==null){SetVisible();return;}
  time+=Time.deltaTime*animationSpeed*(reverse?-1:1);time=Mathf.Clamp(time,0,state.length);Sample(state,time);
  if(!reverse&&impact!=null&&time>=impactTime){var callback=impact;impact=null;callback();}
  if(!reverse&&time>=state.length&&!holdBlock){if(weapon==FoodKind.Pan){state=null;Idle();}else reverse=true;}
  else if(reverse&&time<=0){state=null;Idle();}
  SetVisible();
 }
}
}

