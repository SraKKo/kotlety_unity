using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety.Editor {
public static class ArenaPrototypeEditor {
 const string Generated="Assets/Generated/MovementArena";
 [MenuItem("Rety Kotlety/Prototyp/Przygotuj arene movementu")]
 public static void Build(){
  if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  Directory.CreateDirectory(Generated);
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  var root=new GameObject("Movement Arena");var session=WorldFactory.Build(root,false,ArenaLayout.SceneName);
  RenderSettings.ambientLight=new Color(.65f,.7f,.76f);RenderSettings.fog=false;RenderSettings.skybox=null;
  session.player.view.backgroundColor=new Color(.12f,.18f,.24f);session.player.view.clearFlags=CameraClearFlags.SolidColor;
  var navPath=Generated+"/Navigation.asset";var existing=AssetDatabase.LoadAssetAtPath<NavMeshData>(navPath);
  if(existing){EditorUtility.CopySerialized(session.navigation,existing);UnityEngine.Object.DestroyImmediate(session.navigation);session.navigation=existing;}else AssetDatabase.CreateAsset(session.navigation,navPath);
  var saved=new Dictionary<Material,Material>();
  foreach(var renderer in root.GetComponentsInChildren<Renderer>(true)){
   var material=renderer.sharedMaterial;if(!material||AssetDatabase.Contains(material)||!material.name.StartsWith("Arena "))continue;
   if(!saved.TryGetValue(material,out var persistent)){
    var path=Generated+"/"+material.name.Replace(" ","_")+".mat";persistent=AssetDatabase.LoadAssetAtPath<Material>(path);
    if(persistent)EditorUtility.CopySerialized(material,persistent);else{persistent=material;AssetDatabase.CreateAsset(persistent,path);}saved[material]=persistent;
   }
   renderer.sharedMaterial=persistent;
  }
  var scenePath="Assets/Scenes/"+ArenaLayout.SceneName+".unity";
  EditorSceneManager.SaveScene(scene,scenePath);
  var scenes=new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
  if(!scenes.Exists(s=>s.path==scenePath))scenes.Add(new EditorBuildSettingsScene(scenePath,true));EditorBuildSettings.scenes=scenes.ToArray();
  AssetDatabase.SaveAssets();Validate();Debug.Log("Movement Arena: scena gotowa.");
 }
 public static void BuildAndCheck(){Build();}
 [MenuItem("Rety Kotlety/Prototyp/Sprawdz arene movementu")]
 public static void Validate(){
  var results=new List<string>();
  Action<bool,string> check=(ok,label)=>{if(!ok)throw new Exception(label);results.Add("PASS "+label);};
  var session=UnityEngine.Object.FindAnyObjectByType<GameSession>();check(session&&session.player,"Player and session are present");
  check(session.navigation,"Baked navigation is saved");
  check(UnityEngine.Object.FindAnyObjectByType<ArenaPractice>(),"Optional enemy pressure controller exists");
  Physics.SyncTransforms();
  foreach(var point in ArenaLayout.SpawnPoints)check(Physics.Raycast(point+Vector3.up*2,Vector3.down,3),"Spawn has supporting geometry: "+point);
  foreach(var x in new[]{-20f,20f}){
   check(Physics.Raycast(new Vector3(x,.1f,0),Vector3.up,out var roof,2)&&Mathf.Abs(roof.point.y-1.35f)<.02f,"Slide roof has 1.35m clearance");
   check(!Physics.CheckCapsule(new Vector3(x,.51f,0),new Vector3(x,.55f,0),.48f),"Crouched capsule fits slide tunnel");
  }
  var nav=NavMesh.AddNavMeshData(session.navigation);
  try{
   foreach(var point in ArenaLayout.SpawnPoints){check(NavMesh.SamplePosition(point,out var hit,2,NavMesh.AllAreas),"Spawn is on navigation: "+point);var path=new NavMeshPath();check(NavMesh.CalculatePath(new Vector3(0,0,-46),hit.position,NavMesh.AllAreas,path)&&path.status==NavMeshPathStatus.PathComplete,"Enemy route connects spawn to start: "+point);}
  }finally{nav.Remove();}
  Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/movement-arena-layout.txt",results);
 }
}
}
