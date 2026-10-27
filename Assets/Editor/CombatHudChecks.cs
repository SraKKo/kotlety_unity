using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class CombatHudChecks {
 const string Key="RetyKotlety.CombatHudChecks";static double start,phaseTime;static int phase;static PlayerController player;static Enemy dummy;static GameHud hud;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static CombatHudChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){GeneratePanIcon();SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/TestLevel.unity");EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS "+label);Debug.Log("COMBAT HUD: "+label);}
 static void Tick(){if(!EditorApplication.isPlaying)return;if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}try{
  if(EditorApplication.timeSinceStartup-start>40)throw new Exception("Combat/HUD test timeout");
  if(phase==0){if(EditorApplication.timeSinceStartup-start<2)return;player=GameSession.Instance.player;hud=player.GetComponentInChildren<GameHud>();Check(hud,"Godot-style 3D HUD is installed in existing scenes");
   foreach(var e in UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None)){e.enabled=false;var a=e.GetComponent<UnityEngine.AI.NavMeshAgent>();a.enabled=false;}
   dummy=UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None).First(e=>e.dummy);player.enabled=false;var controller=player.GetComponent<CharacterController>();controller.enabled=false;player.transform.position=dummy.transform.position+new Vector3(0,.9f,-6);controller.enabled=true;
   var target=dummy.GetComponentsInChildren<Collider>().First(c=>!c.isTrigger).bounds.center;player.view.transform.rotation=Quaternion.LookRotation(target-player.view.transform.position);
   player.Receive(FoodKind.Chrupiaca);hud.Refresh();Check(hud.SelectedWeaponTexture.name=="kotlet_mielony","Chrupiaca uses the original weapon image");
   Time.timeScale=0;typeof(PlayerController).GetMethod("AttackImpact",BindingFlags.NonPublic|BindingFlags.Instance).Invoke(player,null);
   var shots=UnityEngine.Object.FindObjectsByType<Projectile>(FindObjectsSortMode.None);Check(shots.Length==3,"One Chrupiaca trigger emits exactly three projectiles");Check(shots.All(s=>Mathf.Abs(s.damage-20)<.001f&&Mathf.Abs(s.speed-100)<.001f),"All three projectiles retain original damage and speed");
   Check(Vector3.Angle(shots[0].direction,shots[1].direction)>1&&Vector3.Angle(shots[1].direction,shots[2].direction)>1,"Triple shot uses separate spread directions");Check(dummy.health==200,"Volley does not deal damage before contact");Time.timeScale=1;phase=1;phaseTime=EditorApplication.timeSinceStartup;
  }else if(phase==1){if(EditorApplication.timeSinceStartup-phaseTime<.15)return;
   Check(Mathf.Abs(dummy.health-140)<.001f,"Three actual contacts deal 60 total damage");var number=UnityEngine.Object.FindObjectsByType<DamageNumber>(FindObjectsSortMode.None).Single(n=>n.targetId==dummy.GetInstanceID());Check(Mathf.Abs(number.total-60)<.001f,"Floating damage counter combines simultaneous hits correctly");
   player.health=65;player.canFly=true;player.fuel=40;hud.Refresh();Check(Mathf.Abs(hud.HealthFill-.65f)<.001f,"Original graphic health bar follows current health");Check(hud.FuelVisible&&Mathf.Abs(hud.FuelFill-.4f)<.001f,"Jetpack image shows remaining fuel");Capture("Validation/hud-chrupiaca.png");
   player.Equip(FoodKind.Schabowy);hud.Refresh();Check(hud.SelectedWeaponTexture.name=="kotlet_schabowy","Weapon image changes to original Schabowy artwork");Capture("Validation/hud-schabowy.png");
   player.Equip(FoodKind.Pan);hud.Refresh();Check(hud.SelectedWeaponTexture.name=="patelnia","Pan has its own image rendered from original model");
   var impact=Effect.Emit(dummy.transform.position+Vector3.up,-player.view.transform.forward,ImpactEffect.Pan);Check(impact.GetComponent<ParticleSystemRenderer>().renderMode==ParticleSystemRenderMode.Mesh,"Pan impact uses mesh fragments");Check(impact.main.maxParticles==15&&impact.colorOverLifetime.enabled,"Pan fragments retain original amount and fade out");
   dummy.Damage(1000);phase=2;phaseTime=EditorApplication.timeSinceStartup;
  }else if(phase==2){if(EditorApplication.timeSinceStartup-phaseTime<.05)return;Check(!dummy,"Enemy death happens once");var death=UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).First(p=>p.name=="Efekt Death");Check(death.GetComponent<ParticleSystemRenderer>().sharedMaterial.mainTexture.name=="bum","Death particles use original BUM artwork");
   typeof(PlayerController).GetField("flightHeld",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(player,true);phase=3;phaseTime=EditorApplication.timeSinceStartup;
  }else if(phase==3){if(EditorApplication.timeSinceStartup-phaseTime<.1)return;Check(player.GetComponentsInChildren<ParticleSystem>().Count(p=>p.name=="Dysza jetpacka"&&p.isEmitting)==2,"Jetpack emits both coloured exhaust effects");typeof(PlayerController).GetField("flightHeld",BindingFlags.NonPublic|BindingFlags.Instance).SetValue(player,false);phase=4;phaseTime=EditorApplication.timeSinceStartup;
  }else if(phase==4){if(EditorApplication.timeSinceStartup-phaseTime<1.2)return;Check(!player.GetComponentsInChildren<ParticleSystem>().Any(p=>p.name=="Dysza jetpacka"&&p.isEmitting),"Jetpack stops emitting after release");Check(!UnityEngine.Object.FindObjectsByType<ParticleSystem>(FindObjectsSortMode.None).Any(p=>p.name.StartsWith("Efekt ")),"One-shot effects clean themselves up");Check(errors.Count==0,"No runtime errors in shooting, damage counters, HUD or effects");Finish(0);}
 }catch(Exception e){errors.Add(e.ToString());Debug.LogException(e);Finish(1);}}
 static void Capture(string path){var view=player.view;var weapon=view.transform.Find("Weapon Camera").GetComponent<Camera>();var target=new RenderTexture(1280,720,24);view.targetTexture=target;weapon.targetTexture=target;view.Render();weapon.Render();RenderTexture.active=target;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());view.targetTexture=null;weapon.targetTexture=null;RenderTexture.active=null;target.Release();UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);}
 static void GeneratePanIcon(){
  var scene=EditorSceneManager.NewPreviewScene();var root=new GameObject("Preview");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,scene);var model=WorldFactory.Model("Pan",root.transform);
  var renderers=model.GetComponentsInChildren<Renderer>();var bounds=renderers[0].bounds;foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
  var cameraObject=new GameObject("Preview Camera");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject,scene);var camera=cameraObject.AddComponent<Camera>();camera.scene=scene;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.orthographic=true;camera.orthographicSize=bounds.extents.magnitude*.95f;camera.nearClipPlane=.01f;camera.farClipPlane=100;
  camera.transform.position=bounds.center+new Vector3(0,1,-.8f).normalized*Mathf.Max(3,bounds.size.magnitude*2);camera.transform.LookAt(bounds.center);
  var lightObject=new GameObject("Preview Light");UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject,scene);var light=lightObject.AddComponent<Light>();light.type=LightType.Directional;light.intensity=1.5f;light.transform.rotation=camera.transform.rotation;
  var target=new RenderTexture(256,256,24,RenderTextureFormat.ARGB32);camera.targetTexture=target;camera.Render();RenderTexture.active=target;var image=new Texture2D(256,256,TextureFormat.RGBA32,false);image.ReadPixels(new Rect(0,0,256,256),0,0);image.Apply();File.WriteAllBytes("Assets/Resources/UI/patelnia.png",image.EncodeToPNG());RenderTexture.active=null;target.Release();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(image);EditorSceneManager.ClosePreviewScene(scene);AssetDatabase.ImportAsset("Assets/Resources/UI/patelnia.png",ImportAssetOptions.ForceSynchronousImport);
 }
 static void Finish(int code){Time.timeScale=1;Application.logMessageReceived-=Log;EditorApplication.update-=Tick;SessionState.SetBool(Key,false);File.WriteAllLines("Validation/combat-hud-results.txt",results);File.WriteAllLines("Validation/combat-hud-errors.txt",errors);EditorApplication.Exit(code);}
}
}
