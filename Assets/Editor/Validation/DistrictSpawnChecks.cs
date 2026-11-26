using System;
using System.IO;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class DistrictSpawnChecks {
 const string Key="RetyKotlety.DistrictSpawnChecks";
 const int WorldMask=~((1<<8)|(1<<9)|(1<<11));
 static readonly List<string> results=new List<string>();
 static double began;
 static DistrictSpawnChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 static void Check(bool valid,string message){if(!valid)throw new Exception(message);results.Add("PASS "+message);}
 public static void Run(){try{
  results.Clear();Directory.CreateDirectory("Validation/Reports");
  EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var root=new GameObject("Spawn factory regression");
  var ground=GameObject.CreatePrimitive(PrimitiveType.Cube);
  ground.transform.SetParent(root.transform);ground.transform.position=new Vector3(-62,0,-60);ground.transform.localScale=new Vector3(12,.24f,12);
  var generated=WorldFactory.Build(root,false,PostSovietDistrict.SceneName,ground.transform);
  Check(Vector3.Distance(generated.player.transform.position,PostSovietDistrict.Spawn)<.001f,"Factory retains Unity district coordinates without Godot Z conversion");
  var body=generated.player.GetComponent<CharacterController>();
  Check(Clear(body,.001f),"Newly generated player capsule fits at spawn");
  File.WriteAllLines("Validation/Reports/district-spawn-results.txt",results);
  EditorSceneManager.OpenScene("Assets/Scenes/PostSovietDistrict.unity");Physics.SyncTransforms();
  var saved=UnityEngine.Object.FindAnyObjectByType<PlayerController>();
  Check(Vector3.Distance(saved.transform.position,PostSovietDistrict.Spawn)<.001f,"Saved scene starts at the authored bus stop");
  Check(Clear(saved.GetComponent<CharacterController>(),.001f),"Saved standing capsule is outside world geometry");
  Check(Physics.Raycast(saved.transform.position,Vector3.down,out var hit,3,1,QueryTriggerInteraction.Ignore)&&hit.normal.y>.7f,"Actual saved spawn has walkable ground below it");
  File.WriteAllLines("Validation/Reports/district-spawn-results.txt",results);
  SessionState.SetBool(Key,true);EditorApplication.isPlaying=true;EditorApplication.update-=Tick;EditorApplication.update+=Tick;
 }catch(Exception error){Fail(error);}}
 static bool Clear(CharacterController body,float inset){Physics.SyncTransforms();var centre=body.transform.TransformPoint(body.center);var half=Vector3.up*(body.height*.5f-body.radius);return !Physics.CheckCapsule(centre-half,centre+half,body.radius-inset,WorldMask,QueryTriggerInteraction.Ignore);}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;
  if(began==0)began=EditorApplication.timeSinceStartup;
  if(EditorApplication.timeSinceStartup-began<2)return;
  try{
   var player=GameSession.Instance.player;var body=player.GetComponent<CharacterController>();
   Check(Vector2.Distance(new Vector2(player.transform.position.x,player.transform.position.z),new Vector2(PostSovietDistrict.Spawn.x,PostSovietDistrict.Spawn.z))<.03f,"Play mode starts at bus stop without horizontal depenetration or drift");
   Check(Mathf.Abs(body.height-1.7f)<.001f&&Mathf.Abs(body.radius-.35f)<.001f,"Play mode applies the 1.7m body profile");
   Check(body.isGrounded,"Player settles on the starting sidewalk");
   Check(Clear(body,body.skinWidth+.01f),"Settled capsule stays outside walls and props");
   Check(Physics.Raycast(player.transform.position,Vector3.down,out var hit,2,1,QueryTriggerInteraction.Ignore)&&Mathf.Abs(hit.point.y-.24f)<.01f,"Player stands on the intended sidewalk rather than garage geometry");
   results.InsertRange(0,File.ReadAllLines("Validation/Reports/district-spawn-results.txt"));
   File.WriteAllLines("Validation/Reports/district-spawn-results.txt",results);
   SessionState.SetBool(Key,false);EditorApplication.update-=Tick;EditorApplication.Exit(0);
  }catch(Exception error){Fail(error);}
 }
 static void Fail(Exception error){SessionState.SetBool(Key,false);Directory.CreateDirectory("Validation/Reports");File.WriteAllText("Validation/Reports/district-spawn-error.txt",error.ToString());Debug.LogException(error);EditorApplication.Exit(1);}
}
}
