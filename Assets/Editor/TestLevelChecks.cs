using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class TestLevelChecks {
 const string Key="RetyKotlety.TestLevelChecks";
 static double start,phaseTime;static int phase,playerId;static Quaternion[] rotations;static TestRotator[] rotators;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static TestLevelChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){MigrationEditor.BuildTestLevel();SessionState.SetBool(Key,true);EditorSceneManager.OpenScene("Assets/Scenes/MainMenu.unity");EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
 static void Log(string m,string s,LogType t){if(t==LogType.Error||t==LogType.Exception||t==LogType.Assert)errors.Add(m);}
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS "+label);Debug.Log("TEST LEVEL: "+label);}
 static void Tick(){if(!EditorApplication.isPlaying)return;if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}try{
  if(EditorApplication.timeSinceStartup-start>40)throw new Exception("Test timeout");
  if(phase==0){if(!GameSession.Instance||EditorApplication.timeSinceStartup-start<1)return;Check(GameSession.Instance.menuScene,"Existing main menu loads");Check(EditorBuildSettings.scenes.Any(s=>s.enabled&&s.path.EndsWith("TestLevel.unity")),"Test level is in build settings");GameSession.Instance.StartTestLevel();phase=1;phaseTime=EditorApplication.timeSinceStartup;return;}
  if(phase==1){if(EditorApplication.timeSinceStartup-phaseTime<3)return;
   Check(SceneManager.GetActiveScene().name=="TestLevel","Menu entry loads the test level");var player=GameSession.Instance.player;Check(player&&player.GetComponent<CharacterController>().isGrounded,"Original test spawn stands on collision");
   var pickups=UnityEngine.Object.FindObjectsByType<Pickup>(FindObjectsSortMode.None);Check(pickups.Length==4,"All four original pickups are present");foreach(var kind in new[]{FoodKind.Schabowy,FoodKind.Chrupiaca,FoodKind.Jetpack,FoodKind.Pierog})Check(pickups.Any(p=>p.kind==kind),"Pickup: "+kind);
   var enemies=UnityEngine.Object.FindObjectsByType<Enemy>(FindObjectsSortMode.None);Check(enemies.Length==4,"Two melee enemies, shooter and dummy are present");Check(enemies.Count(e=>e.shooting)==1&&enemies.Count(e=>e.dummy)==1,"Shooter and training dummy retain their roles");
   foreach(var enemy in enemies.Where(e=>!e.dummy))Check(enemy.GetComponent<UnityEngine.AI.NavMeshAgent>().isOnNavMesh,"Enemy is placed on the test NavMesh");
   var dummy=enemies.First(e=>e.dummy);dummy.Damage(25);Check(Mathf.Abs(dummy.health-175)<.01f,"Training dummy takes weapon damage");
   Check(UnityEngine.Object.FindObjectsByType<Rigidbody>(FindObjectsSortMode.None).Count(r=>!r.isKinematic&&r.useGravity)==1,"Original ball uses rigid-body gravity");
   Check(UnityEngine.Object.FindObjectsByType<ArenaGate>(FindObjectsSortMode.None).Length==0,"City arena walls are absent from the test map");
   rotators=UnityEngine.Object.FindObjectsByType<TestRotator>(FindObjectsSortMode.None);Check(rotators.Length==2,"Both original rotating platforms are present");rotations=rotators.Select(r=>r.transform.rotation).ToArray();phase=2;phaseTime=EditorApplication.timeSinceStartup;
  }else if(phase==2){if(EditorApplication.timeSinceStartup-phaseTime<.7)return;for(int i=0;i<rotators.Length;i++)Check(Quaternion.Angle(rotations[i],rotators[i].transform.rotation)>1,"Platform rotation advances in physics time");
   var player=GameSession.Instance.player;Capture(player);playerId=player.GetInstanceID();player.Damage(1000);phase=3;phaseTime=EditorApplication.timeSinceStartup;
  }else if(phase==3){if(EditorApplication.timeSinceStartup-phaseTime<1)return;Check(SceneManager.GetActiveScene().name=="TestLevel","Death reloads the test map instead of the city");Check(GameSession.Instance.player.GetInstanceID()!=playerId,"Player respawns after death");Check(errors.Count==0,"No runtime errors during test-level checks");Finish(0);}
 }catch(Exception e){errors.Add(e.ToString());Debug.LogException(e);Finish(1);}}
 static void Capture(PlayerController player){var target=new RenderTexture(1280,720,24);player.view.targetTexture=target;player.view.Render();var weapon=player.view.transform.Find("Weapon Camera").GetComponent<Camera>();weapon.targetTexture=target;weapon.Render();RenderTexture.active=target;var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("Validation/test-level.png",image.EncodeToPNG());player.view.targetTexture=null;weapon.targetTexture=null;RenderTexture.active=null;target.Release();UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(target);}
 static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllLines("Validation/test-level-results.txt",results);File.WriteAllLines("Validation/test-level-errors.txt",errors);EditorApplication.Exit(code);}
}
}
