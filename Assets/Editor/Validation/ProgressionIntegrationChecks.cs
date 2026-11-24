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
public static class ProgressionIntegrationChecks {
 const string Key="RetyKotlety.ProgressionIntegrationChecks";static double start;
 static readonly List<string> results=new List<string>(),errors=new List<string>();
 static ProgressionIntegrationChecks(){if(SessionState.GetBool(Key,false))EditorApplication.update+=Tick;}
 public static void Run(){if(!Application.isBatchMode&&!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())return;EditorSceneManager.OpenScene("Assets/Scenes/MovementArena.unity");SessionState.SetBool(Key,true);EditorApplication.update-=Tick;EditorApplication.update+=Tick;EditorApplication.isPlaying=true;}
 static void Check(bool ok,string label){if(!ok)throw new Exception(label);results.Add("PASS "+label);}
 static void Log(string message,string stack,LogType type){if(type==LogType.Error||type==LogType.Exception||type==LogType.Assert)errors.Add(message);}
 static Enemy MakeEnemy(bool dummy){var root=new GameObject("Progression test enemy");root.transform.position=new Vector3(1000,10,1000);root.AddComponent<NavMeshAgent>().enabled=false;var enemy=root.AddComponent<Enemy>();enemy.enabled=false;enemy.dummy=dummy;enemy.health=1;return enemy;}
 static void Tick(){
  if(!EditorApplication.isPlaying)return;if(start==0){start=EditorApplication.timeSinceStartup;Application.logMessageReceived+=Log;}
  if(EditorApplication.timeSinceStartup-start<2)return;
  try{
   var session=GameSession.Instance;Check(session&&session.player,"Arena installs player and session");var player=session.player;player.enabled=false;
   var progression=player.GetComponent<PlayerProgression>();var upgrades=player.GetComponent<PlayerUpgrades>();var shop=session.GetComponent<LevelShop>();
   Check(progression&&shop,"Progression and shop install automatically");
   var definitions=Resources.LoadAll<UpgradeDefinition>("Upgrades");Check(definitions.Count(d=>d.IsShopUpgrade)==13&&definitions.Count(d=>d.IsWeaponUpgrade)==6,"Shop contains eleven incremental items and two movement unlocks; chests contain six weapon upgrades");
   foreach(var point in UnityEngine.Object.FindObjectsByType<PointPickup>())UnityEngine.Object.DestroyImmediate(point.gameObject);
   var enemy=MakeEnemy(false);enemy.Damage(1);enemy.Damage(1);
   Check(progression.State.Kills==1&&progression.State.Experience==10&&progression.State.Points==0,"Death gives XP once while currency remains on the ground");
   var drops=UnityEngine.Object.FindObjectsByType<PointPickup>();Check(drops.Length==1,"Death creates exactly one currency pickup");
   drops[0].transform.position=player.transform.position+Vector3.up*.1f;
   typeof(PointPickup).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(drops[0],null);
   typeof(PointPickup).GetMethod("Update",BindingFlags.Instance|BindingFlags.NonPublic).Invoke(drops[0],null);
   Check(progression.State.Points==5,"Collecting a pickup credits its value exactly once");
   var dummy=MakeEnemy(true);dummy.Damage(1);Check(progression.State.Kills==1&&progression.State.Experience==10,"Training targets do not farm XP or currency");
   shop.Open();Check(!session.shopOpen,"Shop is locked before first level up");
   var farDrop=player.transform.position+Vector3.right*30;
   progression.RewardKill(farDrop,false);
   Check(progression.State.Points==5,"Distant points wait on the ground before level up");
   progression.RewardKill(farDrop,false);
   Check(progression.State.Points==15,"Level up collects old and triggering drops before the shop opens, without recounting manual pickups");
   Check(progression.State.Level==2&&progression.LevelNoticePending,"Level up queues a shop notice");
   session.SetBuildOpen(true);shop.Open();Check(!session.shopOpen&&progression.LevelNoticePending,"Queued shop waits while another menu is open");session.SetBuildOpen(false);
   session.modalClosedFrame=-1;shop.Open();Check(session.shopOpen&&session.paused&&Time.timeScale==0&&!progression.LevelNoticePending,"Shop pauses combat and acknowledges the level");
   var bread=Resources.Load<UpgradeDefinition>("Upgrades/DoubleBreading");var ricochet=Resources.Load<UpgradeDefinition>("Upgrades/SchabowyRicochet");
   var extra=Resources.Load<UpgradeDefinition>("Upgrades/ExtraProjectiles");
   Check(!progression.TryBuy(extra)&&progression.State.Points==15&&upgrades.Count(extra)==0,"Insufficient funds neither grant nor charge an item");
   progression.State.AddPoints(100);int before=progression.State.Points;
   Check(progression.TryBuy(bread)&&upgrades.Count(bread)==1&&progression.State.Points==before-10,"Purchase grants one stack and spends its displayed price");
   Check(progression.Price(bread)==15&&progression.TryBuy(bread)&&upgrades.Count(bread)==2&&progression.State.Points==before-25,"Repeat purchase uses the increased stack price");
   before=progression.State.Points;Check(!progression.TryBuy(ricochet)&&progression.State.Points==before&&!upgrades.Has(ricochet),"Weapon upgrades cannot be purchased through the shop");
   var slide=Resources.Load<UpgradeDefinition>("Upgrades/GreasySlide");Check(progression.TryBuy(slide),"Shop grants movement unlocks");before=progression.State.Points;Check(!progression.TryBuy(slide)&&progression.State.Points==before,"Owned single-use item cannot charge again");
   var vitality=Resources.Load<UpgradeDefinition>("Upgrades/MaxHealth");
   player.health=60;
   Check(progression.TryBuy(vitality)&&progression.TryBuy(vitality)&&player.MaxHealth==140&&player.health==100,"HP purchases preserve missing health and stack");
   player.RestoreHealth(100);
   Check(player.health==140,"Healing uses upgraded HP limit");
   Check(progression.TryBuy(Resources.Load<UpgradeDefinition>("Upgrades/ShotSpeed"))&&Mathf.Approximately(PlayerUpgrades.ShotSpeedFor(player.gameObject),1.15f)&&upgrades.AttackSpeedMultiplier==1,"Shot speed purchase increases flight speed independently of attack rate");
   Check(progression.TryBuy(Resources.Load<UpgradeDefinition>("Upgrades/AttackRate"))&&Mathf.Approximately(upgrades.AttackSpeedMultiplier,1.1f),"Attack rate purchase increases firing cadence");
   Check(progression.TryBuy(Resources.Load<UpgradeDefinition>("Upgrades/Regeneration"))&&upgrades.RegenPerSecond==.5f,"Regeneration is granted through the shop");
   shop.Close();Check(!session.paused&&!session.shopOpen&&Time.timeScale==1,"Closing the shop restores combat");Check(!progression.TryBuy(bread),"Purchases outside an open shop are rejected");
   var root=new GameObject("Progression test chest");var chest=root.AddComponent<UpgradeChest>();chest.reward=bread;chest.Open(player);
   var cards=(UpgradeDefinition[])typeof(UpgradeChest).GetField("cards",BindingFlags.Instance|BindingFlags.NonPublic).GetValue(chest);
   Check(chest.opened&&chest.reward.IsWeaponUpgrade&&cards.All(d=>d&&d.IsWeaponUpgrade),"Chest rejects a legacy item reward and rolls only weapon upgrades");UnityEngine.Object.DestroyImmediate(root);
   Check(!session.upgradeOpen&&!session.paused,"Closing a chest does not leave a stuck modal pause");
   Check(errors.Count==0,"No runtime errors in progression integration checks");Finish(0);
  }catch(Exception exception){errors.Add(exception.ToString());Debug.LogException(exception);Finish(1);}
 }
 static void Finish(int code){SessionState.SetBool(Key,false);EditorApplication.update-=Tick;Application.logMessageReceived-=Log;Directory.CreateDirectory("Validation/Reports");File.WriteAllLines("Validation/Reports/progression-integration-results.txt",results);File.WriteAllLines("Validation/Reports/progression-integration-errors.txt",errors);if(Application.isBatchMode)EditorApplication.Exit(code);else EditorApplication.isPlaying=false;}
}
}
