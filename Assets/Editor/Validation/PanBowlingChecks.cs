using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class PanBowlingChecks {
 const string Key="RetyKotlety.PanBowlingChecks";static double start;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static PanBowlingChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Scenes/MovementArena.unity");SessionState.SetBool(Key,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS "+label);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static Enemy Make(Vector3 point){var root=new GameObject("Bowling test enemy");root.layer=9;root.transform.position=point;var agent=root.AddComponent<NavMeshAgent>();agent.enabled=false;var body=root.AddComponent<CapsuleCollider>();body.height=2;body.radius=.35f;body.center=Vector3.up;var enemy=root.AddComponent<Enemy>();enemy.enabled=false;enemy.dummy=true;enemy.health=1000;return enemy;}
 static void Step(EnemyLaunch flight,int count){var update=typeof(EnemyLaunch).GetMethod("FixedUpdate",BindingFlags.Instance|BindingFlags.NonPublic);for(int i=0;i<count;i++){Physics.SyncTransforms();update.Invoke(flight,null);}}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}
  if(EditorApplication.timeSinceStartup-start<2)return;
  try{
   var origin=new Vector3(1000,20,1000);var ball=Make(origin);var target=Make(origin+Vector3.forward);var secondBody=target.gameObject.AddComponent<BoxCollider>();secondBody.center=Vector3.up;secondBody.size=Vector3.one*.5f;
   ball.Bat(Vector3.forward,new PanBowlingStats(1),default);var flight=ball.GetComponent<EnemyLaunch>();Step(flight,8);
   Check(target.health==940,"Swept body collision hits a target once despite multiple colliders and frames");
   Check(target.IsAirborne&&!target.GetComponent<EnemyLaunch>().Batted,"Collision target receives a light shove without becoming another damaging projectile");
   Check(ball.health==1000,"A batted body cannot damage itself");
   UnityEngine.Object.DestroyImmediate(ball.gameObject);UnityEngine.Object.DestroyImmediate(target.gameObject);
   ball=Make(origin);target=Make(origin+Vector3.forward*2);var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=origin+Vector3.forward+Vector3.up;wall.transform.localScale=new Vector3(4,4,.2f);
   ball.Bat(Vector3.forward,new PanBowlingStats(3),default);flight=ball.GetComponent<EnemyLaunch>();Step(flight,12);
   Check(!flight.Active&&ball.transform.position.z<wall.transform.position.z,"Terrain stops a batted enemy before it crosses a wall");Check(target.health==1000,"Enemies behind the wall do not receive collision damage");
   UnityEngine.Object.DestroyImmediate(wall);UnityEngine.Object.DestroyImmediate(ball.gameObject);UnityEngine.Object.DestroyImmediate(target.gameObject);
   ball=Make(origin);ball.health=10;ball.Bat(Vector3.forward,new PanBowlingStats(1),default);ball.Damage(100);
   Check(ball&&ball.health==0&&ball.GetComponent<EnemyLaunch>().Batted,"A lethal melee hit keeps the launched body available until landing");
   Check(errors.Count==0,"No runtime errors in bowling checks");Finish(0);
  }catch(Exception exception){errors.Add(exception.ToString());Debug.LogException(exception);Finish(1);}
 }
 static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/pan-bowling-results.txt",results);File.WriteAllLines("Validation/Reports/pan-bowling-errors.txt",errors);if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
}
}
