using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class MielonyChecks {
 const string Key="RetyKotlety.MielonyChecks";static int phase;static double started;static float phaseTime,previousHealth;static Enemy[] targets;static MielonyProjectile lob;static MielonyTuning profile;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static MielonyChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Scenes/MovementArena.unity");SessionState.SetBool(Key,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
 static void Check(bool ok,string message){if(!ok)throw new Exception(message);results.Add("PASS "+message);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static void Tick(){if(!EditorApplication.isPlaying)return;if(started==0){started=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}try{
  if(EditorApplication.timeSinceStartup-started>20)throw new Exception("Mielony test timeout");
  var session=GameSession.Instance;
  if(phase==0){
   if(EditorApplication.timeSinceStartup-started<2||!session||!session.player)return;
   Check(session.player.hasMielony,"Prototype grants Mielony from start");session.player.Equip(FoodKind.Mielony);session.player.enabled=false;
   targets=UnityEngine.Object.FindObjectsByType<Enemy>().Where(e=>e.dummy).Take(4).ToArray();Check(targets.Length==4,"Four targets are available");
   targets[0].transform.position=Vector3.zero;targets[1].transform.position=new Vector3(1.5f,0,0);targets[2].transform.position=new Vector3(6,0,0);targets[3].transform.position=new Vector3(0,4,0);foreach(var enemy in targets)enemy.health=10000;
   targets[0].gameObject.AddComponent<SphereCollider>().radius=.3f;Physics.SyncTransforms();
   profile=UnityEngine.Object.Instantiate(MielonyProjectile.Tuning);profile.greaseDuration=1;GreasePatch.Spawn(Vector3.zero,Vector3.up,profile);phase=1;phaseTime=Time.time;
  }else if(phase==1){
   if(Time.time-phaseTime<1.2f)return;
   Check(targets[0].health==9980&&targets[1].health==9980,"Grease deals four ticks with no duplicate hits from extra colliders");Check(targets[0].SpeedMultiplier<1&&targets[2].SpeedMultiplier==1,"Only enemies in grease receive the slow");Check(targets[2].health==10000&&targets[3].health==10000,"Targets outside the footprint or above another floor are unharmed");Check(UnityEngine.Object.FindObjectsByType<GreasePatch>().Length==0,"Grease expires after its configured duration");
   profile.greaseDuration=3;GreasePatch.Spawn(Vector3.zero,Vector3.up,profile);phase=2;phaseTime=Time.time;
  }else if(phase==2){
   if(Time.time-phaseTime<.55f)return;previousHealth=targets[1].health;targets[1].transform.position=new Vector3(6,0,2);Physics.SyncTransforms();phase=3;phaseTime=Time.time;
  }else if(phase==3){
   if(Time.time-phaseTime<.6f)return;Check(targets[1].SpeedMultiplier==1&&targets[0].SpeedMultiplier<1,"Leaving grease restores speed without stacking slows");Check(targets[1].health==previousHealth&&targets[0].health<previousHealth,"Leaving grease stops damage while targets inside continue taking damage");
   targets[1].transform.position=new Vector3(1.5f,0,0);Physics.SyncTransforms();phase=4;phaseTime=Time.time;
  }else if(phase==4){
   if(Time.time-phaseTime<.3f)return;Check(targets[1].health<previousHealth,"Reentering grease resumes damage");
   lob=MielonyProjectile.Spawn(new Vector3(0,5,-12),Vector3.forward,session.player.gameObject);phase=5;phaseTime=Time.time;
  }else if(phase==5){
   if(Time.time-phaseTime<.1f)return;Check(lob&&lob.transform.position.y>5,"Horizontal throw initially rises due to lob velocity");phase=6;phaseTime=Time.time;
  }else if(phase==6){
   if(Time.time-phaseTime<2)return;Check(!lob,"Lob terminates on collision");Check(UnityEngine.Object.FindObjectsByType<GreasePatch>().Length>0,"Lob leaves grease on the ground");
   foreach(var patch in UnityEngine.Object.FindObjectsByType<GreasePatch>())UnityEngine.Object.Destroy(patch.gameObject);
   var upgrades=session.player.GetComponent<PlayerUpgrades>();upgrades.Grant(Resources.Load<UpgradeDefinition>("Upgrades/MielonyBounce"));Check(upgrades.MielonyGroundBounces==2,"Bounce upgrade grants exactly two ground rebounds");phase=7;phaseTime=Time.time;
  }else if(phase==7){
   if(Time.time-phaseTime<.1f)return;lob=MielonyProjectile.Spawn(new Vector3(0,5,-12),Vector3.forward,session.player.gameObject);session.player.GetComponent<PlayerUpgrades>().ConfigureMielony(lob);Check(lob.GetComponent<ProjectileVisual>(),"Mielony uses the shared projectile outline and trail");phase=8;phaseTime=Time.time;
  }else if(phase==8){
   if(lob){if(Time.time-phaseTime>4)throw new Exception("Bounce did not terminate");return;}
   Check(UnityEngine.Object.FindObjectsByType<GreasePatch>().Length==3,"Two rebounds and final landing leave three grease patches");Check(errors.Count==0,"No runtime errors during Mielony checks");Finish(0);
  }
 }catch(Exception exception){errors.Add(exception.ToString());Debug.LogException(exception);Finish(1);}}
 static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/mielony-results.txt",results);File.WriteAllLines("Validation/Reports/mielony-errors.txt",errors);if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
}
}
