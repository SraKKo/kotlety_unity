using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class CorrectionChecks {
 const string Key="RetyKotlety.CorrectionsRunning";static double began,phaseTime;static int phase,impactCount;static bool attached;static readonly List<string> results=new List<string>();static readonly List<string> errors=new List<string>();static PlayerController player;static Vector3 jumpOrigin,idlePan;static Quaternion idlePanRotation;
 static CorrectionChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){MigrationEditor.Validate();SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/Level001.unity");EditorApplication.isPlaying=true;EditorApplication.update-=Tick;EditorApplication.update+=Tick;}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);results.Add("PASS "+message);Debug.Log("CORRECTION CHECK: "+message);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;
  if(began==0){began=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}
  double elapsed=EditorApplication.timeSinceStartup-began;
  try {
   if(elapsed>35)throw new Exception("Play Mode test timeout");
   if(phase==0){if(elapsed<3)return;player=GameSession.Instance.player;
    Check(player!=null,"Player spawned");Check(player.GetComponent<CharacterController>().isGrounded,"Player settles on map collision");
    Check(Mathf.Abs(player.GetComponent<CharacterController>().radius-.5f)<.001f,"Original player radius retained");
    Check(Mathf.Abs(player.cameraRest.y-1.091f)<.001f,"Original eye height retained");
    var wc=player.view.transform.Find("Weapon Camera").GetComponent<Camera>();
    Check(Mathf.Abs(wc.fieldOfView-110)<.001f&&wc.cullingMask==(1<<11),"Dedicated weapon camera has original FOV and layer");
    foreach(var key in new[]{"melee_idle2","melee_attack2","melee_block","throw_schabowy","throw_chrupiaca"})Check(WeaponPresentation.FindAnimation(player.hand.gameObject,key)!=null,"Authored clip exists: "+key);
    Check(player.hookHand.anchor!=null,"Original hook anchor is connected");
    idlePan=player.pan.transform.position;idlePanRotation=player.pan.transform.rotation;player.enabled=false;Check(player.weapons.Attack(()=>impactCount++),"Melee animation starts");phase=1;phaseTime=EditorApplication.timeSinceStartup;
   }else if(phase==1){if(EditorApplication.timeSinceStartup-phaseTime<.4)return;Check(impactCount==0,"Melee damage waits for authored impact frame");Check(Vector3.Distance(idlePan,player.pan.transform.position)>.01f||Quaternion.Angle(idlePanRotation,player.pan.transform.rotation)>5,"Authored animation drives the pan transform");phase=2;
   }else if(phase==2){if(EditorApplication.timeSinceStartup-phaseTime<1.2)return;Check(impactCount==1,"Melee damage event fires exactly once");Check(!player.weapons.IsBusy,"Melee returns to idle");
    player.hookHand.Begin(()=>{attached=true;player.rope.Show(player.view.transform.position+player.view.transform.forward*12);},true);phase=3;phaseTime=EditorApplication.timeSinceStartup;
   }else if(phase==3){if(EditorApplication.timeSinceStartup-phaseTime<.7)return;Check(attached,"Hook attaches after launch animation");
    int links=0;foreach(var r in player.rope.GetComponentsInChildren<Renderer>())if(r.gameObject.activeInHierarchy)links++;
    Check(links>5,"Hook rope renders authored sausage mesh segments");
    Check(player.rope.GetComponentInChildren<LineRenderer>()==null,"Rope no longer uses a substitute line");
    player.hookHand.Aim(player.view.transform.position+player.view.transform.forward*12);var leftRenderers=player.hookHand.GetComponentsInChildren<Renderer>();var leftBounds=leftRenderers[0].bounds;foreach(var renderer in leftRenderers)leftBounds.Encapsulate(renderer.bounds);File.WriteAllText("Validation/left-hand-size.txt","Center: "+leftBounds.center+" size: "+leftBounds.size+" camera-local: "+player.view.transform.InverseTransformPoint(leftBounds.center));Capture("Validation/corrected-hook.png");
    player.hookHand.Release();player.rope.Hide();
    var waves=UnityEngine.Object.FindObjectsByType<WaveSpawner>(FindObjectsSortMode.None);waves[0].SpawnWave();phase=4;phaseTime=EditorApplication.timeSinceStartup;
   }else if(phase==4){if(EditorApplication.timeSinceStartup-phaseTime<1)return;
    var enemies=UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);Check(enemies.Length==3,"First wave spawns its original enemy count");
    foreach(var enemy in enemies){Check(enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().isOnNavMesh,"Enemy has a valid navigation position");Check(WeaponPresentation.FindAnimation(enemy.gameObject,"walking")!=null,"Enemy locomotion clip is present");}
    Capture("Validation/corrected-view.png");
    var visual=enemies[0].GetComponentsInChildren<Renderer>();var bounds=visual[0].bounds;foreach(var renderer in visual)bounds.Encapsulate(renderer.bounds);
    File.WriteAllText("Validation/enemy-size.txt","Authored melee renderer bounds: "+bounds.size);
    Check(bounds.size.y>.8f&&bounds.size.y<3,"Authored enemy is human-sized without bounds fitting");
    var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);floor.name="Movement test platform";floor.transform.position=new Vector3(500,100,500);floor.transform.localScale=new Vector3(50,.5f,50);
    var controller=player.GetComponent<CharacterController>();controller.enabled=false;player.transform.position=new Vector3(500,101.3f,500);controller.enabled=true;controller.Move(Vector3.up*.1f);Check(!controller.isGrounded,"Airborne wall-jump test starts without stale ground contact");player.velocity=Vector3.zero;player.enabled=true;jumpOrigin=player.transform.position;
    var flags=System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Instance;
    typeof(PlayerController).GetField("lastWallContact",flags).SetValue(player,Time.fixedTime);
    typeof(PlayerController).GetField("wallNormal",flags).SetValue(player,Vector3.left);
    typeof(PlayerController).GetField("jumpQueued",flags).SetValue(player,true);
    phase=5;phaseTime=EditorApplication.timeSinceStartup;
   }else if(phase==5){if(EditorApplication.timeSinceStartup-phaseTime<.2||!player.GetComponent<CharacterController>().isGrounded)return;float travel=Mathf.Abs(player.transform.position.x-jumpOrigin.x);
    Check(player.GetComponent<CharacterController>().isGrounded,"Wall jump lands on real Unity collision");
    File.WriteAllText("Validation/unity-wall-jump.txt",travel.ToString("F3")+" metres");
    Check(travel>5&&travel<9,"Real CharacterController wall jump travels less than 9 metres");
    File.WriteAllText("Validation/unity-wall-jump.txt",travel.ToString("F3")+" metres");
    Check(errors.Count==0,"No runtime errors during scene, animation, hook and wave checks");Finish(0);
   }
  }catch(Exception exception){errors.Add(exception.ToString());Debug.LogException(exception);Finish(1);}
 }
 static void Capture(string path){
  var target=new RenderTexture(1280,720,24);player.view.targetTexture=target;player.view.Render();var weapon=player.view.transform.Find("Weapon Camera").GetComponent<Camera>();weapon.targetTexture=target;weapon.Render();RenderTexture.active=target;
  var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());player.view.targetTexture=null;weapon.targetTexture=null;RenderTexture.active=null;target.Release();UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);
 }
 static void Finish(int code){Application.logMessageReceived-=Log;EditorApplication.update-=Tick;SessionState.SetBool(Key,false);Directory.CreateDirectory("Validation");File.WriteAllLines("Validation/playmode-results.txt",results);File.WriteAllLines("Validation/playmode-errors.txt",errors);EditorApplication.Exit(code);}
}
}




