using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.AI;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class ProgressionArenaChecks {
 const string Key="RetyKotlety.ProgressionArenaChecks";static double start;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static ProgressionArenaChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Scenes/ProgressionArena.unity");SessionState.SetBool(Key,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS "+label);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}
  if(EditorApplication.timeSinceStartup-start<2)return;
  try{
   var session=GameSession.Instance;Check(session&&session.player,"Sample scene boots a player and game session");var player=session.player;player.enabled=false;
   var director=UnityEngine.Object.FindAnyObjectByType<ProgressionArenaWaves>();Check(director&&director.Wave==0&&!director.Complete,"Sample arena installs its own finite wave director");
   Check(!UnityEngine.Object.FindAnyObjectByType<ArenaPractice>(),"Sandbox pressure and immortal targets are absent");
   Check(player.hasSchabowy&&player.hasChrupiaca&&player.hasMielony&&player.hasLosos,"Sample arena grants baseline ranged weapons");
   var progression=player.GetComponent<PlayerProgression>();Check(progression&&progression.State.Level==1&&progression.State.Points==0&&player.GetComponent<PlayerUpgrades>().TotalStacks==0,"Sample run starts without free XP, currency or upgrades");
   Check(UnityEngine.Object.FindObjectsByType<UpgradeChest>().Length==0,"Weapon chests must be earned by clearing waves");
   foreach(var point in director.spawnPoints)Check(NavMesh.SamplePosition(point,out var hit,4,NavMesh.AllAreas),"Wave entry has a reachable NavMesh point: "+point);
   // NavMesh was already baked. Simulate a map prop covering every entry and verify
   // physics vetoes those points, then opening the entries resumes the same wave.
   var blockers=new List<GameObject>();
   foreach(var point in director.spawnPoints){
    var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);
    blocker.transform.position=point+Vector3.up*1.9f;
    blocker.transform.localScale=new Vector3(9,1,9);
    blockers.Add(blocker);
   }
   Physics.SyncTransforms();
   typeof(ProgressionArenaWaves).GetMethod("SpawnWave",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(director,new object[]{session});
   var pack=typeof(ProgressionArenaWaves).GetMethod("SpawnPack",BindingFlags.Instance|BindingFlags.NonPublic);
   Check(director.LivingCount==0,"Low overhead props prevent spawning on otherwise valid navigation");
   foreach(var blocker in blockers)UnityEngine.Object.DestroyImmediate(blocker);
   Physics.SyncTransforms();
   pack.Invoke(director,new object[]{session});
   var tick=typeof(ProgressionArenaWaves).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic);
   for(int wave=1;wave<=3;wave++){
    int defeated=0;
    while(defeated<director.waveSizes[wave-1]){
     var enemies=UnityEngine.Object.FindObjectsByType<Enemy>().Where(e=>e.health>0&&!e.dummy).ToArray();
     Check(enemies.Length>0&&enemies.Length<=48&&enemies.All(e=>e.GetComponent<NavMeshAgent>().isOnNavMesh),"Pack is on navigation and respects the population cap");
     foreach(var enemy in enemies)enemy.Damage(10000);
     defeated+=enemies.Length;
     tick.Invoke(director,null);
     if(defeated<director.waveSizes[wave-1])pack.Invoke(director,new object[]{session});
    }
    Check(UnityEngine.Object.FindObjectsByType<UpgradeChest>().Length==wave/3,"Chest cadence on clear of wave "+wave);
    if(wave<3)typeof(ProgressionArenaWaves).GetMethod("SpawnWave",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(director,new object[]{session});
   }
   Check(progression.State.ShopUnlocked&&progression.State.Kills==74,"Three waves award a kill for each horde member");
   session.GetComponent<LevelShop>().Open();Check(session.shopOpen&&session.paused,"First wave can open the unlocked shop");
   Check(errors.Count==0,"No runtime errors in sample arena checks");Finish(0);
  }catch(Exception exception){errors.Add(exception.ToString());Debug.LogException(exception);Finish(1);}
 }
 static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/progression-arena-results.txt",results);File.WriteAllLines("Validation/Reports/progression-arena-errors.txt",errors);if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
}
}
