using System;
using UnityEngine;
namespace RetyKotlety {
public class HookHand : MonoBehaviour {
 public Animation animationPlayer; public Transform anchor;
 AnimationState clip; Action attach; float time;bool backwards,latched,success;Quaternion rest;
 public bool IsBusy=>gameObject.activeSelf;
 void Bind(){rest=transform.localRotation;animationPlayer=WeaponPresentation.FindAnimation(gameObject,"hook");if(animationPlayer)foreach(AnimationState s in animationPlayer)if(s.name.EndsWith("hook"))clip=s;}
 public void Configure(){Bind();gameObject.SetActive(false);}
 public void Begin(Action onAttach,bool hit){if(clip==null)Bind();if(clip==null){Debug.LogError("Brak animacji haka",this);return;}attach=onAttach;success=hit;time=0;latched=false;backwards=false;transform.localRotation=rest;gameObject.SetActive(true);}
 public void Aim(Vector3 point){if(latched)transform.rotation=Quaternion.LookRotation(point-transform.position,transform.parent.up);}
 public void Release(){attach=null;latched=false;backwards=true;}
 void LateUpdate(){if(clip==null)return;time=Mathf.Clamp(time+Time.deltaTime*(backwards?-3:latched?0:2),0,clip.length);
  animationPlayer.Stop();animationPlayer.playAutomatically=false;animationPlayer.cullingType=AnimationCullingType.AlwaysAnimate;clip.wrapMode=WrapMode.ClampForever;clip.enabled=true;clip.weight=1;clip.speed=0;clip.time=time;animationPlayer.Sample();
  if(!backwards&&time>=.5f&&attach!=null){var callback=attach;attach=null;callback();}
  if(!backwards&&time>=clip.length){if(success)latched=true;else backwards=true;}
  if(backwards&&time<=0){transform.localRotation=rest;gameObject.SetActive(false);}
 }
}
}


