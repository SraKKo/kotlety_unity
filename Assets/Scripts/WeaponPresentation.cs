using System;
using UnityEngine;
namespace RetyKotlety {
public class WeaponPresentation : MonoBehaviour {
 public Animation animationPlayer;
 public GameObject pan,schabowy,chrupiaca;
 public float animationSpeed=1.465f;
 AnimationState state; Action impact; FoodKind weapon; float time,impactTime; bool reverse,holdBlock;
 public bool IsBusy=>state!=null;
 public bool Blocking=>holdBlock && state!=null && time>=.2088f;
 public static Animation FindAnimation(GameObject root,string key){foreach(var a in root.GetComponentsInChildren<Animation>(true))foreach(AnimationState s in a)if(s.name==key||s.name.EndsWith("/"+key)||s.name.EndsWith("|"+key))return a;return null;}
 public static Transform Find(Transform root,string name){foreach(var t in root.GetComponentsInChildren<Transform>(true))if(t.name==name)return t;throw new InvalidOperationException("Brak węzła rig: "+name);}
 AnimationState Clip(string key){if(!animationPlayer)return null;foreach(AnimationState s in animationPlayer)if(s.name==key||s.name.EndsWith("/"+key)||s.name.EndsWith("|"+key))return s;return null;}
 void Sample(AnimationState clip,float t){animationPlayer.Stop();clip.wrapMode=WrapMode.ClampForever;clip.enabled=true;clip.weight=1;clip.speed=0;clip.time=t;animationPlayer.Sample();}
 public void Equip(FoodKind kind){weapon=kind;impact=null;state=null;holdBlock=false;SetVisible();Idle();}
 void SetVisible(){if(pan)pan.SetActive(weapon==FoodKind.Pan);if(schabowy)schabowy.SetActive(weapon==FoodKind.Schabowy);if(chrupiaca)chrupiaca.SetActive(weapon==FoodKind.Chrupiaca);}
 void Idle(){var idle=Clip("melee_idle2");if(idle!=null)Sample(idle,0);}
 public bool Attack(Action onImpact){if(IsBusy)return false;string name=weapon==FoodKind.Pan?"melee_attack2":weapon==FoodKind.Schabowy?"throw_schabowy":"throw_chrupiaca";state=Clip(name);if(state==null){Debug.LogError("Brak animacji "+name,this);return false;}impact=onImpact;impactTime=weapon==FoodKind.Pan?.8194f:weapon==FoodKind.Schabowy?.28f:.4f;time=0;reverse=false;holdBlock=false;Sample(state,0);return true;}
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

