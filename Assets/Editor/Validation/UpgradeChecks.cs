using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
namespace RetyKotlety.Editor {
[InitializeOnLoad]
public static class UpgradeChecks {
 const string Key="RetyKotlety.UpgradeChecks";static int phase;static double start,phaseTime;
 static Enemy[] targets;static UpgradeChest chest;static PlayerController player;static PlayerUpgrades upgrades;static GameObject blocker;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static UpgradeChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){
  if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;
  Directory.CreateDirectory("Validation/Reports");EditorSceneManager.OpenScene("Assets/Scenes/MovementArena.unity");SessionState.SetBool(Key,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;
 }
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS "+label);Debug.Log("UPGRADE: "+label);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static void ResetTargets(){foreach(var enemy in targets)enemy.health=10000;}
 static void Fire(){var session=GameSession.Instance;var shot=Projectile.Spawn(new Vector3(-12,1,0),Vector3.right,30,50,0,false,session.schabowyModel,player.gameObject);upgrades.ConfigureSchabowy(shot);}
 static void FireChrupiaca(){var shot=Projectile.Spawn(new Vector3(-12,1,0),Vector3.right,1200,20,0,false,GameSession.Instance.chrupiacaModel,player.gameObject);upgrades.ConfigureChrupiaca(shot);}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;
  if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}
  try{
   if(EditorApplication.timeSinceStartup-start>35)throw new Exception("Upgrade check timeout");
   double now=EditorApplication.timeSinceStartup;
   if(phase==0){
    if(now-start<2||!GameSession.Instance||!GameSession.Instance.player)return;
    player=GameSession.Instance.player;player.enabled=false;upgrades=player.GetComponent<PlayerUpgrades>();chest=UnityEngine.Object.FindAnyObjectByType<UpgradeChest>();
    Check(chest&&chest.CanOpen,"Arena contains an unopened upgrade chest");chest.reward=Resources.Load<UpgradeDefinition>("Upgrades/SchabowyRicochet");Check(upgrades&&upgrades.SchabowyExtraTargets==0,"Player begins without ricochet");
    var dummies=UnityEngine.Object.FindObjectsByType<Enemy>().Where(e=>e.dummy).Take(4).ToList();Check(dummies.Count==4,"Arena has four ricochet training targets");
    var fifth=UnityEngine.Object.Instantiate(dummies[0],dummies[0].transform.parent);dummies.Add(fifth);targets=dummies.ToArray();
    for(int i=0;i<targets.Length;i++)targets[i].transform.position=new Vector3(-8+i*4,0,0);
    Physics.SyncTransforms();ResetTargets();Fire();phase=1;phaseTime=now;
   }else if(phase==1){
    if(now-phaseTime<1)return;Check(targets[0].health==9950&&targets.Skip(1).All(e=>e.health==10000),"Unupgraded Schabowy stops after the first enemy");
    chest.Open(player);Check(GameSession.Instance.paused&&GameSession.Instance.upgradeOpen&&Time.timeScale==0,"Opening the chest pauses combat and owns its modal UI");Check(upgrades.SchabowyExtraTargets==0&&upgrades.Acquired.Count==0,"Reward is not listed in the build before claiming");GameSession.Instance.SetBuildOpen(true);Check(!GameSession.Instance.buildOpen&&GameSession.Instance.upgradeOpen,"Build menu cannot overlap a chest opening");phase=2;phaseTime=now;
   }else if(phase==2){
    if(now-phaseTime<3.4)return;typeof(UpgradeChest).GetMethod("Claim",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(chest,null);
    Check(chest.opened&&!chest.CanOpen&&upgrades.SchabowyExtraTargets==3,"Claim grants three extra targets and consumes the chest");
    Check(!GameSession.Instance.paused&&!GameSession.Instance.upgradeOpen&&Time.timeScale==1,"Claim restores the game from modal pause");
    Check(upgrades.Acquired.Count==1&&upgrades.Acquired[0]==chest.reward,"Build lists the actual acquired upgrade definition");
    Check(!upgrades.Grant(chest.reward)&&upgrades.SchabowyExtraTargets==3&&upgrades.Acquired.Count==1,"Duplicate reward does not stack or duplicate build cards");
    GameSession.Instance.SetBuildOpen(true);Check(GameSession.Instance.buildOpen&&GameSession.Instance.paused&&Time.timeScale==0,"Build menu opens and pauses the run");
    GameSession.Instance.SetBuildOpen(false);Check(!GameSession.Instance.buildOpen&&!GameSession.Instance.paused&&Time.timeScale==1,"Closing the build menu restores gameplay");
    ResetTargets();Fire();phase=3;phaseTime=now;
   }else if(phase==3){
    if(now-phaseTime<1.2)return;Check(targets.Take(4).All(e=>e.health==9950)&&targets[4].health==10000,"Ricochet hits the original plus exactly three distinct enemies");
    ResetTargets();blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.position=new Vector3(-6,1,0);blocker.transform.localScale=new Vector3(.3f,3,5);Physics.SyncTransforms();Fire();phase=4;phaseTime=now;
   }else if(phase==4){
    if(now-phaseTime<1.2)return;Check(targets[0].health==9950&&targets.Skip(1).All(e=>e.health==10000),"Ricochet does not choose targets through solid cover");
    Check(UnityEngine.Object.FindObjectsByType<Projectile>().Length==0,"Projectile terminates when no visible target remains");phase=5;phaseTime=now;
   }else if(phase==5){
    if(now-phaseTime<2)return;Check(!chest,"Used chest disappears after four seconds");Check(GameObject.Find("Dym otwartej skrzynki"),"Smoke survives the chest long enough to fade");phase=6;phaseTime=now;
   }else if(phase==6){
    if(now-phaseTime<2)return;Check(!GameObject.Find("Dym otwartej skrzynki"),"Smoke is cleaned up after fading");
    var console=GameSession.Instance.GetComponent<DebugConsole>();Check(console,"Debug console is installed");console.Execute("give 2.1");console.Execute("give 2.2");
    Check(upgrades.ChrupiacaPiercing&&upgrades.ChrupiacaBleedDamage==5&&upgrades.ChrupiacaBleedInterval==.25f&&upgrades.ChrupiacaBleedDuration==3,"Give commands grant both Chrupiaca upgrades");
    console.Execute("give 2.1");console.Execute("give 9.9");Check(upgrades.Acquired.Count==3,"Duplicate and invalid give commands do not change the build");
    UnityEngine.Object.Destroy(blocker);phase=7;phaseTime=Time.time;
   }else if(phase==7){
    if(Time.time-phaseTime<.2f)return;ResetTargets();FireChrupiaca();phase=8;phaseTime=Time.time;
   }else if(phase==8){
    if(Time.time-phaseTime<3.5f)return;Check(targets.All(e=>e.health==9920),"Piercing hits all five targets once and bleed deals exactly sixty damage to each");
    ResetTargets();blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.position=new Vector3(-6,1,0);blocker.transform.localScale=new Vector3(.3f,3,5);Physics.SyncTransforms();FireChrupiaca();phase=9;phaseTime=Time.time;
   }else if(phase==9){
    if(Time.time-phaseTime<3.5f)return;Check(targets[0].health==9920&&targets.Skip(1).All(e=>e.health==10000),"Piercing stops at walls and does not apply bleed through cover");
    Check(errors.Count==0,"No runtime errors during upgrade checks");Finish(0);
   }
  }catch(Exception exception){errors.Add(exception.ToString());Debug.LogException(exception);Finish(1);}
 }
 static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;File.WriteAllLines("Validation/Reports/upgrade-results.txt",results);File.WriteAllLines("Validation/Reports/upgrade-errors.txt",errors);if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
}
}
